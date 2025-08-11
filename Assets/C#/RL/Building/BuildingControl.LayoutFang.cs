 using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using System;
public partial class BuildingControl : MonoBehaviour
{

    /*.....................................һ������UI�������������ͷ�����Ŀ��Ȼ����з���İڷźͷ���ǽ�ڵ�����.......................................*/
    public void GenerateRoomsFang()
    {
        /*foreach (float part in roomAreas)
        {
            Debug.Log("�����������ܵ������ݣ�" + part);
        }*/
        RoomNum = 1;  //���÷��������
        doorNum = 1;  //�����ż�����

        // ���������
        totalArea = 0f;
        foreach (var area in roomAreas)
        {
            totalArea += area;
        }

        // �ҳ���ӽ������εĳ������
        FindBestDimensions(totalArea);
       
        // ʹ�÷�����ͼ���ɷ���
        CreateRoomRectsFangTree(roomAreas, 0, roomAreas.Length, 0, 0, totalWidth, totalHeight, totalArea, (totalHeight / (float)totalWidth) > 1);

        // ��������֮����ţ�ȷ��������ͨ
        Room[][] CN = GenerateCN(roomList.ToArray());
        //CreateDoorBetweenRooms();

        CreateDoorBetweenRooms(roomList.ToArray(), CN); //������ͨͼCN������

        //�����һ��������������ǽ����������������Ϊ��������
        AddExitDoors(roomList[roomList.Count - 1]); 
    }
    public void ClearPreviousRooms()// �������ú���
    {
        //
       // Debug.Log("1.��ճ�������");
        // ��� AllObjects �Ƿ�Ϊ null������ǣ���ʼ��Ϊһ��������
        roomList = new List<Room>();
        RoomNum = 1;
        doorNum = 1;
        if (AllObjects == null)
        {
            AllObjects = new GameObject[0];
        }
        else
        {
            // ���������Ѿ����ɵķ����������
            foreach (var room in AllObjects)
            {
                if (room != null)
                {
                    //Debug.Log(room.name + "�Ѿ������");
                    //ע�� Destroy()��������ִ�У�����Ҫ���������ٲ���ִ�У�Ӧ����DestroyImmediate()
                    DestroyImmediate(room);

                }
            }
        }
        // ��շ����б�
        AllObjects = new GameObject[0];

        ParentObject = GameObject.Find("Building");//���ý����ĸ�����
    }
    void FindBestDimensions(float totalArea)// �ҳ��������ӽ�1�Ŀ����ϣ����BestRatio>3��<1/3���򽫸���������Ϊ������
    {
        int bestWidth = 0;
        int bestHeight = 0;
        float bestRatio = float.MaxValue;  // ��ӽ�1�ı�ֵ

        // �������п��ܵĿ��ֵ
        for (int width = 1; width <= Mathf.FloorToInt(Mathf.Sqrt(totalArea)); width++)
        {
            if (totalArea % width == 0)  // ��������������õ���Ӧ�ĸ߶�
            {
                int height = Mathf.FloorToInt(totalArea / width);

                // ���㵱ǰ�ĳ����
                float ratio;
                if ((float)width / height > 1)
                {
                    // ����ȴ���1��ֱ�Ӽ����ֵ
                    ratio = Mathf.Abs((float)width / height - 1);
                }
                else
                {
                    // �����С��1��ȡ����������ֵ
                    ratio = Mathf.Abs((float)height / width - 1);
                }

                // �����ǰ�ĳ���ȸ��ӽ�1���������ѽ��
                if (ratio < bestRatio)
                {
                    bestRatio = ratio;
                    bestWidth = width;
                    bestHeight = height;
                }
            }
        }

        // �������յĳ����
        float finalRatio = (float)bestWidth / bestHeight;

        //�����ѳ���ȴ���3��С��1/3�� ����Ϊ������
        if (finalRatio > 3 || finalRatio < 1 / 3f)
        {
            // ����Ϊ�����Σ������ȣ�
            bestWidth = Mathf.FloorToInt(Mathf.Sqrt(totalArea));
            bestHeight = bestWidth;
        }


        // ������ѵĿ�Ⱥ͸߶�
        totalWidth = bestWidth;
        totalHeight = bestHeight;
    }
    void CreateRoomRectsFangTree(float[] areas, int start, int end, float x, float z, float width, float height, float totalArea, bool isHorizontal)
    {
        // ������ͼ�����ַ���
        //areas ָ��Ҫ���ֵķ��䣬
        //start�ǵ�ǰ������������ʼ�㣬end������Ľ����㣬
        //x��z�ֱ��ʾ��ǰ������������½ǣ�width�ǵ�ǰ����ĳ���x���򣩣�height�ǵ�ǰ����ĸߣ�z���򣩣�totalArea�ǵ�ǰ������������С
        // isHorizontal �����1�����ʾ������ʹ�õ�����ֱ���֣���z/x>1;��֮��z/x<1������һ�����ֵ��в����෴�Ļ��ַ���
        // �����ǰ�ݹ�Ĳ�����Ϣ�� Unity ����̨
        /* Debug.Log("........................................");
         Debug.Log("Calling CreateRoomRects:");
         Debug.Log($"  start: {start}, end: {end}");
         Debug.Log($"  x: {x}, z: {z}");
         Debug.Log($"  width: {width}, height: {height}");
         Debug.Log($"  totalArea: {totalArea}, isHorizontal: {isHorizontal}");
         Debug.Log("........................................");*/


        if (start >= end) return;
        // ���ֻ��һ�����䣬ֱ�����ɾ��Σ����ٵݹ�
        if (end - start == 1)
        {
            GameObject room = CreateRoom(x, z, width, height);
            AddObjectToList(room);
            return;
        }

        // �ҵ���ѻ��ֵ㣬ʹ�����������ӽ�
        // ������Ҫ�ҵ�һ�����ֵ㣬ʹ�ô� start �� splitIndex ������ܺ;����ܽӽ� currentArea / 2��
        // ��������ǲ��õķ�����ͼ�����������ֵ����ķ�����ͼ��ʹС����ȫ��������һ�����䣬�Ⲣ���������ǵ�Ԥ�ڣ���˽�����һ���ĸĽ�
        // ��������
        int splitIndex = start;
        float splitArea = 0; //���ֺ�����
        float targetArea = totalArea / 5; //Ŀ�����,
                                          //������ʱ�����ֵ��Ϊ�������1/10���Ա��ں�����ͨͼ�Ĳ�������ͨͼ���»���໮��Ϊ10���������Բ����ķ���������ӦΪ10����2025.1.1 Ŀǰ���ڿ���
        float minDifference = float.MaxValue; // ������¼��Ŀ���������С���
        float currentArea = 0;//��ǰ���ֵ����


        for (int i = start; i < end; i++)//������ֵ�
        {
            currentArea += areas[i];  // �ۼӵ�ǰ���ֵ����
            float currentDifference = Math.Abs(currentArea - targetArea); // ������Ŀ������Ĳ��

            // �����ǰ���С����С��࣬�������ѻ��ֵ�
            if (currentDifference < minDifference)
            {
                minDifference = currentDifference;
                splitIndex = i;
            }
            // �����ǰ���ֵ�����Ѿ�������Ŀ�������������ǰ��ֹ
            if (currentArea >= targetArea)
            {
                break;
            }
        }

        //������ֵ�֮ǰ�������������
        for (int i = start; i <= splitIndex; i++)
        {
            splitArea += areas[i];
        }
        //splitArea���ֳ�����������
        // ��ǰ����Ļ���
        if (isHorizontal)
        {
            // ˮƽ���֣����ָ߶�
            float splitHeight = (splitArea / totalArea) * height;
            // �°벿�����򣨴�start��splitIndex��
            float currentX = x;
            // ����ÿ��С����
            for (int i = start; i <= splitIndex; i++)
            {
                float roomWidth = (areas[i] / splitArea) * width; // ��ǰС����Ŀ��
                GameObject room = CreateRoom(currentX, z, roomWidth, splitHeight);
                AddObjectToList(room);
                currentX += roomWidth; // ������һ�������y����
            }
            //�ݹ������ϰ벿��
            CreateRoomRectsFangTree(areas, splitIndex + 1, end, x, z + splitHeight, width, height - splitHeight, totalArea - splitArea, !isHorizontal);
        }
        else

        {
            // ��ֱ���֣����ֿ��
            float splitWidth = (splitArea / totalArea) * width;
            // ��벿�����򣨴�start��splitIndex��
            float currentZ = z;
            // ����ÿ��С����
            for (int i = start; i <= splitIndex; i++)
            {
                float roomHeight = (areas[i] / splitArea) * height; // ��ǰС����ĸ߶�
                GameObject room = CreateRoom(x, currentZ, splitWidth, roomHeight);
                AddObjectToList(room);
                currentZ += roomHeight; // ������һ�������x����
            }
            //�ݹ������Ұ벿��
            CreateRoomRectsFangTree(areas, splitIndex + 1, end, x + splitWidth, z, width - splitWidth, height, totalArea - splitArea, !isHorizontal);
        }
    }

