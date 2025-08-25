using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;
using System.Globalization;


/*Resources路径设置：

您的文件在 Assets/Resources/FireData/Plot3D_23.5.csv

Resources加载路径应该是："FireData/Plot3D_23.5"（不要包含.csv扩展名）

文件要求：

确保CSV文件在 Assets/Resources/ 或其子文件夹中

Unity会自动将.csv文件作为TextAsset处理*/

public class CSVRead : MonoBehaviour
{
    [Header("测试设置")]
    public string resourcePath = "FireData/Plot3D_23.5"; // Resources下的路径
    public bool logDetailedData = true;

    void Start()
    {
        Debug.Log("=== 开始测试FireData读取 ===");
        Debug.Log($"资源路径: {resourcePath}");

        TestFireDataLoading();
    }

    private void TestFireDataLoading()
    {
        try
        {
            // 使用Resources.Load加载文本资源
            TextAsset csvFile = Resources.Load<TextAsset>(resourcePath);

            if (csvFile == null)
            {
                Debug.LogError($"找不到CSV文件: {resourcePath}");
                Debug.Log("请检查：");
                Debug.Log("1. 文件是否在Assets/Resources/FireData/文件夹中");
                Debug.Log("2. 文件名是否为Plot3D_23.5（不需要.csv扩展名）");
                Debug.Log("3. 文件格式是否为TextAsset");
                return;
            }

            Debug.Log($"成功加载文本资源，大小: {csvFile.bytes.Length} 字节");

            // 读取CSV数据
            var fireDataList = LoadFireDataFromTextAsset(csvFile);

            Debug.Log($"=== 数据加载完成 ===");
            Debug.Log($"成功加载 {fireDataList.Count} 条数据");

            if (fireDataList.Count == 0)
            {
                Debug.LogWarning("加载了0条数据，请检查CSV文件格式");
                return;
            }

            // 打印统计信息
            PrintStatistics(fireDataList);

            // 打印详细数据
            if (logDetailedData && fireDataList.Count > 0)
            {
                Debug.Log("=== 前5条详细数据 ===");
                for (int i = 0; i < Mathf.Min(5, fireDataList.Count); i++)
                {
                    PrintFireData(fireDataList[i], i);
                }

                Debug.Log("=== 最后5条详细数据 ===");
                for (int i = Mathf.Max(0, fireDataList.Count - 5); i < fireDataList.Count; i++)
                {
                    PrintFireData(fireDataList[i], i);
                }
            }

            // 测试数据查询功能
            TestDataQuery(fireDataList);

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载数据时出错: {ex.Message}");
            Debug.LogError($"堆栈跟踪: {ex.StackTrace}");
        }
    }

    private List<FireData> LoadFireDataFromTextAsset(TextAsset textAsset)
    {
        var dataList = new List<FireData>();

        using (var reader = new StringReader(textAsset.text))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = "\t",
            MissingFieldFound = null,
            BadDataFound = null,
            HeaderValidated = null,
            PrepareHeaderForMatch = args => args.Header.Trim()
        }))
        {
            // 注册映射配置
            csv.Context.RegisterClassMap<FireDataMap>();

            // 跳过第一行表头
            csv.Read();
            csv.ReadHeader();

            // 跳过第二行表头（单位行）
            csv.Read();

            // 读取数据行
            while (csv.Read())
            {
                try
                {
                    var record = csv.GetRecord<FireData>();
                    dataList.Add(record);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"解析行失败: {ex.Message}");
                    continue;
                }
            }
        }

        return dataList;
    }

    private void PrintStatistics(List<FireData> dataList)
    {
        if (dataList.Count == 0) return;

        float minTemp = float.MaxValue;
        float maxTemp = float.MinValue;
        float minCO = float.MaxValue;
        float maxCO = float.MinValue;
        float minVis = float.MaxValue;
        float maxVis = float.MinValue;

        foreach (var data in dataList)
        {
            minTemp = Mathf.Min(minTemp, data.Temperature);
            maxTemp = Mathf.Max(maxTemp, data.Temperature);
            minCO = Mathf.Min(minCO, data.COConcentration);
            maxCO = Mathf.Max(maxCO, data.COConcentration);
            minVis = Mathf.Min(minVis, data.Visibility);
            maxVis = Mathf.Max(maxVis, data.Visibility);
        }

        Debug.Log($"数据统计:");
        Debug.Log($"温度范围: {minTemp:F2}°C - {maxTemp:F2}°C");
        Debug.Log($"CO浓度范围: {minCO:E2} - {maxCO:E2} mol/mol");
        Debug.Log($"能见度范围: {minVis:F2}m - {maxVis:F2}m");
        Debug.Log($"空间范围: X[{dataList[0].X:F2} to {dataList[dataList.Count - 1].X:F2}]");
    }

    private void PrintFireData(FireData data, int index)
    {
        Debug.Log($"[{index:00000}] " +
                  $"位置: ({data.X:0.00}, {data.Y:0.00}, {data.Z:0.00}) | " +
                  $"CO: {data.COConcentration:0.000E00} | " +
                  $"温度: {data.Temperature:00.0}°C | " +
                  $"能见度: {data.Visibility:00.0}m");
    }

    private void TestDataQuery(List<FireData> dataList)
    {
        if (dataList.Count == 0) return;

        Debug.Log("=== 数据查询测试 ===");

        // 测试各种查询
        var highTempData = dataList.FindAll(data => data.Temperature > 50f);
        var dangerousCOData = dataList.FindAll(data => data.COConcentration > 0.001f);
        var lowVisibilityData = dataList.FindAll(data => data.Visibility < 10f);

        Debug.Log($"高温数据(>50°C): {highTempData.Count} 条");
        Debug.Log($"危险CO浓度(>0.001): {dangerousCOData.Count} 条");
        Debug.Log($"低能见度(<10m): {lowVisibilityData.Count} 条");

        // 显示一些样本数据
        if (highTempData.Count > 0)
            PrintFireData(highTempData[0], 0);
        if (dangerousCOData.Count > 0)
            PrintFireData(dangerousCOData[0], 0);
    }

    [ContextMenu("运行测试")]
    public void RunTest()
    {
        Start();
    }
}