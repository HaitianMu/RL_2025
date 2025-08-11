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

public class MultiAgent : Agent
{
    //��������
    // �����˱���
    public GameObject robot;

    // �����˵��ƶ�Ŀ�ĵ�
    public Vector3 robotDestinationCache;

    // �����˵�NavMeshAgent���
    [HideInInspector] public NavMeshAgent robotNavMeshAgent;

    // �����˵ĸ������
    [HideInInspector] public Rigidbody robotRigidbody;

    // �����˵Ľű���
    [HideInInspector] public Robot robotInfo;

    // ���ڼ�����ʱ·����"ê"��Ԥ����
    public GameObject navMeshAnchorPrefab;

    // ��ǰ����¥��
    public int currentFloor;

    //ADD ԭʼ¥��
    public int originFloor;

    // ���߼�����
    public int decisionCountDown;

    // ��������
    public int maxDecisionCountDown;

    // �����˿���������
    public int stuckCounter;

    // ��������̼�����
    public float mileageCounter;

    // ������λ�ü�¼��
    public Vector3 mileageRecorder;

    public bool isTrans = false;//�������Ƿ��ƶ���
    //��������
    public Env myEnv;
    // ����Ѫ��˥������
    private const float HumanHealthDecayRate = 0.01f;
    //��ǰ¥������
    public int floor_human;
    int previousActiveHumanCount;
    private void FixedUpdate()
    {

        int currentFloorhuman = 0;

        foreach (Person human in myEnv.personList)//ͳ�Ƶ�ǰ¥�������
        {
            if (human.isActiveAndEnabled)
            {
                currentFloorhuman++;
            }
        }
        floor_human = currentFloorhuman;

        // ��ȡ�����˵�ǰ¥���λ��
        Vector3 robotPosition = robot.transform.position;
        robotPosition.y = 0.5f;

        // �������ѵ��ģʽ�������˾��Լ������ƶ�����ʱ��ʹ��ѵ���ռ�������

        //Debug.Log("ÿһ֡����");

        /*   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
        // ÿ��ʱ�䲽��Ҫ�����,���ߺ�Ż��ռ���Ϣ�Լ�ִ�в���,��������ִ�е�ǰ������
        RequestDecision();


        if (myEnv.isTraining is false)
        {
            //print("��ǰ¥������Ϊ:" + floor_human);
            int lonelyHumanLeaderCounter = (from human in myEnv.personList
                                            let humanPosition = human.transform.position - new Vector3(0, 0.5f, 0)
                                            where human.isActiveAndEnabled && Mathf.Abs(humanPosition.y - robotPosition.y) < 0.5f
                                            select human).Count(human => human.myBehaviourMode is "Leader" && human.transform.position.z > 0);

            //print("�¶������쵼�ߵ�����Ϊ��"+lonelyHumanLeaderCounter);

            if (lonelyHumanLeaderCounter <= 5)
            {//�����쵼��������lonelyHumanLeaderCounter��С�ڵ���4�����һ����˸�����������robotInfo.robotFollowerCounter������0ʱ������1Ϊ��;�����쵼��������lonelyHumanLeaderCounter������0ʱ������2Ϊ��
                robot.GetComponent<Robot>().isRunning = true;//�����˿�ʼ����,���࿪ʼ���������
                GMoveAgent();
                return;
            }
        }
    }//��֡����

    /*private void Awake()*///awake����ִ����start����֮ǰ��
    public override void Initialize()
    {
        Debug.Log("���ǳ�ʼ����������ִ����!");
        // ���� robot,��������������Ϊ����״̬����������Ż���и���
        robot.SetActive(true);
        robot.GetComponent<Robot>().isRunning = true;
        // ���ó�ʼλ��
        robotDestinationCache = robot.transform.position;
        robotDestinationCache.y = 0.5f + 4 * (currentFloor - 1);

        // ��ʼ����������
        decisionCountDown = maxDecisionCountDown;
        stuckCounter = 0;
        mileageCounter = 0;
        mileageRecorder = new Vector3();
        originFloor = currentFloor;
        foreach (Person human in myEnv.personList)
        {
            if (human.isActiveAndEnabled)
            {
                previousActiveHumanCount++;
            }
        }
        // �����ʼ����Ϣ
        /* Debug.Log("Robot initialized at position: " + robot.transform.position);
         Debug.Log("myEnv: " + myEnv.gameObject.name);

         // ��ӡ personList �� robot ��Ϣ
         foreach (Person human in myEnv.personList)
         {
             Debug.Log("Human: " + human.transform.name);
         }
         Debug.Log("Robot: " + robot.transform.name);
         Debug.Log("myEnv.useRobot " + myEnv.useRobot);//myEnv.isTraining
         Debug.Log("myEnv.isTraining " + myEnv.isTraining);*/
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //��RequestDecision����ִ�к󣬻���ִ�иú������ռ������۲�ֵ

        if (myEnv.useRobot is false)
            return;

        //Debug.Log("CollectObservations called.");
        if (myEnv == null || myEnv.useRobot is false)
        {
            Debug.Log("myEnv is null or useRobot is false.");
            return;
        }
        int num = 0;

        // ��� Agent �۲�ֵ
        foreach (MultiAgent agent in myEnv.agentList)
        {
            sensor.AddObservation((agent.robot.transform.position - new Vector3(5, 0, 5)) / 5.0f);
            num++;
        }

        // ��� Human �۲�ֵ
        foreach (Person human in myEnv.personList)
        {
            //Debug.Log("Human position: " + human.transform.position);
            sensor.AddObservation((human.transform.position - new Vector3(5, 0, 5)) / 5.0f);
            num++;
        }

       // Debug.Log("Total observations added: " + RoomNum);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (myEnv.useRobot is false)
            return;
        if (isTrans)  // �жϹ���״̬
            return;

        MoveAgent(actions);  // �ƶ�Agent
    }
    /*public override void OnActionReceived(ActionBuffers actions)
    {
        if (myEnv.useRobot is false)
            return;
        if (isTrans)  // �жϹ���״̬
            return;

        MoveAgent(actions);  // �ƶ�Agent

        // ����¥�������ı仯���н���
        if (floor_human == 0)
        {
            EndEpisode();  // ����ѵ��
            Debug.Log("��ǰ¥������Ϊ0�������˴�ѵ��");
            SetReward(10f);  // ���������
            return;
        }

        // ���������Ǽ���¥������
        // ÿ����һ���˿��Ը�һ��������
        SetReward(floor_human * -0.1f);  // ÿ��ʣ���������ٽ���

        // ������״̬�ͷ�����������˴�����Ч״̬�������ͷ�
        if (isTrans)  // ����ǹ���״̬�����踺����
        {
            SetReward(-1f);  // �������ͣ��
        }

        // �ͷ�����Ҫ���ƶ����������������û������Ķ���
        if (IsIrrelevantMove(actions))  // ����Ը��ݾ������������Ч�������ж�
        {
            SetReward(-0.5f);  // �ͷ���������ƶ�
        }

        // �㻹���Լ�������ӵĽ������������ʱ�����Ч�ʵĽ���
        float timePenalty = Time.timeSinceLevelLoad * -0.05f;  // ����ʱ��ĳͷ�
        SetReward(timePenalty);
    } //��������V1.0*/

    //������صĺ���������������������������������������
    private bool IsIrrelevantMove(ActionBuffers actions)
    {
        // ��ȡ��һ����Ŀ������
        float targetX = Mathf.Clamp(actions.ContinuousActions[0], -1, 1) * 5.0f + 5f;
        float targetZ = Mathf.Clamp(actions.ContinuousActions[1], -1, 1) * 5.0f + 5f;

        // ����Ŀ��λ��
        Vector3 targetPosition = new Vector3(targetX, transform.position.y, targetZ);

        //  ���Ŀ��λ���뵱ǰλ�ü�����ͬ���ƶ������С����Ϊ����Ч����
        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            //Debug.Log("����������Ч����");
            return true; // Ŀ��λ���뵱ǰλ�ü����غϣ���Ч����
        }

        return false; // ���û����Ч���������� false
    }
    //������صĺ���������������������������������������
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
        Vector3 positionExit = GetCrossDoorDestination(myEnv.Exits[0].gameObject); ;

