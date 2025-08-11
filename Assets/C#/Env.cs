using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;

public partial class Env : MonoBehaviour
{
    // �����˿����������б�
    public List<MultiAgent> agentList = new();

    // �����б�
    public List<Person> personList = new();

    // ��ʱ�����б�
    public List<GameObject> tmpFireList = new();

    // ������ݻٵ��ŵ��б�
    public List<GameObject> disabledDoorList = new();

    public List<GameObject> Exits;//�����еĳ���

    // MA-POCA��������
    public SimpleMultiAgentGroup agentGroup;

    // ���������
    [HideInInspector] public int humanCounter;

    // �������ü�����
    [HideInInspector] public int resetCountDown;

    // ÿ����Ϸ��֡��������
    [HideInInspector] public int stepCounter;

    // �ۼƽ���������
    [HideInInspector] public float[] episodeTotalRewardCounter = new float[3];

    // �Ƿ���ѵ��
    public bool isTraining;

    // �Ƿ�ʹ�û�Դ������
    public bool useFireAgent;

    // �Ƿ�ʹ��ǿ��ѧϰ������
    public bool useFloorAgent;

    // �Ƿ�ʹ�û�����
    public bool useRobot;

    private StatsRecorder m_StatsRecorder;///ADD
    public int hardness;///ADD
    public int fire_human_count;///ADD
    public double Eva;//ADD
    //public double frontE = 0;//ADD
   


    private void Start()
    {
        //print("Env������ʼ���ű���ʼ��");
        agentGroup = new SimpleMultiAgentGroup();
        m_StatsRecorder = Academy.Instance.StatsRecorder;///ADD
        hardness = 0;///ADD
        fire_human_count = 0;//ADD
        Eva = 0;//ADD
        AddRobot();
        AddPerson();
        AddExits();
        foreach (MultiAgent agent in agentList)//�������ҲҪ��
        {
            agentGroup.RegisterAgent(agent);
            agent.myEnv = this;
        }

        resetCountDown = 50;
        stepCounter = 0;
        episodeTotalRewardCounter = new float[] { 0, 0, 0 };
        CleanTheScene();
    }

    /// <summary>
    /// ��ȡ�������λ�ã���
    /// </summary>
    /// <param name="floor"></param>
    /// <returns></returns>
    private static Vector3 GetSpawnBlockPosition(int floor)//�������������������̵ģ����������ҲҪע��һ��,��û�и�����Ĳ鿴��������
    {
        Vector3 spawnBlockPosition = new();
        for (int tryCounter = 80000; tryCounter > 0; tryCounter--)
        {
            // Generate random X and Z coordinates within the (0, 10) range.
            float randomX = Random.Range(1, 9) + 0.5f;
            float randomZ = Random.Range(1, 9) + 0.5f;

            // ����������
            if (floor == 1 && randomX is > 5 and < 8 && randomZ is > 2 and < 5)
                continue;

            // Set the spawn position at the appropriate floor level
            spawnBlockPosition.Set(randomX, (floor - 1) * 4, randomZ);

            // Check if the spawn position is valid (no collision)
            if (Physics.CheckBox(spawnBlockPosition + Vector3.up, new Vector3(0.49f, 0.49f, 0.49f)) is false)

                return spawnBlockPosition;
        }
        print("���ɵ����λ����" +spawnBlockPosition);
        // Return a default value if no valid position is found
        return new Vector3();
    }

    private void CleanTheScene()
    {
        ///add
        ///
        fire_human_count = 0; //reset
        Eva = 0;//ADD
        ///end
        //print("ִ��CleanScene����");
        ResetAllAgents();

        /*foreach (Person person in personList)
            person.gameObject.SetActive(false);*/
    }
    /// <summary>
    /// �������л����˿���������
    /// </summary>
    private void ResetAllAgents()
    {
        if (useRobot is false)
            return;
        foreach (MultiAgent agent in agentList)
        {
            //print("ִ�����л��������ú���");
            //agent.gameObject.SetActive(true);
            //agent.robotInfo.gameObject.SetActive(false);//ADD
            //print("1");
            agent.ResetAgent();
            //print("2");
            agent.gameObject.SetActive(true);
            agent.robotInfo.gameObject.SetActive(true);
            agent.robotInfo.Start();
        }
    }
    /// <summary>
    /// ������������
    /// </summary>
    public void ResetAllHumans()
    {
        Debug.Log("��������������ĺ���");
        foreach (Person human in personList)
        {
            Debug.Log("����ʱ�ҵ����б��е�����"+human);
            int randomFloor = 1;
            Vector3 spawnPosition = GetSpawnBlockPosition(randomFloor) + new Vector3(0, 1, 0);

            if (spawnPosition == Vector3.zero)
                continue;
            if (!human.gameObject.activeSelf)
            {
                human.gameObject.SetActive(true);
                human.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            }  // ��������}
            human.Start();
        }
    }

    private void AddRobot() {//�����л����˼���agentlist��Ȼ��ʵ�ֻ����˺ͻ����˴���֮��ı�������
        //Find�����޷��ݹ�����Ѱ��
        //print("ִ�м�������˺���");
        //�ҵ����ض�������ű�������
        Transform Multiagent = transform.Find("MultiAgent");
       // print(Multiagent.name);
        agentList.Add(Multiagent.GetComponent<MultiAgent>());
        //�ҵ����еĻ����ˣ�������robots����
        Transform robotList = transform.Find("RobotList");
        //print(robotList.name);
       // print("�����˵�����Ϊ��"+robotList.childCount);
        Robot[] robots=new Robot[robotList.childCount];


        //һ��Ѹ�ֵ�����������˴���ͻ�����֮����໥��ֵ
        for (int i = 0; i < robotList.childCount; i++)
        {
            robots[i] = robotList.GetChild(i).GetComponent<Robot>();
            Multiagent.GetComponent<MultiAgent>().robot = robots[i].gameObject;
            Multiagent.GetComponent<MultiAgent>().robotRigidbody = robots[i].GetComponent<Rigidbody>();
            Multiagent.GetComponent<MultiAgent>().robotInfo = robots[i];
            Multiagent.GetComponent<MultiAgent>().robotNavMeshAgent = robots[i].GetComponent<NavMeshAgent>();
            Multiagent.GetComponent<MultiAgent>().myEnv = this;
            robots[i].myAgent= Multiagent.GetComponent<MultiAgent>();
        }
    
        //print("�����˴�����صĻ�����������" + Multiagent.GetComponent<MultiAgent>().robot.gameObject.name);
        //�������˹��ص����������ϣ�����������Ҫ���Ƿ񽫣��������������е�robotAgent��չ��robotAgents
    }
    private void AddPerson()
    {//�Ѷ����������agentlist��Ȼ��ÿһ�������˼ӵ�������������robotAgeent���Ե���
        //print("ִ�м������ຯ��");
        //�ҵ����ض�������ű�������
        Transform humanList = transform.Find("HumanList");
        for (int i = 0; i < humanList.childCount; i++)
        {
           personList.Add(humanList.GetChild(i).GetComponent<Person>());
            humanList.GetChild(i).GetComponent<Person>().myEnv = this;
        }
        
    }
    private void AddExits() {
        Transform ExitList = transform.Find("Building").Find("Exit");
        //GameObject[] targets = GameObject.FindGameObjectsWithTag("tag");//����tag��ͬ����������,,,find�����������ܽϴ�
        for (int i = 0; i < ExitList.childCount; i++)
        {
           Exits.Add(ExitList.GetChild(i).gameObject);
        }
    }
}
