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

    public Transform targetPosition; // Ŀ��λ��
    public int visionLimit = 1; // ���߼�����
    private NavMeshAgent _myNavMeshAgent; // �����������
    public Queue<GameObject> _doorMemoryQueue;//���ڼ�¼�˿�������
    public GameObject myTargetDoor = null;  // ��ǰ�ƻ�ǰ������                              
    public GameObject lastDoorWentThrough;// ��һ�Ⱦ�������
    public Vector3 myDestination;     // �ƶ���Ŀ�ĵ� 
    public bool isReturnFromLastDoor;  // �Ƿ������ͬ����
    public String myBehaviourMode; //���������Ϊģʽ

    public int myFollowerCounter;  //�����ߵ�����

    public List<GameObject> RbtList;  //���ֵĻ������б�
    public GameObject myLeader;//�쵼��Ŀǰֻ���ǻ����� 1.19

    private const float FOLLOWER_DISTANCE_THRESHOLD = 0.5f; //�л�״̬����С����

    //������ز���
    public float health;//����Ѫ��
    private float DelayRate = 0.01f;//����Ѫ��˥������


    //����ֻ�״̬��ز���
    [SerializeField] float exitDistance;//������ڵľ���
    [SerializeField] float startDistanceToExit;//������ʼʱ������ڵľ���

    [SerializeField] float startdesiredSpeed;//�����侲״̬�µ������ٶ�
    [SerializeField] float MaxSpeed;         //����ֻ�״̬�µ������ٶ�

    public float panicLevel;  //�ֻŵȼ�
    public int CurrentState;    //�������Ϊ״̬��
                                //0������ģʽ����������˻���������
                                //1�ǽ���ģʽ�����·��ƫ��
                                //2�ǿֻ�ģʽ����ȫ����ƶ�
     //������¼������ڵľ���
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
        //�ֻ�ֵ����Ĳ���!!!!
        UsePanic =true;
        CurrentState = 0;  //��ʼ״̬����Ϊ����ģʽ
        //�ֻ��ƶ�!!!!!!!
        // ÿ�������ʼ����ͬ���ӳ�ƫ������0 - 2�룩
         robotDetectTime = 0;


    }
    private void FixedUpdate()
    {
        if (!myEnv.useHumanAgent) {
            //ÿ���˸տ�ʼ���Ƕ������쵼�ߣ��������ų���Ľ��У�
            //������������ʱ���������и���
           // print("���಻ʹ�ô���");
            if (myEnv.usePanic && UsePanic)
            {
                UpdatePanicLevel();    //��������ĿֻŶȵȼ�
            }
            UpdateBehaviorModel(); //������Ϊģʽ

        }
        else
        {
            // print("ʹ��������Ծ����Լ��ƶ�״̬");
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


        //�������޸����������ֵ
        if (health > 0)
        {
            health -= DelayRate;
            if (health > 40&&myEnv.useHumanAgent)
            {
                float AliveReward = 0.0002f * (health - 40);
                myHumanBrain.AddReward(AliveReward);//�������
                myHumanBrain.LogReward("�������",AliveReward);
            }

            if (myLeader != null)
            {
                float currentDistance = Vector3.Distance(transform.position, myEnv.Exits[0].transform.position);
                float deltaDistance = LastDistanceToExit - currentDistance; // ע��˳�򣬱��������
                LastDistanceToExit = currentDistance;

                if (myEnv.useRobot)
                {

                    if (deltaDistance > 0.01f) // ����ˣ��ұ仯������ֵ
                    {
                        myEnv.RobotBrainList[0].AddReward(0.04f * deltaDistance); // �������Ŵ�������ϵ����
                        myEnv.RobotBrainList[0].LogReward("�������೯�����ƶ�������", 0.04f * deltaDistance);
                    }
                    else if (deltaDistance < -0.01f) // ��Զ��
                    {
                        myEnv.RobotBrainList[0].AddReward(0.08f * deltaDistance); // С���ͷ�������delta��
                        myEnv.RobotBrainList[0].LogReward("Զ����ڸ�����", 0.08f * deltaDistance);
                    }
                }

                //������ϵ��(0.05) > ����ͷ�ϵ��(0.02����ֵ)�����ܵ��»����˹��ⷴ������/Զ�����ˢ��4.28,17:30

                // delta�仯��С��-0.01��0.01֮�䣩�Ͳ������ˣ���Ϊ������վ�ȣ�������
            }

        }
        else if (health <= 0)
        {
            if (myLeader is not null)
            {
                myLeader.GetComponent<RobotControl>().myDirectFollowers.Remove(gameObject.GetComponent<HumanControl>());
            }
            Debug.Log("��������");

            if (myEnv.useRobot) {
                myEnv.RobotBrainList[0].AddReward(-300);
                myEnv.RobotBrainList[0].LogReward("���������ͷ�", -300);
            }
            if (myEnv.useHumanAgent)
            {
                myHumanBrain.AddReward(-200);//"����������������Եĳͷ�"
                myHumanBrain.LogReward("����������������Եĳͷ�",-200);
                myHumanBrain.EndEpisode();
            }
            //TO ADD
            gameObject.SetActive(false);
        }
    }

    private List<Vector3> GetVision(int visionWidth, int visionDiff)//�����������ߣ�������һ����������
    {
        List<Vector3> myVisions = new();

        int visionBias = visionWidth / (2 * visionDiff);
        for (int visionIndex = -visionBias; visionIndex <= visionBias; visionIndex++)
        {
            Vector3 vision = Quaternion.AngleAxis(visionDiff * visionIndex, Vector3.up) * transform.forward;
            // �����ɵ����߷���������ӵ��б���
            myVisions.Add(vision);
            //Debug.DrawRay(transform.position, vision , Color.green);
            // �����õĴ��룬�����ڳ����л������߷��򣨿�ע�͵���
        }
        return myVisions;
    }

    
    private void OnTriggerEnter(Collider trigger)
    {
        // Debug.Log("��ײ��������ײ��ı�ǩ�ǣ�" + trigger.transform.tag);
        GameObject triggerObject = trigger.gameObject;
        isReturnFromLastDoor = triggerObject == lastDoorWentThrough;
        switch (trigger.transform.tag)
        {
            case "Door":
                lastDoorWentThrough = triggerObject;
                //print("��һ�Ⱦ��������ǣ�"+ triggerObject.name);
                if (_doorMemoryQueue.Count>0&&!_doorMemoryQueue.Contains(triggerObject))
                    _doorMemoryQueue.Enqueue(triggerObject);
              // print(triggerObject.name + "�Ѿ����������");
                if (_doorMemoryQueue.Count > 3)
                    _doorMemoryQueue.Dequeue();
                break;

            case "Exit":
                // print("�ҳɹ�������");
                /*  myEnv.personList.Remove(this);*/
                this.gameObject.SetActive(false);
                myEnv.sumhealth += health;

                if (myEnv.useRobot)
                {
                    myEnv.RobotBrainList[0].AddReward((health));//����������������,�������п����Լ����������ڣ����ܻ�Ӱ��ѵ����������Բ���̫��
                    myEnv.RobotBrainList[0].LogReward("����������������", (health));

                    //!!!!!!!!!!!!!!!!!�����ʼ���
                    myEnv.RobotBrainList[0].LogReward("��������", 1);
                }
                if (myEnv.useHumanAgent)
                {

                    float Exitreward = health <= 40 ? 3f * health : 3f * (100 - health);
                    myHumanBrain.AddReward(Exitreward);//"�������ѽ���"
                    myHumanBrain.LogReward("�������ѽ���", Exitreward);
                    myHumanBrain.EndEpisode();
                }
               
                    myEnv.LogReward("��������", 1);
                    myEnv.LogReward("��������������ֵ", (health));


                myEnv.EscapeHuman++;
                //�������������˽���
                break;

            case "Fire":
                this.health -= 5;  //���ཡ��ֵ-5
                if (myEnv.useHumanAgent)
                {
                    float FireReward = health > 40 ? -0.5f : -0.2f;
                    myHumanBrain.AddReward(FireReward);//"��������ͷ�"
                    myHumanBrain.LogReward("��������ͷ�", FireReward);
                }
                break;
        }
    }
}


