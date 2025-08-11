 using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using System;
using Unity.VisualScripting;
public partial class BuildingControl : MonoBehaviour
{
    //���ɷ����Ҫ��
    //1.���ܹ�����߹�խ
    //2.�������Ӧ����һ����Сֵ����ʵ��һ�����һ�����ظ���
    //3.����ĳ���Ȳ��ܹ������
    //�÷����õ����������ݣ�
    //�����������areaSize
    /*.....................................һ������UI�������������ͷ�����Ŀ��Ȼ����з���İڷźͷ���ǽ�ڵ�����.......................................*/
    public void GenerateRoomsBinary(float areasize,int roomnum)
    {
      // Debug.Log("��ʼִ�ж��ַ����ɷ��䲼��......");
       totalArea=areasize;
       roomNum = roomnum;
       FindBestDimensions(totalArea);//���øú����󣬵õ���������ĳ��Ϳ� totalWidth��TotalHeight��zֵ��
        if (roomList.Count == 0)//roomList��ʼ������������Ϊ����ĵ�һ��
        {
           // Debug.Log("��ӵ�һ������");
            Room room = new Room(new Vector3(0, y / 2, 0), totalWidth, totalHeight, totalHeight * totalWidth);
            roomList.Add(room);
        }
        CreateRoomRectBinary(roomList[0]);      //�Է��䲼�ֽ��л��֣����ɵķ������ȫ���洢��roomList��
        CreateRoomBinary(roomList);

        // ������ͨͼ
        Room[][] CN = GenerateCN(roomList.ToArray());
        //������ͨͼ��������
        CreateDoorBetweenRooms(roomList.ToArray(), CN); //������ͨͼCN������
        //�����Ͻǵķ���������
        AddExitDoors(FindEscapeRoom());
    }

    private void CreateRoomRectBinary(Room room) {
        if (roomList.Count >= roomNum)//��ֹ����
        {
           // Debug.Log("�����еķ�������Ϊ��" + roomList.Count + ",��Ҫ���ɵķ�������Ϊ��" + roomNum+"���������");
            return;
        }
        // ѡ��ָ��
        bool splitVertical; // ���ѡ��ֱ��ˮƽ�ָ�
                            // �����ȴ��ڸ߶ȣ����ȴ�ֱ�ָ�
        if (room.width > 2*room.height)
        {
            splitVertical = true;
        }
        // ����߶ȴ��ڿ�ȣ�����ˮƽ�ָ�
        else if (room.height > 2*room.width)
        {
            splitVertical = false;
        }
        // �����Ⱥ͸߶���ȣ����ѡ��ָ��
        else
        {
            splitVertical = UnityEngine.Random.Range(0, 2) == 0;
        }


        //�����˷��������խ
        if (room.width<minWidth)
        {
            splitVertical = false;
        }
        else if(room.height<minheight)
        { splitVertical = true; }



        if (splitVertical)  //��ֱ�ָͼ�κܳ�
        {
           // Debug.Log("ѡ��ķָ���ǣ���ֱ�ָ�");
            // ��ֱ�ָ���ѡ��ָ��
            float minSplit =  minWidth; // ��С�ָ���
            float maxSplit = room.width - minWidth; // ���ָ���
           

            if (maxSplit < minSplit)//���ܳ��֣�����ķ�����Ŀ���࣬�����������ֹ�Ļ������ջ��ֵķ�����С
            {
                Debug.Log("���ֺ�ķ����С�����ܼ������л�����");
                return;
            }

            // ����ָ��ȣ������Ƴ����
            float splitWidth = FindValidSplit(minSplit, maxSplit, room.width, room.height);


            // �����󷿼���ҷ���
            Room leftRoom = new Room(
                new Vector3(room.xzPosition.x,0,room.xzPosition.z),
                splitWidth,
                room.height,
                splitWidth*room.height);

            Room RightRoom= new Room(
                new Vector3(room.xzPosition.x+splitWidth,0,room.xzPosition.z),
                room.width-splitWidth,
                room.height,
                (room.width - splitWidth)*room.height);

            roomList.Remove(room);//�Ƴ�ƥ��ĵ�һ��Ԫ�أ�������ǰ��
            roomList.Add(leftRoom);//���������ĩβ
            roomList.Add(RightRoom);


            Room newBigRoom=FindTheBiggestRoom(roomList);
            CreateRoomRectBinary(newBigRoom);
        }
        else
        {
         //   Debug.Log("ѡ��ķָ���ǣ�ˮƽ�ָ�");
            // ˮƽ�ָ���ѡ��ָ��
            float minSplit =minheight; // ��С�ָ�߶�
            float maxSplit = room.height - minheight; // ���ָ�߶�
            

            if (maxSplit < minSplit)
            {
                Debug.Log("���ֺ�ķ����С�����ܼ������л�����");
                return;
            }
            // ѡȡ�ָ��
            float splitHeight = FindValidSplit(minSplit, maxSplit, room.width, room.height);


            // �����·�����Ϸ���
            Room bottomRoom = new Room(
                new Vector3(room.xzPosition.x, 0, room.xzPosition.z), 
                room.width, 
                splitHeight,
                room.width*splitHeight);

            Room topRoom = new Room(
                new Vector3(room.xzPosition.x, 0, room.xzPosition.z+splitHeight),
                room.width,
                room.height - splitHeight, 
                room.width * (room.height - splitHeight));

            roomList.Remove(room);
            roomList.Add (bottomRoom);
            roomList.Add(topRoom);
            Room newBigRoom = FindTheBiggestRoom(roomList);
            CreateRoomRectBinary(newBigRoom);
        }
    }
    public Room FindTheBiggestRoom(List<Room> roomlist)
    {
        //Debug.Log("Ѱ�ҷ���������ķ��䣡����������������������������");
        float MaxroomSize = 0f;
        int num = 1;
        int max = 0;
        Room biggestroom = new();
        foreach (Room room in roomlist) {
            //Debug.Log("����"+num + "������ǣ�" + room.roomSize);
          if(room.roomSize > MaxroomSize) {
            MaxroomSize = room.roomSize; // ����������
            biggestroom = room;
             max = num;
            }
            num++;
        }
       // Debug.Log( "��󷿼��ǣ�" + "����" + max);

        return biggestroom;

    }
    public void CreateRoomBinary(List<Room> roomlist)
    {
      //  Debug.Log("ִ�����ڳ����д�������ĺ���");
        foreach (Room room in roomlist) {
             GameObject Realroom=  CreateRoomInScene(room.xzPosition.x, room.xzPosition.z, room.width, room.height,room.roomName);
            AddObjectToList(Realroom);
            //Debug.Log("����"+num+"��XZ����Ϊ"+room.xzPosition.x+"��"+room.xzPosition.z);
            //Debug.Log("����" + num + "�ĳ��͸�Ϊ" + room.width + "��" + room.height);
        }
    }

