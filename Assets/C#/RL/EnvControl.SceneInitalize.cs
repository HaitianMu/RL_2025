using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public partial class EnvControl : MonoBehaviour
{
    private void ResetAgentandClearList()
    {
        // �������
        if (personList.Count > 0)
        {
            foreach (HumanControl person in personList)
            {
                if (person != null && person.gameObject != null)
                {
                    Destroy(person.gameObject);
                }
            }
            personList.Clear();
        }

        // ���������
        if (RobotList.Count > 0)
        {
            foreach (RobotControl robot in RobotList)
            {
                if (robot != null && robot.gameObject != null)
                {
                    Destroy(robot.gameObject);
                }
            }
            RobotList.Clear();
        }

        // ��������壬ֻ����������б��ɣ�ʵ�岻�����
        if (RobotBrainList.Count > 0)
        {
            RobotBrainList.Clear();
        }
        //��������壬ֻ����������б��ɣ�ʵ�岻�����
        if (HumanBrainList.Count > 0)
        {
            HumanBrainList.Clear();
        }
        

        // �������
        if (Exits.Count > 0)
        {
            foreach (var exit in Exits)
            {
                if (exit != null && exit.gameObject != null)
                {
                    Destroy(exit.gameObject);
                }
            }
            Exits.Clear();
        }
        cachedRoomPositions.Clear();

        if (FireList.Count > 0)  //���������Դ
        {
            foreach (FireControl fire in FireList)
            {
                if (fire != null && fire.gameObject != null)
                {
                    // ��ִ�л�����Զ��������߼�
                    fire.StopAllCoroutines();

                    // ֱ�Ӵӳ������٣������ض���أ���Ϊ��������Ҫ���ã�
                    Destroy(fire.gameObject);
                }
            }
            FireList.Clear();
        }
        if (FirePoolManager.Instance != null)
        {
            FirePoolManager.Instance.ClearPool(); // ��Ҫʵ���������
        }
    }
    public void AddRobot()  //�����ﶯ̬��ӻ����ˣ���ȷ�����������ڻ�������֮��������ȥ�ģ���ȷ�������˵���������ʹ��
    {
        // �ڳ���������num�������ˣ��������Ǽ��뵽List��
        Vector3 spawnPosition = Vector3.zero;
        // �����ҵ�һ��û����ײ��λ��
        // �������λ��
        //��ע����ֻ�����ھ��β���
        /*float randomX = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalWidth);
        float randomZ = UnityEngine.Random.Range(1f, complexityControl.buildingGeneration.totalHeight);*/

        spawnPosition = GetRandomPosInLayout();
        // ʵ����������
        GameObject Robot = Instantiate(RobotPrefab, spawnPosition, Quaternion.identity);//ʵ���������˵�λ��
        RobotList.Add(Robot.GetComponent<RobotControl>()); //�������˼����б�
        Robot.transform.parent = RobotParent.transform;    //�������˷��ڳ�����RobotList������
    }

    public void AddRobotBrain()  //��������ӻ����˴���. ��������װ  �����˵��������ٶ���ȷ�������ֱ���ڳ����н������
    {
        GameObject RobotBrain = GameObject.Find("RobotBrain1");
        RobotBrain robotBrain = RobotBrain.GetComponent<RobotBrain>();
        RobotBrainList.Add(robotBrain);
        /*  ��������л����˺ʹ��ԵĽű���ʼ������������������������������������������������������������������������������������*/
        RobotControl robot = RobotList[0];
        robot.myAgent = robotBrain;
        robotBrain.robot = robot.gameObject;
        robotBrain.robotNavMeshAgent = robot.GetComponent<NavMeshAgent>();
        robotBrain.robotInfo = robot;
        robotBrain.robotRigidbody = robot.GetComponent<Rigidbody>();
        robotBrain.robotDestinationCache = robot.gameObject.transform.position;
        robotBrain.stuckCounter = 0;
        robotBrain.SignalcostTime = 0;
        robotBrain.TotalcostTime = 0;
        RobotBrainList[0].RobotIsInitialized = true;//��ʼ������ɣ�����ִ�к�������
    }
    public void AddPerson(int num)
    {
        // print("������ຯ��");
        // �ڳ���������num�����࣬�������Ǽ��뵽personList��
        for (int i = 0; i < num; i++)
        {

            Vector3 spawnPosition = Vector3.zero;

            // �����ҵ�һ��û����ײ��λ��
            spawnPosition = GetRandomPosInLayout();

            // ʵ��������
            GameObject Person = Instantiate(HumanPrefab, spawnPosition, Quaternion.identity);
            personList.Add(Person.GetComponent<HumanControl>());
            Person.transform.parent = humanParent.transform;
            Person.GetComponent<HumanControl>().myEnv = this;
            Person.GetComponent<HumanControl>().Start();//��ʼ������ĸ�������  

        }

        // Debug.Log("ִ���������ຯ��");
    }
    public void AddHumanBrain(int num)//�ҵ������е��������Ԥ���壬��������������й��أ����������״̬
    {
        //2025.5.3��������������������ı�д
        //print("ִ�����������Ժ���");
       GameObject[] HumanBrains = GameObject.FindGameObjectsWithTag("HumanBrain");
        //��������Ϊ��unActive��״̬֮����Ҳ�����
       // print("�����е������������Ϊ"+HumanBrains.Length);

        for (int i = 0; i <num; i++)
        {
           // print("��������Ϊ��" + HumanBrains[i].name);
            HumanBrainList.Add(HumanBrains[i].GetComponent<HumanBrain>());
            //��������ӵ������е���������б�
            HumanBrains[i].GetComponent<HumanBrain>().myHuman = personList[i];
            personList[i].myHumanBrain = HumanBrains[i].GetComponent<HumanBrain>();
            //����Ӧ���������������Ӧ�Ĵ���
            HumanBrains[i].GetComponent<HumanBrain>().HumanIsInitialized = true;
            //�����������Ѿ��໥����
            //HumanBrains[i].GetComponent<HumanBrain>().myEnv
        }
        


    }
    public void AddExits() //��������ӵ������б���
    {
        GameObject Exit = GameObject.Find("Exit");
        if (Exit != null)
        {
            Exits.Add(Exit);
            //   Debug.Log("ִ����ӳ��ں�������ֵ");
        }
        else { Debug.Log("û���ҵ����ڵ���Ϸ����"); }

    }

    public void AddFire(Vector3 FirePosition) //��ӻ��棬ֻ�贫��x��z���꼴��
    {
        //������ɻ���λ��
        /*Vector3 spawnPosition = Vector3.zero;
        spawnPosition = GetRandomPosInLayout();
        spawnPosition.x = Mathf.Round(spawnPosition.x);
        spawnPosition.z = Mathf.Round(spawnPosition.z);//��������ȡ��*/
        // ����Ƿ�ﵽ����������
        // ʹ�ö���ذ�ȫ��ȡ
        GameObject fire = FirePoolManager.Instance.GetFire(FirePosition + new Vector3(0, 0.5f, 0), Quaternion.identity, this);

        if (fire != null)
        {
            var fireControl = fire.GetComponent<FireControl>();
            if (fireControl != null)
            {
                FireList.Add(fireControl);
            }
            else
            {
                Debug.LogError("���ɵĻ���ȱ��FireControl���");
                FirePoolManager.Instance.ReturnFire(fire);
            }
        }

    }
    public void AddSmoke(Vector3 FirePosition) //��ӻ��棬ֻ�贫��x��z���꼴��
    {
        // ����У��
        if (SmokePrefab == null)
        {
            Debug.LogError("����Ԥ����δ��ֵ��", this);
            return;
        }

        // ��������λ�ã�����ԭ��XZ���꣬���Yƫ�ƣ�
        Vector3 spawnPosition = new Vector3(
            FirePosition.x,
             0,
            FirePosition.z
        );

        // ��������
        GameObject smokeInstance = Instantiate(
            SmokePrefab,
            spawnPosition,
            Quaternion.identity
        );

        GameObject.Find("SmokeList");
        // ��ѡ�����ø����壨���ֲ㼶���ࣩ
        smokeInstance.transform.SetParent(transform);
    }
    /// <summary>
    /// ��ȡ����������ײ�����λ��
    /// ��Ȩ����ֲ�,�ϴ���������ڴ󷿼��ڲ������ֱ�֤����������
    /// </summary>
    public Vector3 GetRandomPosInLayout()
    {
        //���ѡȡһ�����䣬�����������ѡȡ�����
        List<Room> rooms = complexityControl.buildingGeneration.roomList;
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("�����б�Ϊ�գ�");
            return Vector3.zero;
        }
        System.Random _random = new System.Random();
        // _random.Next(maxValue)
        //���ã����� 0 �� maxValue-1 ֮����������
        //NextDouble() ���ɵ���һ����Χ�� [0, 1) �ĸ���ֵ
        Room room = rooms[_random.Next(rooms.Count)];
        // ������Ч��Χ
        float xMin = room.xzPosition.x + room.width / 4;
        float xMax = room.xzPosition.x + room.width * 3 / 4; // ��ȷ�ұ߽�
        float zMin = room.xzPosition.z + room.height / 4;
        float zMax = room.xzPosition.z + room.height * 3 / 4; // ��ȷ�ϱ߽�

        float x = xMin + (float)_random.NextDouble() * (xMax - xMin);
        float z = zMin + (float)_random.NextDouble() * (zMax - zMin);

        Vector3 RandomPos = new Vector3(x, 0.5f, z);

        return RandomPos;
    }



    public void AddFirePosition()//�ҵ������е����ϣ����ϣ������������䣬�����ǵ�������Ϊ������Դ��λ��
    {
        if (complexityControl.buildingGeneration.roomList == null || complexityControl.buildingGeneration.roomList.Count == 0)
        {
            Debug.LogError("�����б�Ϊ�գ�");
            return;
        }

        // ��ʼ���ؼ����������α���������м��㣩
        Room topLeftRoom = null;
        Room topRightRoom = null;
        Room bottomRightRoom = null;
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        // ���α������������߼�
        foreach (Room room in complexityControl.buildingGeneration.roomList)
        {
            float x = room.xzPosition.x;
            float z = room.xzPosition.z;

            // �������Ϸ����߼�
            if (x < minX || (x == minX && z > topLeftRoom?.xzPosition.z))
            {
                minX = x;
                topLeftRoom = room;
            }
            else if (x == minX && z > topLeftRoom.xzPosition.z)
            {
                topLeftRoom = room;
            }

            // �������Ϻ����·����߼�
            if (x > maxX)
            {
                maxX = x;
                topRightRoom = room;
                bottomRightRoom = room;
            }
            else if (x == maxX)
            {
                // �������ϣ�ȡZ���
                if (z > topRightRoom.xzPosition.z)
                {
                    topRightRoom = room;
                }
                // �������£�ȡZ��С��
                if (z < bottomRightRoom.xzPosition.z)
                {
                    bottomRightRoom = room;
                }
            }
        }

        // �������ĵ�
        Vector3 topLeftCenter = CalculateRoomCenter(topLeftRoom);
        Vector3 topRightCenter = CalculateRoomCenter(topRightRoom);
        Vector3 bottomRightCenter = CalculateRoomCenter(bottomRightRoom);

        FirePosition.Add(topLeftCenter);
        FirePosition.Add(topRightCenter);
        FirePosition.Add(bottomRightCenter);
    }

    // ����ԭ�����ļ��㷽��
    private Vector3 CalculateRoomCenter(Room room) =>
        new Vector3(
            room.xzPosition.x + room.width / 2f,
            room.xzPosition.y,
            room.xzPosition.z + room.height / 2f
        );

    private void RecordRoomPosition(List<Room> roomList)
    {
        foreach (Room room in roomList)
        {
            cachedRoomPositions.Add(new Vector3(
                room.xzPosition.x + room.width / 2,
                0,
                room.xzPosition.z + room.height / 2));
        }
    }
}
