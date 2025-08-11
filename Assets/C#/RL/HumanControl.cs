using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static BuildingGeneratiion;
using Random = UnityEngine.Random;

public partial class HumanControl: MonoBehaviour
{
    public EnvControl myEnv;
    public HumanBrain myHumanBrain;

    public Transform targetPosition; // 目标位置
    public int visionLimit = 1; // 射线检测距离
    private NavMeshAgent _myNavMeshAgent; // 导航代理组件
    public Queue<GameObject> _doorMemoryQueue;//用于记录人看到的门
    public GameObject myTargetDoor = null;  // 当前计划前往的门                              
    public GameObject lastDoorWentThrough;// 上一扇经过的门
    public Vector3 myDestination;     // 移动的目的地 
    public bool isReturnFromLastDoor;  // 是否从死胡同返回
    public String myBehaviourMode; //该人类的行为模式

    public int myFollowerCounter;  //跟随者的数量

    public List<GameObject> RbtList;  //发现的机器人列表
    public GameObject myLeader;//领导者目前只能是机器人 1.19

    private const float FOLLOWER_DISTANCE_THRESHOLD = 0.5f; //切换状态的最小距离

    //奖励相关参数
    public float health;//人类血量
    private float DelayRate = 0.01f;//人类血量衰减速率


    //人类恐慌状态相关参数
    [SerializeField] float exitDistance;//距离出口的距离
    [SerializeField] float startDistanceToExit;//场景开始时距离出口的距离

    [SerializeField] float startdesiredSpeed;//人类冷静状态下的期望速度
    [SerializeField] float MaxSpeed;         //人类恐慌状态下的期望速度

    public float panicLevel;  //恐慌等级
    public int CurrentState;    //人类的行为状态，
                                //0是理性模式：跟随机器人或自主导航
                                //1是焦虑模式：随机路径偏移
                                //2是恐慌模式：完全随机移动
     //用来记录距离出口的距离
    float LastDistanceToExit;

    public void Start()
    {
        myLeader = null;
        myBehaviourMode = "Leader";
        _myNavMeshAgent = GetComponent<NavMeshAgent>();
        myDestination = new Vector3();
        _doorMemoryQueue = new Queue<GameObject>();
        myTargetDoor = null;
        lastDoorWentThrough = null;
        health = 100.0f;
        myTopLevelLeader = gameObject;
        dazingCountDown = Random.Range(2, 8);
        //恐慌值计算的参数!!!!
        UsePanic =true;
        CurrentState = 0;  //初始状态设置为理性模式
        //恐慌移动!!!!!!!
        // 每个个体初始化不同的延迟偏移量（0 - 2秒）
         robotDetectTime = 0;


    }
    private void FixedUpdate()
    {
        if (!myEnv.useHumanAgent) {
            //每个人刚开始都是独立的领导者，但是随着程序的进行，
            //当看到机器人时，人类会进行跟随
           // print("人类不使用大脑");
            if (myEnv.usePanic && UsePanic)
            {
                UpdatePanicLevel();    //更新人类的恐慌度等级
            }
            UpdateBehaviorModel(); //更新行为模式

        }
        else
        {
            // print("使用人类大脑决定自己移动状态");
           // CurrentState = myHumanBrain.HumanState;
            if (health > 40)
            {
                CurrentState = myHumanBrain.HumanState;
            }
            else
            {
                CurrentState = 1;
            }

            switch (CurrentState)
            {
                case 0: MoveModel0(); break;
                case 1: MoveModel1(); break;
                case 2: MoveModel2(); break;
            }
        }


        //在这里修改人类的生命值
        if (health > 0)
        {
            health -= DelayRate;
            if (health > 40&&myEnv.useHumanAgent)
            {
                float AliveReward = 0.0002f * (health - 40);
                myHumanBrain.AddReward(AliveReward);//人类存活奖励
                myHumanBrain.LogReward("人类存活奖励",AliveReward);
            }

            if (myLeader != null)
            {
                float currentDistance = Vector3.Distance(transform.position, myEnv.Exits[0].transform.position);
                float deltaDistance = LastDistanceToExit - currentDistance; // 注意顺序，变近是正的
                LastDistanceToExit = currentDistance;

                if (myEnv.useRobot)
                {

                    if (deltaDistance > 0.01f) // 变近了，且变化大于阈值
                    {
                        myEnv.RobotBrainList[0].AddReward(0.04f * deltaDistance); // 奖励（放大正向奖励系数）
                        myEnv.RobotBrainList[0].LogReward("带领人类朝出口移动正奖励", 0.04f * deltaDistance);
                    }
                    else if (deltaDistance < -0.01f) // 变远了
                    {
                        myEnv.RobotBrainList[0].AddReward(0.08f * deltaDistance); // 小幅惩罚（负的delta）
                        myEnv.RobotBrainList[0].LogReward("远离出口负奖励", 0.08f * deltaDistance);
                    }
                }

                //正向奖励系数(0.05) > 负向惩罚系数(0.02绝对值)，可能导致机器人故意反复靠近/远离出口刷分4.28,17:30

                // delta变化很小（-0.01到0.01之间）就不奖励了，视为抖动或站稳，不处理
            }

        }
        else if (health <= 0)
        {
            if (myLeader is not null)
            {
                myLeader.GetComponent<RobotControl>().myDirectFollowers.Remove(gameObject.GetComponent<HumanControl>());
            }
            Debug.Log("人类死亡");

            if (myEnv.useRobot) {
                myEnv.RobotBrainList[0].AddReward(-300);
                myEnv.RobotBrainList[0].LogReward("人类死亡惩罚", -300);
            }
            if (myEnv.useHumanAgent)
            {
                myHumanBrain.AddReward(-200);//"人类死亡给人类大脑的惩罚"
                myHumanBrain.LogReward("人类死亡给人类大脑的惩罚",-200);
                myHumanBrain.EndEpisode();
            }
            //TO ADD
            gameObject.SetActive(false);
        }
    }