    /// <summary>
    /// �����ҵ����ϳ����Ҫ��ķָ��
    /// </summary>
    /// <param name="minSplit">��С�ָ�ֵ</param>
    /// <param name="maxSplit">���ָ�ֵ</param>
    /// <param name="roomWidth">������</param>
    /// <param name="roomHeight">����߶�</param>
    /// <returns>����Ҫ��ķָ�㣻���δ�ҵ������� -1</returns>
    private float FindValidSplit(float minSplit, float maxSplit, float roomWidth, float roomHeight)
    {
        int maxAttempts = 2000; // ����Դ���
        int attempts = 0;

        // ��ʼ���������
        float minAspectRatio = 0.618f; // 
        float maxAspectRatio = 1.618f;    // 4

        do
        {
            // ������ɷָ��
            float splitValue = UnityEngine.Random.Range(minSplit, maxSplit);

            // ������������ĳ����
            float aspectRatio1 = splitValue / roomHeight;
            float aspectRatio2 = (roomWidth - splitValue) / roomHeight;

            attempts++;

            // ÿ 100 �γ��Ժ�ſ�����
            if (attempts % 2000== 0 && attempts > 0)
            {
                minAspectRatio *= 0.8f; // �ſ�����
                maxAspectRatio *= 1.2f; // �ſ�����
              //  Debug.Log($"���� {attempts} ��δ�ɹ����ſ��������Ϊ {minAspectRatio} �� {maxAspectRatio}");
            }

            // ��鳤����Ƿ����Ҫ��
            if (aspectRatio1 >= minAspectRatio && aspectRatio1 <= maxAspectRatio &&
                aspectRatio2 >= minAspectRatio && aspectRatio2 <= maxAspectRatio)
            {
                return splitValue; // ���ط���Ҫ��ķָ��
            }
        }
        while (attempts < maxAttempts);

        // ���δ�ҵ�����Ҫ��ķָ�㣬���� -1

        //Debug.Log("û���ҵ�����ķָ�㣬�������ֵ");
        return UnityEngine.Random.Range(minSplit, maxSplit);
    }

    public GameObject CreateRoomInScene(float x, float z, float width, float height,string roomName)
    {
        // �������������

        GameObject room = new GameObject(roomName);
        room.tag = "Room";
        room.transform.parent = ParentObject.transform;
        RoomNum++;

        // ���ɷ���ĵײ��������Ҫ�Ļ���������ӵײ�����Ϊ������棩
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.AddComponent<NavMeshModifier>();
        floor.name = "floor";
        floor.tag = "Floor";
        floor.layer = 8;
        floor.transform.parent = room.transform;
        floor.transform.position = new Vector3(x + width / 2, 0f, z + height / 2);
        floor.transform.localScale = new Vector3(width, 0.1f, height); // �ײ���Ⱥ͸߶�

        // ������ײ���ɫ Assets/Material/Floor.mat
        floor.GetComponent<Renderer>().material = Floor;

        /* AddObjectToList(floor); // ��������뵽�����б���*/ //ǽ���Ƿ���������壬�����Ѿ��������
                                                    // �����ĸ�ǽ��
        CreateWall(x, z, width, height, room);
        return room;
    }
    public Room FindEscapeRoom()
    {
        if (roomList == null || roomList.Count == 0)
        {
            Debug.LogWarning("�����б�Ϊ�գ��޷��ҵ����Ͻǵķ��䣡");
            return null;
        }

        Room escapeRoom = null;
        float maxSum = float.MinValue; // ���ڼ�¼�������Ͻ�����֮��

        foreach (Room room in roomList)
        {
            // ���㷿�����Ͻǵ�����֮��
            float sum = room.xzPosition.x + room.width + room.xzPosition.z + room.height;

            // �����ǰ��������Ͻ�����֮�͸�����������ֵ�ͷ���
            if (sum > maxSum)
            {
                maxSum = sum;
                escapeRoom = room;
            }
        }

        // ����ҵ��ķ�����Ϣ
        if (escapeRoom != null)
        {
            //Debug.Log($"�ҵ����Ͻǵķ��䣺λ��=({escapeRoom.xzPosition.x}, {escapeRoom.xzPosition.z}), ���={escapeRoom.width}, �߶�={escapeRoom.height}");
        }
        else
        {
            Debug.Log("δ�ҵ����Ͻǵķ���");
        }

        return escapeRoom;
    }
}