        float targetX = Mathf.Clamp(continuousActions[0], -1, 1) * 5.0f+5f;
        float targetZ = Mathf.Clamp(continuousActions[1], -1, 1) * 5.0f+5f;
        
        Vector3 targetPosition = new(targetX, 0.5f, targetZ);
        //Debug.Log("�����˵�Ŀ�ĵ�Ϊ��" +targetPosition );
        if (Vector3.Distance(targetPosition, positionExit) < 20 && currentFloor == 1)
            targetPosition = positionExit;

        if (myEnv.isTraining is false)
        {
            // if (currentFloor == 1 && robotInfo.robotFollowerCounter > 4 && IsPathOnCurrentFloor(GetWayPoints(robotPosition, positionExit)) && Vector3.Distance(robotPosition, positionA) < 40)
            // {
            //     targetPosition = positionExit;
            // }
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
            // ���Ŀ��ɴ���ǰ��Ŀ�꣬���ɴ��������һ������
            stuckCounter = 0;
            //
            if (robotDestinationCache == targetPosition)
            {
                //targetPosition = new Vector3(2, 0.5f, -18);//λ��
                GMoveAgent();
                //Debug.Log("ǿ��λ��" + robotDestinationCache);
            }
            else
            {
                robotDestinationCache = targetPosition;
                robotNavMeshAgent.SetDestination(robotDestinationCache);
               // Debug.Log(robot.name + robotDestinationCache);
            }
            //
            //robotDestinationCache = targetPosition;
            //robotNavMeshAgent.SetDestination(robotDestinationCache);
            //Debug.Log(robotAgent.name + robotDestinationCache + pathFromHereToTarget.Count);
            //robotInfo.robotCommand = GetRobotCommand();
    }
    private void GMoveAgent()
    {
        Vector3 targetPosition = new();
        Vector3 robotPosition = robot.transform.position;
        //print("�����˵ĸ���������Ϊ��" + robotInfo.myDirectFollowers.Count);
        if (robotInfo.myDirectFollowers.Count > 0)//�����ǰ�����˵�ǰ�����ߴ���0����ǰ������
        {
            //���һ�����ڣ������͵�����
            //print("2�����˼�⵽�ĳ�������Ϊ��"+myEnv.Exits.Count);
            targetPosition =GetCrossDoorDestination( myEnv.Exits[0].gameObject);  
        }
        else
        {
            //�ҵ����������������࣬����������ƶ�
            float minDist = int.MaxValue;
            //float minDist = 18f;
            foreach (Person human in myEnv.personList)
            {
                Vector3 humanPosition = human.transform.position - new Vector3(0, 0.5f, 0);
                if (human.isActiveAndEnabled is false || Mathf.Abs(humanPosition.y - robotPosition.y) > 1 || humanPosition.x < -20 || humanPosition.z > 20)
                    continue;
                if (Vector3.Distance(humanPosition, robotPosition) < minDist)
                {
                    minDist = Vector3.Distance(humanPosition, robotPosition);
                    targetPosition = humanPosition - human.transform.forward/2;
                }
            }
            robotDestinationCache = targetPosition;
            robotNavMeshAgent.SetDestination(robotDestinationCache);
            return;
        }

        if (targetPosition != Vector3.zero  && !isTrans)
        {
            robotDestinationCache = targetPosition;
            robotNavMeshAgent.SetDestination(robotDestinationCache);
           
            return;
        }

    }//̰���㷨�ƶ�������


