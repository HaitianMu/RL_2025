using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public partial class EnvControl : MonoBehaviour
{
   
    //�����б�
    public List<HumanControl> personList = new();
    //��������б�

    public List<HumanBrain> HumanBrainList = new();
    //�������б�
    public List<RobotControl> RobotList = new();
    // �����˴����б�
    public List<RobotBrain> RobotBrainList = new();

    public List<FireControl>FireList = new();

    //�����еĳ���
    public List<GameObject> Exits=new();
    //�洢�����λ����Ϣ
    public List<Vector3> cachedRoomPositions;
 
    /*���ɳ����͵���ʱ�õ������ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public ComplexityControl complexityControl;//���ɻ����õ������
    public NavMeshSurface surface;//���ɵ��������
    public GameObject HumanPrefab;//���������õ������
    public GameObject RobotPrefab;//��ӻ������õ������
    public GameObject BrainPrefab;//�����˴���Ԥ����
    public GameObject FirePrefab; //����Ԥ����
    public GameObject SmokePrefab;//����Ԥ����

    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public GameObject RobotParent;//�����˵ĸ����壬������������
    public GameObject FireParent;//�����˵ĸ����壬������������

    public GameObject RobotBrainParent;//�����˴��Եĸ����壬�����������ģ���Ŀǰ����ò���

    public GameObject HumanBrainParent;//������Ը����壬

    public GameObject humanParent;//�����˵ĸ����壬������������

    public float TotalSize;//�����ܴ�С
    public int RoomNum;//������Ŀ
    // �Ƿ���ѵ��
    public bool isTraining;
    // �Ƿ�ʹ�û�����
    public bool useRobot;
    //�Ƿ�ʹ�û���
    public bool useFire;
    //�Ƿ�ʹ�û���������
    public bool useHumanAgent;
    public bool usePanic; //�Ƿ���������ֻ�

    /*չʾDemoʹ�ã����ڳ�������!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public int currentFloorhuman=0;
    public int FireStep;//���������������ɻ���
    public int StepCount;//��������������������ʱ���ó���
    public int MaxStep;//�����
    public int layoutNum;

   //����������4.20 ����������������������������������������������������������������������������������������
    //�ֶ���ӻ���ʱ�Ļ���λ�û���
    public List<Vector3> FirePosition;
    //��ӵĻ�������
    public int FireNum;


    //���ڽ�������
    public float sumhealth;//���ڼ�¼��������ʱ�����彡��ֵ
    public float startTime;//��¼ÿ���غϿ�ʼ��ʱ��
    //
    public int EnpisodeNum=0;//���ڼ�¼�غ��������������н׶���ѵ��
    public int HumanNum = 1;//ÿһ�غ���ӵ����������������ӣ�����һ��ʼ�ͷ�10��


    public int EscapeHuman;//�ɹ����ѵ�����

    public float EnpisodeTime;//��¼ÿһ�غϵ�ʱ��

    public int humanBrainNum=10;//���������Ե�����
    private void Start()
    {
        EnpisodeNum = 0;
        HumanNum = 10;
        EnpisodeTime = 0;
     
        //Ԥ��ģʽ��������ʹ��̰���㷨������ʹ�������ƶ���������Ϊ�������ɵص�!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        if (!isTraining||isTraining)
        {
            Debug.Log("Env��start����");

/*            if (currentFloorhuman == 0)//�ڶ��γ�����ʼ��������ⳡ���е���������
            {*/
                CleanTheScene();
                string filename = "layout";//layout,layout_900_15
            string[] name = { "layout1", "layout2", "layout3", "layout4",
                "apartment", "family_house", "clinic", "elementary_school", "hospital_er","tech_office", "shopping_mall" };
                //string layoutname = name[layoutNum];

                string layoutname = "layout_9";//office_floor_1
                                               //print("��ȡ�Ĳ�������Ϊ��" + layoutname);

              //!!!!!!!!!!!!!!!!!!!!!!!!����ʵ��
                complexityControl.BeginGenerationJsonLoad(filename, layoutname); //������ָ���������ݵ��ļ�
                AddExits();//��ӳ��ڣ��Ա��ں��������˵���ʹ��
                surface.BuildNavMesh();//���ɵ���
                                       //!!!!!!!!!!!!!!!!!!!!!!!!����ʵ��

                AddPerson(10);

               if (useHumanAgent) {
                AddHumanBrain(humanBrainNum);
                }

                currentFloorhuman = 0;
                //���㳡�����м������࣬�Գ�������û��Ӱ��
                foreach (HumanControl human in personList)//ͳ�Ƶ�ǰ¥�������
                {
                    if (human.isActiveAndEnabled)
                    {
                        currentFloorhuman++;
                    }
                    //Debug.Log(currentFloorhuman);
                }

            if (useRobot)
            {
                AddRobot();//��ӻ�����
                AddRobotBrain();//��ӻ����˴���
            }
                /*  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
                FireStep = 0;//ѵ��������Ŀ
                MaxStep = 12000;

                /*  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
                //�ֶ���ӵĻ���λ��
                AddFirePosition();
                FireNum = 0;
            }
        /*}*/
        //Ԥ��ģʽ��������ʹ��̰���㷨������ʹ�������ƶ���������Ϊ�������ɵص�!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }

    private void FixedUpdate()
    {
        EnpisodeTime += Time.deltaTime;
        //Ԥ��ģʽ��������ʹ��̰���㷨������ʹ�������ƶ���������Ϊ�������ɵص�!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        if (!isTraining)
        {
            if (useFire)
            {
                FireStep++;
                if (FireStep % 100 == 0)
                {
                    if (FireNum < FirePosition.Count)
                    {
                        AddSmoke(FirePosition[FireNum]);
                        AddFire(FirePosition[FireNum]);
                        FireNum++;
                        FireStep++;
                    }
                    else { FireStep++; }
                }
                if (FireStep % 100 == 0)//ÿ��һ��֡���µ�����ͼ
                {
                    var surface = FindObjectOfType<NavMeshSurface>();
                    if (surface != null)
                    {
                        surface.UpdateNavMesh(surface.navMeshData);
                    }
                    FireStep++;
                }
            }
        }


        else if (isTraining)
        {

            //�ȼ��㳡���е�����
            currentFloorhuman = 0;
            //���㳡�����м������࣬�Գ�������û��Ӱ��
            foreach (HumanControl human in personList)//ͳ�Ƶ�ǰ¥�������
            {
                if (human.isActiveAndEnabled)
                {
                    currentFloorhuman++;
                }
            }
            //�ȼ��㳡���е�����

            if (useRobot)
            {
                if (currentFloorhuman == 0 || RobotBrainList[0].stuckCounter > 100)  //�������е���������Ϊ0ʱ�����¹�����һ�ֵ�ѵ������;��������������ײ��100��
                {
                  
                    this.LogReward("������", 10);
                    this.LogReward("�غ���", 1);
                    this.LogReward("���л��ѵ���ʱ��", EnpisodeTime);
                    EnpisodeTime = 0;

                    RobotBrainList[0].LogReward("������", 10);//ÿ�ɹ�����һ���ˣ��������100�㽱��
                    RobotBrainList[0].AddReward(EscapeHuman * 50);//ÿ�ɹ�����һ���ˣ��������100�㽱��
                    RobotBrainList[0].LogReward("���ս���", EscapeHuman * 50);//ÿ�ɹ�����һ���ˣ��������100�㽱��
                    RobotBrainList[0].EndEpisode();
                    string filename = "layout";
                    //string layoutname = name[layoutNum];

                    string layoutname = "layout_6";
                    //print("��ȡ�Ĳ�������Ϊ��" + layoutname);
                    CleanTheScene();
                    //UnityEngine.Random.Range(8, 15); // ���ֵķ�������
                    complexityControl.BeginGenerationJsonLoad(filename, layoutname); //������ָ���������ݵ��ļ�
                    RecordRoomPosition(complexityControl.buildingGeneration.roomList);//��¼�����λ�ã����ں����Ĺ�һ��ѵ��

                    AddExits();//��ӳ��ڣ��Ա��ں��������˵���ʹ��


                    surface.BuildNavMesh();//���ɵ���

                    AddPerson(HumanNum);
                    if (useHumanAgent)
                    {
                        AddHumanBrain(humanBrainNum);
                    }

                    foreach (HumanControl human in personList)//��ʼ��ͳ�Ƶ�ǰ¥���������
                                                              //����ͨ������currentFloorhuman���������������Ƿ�ʼ�µĻغϣ�����ÿһ֡���в��ң�Ӱ������
                    {
                        if (human.isActiveAndEnabled)
                        {
                            currentFloorhuman++;
                        }
                        //Debug.Log(currentFloorhuman);
                    }
                        AddRobot();//��ӻ�����
                        AddRobotBrain();//��ӻ����˴���    
                        AddFirePosition();
                    FireNum = 0;
                    FireStep = 0;
                    StepCount = 0;
                    MaxStep = 10000;
                    startTime = Time.time;
                    EscapeHuman = 0;
                    HumanNum = 10;
                    useFire = true;
                    usePanic = true;
                    //myEnv.cachedDoorPositions=myEnv.GetAllDoorPositions();//����ŵ�λ����Ϣ
                    //myEnv.cachedRoomPositions = myEnv.GetAllRoomPositions();//��ӷ����λ����Ϣ
                }
            }
            else if(!useRobot) //����ӻ����˵ĳ����ع�
            {
                if (currentFloorhuman==0) {
                    this.LogReward("������", 10);
                    this.LogReward("�غ���", 1);
                    this.LogReward("���л��ѵ���ʱ��", EnpisodeTime);
                    EnpisodeTime = 0;

                    string filename = "layout";
                    string layoutname = "layout_6";
                    CleanTheScene();
                    //UnityEngine.Random.Range(8, 15); // ���ֵķ�������
                    complexityControl.BeginGenerationJsonLoad(filename, layoutname); //������ָ���������ݵ��ļ�
                    RecordRoomPosition(complexityControl.buildingGeneration.roomList);//��¼�����λ�ã����ں����Ĺ�һ��
                    AddExits();//��ӳ��ڣ��Ա��ں��������˵���ʹ��
                    surface.BuildNavMesh();//���ɵ���
                    AddPerson(HumanNum);
                    if (useHumanAgent)
                    {
                       AddHumanBrain(humanBrainNum);
                    }      //
                   
                    foreach (HumanControl human in personList)//��ʼ��ͳ�Ƶ�ǰ¥���������
                                                              //����ͨ������currentFloorhuman���������������Ƿ�ʼ�µĻغϣ�����ÿһ֡���в��ң�Ӱ������
                    {
                        if (human.isActiveAndEnabled)
                        {
                            currentFloorhuman++;
                        }
                        //Debug.Log(currentFloorhuman);
                    }
                    FireNum = 0;
                    FireStep = 0;
                    StepCount = 0;
                    MaxStep = 10000;
                    startTime = Time.time;
                    EscapeHuman = 0;
                    HumanNum = 10;
                    useFire = true;
                    usePanic = true;
                }
            }


            // ��ӻ��棡��������������������������������������������������������������������������������������������
            if (useFire)
            {
                FireStep++;
                if (FireStep % 100 == 0)
                {
                    if (FireNum < FirePosition.Count)
                    {
                        AddFire(FirePosition[FireNum]);
                        FireNum++;
                        FireStep++;
                    }
                    else { FireStep++; }
                }
                if (FireStep % 100 == 0)//ÿ��һ��֡���µ�����ͼ
                {
                    surface.UpdateNavMesh(surface.navMeshData);
                    FireStep++;
                }
            }
            // ����������������������������������������������������������������������������������������������

            // ������ʱ���û�������������������������������������������������������������������������������������������������
            if (StepCount > MaxStep&&useRobot)
            {
                if (useRobot)//�غϽ���ʱ��û�����ѵ�����ȫ������������
                {
                   /* RobotBrainList[0].AddReward(-300*currentFloorhuman);
                    RobotBrainList[0].LogReward("���������ͷ�", -300* currentFloorhuman);*/
                }

                RobotBrainList[0].EpisodeInterrupted();//��������ֹ�ûغ�
                foreach(HumanBrain humanBrain in HumanBrainList)
                {
                    humanBrain.EpisodeInterrupted();
                }
            }
            StepCount++;
        }
    }
    public void CleanTheScene()
    {
        // print("ִ��CleanScene����");
        // 1. ��ֹͣ���л���Ĵ���
        foreach (var fire in FindObjectsOfType<FireControl>())
        {
            fire.StopAllCoroutines();
        }
        // 2. ִ�г�������
        ResetAgentandClearList();

        // 3. ǿ���������գ���ѡ��
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }
    /// <summary>
    /// �������л����˿���������
    /// </summary>
}
