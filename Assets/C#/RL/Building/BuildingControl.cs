using System;
using System.Collections.Generic;
using UnityEngine;
public partial class BuildingControl : MonoBehaviour
{

    /*........................һ�����������õ������ݽṹ.....................................*/
    private float totalArea; // �������С
    public int roomNum=10; //��Ҫ���ɵķ�������
    public float totalWidth;//���ڼ�¼��������Ŀ�
    public float totalHeight;//���ڼ�¼��������ĸ�


    public float[] roomAreas;// ����ķ���������飨��֪���飩
    public int RoomNum = 0;//��¼�Ѿ����ɵķ�������,���ڸ�������  
    public int doorNum = 1;//��¼�Ѿ����ɵ�������,���ڸ��ű��
    float y = 3.0f;//ǽ��ĸ߶� ,
    float doorWidth = 1.5f;//�ŵĿ��

    private GameObject[] AllObjects; //�洢�������ɵ��������壬����������ڶ�����ɻ���ʱ�����֮ǰ�ĳ���
    public Material Floor;
    public Material Door;
    public Material Exit;
    public Material Wall;
    /*.............................��������֮���������õ������ݽṹ................................*/

    public float minWidth = 2.5f;
    public float minheight = 2.5f;


    [System.Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new Vector3(x, y, z);
    } 
    public List<Room> roomList = new List<Room>();  // �洢���ɵ����з������

    /*.............................�������ɵ���ʹ�õ����ݽṹ................................*/

    public GameObject ParentObject;

    }
