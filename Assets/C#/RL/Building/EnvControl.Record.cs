using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class EnvControl : MonoBehaviour
{
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
        string directoryPath = Path.Combine(Application.persistentDataPath, "Test_Data");
        string filePath = Path.Combine(directoryPath, $"DifferentScene_{DateTime.Now:yyyyMMdd_HHmmss}.json");//����Ҫ��һ������Ȼ���֮ǰ�ĸ���

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