    private List<Vector3> GetVision(int visionWidth, int visionDiff)//生成人类视线，本质是一个向量数组
    {
        List<Vector3> myVisions = new();

        int visionBias = visionWidth / (2 * visionDiff);
        for (int visionIndex = -visionBias; visionIndex <= visionBias; visionIndex++)
        {
            Vector3 vision = Quaternion.AngleAxis(visionDiff * visionIndex, Vector3.up) * transform.forward;
            // 将生成的视线方向向量添加到列表中
            myVisions.Add(vision);
            //Debug.DrawRay(transform.position, vision , Color.green);
            // 调试用的代码，用于在场景中绘制视线方向（可注释掉）
        }
        return myVisions;
    }

    
    private void OnTriggerEnter(Collider trigger)
    {
        // Debug.Log("碰撞发生，碰撞体的标签是：" + trigger.transform.tag);
        GameObject triggerObject = trigger.gameObject;
        isReturnFromLastDoor = triggerObject == lastDoorWentThrough;
        switch (trigger.transform.tag)
        {
            case "Door":
                lastDoorWentThrough = triggerObject;
                //print("上一扇经过的门是："+ triggerObject.name);
                if (_doorMemoryQueue.Count>0&&!_doorMemoryQueue.Contains(triggerObject))
                    _doorMemoryQueue.Enqueue(triggerObject);
              // print(triggerObject.name + "已经被加入记忆");
                if (_doorMemoryQueue.Count > 3)
                    _doorMemoryQueue.Dequeue();
                break;

            case "Exit":
                // print("我成功逃离了");
                /*  myEnv.personList.Remove(this);*/
                this.gameObject.SetActive(false);
                myEnv.sumhealth += health;

                if (myEnv.useRobot)
                {
                    myEnv.RobotBrainList[0].AddReward((health));//单个人类逃生奖励,但人类有可能自己导航到出口，可能会影响训练结果，所以不能太大
                    myEnv.RobotBrainList[0].LogReward("单个人类逃生奖励", (health));

                    //!!!!!!!!!!!!!!!!!逃生率计算
                    myEnv.RobotBrainList[0].LogReward("逃生人数", 1);
                }
                if (myEnv.useHumanAgent)
                {

                    float Exitreward = health <= 40 ? 3f * health : 3f * (100 - health);
                    myHumanBrain.AddReward(Exitreward);//"人类逃脱奖励"
                    myHumanBrain.LogReward("人类逃脱奖励", Exitreward);
                    myHumanBrain.EndEpisode();
                }
               
                    myEnv.LogReward("逃生人数", 1);
                    myEnv.LogReward("人类逃生的生命值", (health));


                myEnv.EscapeHuman++;
                //在这里给予机器人奖励
                break;

            case "Fire":
                this.health -= 5;  //人类健康值-5
                if (myEnv.useHumanAgent)
                {
                    float FireReward = health > 40 ? -0.5f : -0.2f;
                    myHumanBrain.AddReward(FireReward);//"人类碰火惩罚"
                    myHumanBrain.LogReward("人类碰火惩罚", FireReward);
                }
                break;
        }
    }
}


