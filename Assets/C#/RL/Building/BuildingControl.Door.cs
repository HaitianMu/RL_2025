using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BuildingControl : MonoBehaviour
{
    /*.......................................���������ɵķ���֮��������.....................................*/
    Room[][] GenerateCN(Room[] rooms)//ȷ�����еĽڵ㶼�ܵ������һ���ڵ㼴��
    {

        //������ͨͼ,������ͨ�ķ���֮�䶼���һ����
        int n = rooms.Length;
        Room[][] connection = new Room[n][];


        for (int i = 0; i < rooms.Length; i++)
        {
            // ��ÿ�����䣬��ʼ��һ���µ��ھ��б�
            connection[i] = new Room[rooms.Length];
        }

        // �����ͨͼ
        for (int i = 0; i < rooms.Length; i++)
        {
            for (int j = i + 1; j < rooms.Length; j++)
            {
                if (rooms[i].IsAdjacentTo(rooms[j])) // ���������������
                {
                    connection[i][j] = rooms[j]; // �������
                    connection[j][i] = rooms[i]; // ˫������
                     //Debug.Log(rooms[i].roomObject.name + "��" + rooms[j].roomObject.name + "����");//������
                }
            }
        }
        return connection;
        // //������ͨͼ,Ȼ����������ͨ�ķ���֮�䶼���һ����

    }

    // �жϱ��Ƿ���ȷ����ͨ���������
    void CreateDoorBetweenRooms(Room[] rooms, Room[][] CN) //������ͨͼCN������
    {
        String DoorName = "Door" + doorNum;
        float distance = 5;
        for (int i = 0; i < rooms.Length; i++)
        {
            for (int j = i+1; j < rooms.Length; j++)
            {
                if (CN[i][j] != null)//˵�������������ڽӵģ������ж������������ϡ��¡������������ڷ�ʽ�����ݲ�ͬ�����ڷ�ʽ���в�ͬ�Ĵ���
                {
                    /*Debug.Log(rooms[i].roomObject.name + "��" + rooms[j].roomObject.name + "����");//������*/
                    Vector3 DoorPosition;
                    // �ҷ�����
                    if (Mathf.Abs(rooms[i].xzPosition.x + rooms[i].width - rooms[j].xzPosition.x) < 0.1f)
                    {
                        // Debug.Log("�ڸ÷����ҷ�����");//������
                        // ������������� x �᷽��������,��������������ڲ�����z�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                        //����ɷ�Ϊ�������
                        if (rooms[j].xzPosition.z < rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height > rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height - rooms[i].xzPosition.z >= distance)
                        {
                            // Debug.Log("�ڸ÷����ҷ����ڣ����1");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x + rooms[i].width, y / 2, (rooms[i].xzPosition.z + rooms[j].xzPosition.z + rooms[j].height) / 2);
                            /* DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                             DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                        if (rooms[j].xzPosition.z < rooms[i].xzPosition.z + rooms[i].height && rooms[j].xzPosition.z + rooms[j].height > rooms[i].xzPosition.z + rooms[i].height && rooms[i].xzPosition.z + rooms[i].height - rooms[j].xzPosition.z >= distance)
                        {
                            // Debug.Log("�ڸ÷����ҷ����ڣ����2");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x + rooms[i].width, y / 2, (rooms[j].xzPosition.z + rooms[i].xzPosition.z + rooms[i].height) / 2);
                            /* DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                             DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                        if (rooms[j].xzPosition.z >= rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height <= rooms[i].xzPosition.z + rooms[i].height && rooms[j].height >= distance)
                        {
                            //  Debug.Log("�ڸ÷����ҷ����ڣ����3");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x + rooms[i].width, y / 2, rooms[j].xzPosition.z + rooms[j].height / 2);
                            /* DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                             DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                        if (rooms[j].xzPosition.z < rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height > rooms[i].xzPosition.z + rooms[i].height && rooms[i].height >= distance)
                        {
                            //   Debug.Log("�ڸ÷����ҷ����ڣ����4");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x + rooms[i].width, y / 2, rooms[i].xzPosition.z + rooms[i].height);
                            /*  DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                              DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                    }

                    //������
                   else if (Mathf.Abs(rooms[i].xzPosition.x - rooms[j].width - rooms[j].xzPosition.x) < 0.1f)
                    {
                        // ������������� x �᷽��������,��������������ڲ�����z�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                        // Debug.Log("�ڸ÷���������");//������
                        //����ɷ�Ϊ�������
                        if (rooms[j].xzPosition.z <= rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height >= rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height - rooms[i].xzPosition.z >= distance)
                        {
                            //  Debug.Log("�����ڵ�һ�����");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x, y / 2, (rooms[i].xzPosition.z + rooms[j].xzPosition.z + rooms[j].height) / 2);
                            /*   DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                               DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                        if (rooms[j].xzPosition.z <= rooms[i].xzPosition.z + rooms[i].height && rooms[j].xzPosition.z + rooms[j].height >= rooms[i].xzPosition.z + rooms[i].height && rooms[i].xzPosition.z + rooms[i].height - rooms[j].xzPosition.z >= distance)
                        {
                            // Debug.Log("�����ڵڶ������");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x, y / 2, (rooms[j].xzPosition.z + rooms[i].xzPosition.z + rooms[i].height) / 2);
                            /*DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                        if (rooms[j].xzPosition.z >= rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height <= rooms[i].xzPosition.z + rooms[i].height && rooms[j].height >= distance)
                        {
                            //  Debug.Log("�����ڵ��������");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x, y / 2, rooms[j].xzPosition.z + rooms[j].height / 2);
                            /*  DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                              DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                        if (rooms[j].xzPosition.z < rooms[i].xzPosition.z && rooms[j].xzPosition.z + rooms[j].height > rooms[i].xzPosition.z + rooms[i].height && rooms[i].height >= distance)
                        {
                            //  Debug.Log("�����ڵ��������");//������
                            DoorPosition = new Vector3(rooms[i].xzPosition.x, y / 2, rooms[i].xzPosition.z + rooms[i].height);
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                            continue;
                        }
                    }
                    //�Ϸ�����
                    else if (Mathf.Abs(rooms[i].xzPosition.z + rooms[i].height - rooms[j].xzPosition.z) < 0.3f)
                    {
                        //  Debug.Log("�ڸ÷����Ϸ�����");//������
                        // ������������� z �᷽�������ڣ���������������ڲ�����x�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                        if (rooms[j].xzPosition.x < rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width > rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width - rooms[i].xzPosition.x >= distance)
                        {
                            DoorPosition = new Vector3((rooms[j].xzPosition.x + rooms[j].width + rooms[i].xzPosition.x) / 2, y / 2, rooms[i].xzPosition.z + rooms[i].height);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;

                        }
                        else if (rooms[j].xzPosition.x < rooms[i].xzPosition.x + rooms[i].width && rooms[j].xzPosition.x + rooms[j].width > rooms[i].xzPosition.x + rooms[i].width && rooms[i].xzPosition.x + rooms[i].width - rooms[j].xzPosition.x >= distance)
                        {
                            DoorPosition = new Vector3((rooms[i].xzPosition.x + rooms[i].width + rooms[j].xzPosition.x) / 2, y / 2, rooms[i].xzPosition.z + rooms[i].height);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;
                        }
                        else if (rooms[j].xzPosition.x >= rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width <= rooms[i].xzPosition.x + rooms[i].width && rooms[j].width >= distance)
                        {
                            DoorPosition = new Vector3(rooms[j].xzPosition.x + rooms[j].width / 2, y / 2, rooms[i].xzPosition.z + rooms[i].height);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;
                        }
                        else if (rooms[j].xzPosition.x < rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width > rooms[i].xzPosition.x + rooms[i].width && rooms[i].width >= distance)
                        {
                            DoorPosition = new Vector3(rooms[i].xzPosition.x + rooms[i].width / 2, y / 2, rooms[i].xzPosition.z + rooms[i].height);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;
                        }
                    }
                    //�·�����
                   else if (Mathf.Abs(rooms[i].xzPosition.z - rooms[j].height - rooms[j].xzPosition.z) < 0.1f)
                    {
                        // ������������� z �᷽�������ڣ���������������ڲ�����x�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                        // Debug.Log("�ڸ÷����·�����");//������
                        if (rooms[j].xzPosition.x < rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width > rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width - rooms[i].xzPosition.x >= distance)
                        {
                            DoorPosition = new Vector3((rooms[j].xzPosition.x + rooms[j].width + rooms[i].xzPosition.x) / 2, y / 2, rooms[i].xzPosition.z);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;
                        }
                        else if (rooms[j].xzPosition.x < rooms[i].xzPosition.x + rooms[i].width && rooms[j].xzPosition.x + rooms[j].width > rooms[i].xzPosition.x + rooms[i].width && rooms[i].xzPosition.x + rooms[i].width - rooms[j].xzPosition.x >= distance)
                        {
                            DoorPosition = new Vector3((rooms[j].xzPosition.x + rooms[j].width + rooms[i].xzPosition.x) / 2, y / 2, rooms[i].xzPosition.z);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;
                        }
                        else if (rooms[j].xzPosition.x >= rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width <= rooms[i].xzPosition.x + rooms[i].width && rooms[j].width >= distance)
                        {
                            DoorPosition = new Vector3((rooms[j].xzPosition.x + rooms[j].width + rooms[i].xzPosition.x) / 2, y / 2, rooms[i].xzPosition.z);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;
                        }
                        else if (rooms[j].xzPosition.x < rooms[i].xzPosition.x && rooms[j].xzPosition.x + rooms[j].width > rooms[i].xzPosition.x + rooms[i].width && rooms[i].width >= distance )
                        {
                            DoorPosition = new Vector3((rooms[j].xzPosition.x + rooms[j].width + rooms[i].xzPosition.x) / 2, y / 2, rooms[i].xzPosition.z);
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                            continue;
                        }
                    }
                }
            }
        }
    }
    void AddExitDoors(Room EscapeRoom) //�����һ��������Ҳ���Ϸ�������
    {
        float wallHeight = y;//�ŵĸ߶�
                             // �ҷ��ŵ�λ��
        Vector3 RightDoorPosition = new Vector3(EscapeRoom.xzPosition.x + EscapeRoom.width, wallHeight / 2, EscapeRoom.xzPosition.z + EscapeRoom.height / 2);
        // �Ϸ��ŵ�λ��
        Vector3 FrontDoorPosition = new Vector3(EscapeRoom.xzPosition.x + EscapeRoom.width / 2, wallHeight / 2, EscapeRoom.xzPosition.z + EscapeRoom.height);
        // �����ҷ���������
        CreateDoor(RightDoorPosition, 0.1f, true, "Exit");
        /* CreateDoor(FrontDoorPosition, 0.1f, false, "Exit");*/
        //Transform child = transform.Find("ChildName");
    }
    void CreateDoor(Vector3 position, float width, bool isHorizontal, String Tag)
    {//���������� �ŵ�λ�ã��ŵĿ�ȣ�����ΰڷţ��ŵı�ǩ���ŵĸ߶�Ĭ����ȫ�ֱ����е� y ֵ
     // ����һ���µ������壨�ţ�������������Ϊ������
     //����ŵĽű�,��ִ����Ӧ�ĳ�ʼ������
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.transform.parent = ParentObject.transform;
        door.GetComponent<BoxCollider>().isTrigger = true;
        DoorControl thisDoor = door.AddComponent<DoorControl>();

        // �����ŵ����ƺͱ�ǩ
        if (Tag == "Door")
        {
            door.name = Tag + doorNum;
            doorNum++;
        }
        else { door.name = Tag; }
        door.tag = Tag;
        // ������ӵ���ɾ���б���
        AddObjectToList(door);
        // �����ŵ�λ��
        door.transform.position = new Vector3(position.x, position.y, position.z);

        // �ж��Ƿ��Ǻ�����
        if (isHorizontal)
        {
            // ����Ǻ����ţ������ŵĴ�С
            // 0.3f ���ŵĺ�ȣ�y-1 ���ŵĸ߶ȣ�1f ���ŵĿ��
            door.transform.localScale = new Vector3(0.4f, 3.2f, doorWidth);
            thisDoor.doorDirection = "Horizontal";
            // �����ŵ���ɫ
            if (Tag == "Exit")
            {
                door.GetComponent<Renderer>().material = Exit;  // ��ɫ
            }
            else
            {
                door.GetComponent<Renderer>().material = Door;  // ��ɫ
            }
        }
        else
        {
            // ����������ţ������ŵĴ�С
            // 1f ���ŵĿ�ȣ�y-1 ���ŵĸ߶ȣ�0.3f ���ŵĺ��
            door.transform.localScale = new Vector3(doorWidth, 3.2f, 0.4f);
            thisDoor.doorDirection = "Vertical";
            // �����ŵ���ɫ
            if (Tag == "Exit")
            {
                door.GetComponent<Renderer>().material = Exit;  // ��ɫ
            }
            else
            {
                door.GetComponent<Renderer>().material = Door;  // ��ɫ
            }
        }
        thisDoor.AddNavMeshLink();


    }
}