   //��������������
    public void ResetAgent()
    {
        if (myEnv.useRobot is false)
            return;
        isTrans = false;
        //Debug.Log("���ڲ���123 " + nextRobotPosition);
        /* Vector3 nextRobotPosition = new Vector3(8,0.25f,8);  //�̶�λ��*/

        Vector3 nextRobotPosition = GetSpawnBlockPosition(1);//fixed
        Debug.Log("���ɵĻ��������λ���ǣ�"+nextRobotPosition);
        robot.transform.SetPositionAndRotation(nextRobotPosition + new Vector3(0, 1, 0), Quaternion.identity); //�������λ�� 
        robot.GetComponent<Robot>().isRunning = true;//������������Ϊ����״̬

        robotDestinationCache = nextRobotPosition + new Vector3(0, 0.5f, 0);
        robotRigidbody.velocity = Vector3.zero;
        robotRigidbody.angularVelocity = Vector3.zero;
        decisionCountDown = maxDecisionCountDown;

        stuckCounter = 0;
        mileageCounter = 0;
        mileageRecorder = new Vector3();
    }

    //��ȡ�������λ��
    private static Vector3 GetSpawnBlockPosition(int floor)//�������������������̵ģ����������ҲҪע��һ�£�0.734��,0.25,6����Χ�ڵ������ǣ�10.734��0.25,16��
    {
        Vector3 spawnBlockPosition = new();
        for (int tryCounter = 80000; tryCounter > 0; tryCounter--)
        {
            float randomX = Random.Range(1, 9) + 0.5f;
            float randomZ = Random.Range(1, 9) + 0.5f;

            if (floor == 1 && randomX is > -5 and < 8 && randomZ is > -19 and < -15)
                continue;

            spawnBlockPosition.Set(randomX, 0.5f+(floor - 1) * 4, randomZ);

            if (Physics.CheckBox(spawnBlockPosition + Vector3.up, new Vector3(0.49f, 0.49f, 0.49f)) is false)
                return spawnBlockPosition;
        }

        return new Vector3();
    }





    private Vector3 GetCrossDoorDestination(GameObject targetDoor)//ȥ����ǰ��λ�ã��ú����Ǹ�������ʹ�ã����÷���ӵ��
    {
        Vector3 myPosition = transform.position;

        if (targetDoor.CompareTag("Door") || targetDoor.CompareTag("Exit"))
        {
            string doorDirection = targetDoor.GetComponent<Door>().doorDirection;
            Vector3 doorPosition = targetDoor.transform.position + new Vector3(0, -1.5f, 0);
            switch (doorDirection)
            {
                case "Horizontal": //ˮƽ
                    if (myPosition.z < doorPosition.z)
                        return doorPosition + new Vector3(0, 0, -1);
                    return doorPosition - new Vector3(0, 0,-1);
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
}
