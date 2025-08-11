using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Unity.Barracuda;
using TMPro;
using System.IO.Abstractions;
using UnityEditor;
using System.IO;
using System;

public class RobotBrain : Agent
{
    //��������
    public EnvControl myEnv;
    // �����˱���
    public GameObject robot;

    // �����˵��ƶ�Ŀ�ĵ�
    public Vector3 robotDestinationCache;

    // �����˵�NavMeshAgent���
    [HideInInspector] public NavMeshAgent robotNavMeshAgent;

    // �����˵ĸ������
    [HideInInspector] public Rigidbody robotRigidbody;

    // �����˵Ľű���
    [HideInInspector] public RobotControl robotInfo;
 
    // ��ǰ����¥��
    public int currentFloor;

    // �����˿���������,�ᱻ���濨��
    public int stuckCounter;

   
    //��ǰ¥������
    public int floor_human;

    public bool RobotIsInitialized = false;//�������Ƴ�ʼ��������ִ��

    //������¼������ڵľ���
    float LastDistanceToExit;
    float DeltDistanceToExit;

    //�۲�ֵ���ռλʹ��
    // ��EnvController�ж��峣��
    public const int MAX_HUMANS = 10; //������������� ��γ�ѧϰ����һ��
    public const int MAX_ROOMS = 15; // �뽨���������һ��
    public const float INVALID_MARKER = -2f; // ����[-1,1]��Χ����Ч���

