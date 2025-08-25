using CsvHelper.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class FireData : MonoBehaviour
{
    // 空间坐标 (单位: m)
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    //CO浓度 (单位: mol/mol)
    public float COConcentration { get; set; }

    // 温度 (单位: °C)
    public float Temperature { get; set; }

    // 能见度 (单位: m)
    public float Visibility { get; set; }

}
