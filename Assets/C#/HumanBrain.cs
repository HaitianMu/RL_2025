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
    // 在EnvController中定义常量
    public const int MAX_HUMANS = 10; //最大人类数量， 与课程学习上限一致
    public const int MAX_ROOMS = 15; // 与建筑设计上限一致
    public const float INVALID_MARKER = -2f; // 超出[-1,1]范围的无效标记

    public EnvControl myEnv;
    public HumanControl myHuman;//大脑对应的人类
    public bool HumanIsInitialized=false;
    public int HumanState;//根据决策确定的人类移动状态；

    public void FixedUpdate()
    {
        if (!HumanIsInitialized)
        {
           // print("初始化已完成，我的小人是"+myHuman.name);
           return;

        }
        if(myEnv.useHumanAgent)
        {
            //请求决策网络支援
            RequestDecision();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //建议先固定观测值数量并确保严格归一化到[0, 1]范围，这是PPO算法稳定训练的前提条件
        //在RequestDecision函数执行后，会先执行该函数来收集环境观测值
        //观测值需要添加：
        //每一个人类的位置，来学习人类的移动逻辑
        //每一个机器人的位置，来学习其他机器人的移动逻辑，但目前只有一个机器人
        //总区域的面积，房间的数量/位置，每一个门的位置， 来学习建筑的生成逻辑
        // 计算环境边界（与Human观测保持一致）

        if (!myEnv.useHumanAgent) return;

        //Debug.Log("CollectObservations called."); 
        if (myEnv == null || myEnv.useHumanAgent is false)
        {
            Debug.Log("myEnv is null or useHumanAgen is false.");
            return;
        }
        //机器人位置观测值：2 * n（其中 n 为机器人的数量）,人类位置观测值：60,,房间位置观测值：30,出口位置观测值：2,火源位置观测值：6
        // 添加 Agent 观测值
        // 使用场景对角线长度归一化，确保所有坐标∈[0,1]
        float sceneDiagonal = Mathf.Sqrt(
            Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalWidth, 2) +
            Mathf.Pow(myEnv.complexityControl.buildingGeneration.totalHeight, 2)
        );
        //  print("场景对角线为长度为："+sceneDiagonal);
        // 归一化 Agent 位置 ，           2个
        foreach (RobotBrain agent in myEnv.RobotBrainList)
        {
            Vector3 normalizedPos = (agent.robot.transform.position) / sceneDiagonal;
            sensor.AddObservation(normalizedPos.x);
            sensor.AddObservation(normalizedPos.z);
            //Debug.Log("机器人的位置为" + normalizedPos);
        }

        // 归一化 Human 位置，人类最多10个            20个

        // 固定观测维度为 MAX_HUMANS * 2
        for (int i = 0; i < MAX_HUMANS; i++)
        {
            if (i < myEnv.personList.Count)
            {
                // 填充实际人类位置
                HumanControl human = myEnv.personList[i];
                Vector3 normalizedPos = human.transform.position / sceneDiagonal;
                sensor.AddObservation(normalizedPos.x);
                sensor.AddObservation(normalizedPos.z);
            }
            else
            {
                // 填充占位值（推荐使用无效坐标）
                sensor.AddObservation(-1f); // x
                sensor.AddObservation(-1f); // z
            }
        }

        // 添加房间位置（相对Agent） 目前固定10个房间，
        int maxRooms = 10;
        for (int i = 0; i < maxRooms; i++)
        {
            if (i < myEnv.cachedRoomPositions.Count)
            {
                Vector3 roomPos = myEnv.cachedRoomPositions[i];
                {
                    // 位置归一化（相对于环境中心）
                    Vector3 normalizedPos = (roomPos) / sceneDiagonal;
                    sensor.AddObservation(normalizedPos.x); // X坐标 [-1, 1]
                    sensor.AddObservation(normalizedPos.z); // Z坐标 [-1, 1]
                    //Debug.Log("房间的位置为" + normalizedPos);
                }
            }
            else
            {
                // 填充占位值（推荐使用无效坐标）
                sensor.AddObservation(-1f); // x
                sensor.AddObservation(-1f); // z
            }
        }

        //添加出口位置   只有1个出口         39+[24,45]=[63,84]     2个
        sensor.AddObservation((myEnv.Exits[0].transform.position.x) / sceneDiagonal);
        sensor.AddObservation((myEnv.Exits[0].transform.position.z) / sceneDiagonal);
        //Debug.Log("出口的位置为" + (myEnv.Exits[0].transform.position) / Mathf.Max(myEnv.complexityControl.buildingGeneration.totalWidth, myEnv.complexityControl.buildingGeneration.totalHeight));

        //添加火源位置，目前火源只设置了三个      6个
        for (int i = 0; i < 3; i++)
        {
            Vector3 firePos = myEnv.FirePosition[i];
            {
                // 位置归一化（相对于环境中心）
                Vector3 normalizedPos = (firePos) / sceneDiagonal;
                sensor.AddObservation(normalizedPos.x); // X坐标 [-1, 1]
                sensor.AddObservation(normalizedPos.z); // Z坐标 [-1, 1]
                // Debug.Log("火源的位置为" + normalizedPos);
            }
        }
        //添加火源数量，火焰数量难以进行归一化，不添加了
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

    // 奖励统计可视化
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
        // 定义保存路径（使用persistentDataPath）
        string directoryPath = Path.Combine(Application.persistentDataPath, "Reward");
        string filePath = Path.Combine(directoryPath, $"Human_Reward_log_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        try
        {
            // 确保目录存在
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
            // 写入文件
            File.WriteAllText(filePath, result);

            // 日志输出
            Debug.Log($"奖励数据已保存到: {filePath}\n{result}");
        }
        catch (Exception e)
        {
            Debug.LogError($"保存奖励数据失败: {e.Message}");
        }
    }
}
