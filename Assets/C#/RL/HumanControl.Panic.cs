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
    public float lastPanicLevel;//���ڼ�¼��һ֡�Ŀֻ�ֵ
    public float deltaPanic;//��¼�ֻ�ֵ�ı仯ֵ
    void UpdatePanicLevel()
    {
        //�ο����ף�����ʫ. �������˵���վ̨Ӧ����ɢ��Ϊ��ģ�����[D]. ����:������ͨ��ѧ,2022.  ˶ʿѧλ���� p22
        exitDistance=Vector3.Distance(this.transform.position, myEnv.Exits[0].transform.position); //Ŀǰ������ڵľ���

        // ���������
        float healthTerm = Mathf.Clamp01(1-(health /100));                    //��������Ľ���ֵ,����ֵԽ�ͣ����ǳ̶�Խ��
        /*float distanceTerm = Mathf.Clamp01(exitDistance /startDistanceToExit); //������ڵľ���*/
        float sceneDiagonal = Mathf.Sqrt(
           Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalWidth, 2) +
           Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalHeight, 2)
       );

        float distanceTerm = Mathf.Clamp01(exitDistance / (sceneDiagonal/3)); //������ڵľ������1/2�Խ��ߵĳ��ȣ�Զ�ڶԽ���һ���һ���ֻ�
        // �ۺϼ���
        panicLevel =
           0.45f * healthTerm +  //��������ڻ���Ϊ0
            0.6f * distanceTerm;
 
        //������ֻŶȺ󣬸�������������ٶ�,��������ĵ�ǰ�ٶ�

        // ���ݿֻŶȸ��µ�ǰ�ٶȡ���ֵ��������һ������ a ��ʾ��ʼֵ���ڶ������� b ��ʾ����ֵ������������ t ��ʾ��ֵ��Ȩ��

        //�ֻŻ��⽱��
       /* float deltaPanic = lastPanicLevel - panicLevel;
        if (deltaPanic > 0.1)
        {
            myEnv.RobotBrainList[0].AddReward(20 * deltaPanic);
            myEnv.RobotBrainList[0].LogReward("�ֻ��������⽱��", 20 * deltaPanic);
        }
        lastPanicLevel = panicLevel;*/
        // Ⱥ�崫Ⱦ�� todo
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
        //����״̬ת�䣺�ο����ף�[1]�ﻪ��.���ǿֻ���������Ⱥ��ɢ��Ϊģ���о�[D].���ϴ�ѧ,2022.DOI:10.27661/d.cnki.gzhnu.2022.000847.

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

            // ��HumanControl��UpdateBehaviorModel()��׷�ӣ�
            if (CurrentState == 2|| CurrentState ==1) // �ֻ�״̬����״̬
            {
              /*  myEnv.RobotBrainList[0].AddReward(-0.02f * panicLevel);
                myEnv.RobotBrainList[0].LogReward("���ദ�ڿֻ�״̬�ĳͷ�", -0.02f * panicLevel);*/
            }
        }
        else MoveModel0();//��������ÿֻ�ģʽ��Ĭ��ʹ������ģʽ

        if (myLeader != null)
        {
            //���쵼�ߣ��͸����쵼���ƶ�
            MoveModel0();
        }
    }

    private void MoveModel0()//�����ƶ��߼�
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
    private void MoveModel1() // �ֻŶȴ���0.3��С��0.7������ģʽ�������ӿ��ƶ��ٶ�
    {
       // print(this.gameObject.name+"������ģʽ1�ƶ�");
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
        /*�ƶ����֣�������������������������������������������������������������������*/
        // ����1���ﵽ���ʱ�� �� ����2���ӽ�Ŀ���
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
        /*�ƶ����֣�������������������������������������������������������������������*/

        /*������˽��жԿ��Ĳ��֣�������������������������������������������������������������������*/
        HandleRobotInteraction();
    }
    //����Ļ����ƶ�����������������������������������������������
    private float panicMoveInterval;  // Ŀ����¼��

    private float panicMoveRadius = 5f;     // ����ƶ��뾶
    private float lastPanicUpdateTime;      // �ϴθ���ʱ��
    private Vector3 currentPanicDirection;  // ��ǰ�ƶ�����
    private Vector3 lastSafePosition;   //��ȫλ�ã��������Ŀ�ĵز��ɴ�ʱ��ԭ·����
    private void UpdatePanicDestination()
    {
        lastSafePosition = transform.position;
        // ��������ԣ�70%���ʱ��ֵ�ǰ����ƫ��
        if (currentPanicDirection != Vector3.zero && Random.value < 0.7f)
        {
            float angleOffset = Random.Range(-30f, 30f);
            Vector3 newDirection = Quaternion.Euler(0, angleOffset, 0) * currentPanicDirection;
            myDestination = transform.position + newDirection.normalized * panicMoveRadius;
        }
        else // 30%����ȫ���������
        {
            currentPanicDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
            myDestination = transform.position + currentPanicDirection * panicMoveRadius;
        }

        // ������֤��������ɴ���ڸ������һ���ص�
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
        // ����1�����Է�����һ����ȫλ��
    if (lastSafePosition != Vector3.zero &&
        Vector3.Distance(transform.position, lastSafePosition) > 2f)
        {
            if (ValidateNavDestination(lastSafePosition))
                return lastSafePosition;
        }
        // ���շ�������ǰλ����Χ�����
        return transform.position + Random.insideUnitSphere * 1f;
    }
    //����Ļ����ƶ�����������������������������������������������


    //������˵ĶԿ���Ϊ������������������������
    public float robotDetectTime; // ��¼�״μ�⵽�����˵�ʱ��
    public void HandleRobotInteraction()
    {
        List<GameObject> leaderCandidates = GetCandidate(new List<string> { "Human", "Robot" }, 360, 3).Item1;
        
        if (leaderCandidates.Count > 0)//����Ұ�￴���˻�����
        {
           // print("�����и������ˣ��Һú���");
            if (robotDetectTime == 0)
            {
                robotDetectTime = Time.time; // ��¼�״μ��ʱ��,����ÿһ֡�������һ��ʱ�䡣��������P
              //  print("�������������"+robotDetectTime+"��ʼ�����ҵ�");
            }
            GameObject leader = leaderCandidates[0];
            float robotDistance = Vector3.Distance(transform.position, leader.transform.position);
            // �Կ��������ֻŶȽϸ��һ����˽ӽ�,ֻ�пֻŶȽϸ�ʱ�Ż�����״̬
          if ( robotDistance < 3f)
             {
                // ��һ�׶Σ�����2s
                if (Time.time - robotDetectTime < 5  )
                {
                   // print("����ʱ����"+"ta������"+ (Time.time - robotDetectTime) + "s�ˣ���Ҫ����Զһ��");
                    // �ƿ���Ϊ
                    Vector3 pushDir = (transform.position - leader.transform.position).normalized;
                    
                    _myNavMeshAgent.velocity = pushDir * 2f;
                }
                // �ڶ��׶Σ�����
                else
                {
                   // print("�������������ҵģ��Ҹ������߰�");
                    robotDetectTime = 0;
                    UsePanic = false;//��������Ŀֻ�״̬������ת��Ϊģʽһ��

                    if (myEnv.useRobot)
                    {
                        leader.GetComponent<RobotControl>().myAgent.AddReward(50);
                        myEnv.RobotBrainList[0].LogReward("��������ֻ�״̬�Ľ���", 50);
                    }
                }
            }
        }
    }
   
}


