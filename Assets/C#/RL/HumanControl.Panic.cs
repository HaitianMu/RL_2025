using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.AI;
using static BuildingGeneratiion;

public partial class HumanControl : MonoBehaviour
{
    // HumanAgent.cs
    public bool UsePanic;
    public float lastPanicLevel;//用于记录上一帧的恐慌值
    public float deltaPanic;//记录恐慌值的变化值
    void UpdatePanicLevel()
    {
        //参考文献：褚若诗. 异质行人地铁站台应急疏散行为建模与仿真[D]. 北京:北京交通大学,2022.  硕士学位论文 p22
        exitDistance=Vector3.Distance(this.transform.position, myEnv.Exits[0].transform.position); //目前距离出口的距离

        // 基础项计算
        float healthTerm = Mathf.Clamp01(1-(health /100));                    //人类自身的健康值,健康值越低，焦虑程度越高
        /*float distanceTerm = Mathf.Clamp01(exitDistance /startDistanceToExit); //距离出口的距离*/
        float sceneDiagonal = Mathf.Sqrt(
           Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalWidth, 2) +
           Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalHeight, 2)
       );

        float distanceTerm = Mathf.Clamp01(exitDistance / (sceneDiagonal/3)); //距离出口的距离比上1/2对角线的长度，远于对角线一半就一定恐慌
        // 综合计算
        panicLevel =
           0.45f * healthTerm +  //健康项初期基本为0
            0.6f * distanceTerm;
 
        //计算完恐慌度后，更新人类的期望速度,就是人类的当前速度

        // 根据恐慌度更新当前速度。插值函数。第一个参数 a 表示起始值，第二个参数 b 表示结束值，第三个参数 t 表示插值的权重

        //恐慌缓解奖励
       /* float deltaPanic = lastPanicLevel - panicLevel;
        if (deltaPanic > 0.1)
        {
            myEnv.RobotBrainList[0].AddReward(20 * deltaPanic);
            myEnv.RobotBrainList[0].LogReward("恐慌情绪缓解奖励", 20 * deltaPanic);
        }
        lastPanicLevel = panicLevel;*/
        // 群体传染项 todo
        /* float socialTerm = 0f;
         Collider[] neighbors = Physics.OverlapSphere(transform.position, 3f);
         foreach (var neighbor in neighbors)
         {
             if (neighbor.TryGetComponent<HumanControl>(out var other))
             {
                 float distance = Vector3.Distance(transform.position, other.transform.position);
                 socialTerm += other.panicLevel / (distance * distance + 0.1f);
             }
         }*/
    }

    private void UpdateBehaviorModel()
    {
        //人类状态转变：参考文献：[1]孙华锴.考虑恐慌情绪的人群疏散行为模型研究[D].中南大学,2022.DOI:10.27661/d.cnki.gzhnu.2022.000847.

        if (myEnv.usePanic&&UsePanic)
        {
            if (panicLevel < 0.3) { CurrentState = 0; }
            else if (panicLevel <= 0.6 && panicLevel >= 0.3) { CurrentState = 1; }
            else if (panicLevel > 0.6 ) { CurrentState = 2; }

            switch (CurrentState)
            {
                case 0: MoveModel0(); break;
                case 1: MoveModel1(); break;
                case 2: MoveModel2(); break;
            }

            // 在HumanControl的UpdateBehaviorModel()中追加：
            if (CurrentState == 2|| CurrentState ==1) // 恐慌状态或焦虑状态
            {
              /*  myEnv.RobotBrainList[0].AddReward(-0.02f * panicLevel);
                myEnv.RobotBrainList[0].LogReward("人类处于恐慌状态的惩罚", -0.02f * panicLevel);*/
            }
        }
        else MoveModel0();//如果不启用恐慌模式，默认使用正常模式

        if (myLeader != null)
        {
            //有领导者，就跟着领导者移动
            MoveModel0();
        }
    }

    private void MoveModel0()//正常移动逻辑
    {
        _myNavMeshAgent.speed = 10;
        switch (myBehaviourMode)
        {
            case "Follower":
                FollowerUpdate();
                break;
            case "Leader":
                LeaderUpdate();
                break;
        }
    }
    private void MoveModel1() // 恐慌度大于0.3，小于0.7，焦虑模式：人类会加快移动速度
    {
       // print(this.gameObject.name+"正在以模式1移动");
        _myNavMeshAgent.speed = 12;
        switch (myBehaviourMode)
        {
            case "Follower":
                FollowerUpdate();
                break;
            case "Leader":
                LeaderUpdate();
                break;
        }
    }

    private void MoveModel2()
    {
        /*移动部分！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！*/
        // 条件1：达到间隔时间 或 条件2：接近目标点
        if (myLeader != null)
        {
            myLeader.GetComponent<RobotControl>().myDirectFollowers.Remove(gameObject.GetComponent<HumanControl>());
            myLeader = null;
        }
        myTargetDoor = null;
      
        myBehaviourMode = "Leader";
        
        _myNavMeshAgent.speed = 10;
        if (Time.time - lastPanicUpdateTime > panicMoveInterval ||
            Vector3.Distance(transform.position, myDestination) < 0.5f)
        {
            UpdatePanicDestination();
            lastPanicUpdateTime = Time.time;
        }
        /*移动部分！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！*/

        /*与机器人进行对抗的部分！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！*/
        HandleRobotInteraction();
    }
    //人类的混乱移动！！！！！！！！！！！！！！！！！！！！！！！
    private float panicMoveInterval;  // 目标更新间隔

    private float panicMoveRadius = 5f;     // 随机移动半径
    private float lastPanicUpdateTime;      // 上次更新时间
    private Vector3 currentPanicDirection;  // 当前移动方向
    private Vector3 lastSafePosition;   //安全位置，用于随机目的地不可达时，原路返回
    private void UpdatePanicDestination()
    {
        lastSafePosition = transform.position;
        // 方向持续性：70%概率保持当前方向偏移
        if (currentPanicDirection != Vector3.zero && Random.value < 0.7f)
        {
            float angleOffset = Random.Range(-30f, 30f);
            Vector3 newDirection = Quaternion.Euler(0, angleOffset, 0) * currentPanicDirection;
            myDestination = transform.position + newDirection.normalized * panicMoveRadius;
        }
        else // 30%概率全新随机方向
        {
            currentPanicDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
            myDestination = transform.position + currentPanicDirection * panicMoveRadius;
        }

        // 导航验证，如果不可达，就在附近随机一个地点
        if (!ValidateNavDestination(myDestination))
        {
            myDestination = GetFallbackPosition();
        }

        _myNavMeshAgent.SetDestination(myDestination);
    }


    private bool ValidateNavDestination(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        if (_myNavMeshAgent.CalculatePath(target, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }
   private Vector3 GetFallbackPosition()
    {
        // 方案1：尝试返回上一个安全位置
    if (lastSafePosition != Vector3.zero &&
        Vector3.Distance(transform.position, lastSafePosition) > 2f)
        {
            if (ValidateNavDestination(lastSafePosition))
                return lastSafePosition;
        }
        // 最终方案：当前位置周围随机点
        return transform.position + Random.insideUnitSphere * 1f;
    }
    //人类的混乱移动！！！！！！！！！！！！！！！！！！！！！！！


    //与机器人的对抗行为：！！！！！！！！！！！
    public float robotDetectTime; // 记录首次检测到机器人的时间
    public void HandleRobotInteraction()
    {
        List<GameObject> leaderCandidates = GetCandidate(new List<string> { "Human", "Robot" }, 360, 3).Item1;
        
        if (leaderCandidates.Count > 0)//在视野里看到了机器人
        {
           // print("附近有个机器人，我好害怕");
            if (robotDetectTime == 0)
            {
                robotDetectTime = Time.time; // 记录首次检测时间,这里每一帧都会更新一次时间。你引导个P
              //  print("这个机器人是在"+robotDetectTime+"开始跟着我的");
            }
            GameObject leader = leaderCandidates[0];
            float robotDistance = Vector3.Distance(transform.position, leader.transform.position);
            // 对抗条件：恐慌度较高且机器人接近,只有恐慌度较高时才会进入该状态
          if ( robotDistance < 3f)
             {
                // 第一阶段：抗拒2s
                if (Time.time - robotDetectTime < 5  )
                {
                   // print("现在时间是"+"ta跟着我"+ (Time.time - robotDetectTime) + "s了，我要离他远一点");
                    // 推开行为
                    Vector3 pushDir = (transform.position - leader.transform.position).normalized;
                    
                    _myNavMeshAgent.velocity = pushDir * 2f;
                }
                // 第二阶段：屈服
                else
                {
                   // print("它好像是来救我的，我跟着他走吧");
                    robotDetectTime = 0;
                    UsePanic = false;//弃用人类的恐慌状态，人类转变为模式一，

                    if (myEnv.useRobot)
                    {
                        leader.GetComponent<RobotControl>().myAgent.AddReward(50);
                        myEnv.RobotBrainList[0].LogReward("人类脱离恐慌状态的奖励", 50);
                    }
                }
            }
        }
    }
   
}