    public GameObject CreateRoom(float x, float z, float width, float height)
    {
        // �������������
        GameObject room = new GameObject("Room" + RoomNum);
        room.transform.parent = ParentObject.transform;
        RoomNum++;

        // ���ɷ���ĵײ��������Ҫ�Ļ���������ӵײ�����Ϊ������棩
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.AddComponent<NavMeshModifier>();
        floor.name = "floor";
        floor.transform.parent = room.transform;
        floor.transform.position = new Vector3(x + width / 2, 0f, z + height / 2);
        floor.transform.localScale = new Vector3(width, 0.1f, height); // �ײ���Ⱥ͸߶�

        // ������ײ���ɫ Assets/Material/Floor.mat
        floor.GetComponent<Renderer>().material = Floor;

        /* AddObjectToList(floor); // ��������뵽�����б���*/ //ǽ���Ƿ���������壬�����Ѿ��������
                                                    // �����ĸ�ǽ��
        CreateWall(x, z, width, height, room);

        // ���·�����뷿���б�,����¼ÿ�������λ�úʹ�С
        Room newRoom = new Room(room, new Vector3(x, 0f, z), width, height);
        roomList.Add(newRoom);  // ���·�����뷿���б�
        return room; // �������ɵķ������
    }
    void CreateWall(float x, float z, float width, float height, GameObject room)
    {
        // ǽ�ڵĺ�ȣ����Ե�����Խ��ǽ��Խ��
        float wallThickness = 0.1f;
        // ��������ǽ�壺�ĸ�����
        // 1. ��ǽ
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.tag = "Wall";
        leftWall.layer = LayerMask.NameToLayer("Robot");
        leftWall.name = "leftWall";
        leftWall.transform.parent = room.transform;
        leftWall.transform.position = new Vector3(x + wallThickness / 2, y / 2, z + height / 2);
        leftWall.transform.localScale = new Vector3(wallThickness, y, height);
        leftWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ


        // 2. ��ǽ
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "rightWall";
        rightWall.tag = "Wall";
        rightWall.transform.parent = room.transform;
        rightWall.transform.position = new Vector3(x + width - wallThickness / 2, y / 2, z + height / 2);
        rightWall.transform.localScale = new Vector3(wallThickness, y, height);
        rightWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ
        rightWall.layer = LayerMask.NameToLayer("Robot"); ;
        // 3. ��ǽ
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.name = "backWall";
        frontWall.tag = "Wall";
        frontWall.transform.parent = room.transform;
        frontWall.transform.position = new Vector3(x + width / 2, y / 2, z + wallThickness / 2);
        frontWall.transform.localScale = new Vector3(width, y, wallThickness);
        frontWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ
        frontWall.layer = LayerMask.NameToLayer("Robot"); ;
        // 4. ǰǽ
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "frontWall";
        backWall.tag = "Wall";
        backWall.transform.parent = room.transform;
        backWall.transform.position = new Vector3(x + width / 2, y / 2, z + height - wallThickness / 2);
        backWall.transform.localScale = new Vector3(width, y, wallThickness);
        backWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ
        backWall.layer = LayerMask.NameToLayer("Robot"); ;
    }
    public void AddObjectToList(GameObject room)// �����ɵķ�����ӵ������б�
    {
        //Debug.Log(room.name+"�Ѿ�����������Ͱ��");
        // �������ɵķ�����뵽�����б���
        Array.Resize(ref AllObjects, AllObjects.Length + 1);
        AllObjects[AllObjects.Length - 1] = room;
    }
}
