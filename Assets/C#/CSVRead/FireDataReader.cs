using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System;

public class FireDataReader
{
    public static async Task<List<FireData>> LoadFireDataAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var dataList = new List<FireData>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,        // 有表头
                Delimiter = "\t",              // 制表符分隔
                MissingFieldFound = null,      // 忽略缺失字段
                BadDataFound = null,           // 忽略错误数据
                HeaderValidated = null,        // 跳过表头验证（因为有两行表头）
                PrepareHeaderForMatch = args => args.Header.Trim()
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                // 注册自定义映射配置
                csv.Context.RegisterClassMap<FireDataMap>();

                // 跳过第一行表头（"X Y Z ..."）
                csv.Read();
                csv.ReadHeader();

                // 跳过第二行表头（"m m m mol/mol C m ..."）
                csv.Read();

                // 开始读取数据行
                while (csv.Read())
                {
                    try
                    {
                        var record = csv.GetRecord<FireData>();
                        dataList.Add(record);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"解析行失败: {ex.Message}");
                    }
                }
            }

            return dataList;
        });
    }
}