    public float SignalcostTime;//��¼�������л��ѵ�ʱ��
    public float TotalcostTime;

    
    private void FixedUpdate()
    {
        if (RobotIsInitialized)//�������Ѿ���ʼ�����
        {
            SignalcostTime += Time.deltaTime;
            //�ȼ��㳡���е�����
            floor_human = 0;
            //���㳡�����м������࣬�Գ�������û��Ӱ��
            foreach (HumanControl human in myEnv.personList)//ͳ�Ƶ�ǰ¥�������
            {
                if (human.isActiveAndEnabled)
                {
                    floor_human++;
                }
            }
            //�ȼ��㳡���е�����

            if (!IsOnNavMesh(transform.position)) { //���ڵ�����ͼ��//�����ƶ�һ����λ
            
                this.transform.position +=Vector3.right;
            }

            DeltDistanceToExit = Vector3.Distance(robot.transform.position, myEnv.Exits[0].transform.position) - LastDistanceToExit;
            LastDistanceToExit = Vector3.Distance(robot.transform.position, myEnv.Exits[0].transform.position);

                //print("��ǰ¥������Ϊ:" + floor_human);
                Vector3 robotPosition = robot.transform.position;
                robotPosition.y = 0.5f;

            // �������ѵ��ģʽ�������˾��Լ������ƶ�����ʱ��ʹ��ѵ���ռ�������

            //Debug.Log("ÿһ֡����");
            // ÿ��ʱ�䲽��Ҫ�����,���ߺ�Ż��ռ���Ϣ�Լ�ִ�в���,��������ִ�е�ǰ������

            AddReward(-0.01f*floor_human);//����ͣ���ڻ��ֳ����ĳͷ�
            LogReward("����ͣ���ڻ��ֳ����ĳͷ�", -0.01f*floor_human);

           
            //�����߼�����ʱ��ͨ�����ò���������������Ƿ�����ƶ���������������������������������������������������������������������
            if (myEnv.isTraining is false)
                {
                    //print("��ǰ¥������Ϊ:" + floor_human);
                    int lonelyHumanLeaderCounter = (from human in myEnv.personList
                                                    let humanPosition = human.transform.position - new Vector3(0, 0.5f, 0)
                                                    where human.isActiveAndEnabled && Mathf.Abs(humanPosition.y - robotPosition.y) < 0.5f
                                                    select human).Count(human => human.myBehaviourMode is "Leader" && human.transform.position.z > 0);

                    //print("�¶������쵼�ߵ�����Ϊ��"+lonelyHumanLeaderCounter);

                    if (lonelyHumanLeaderCounter <= 10)
                    {//�����쵼��������lonelyHumanLeaderCounter��С�ڵ���4�����һ����˸�����������robotInfo.robotFollowerCounter������0ʱ������1Ϊ��;�����쵼��������lonelyHumanLeaderCounter������0ʱ������2Ϊ��
                        robot.GetComponent<RobotControl>().isRunning = true;//�����˿�ʼ����,���࿪ʼ���������
                        GMoveAgent();
                        return;
                    }
                }
                //�����߼�����ʱ��ͨ�����ò���������������Ƿ�����ƶ���������������������������������������������������������������������
  
            //ѵ���������һ��������ڵ�λ��������е�λ�þ���С��1
            if (myEnv.isTraining) {
                RequestDecision();
            }
        }
    }//��֡����
    public override void OnEpisodeBegin()
    {
        print("������һ���µĻغϿ�ʼ��");

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //�����ȹ̶��۲�ֵ������ȷ���ϸ��һ����[0, 1]��Χ������PPO�㷨�ȶ�ѵ����ǰ������
        //��RequestDecision����ִ�к󣬻���ִ�иú������ռ������۲�ֵ
        //�۲�ֵ��Ҫ��ӣ�
        //ÿһ�������λ�ã���ѧϰ������ƶ��߼�
        //ÿһ�������˵�λ�ã���ѧϰ���������˵��ƶ��߼�����Ŀǰֻ��һ��������
        //���������������������/λ�ã�ÿһ���ŵ�λ�ã� ��ѧϰ�����������߼�
        // ���㻷���߽磨��Human�۲Ᵽ��һ�£�

        if (myEnv.useRobot is false)
            return;

        //Debug.Log("CollectObservations called."); 
        if (myEnv == null || myEnv.useRobot is false)
        {
            Debug.Log("myEnv is null or useRobot is false.");
            return;
        }
        //������λ�ù۲�ֵ��2 * n������ n Ϊ�����˵�������,����λ�ù۲�ֵ��60,,����λ�ù۲�ֵ��30,����λ�ù۲�ֵ��2,��Դλ�ù۲�ֵ��6
        // ��� Agent �۲�ֵ
        // ʹ�ó����Խ��߳��ȹ�һ����ȷ�����������[0,1]
        float sceneDiagonal = Mathf.Sqrt(
            Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalWidth, 2) +
            Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalHeight, 2)
        );
      //  print("�����Խ���Ϊ����Ϊ��"+sceneDiagonal);
        // ��һ�� Agent λ�� ��           2��
        foreach (RobotBrain agent in myEnv.RobotBrainList)
        {
            Vector3 normalizedPos = (agent.robot.transform.position) / sceneDiagonal;
            sensor.AddObservation(normalizedPos.x);
            sensor.AddObservation(normalizedPos.z);
             //Debug.Log("�����˵�λ��Ϊ" + normalizedPos);
        }

        // ��һ�� Human λ�ã��������10��            20��

        // �̶��۲�ά��Ϊ MAX_HUMANS * 2
        for (int i = 0; i < MAX_HUMANS; i++)
        {
            if (i < myEnv.personList.Count)
            {
                // ���ʵ������λ��
                HumanControl human = myEnv.personList[i];
                Vector3 normalizedPos = human.transform.position /sceneDiagonal;
                sensor.AddObservation(normalizedPos.x);
                sensor.AddObservation(normalizedPos.z);
            }
            else
            {
                // ���ռλֵ���Ƽ�ʹ����Ч���꣩
                sensor.AddObservation(-1f); // x
                sensor.AddObservation(-1f); // z
            }
        }

        // ��ӷ���λ�ã����Agent�� Ŀǰ�̶�10�����䣬
        int maxRooms = 10;
        for (int i = 0; i < maxRooms; i++)
        {
            if (i < myEnv.cachedRoomPositions.Count)
            {
                Vector3 roomPos = myEnv.cachedRoomPositions[i];
                {
                    // λ�ù�һ��������ڻ������ģ�
                    Vector3 normalizedPos = (roomPos) / sceneDiagonal;
                    sensor.AddObservation(normalizedPos.x); // X���� [-1, 1]
                    sensor.AddObservation(normalizedPos.z); // Z���� [-1, 1]
                    //Debug.Log("�����λ��Ϊ" + normalizedPos);
                }
            }
            else
            {
                // ���ռλֵ���Ƽ�ʹ����Ч���꣩
                sensor.AddObservation(-1f); // x
                sensor.AddObservation(-1f); // z
            }
        }

        //��ӳ���λ��   ֻ��1������         39+[24,45]=[63,84]     2��
        sensor.AddObservation((myEnv.Exits[0].transform.position.x) / sceneDiagonal);
        sensor.AddObservation((myEnv.Exits[0].transform.position.z) / sceneDiagonal);
         //Debug.Log("���ڵ�λ��Ϊ" + (myEnv.Exits[0].transform.position) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));

        //��ӻ�Դλ�ã�Ŀǰ��Դֻ����������      6��
        for (int i = 0;i<3;i++)
        {
            Vector3 firePos = myEnv.FirePosition[i];
            {
                // λ�ù�һ��������ڻ������ģ�
                Vector3 normalizedPos = (firePos) / sceneDiagonal;
                sensor.AddObservation(normalizedPos.x); // X���� [-1, 1]
                sensor.AddObservation(normalizedPos.z); // Z���� [-1, 1]
                // Debug.Log("��Դ��λ��Ϊ" + normalizedPos);
            }
        }
        sensor.AddObservation(robotInfo.myDirectFollowers.Count / 10);//��������˵���������
        sensor.AddObservation(floor_human / 10);//�����е���������
        //��ӻ�Դ�����������������Խ��й�һ�����������
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (myEnv.useRobot is false)
            return;
        //print("���յ��˶���");
        MoveAgent(actions);  // �ƶ�Agent
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (myEnv.useRobot is false)
            return;

        if (currentFloor != 3)
            return;

        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f + 4 * (currentFloor - 1);
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(0, 0, 1);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(0, 0, -1);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(-1, 0, 0);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActions[0] = 1;
            Vector3 targetPosition = robotPosition + new Vector3(1, 0, 0);
            continuousActions[0] = targetPosition.x / 18.0f;
            continuousActions[1] = targetPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 2;
            continuousActions[0] = robotPosition.x / 18.0f;
            continuousActions[1] = robotPosition.z / 18.0f;
        }
        else if (Input.GetKey(KeyCode.B))
        {
            discreteActions[0] = 3;
            continuousActions[0] = robotPosition.x / 18.0f;
            continuousActions[1] = robotPosition.z / 18.0f;
        }
    }


    /// <summary>
    /// ����ģ�;��߽���ƶ������˱���
    /// </summary>
    /// <param name="actions"></param>
    private void MoveAgent(ActionBuffers actions)
    {
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f + 4 * (currentFloor - 1);
        Vector3 positionExit = myEnv.Exits[0].gameObject.transform.position;

        //��Ŀ��λ��ӳ�����������������ڲ�
       // Debug.Log("����Ŀ��Ϊ"+myEnv.complexityControl.buildingGeneration.totalWidth);
        //Debug.Log("����ĸ߶�Ϊ" + myEnv.complexityControl.buildingGeneration.totalHeight);
       // Debug.Log($"Actions: [{continuousActions[0]}, {continuousActions[1]}]");
        float targetX = Mathf.Clamp(continuousActions[0], -1, 1) * (myEnv.complexityControl.buildingGeneration.totalWidth / 2f) + (myEnv.complexityControl.buildingGeneration.totalWidth / 2f);
        float targetZ = Mathf.Clamp(continuousActions[1], -1, 1) * (myEnv.complexityControl.buildingGeneration.totalHeight / 2f) + (myEnv.complexityControl.buildingGeneration.totalHeight / 2f);
        Vector3 targetPosition = new(targetX, 0.5f, targetZ);
        //print("Ŀ�ĵ��ǣ�"+targetPosition);

        // ���㵱ǰ��Ŀ���ƽ����루����Y�ᣩ
        float currentDistance;
        Vector3 currentPos = transform.position;
        currentPos.y = targetPosition.y;
        currentDistance = Vector3.Distance(currentPos, targetPosition);

        if (currentDistance < 0.5||!IsReachable(targetPosition)) {
            AddReward(-0.05f);//�����ƶ������С�ͷ����ƶ�Ŀ�겻�ɴ�ĳͷ�
            LogReward("�����ƶ������С���ƶ�Ŀ�겻�ɴ�ĳͷ�", -0.05f);
        }

        if (!IsReachable(targetPosition))//��ЧĿ�ĵأ�����
        {
            GMoveAgent();
            return;
        }

        float sceneDiagonal = Mathf.Sqrt(
           Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalWidth, 2) +
           Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalHeight, 2)
       );
        if (Vector3.Distance(targetPosition, positionExit) < sceneDiagonal / 2 &&robotInfo.myDirectFollowers.Count>0)

            targetPosition = positionExit+new Vector3(1,0,0);//���ұ�ȥһ�㣬ʡ�Ķ���
        //������һ����Χ��֮�󣬽�Ŀ�ĵ�����Ϊ����
        //Debug.Log("��һ֡��Ŀ�ĵ��ǣ�"+targetPosition);
        if (myEnv.isTraining is false)
        {
            if (robotInfo.robotFollowerCounter > 0)
            {

                if (currentFloor == 1 && robotDestinationCache == positionExit && Vector3.Distance(targetPosition, positionExit) < 40 && robotInfo.robotCommand is "LightOn")
                    return;
            }
            else if (robotInfo.robotFollowerCounter <= 0)
            {

                if (targetPosition == positionExit)
                    return;
            }
        }

        if (IsReachable(targetPosition))
        {
            // ���Ŀ��ɴ���ǰ��Ŀ�꣬���ɴ��������һ������
            stuckCounter = 0;
            //
            if (robotDestinationCache == targetPosition)
            {
               // print("��ЧĿ�ĵأ�ʹ��̰���ƶ��㷨1");
                GMoveAgent();
            }
            else
            {
                robotDestinationCache = targetPosition;
                robotNavMeshAgent.SetDestination(robotDestinationCache);
            }
        }
        }

        bool IsReachable(Vector3 targetPosition)//���ɴ���false
    {
        NavMeshPath path = new NavMeshPath();
        if (robot.GetComponent<NavMeshAgent>().CalculatePath(targetPosition, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true; // ����·�������Ե���
            }
        }
        return false; // ·�������������ܱ�ǽ���������ϰ���ס��
    }


    private void GMoveAgent()
    {
        //print("�ƶ������˺���");
        Vector3 targetPosition = new();
        Vector3 robotPosition = robot.transform.position;
        //print("�����˵ĸ���������Ϊ��" + robotInfo.myDirectFollowers.Count);
        if (robotInfo.robotFollowerCounter > 0)//�����ǰ�����˵�ǰ�����ߴ���0����ǰ������
        {
          //  print("����������������0");
            //���һ�����ڣ������͵�����
            //print("2�����˼�⵽�ĳ�������Ϊ��"+myEnv.Exits.Count);
            targetPosition = targetPosition = myEnv.Exits[0].gameObject.transform.position + new Vector3(1, 0, 0);//���ұ�ȥһ�㣬ʡ�Ķ���
        }
        else
        {
            //print("û��������棬Ѱ�Ҿ������������");
            //�ҵ����������������࣬����������ƶ�
            float minDist = int.MaxValue;
            //float minDist = 18f;
            foreach (HumanControl human in myEnv.personList)
            {
                Vector3 humanPosition = human.transform.position - new Vector3(0, 0.5f, 0);
                if (human.isActiveAndEnabled is false
                    /*|| Mathf.Abs(humanPosition.y - robotPosition.y) > 1 || humanPosition.x < -20 || humanPosition.z > 20*/)
                    continue;
                if (Vector3.Distance(humanPosition, robotPosition) < minDist)
                {
                    minDist = Vector3.Distance(humanPosition, robotPosition);
                    targetPosition = humanPosition + human.transform.forward ;
                }
            }

            robotDestinationCache = targetPosition;
            robotNavMeshAgent.SetDestination(robotDestinationCache);
            return;
        }

        if (targetPosition != Vector3.zero)
        {
            robotDestinationCache = targetPosition;
            robotNavMeshAgent.SetDestination(robotDestinationCache);
            return;
        }

    }//̰���㷨�ƶ�������

    private Vector3 GetCrossDoorDestination(GameObject targetDoor)//ȥ����ǰ��λ�ã��ú����Ǹ�������ʹ�ã����÷���ӵ��
    {
        Vector3 myPosition = transform.position;

        if (targetDoor.CompareTag("Door") || targetDoor.CompareTag("Exit"))
        {
            string doorDirection = targetDoor.GetComponent<DoorControl>().doorDirection;
            Vector3 doorPosition = targetDoor.transform.position + new Vector3(0, -1.5f, 0);
            switch (doorDirection)
            {
                case "Horizontal": //ˮƽ
                    if (myPosition.z < doorPosition.z)
                        return doorPosition + new Vector3(0, 0, -1);
                    return doorPosition - new Vector3(0, 0, -1);
                case "Vertical"://��ֱ
                    if (myPosition.x < doorPosition.x)
                        return doorPosition + new Vector3(-1, 0, 0);
                    return doorPosition - new Vector3(-1, 0, 0);
                default:
                    return myPosition;
            }
        }
        else
        {
            return myPosition;
        }
    }

    private bool IsOnNavMesh(Vector3 targetPosition)
    {
        return NavMesh.SamplePosition(targetPosition, out NavMeshHit _, 0.1f, 1);
    }


    // ����ͳ�ƿ��ӻ�
    private Dictionary<string, float> rewardLog = new Dictionary<string, float>();

    [System.Serializable]
    private class RewardData
    {
        public string timestamp;
        public Dictionary<string, float> rewards;
    }
    public void LogReward(string type, float value)
    {
        rewardLog.TryGetValue(type, out float current);
        rewardLog[type] = current + value;
    }

    void OnDestroy()
    {


        // ���屣��·����ʹ��persistentDataPath��
        string directoryPath = Path.Combine(Application.persistentDataPath, "results");
        string filePath = Path.Combine(directoryPath, $"Robot_Reward_log_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        try
        {
            // ȷ��Ŀ¼����
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string result="";



            foreach(var kv in rewardLog)
         {
                Debug.Log($"{kv.Key}: {kv.Value}");
                result += $"{kv.Key}: {kv.Value}\n";
            }
            // д���ļ�
            File.WriteAllText(filePath, result);

            // ��־���
            Debug.Log($"���������ѱ��浽: {filePath}\n{result}");
        }
        catch (Exception e)
        {
            Debug.LogError($"���潱������ʧ��: {e.Message}");
        }
    }
    }
