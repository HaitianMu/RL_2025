using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;

public class HumanBrain : Agent
{
    // ��EnvController�ж��峣��
    public const int MAX_HUMANS = 10; //������������� ��γ�ѧϰ����һ��
    public const int MAX_ROOMS = 15; // �뽨���������һ��
    public const float INVALID_MARKER = -2f; // ����[-1,1]��Χ����Ч���

    public EnvControl myEnv;
    public HumanControl myHuman;//���Զ�Ӧ������
    public bool HumanIsInitialized=false;
    public int HumanState;//���ݾ���ȷ���������ƶ�״̬��

    public void FixedUpdate()
    {
        if (!HumanIsInitialized)
        {
           // print("��ʼ������ɣ��ҵ�С����"+myHuman.name);
           return;

        }
        if(myEnv.useHumanAgent)
        {
            //�����������֧Ԯ
            RequestDecision();
        }
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

        if (!myEnv.useHumanAgent) return;

        //Debug.Log("CollectObservations called."); 
        if (myEnv == null || myEnv.useHumanAgent is false)
        {
            Debug.Log("myEnv is null or useHumanAgen is false.");
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
                Vector3 normalizedPos = human.transform.position / sceneDiagonal;
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
        for (int i = 0; i < 3; i++)
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
        //��ӻ�Դ�����������������Խ��й�һ�����������
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!myEnv.useHumanAgent) return;
        ChangeHumanState(actions);
    }

    public void ChangeHumanState(ActionBuffers actions)
    {

        ActionSegment<int> DiscreteActions = actions.DiscreteActions;
        //Debug.Log(this.name+"Actions:"+ DiscreteActions[0]);
        //HumanState = DiscreteActions[0];
            HumanState = DiscreteActions[0];
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
        string directoryPath = Path.Combine(Application.persistentDataPath, "Reward");
        string filePath = Path.Combine(directoryPath, $"Human_Reward_log_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        try
        {
            // ȷ��Ŀ¼����
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string result = "";
            foreach (var kv in rewardLog)
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
