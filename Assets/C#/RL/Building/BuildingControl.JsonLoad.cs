using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static JsonLoad;

public partial class BuildingControl : MonoBehaviour
{

    [System.Serializable]
    public class Layout                 //���������࣬������ȡjson����
    {
        public string name;
        public String ExitRoom;
        public String ExitDoorPosition;//right left forward backward
        public Room[] rooms;
    }

    [System.Serializable]
    public class LayoutList
    {
        public List<Layout> Layouts;
    }
    // Start is called before the first frame update
    public void GenerateRoomsJsonLoad(string filename, string layoutname)
    {
        LoadRoomDataFromJson(filename, layoutname);//��json�ļ��м��ط������������
        CreateRoomBinary(roomList);//�ڳ��������ɷ���

        GetMaxXZofLayout(roomList);//�õ��ó������ھ��ε����Ͻ����꣬���ں���ѵ��ʱ�Ĺ۲�����һ��

        Room[][] CN = LoadDoorDataFromJson(filename, layoutname);//���ɵĲ������ϸ�ĶԳƾ��󣬿����Ǿ���εģ�ֻ�洢����Ҫ����Ϣ
                                                                 // print("roomList�еķ���Ϊ��" + roomList);
        CreateDoorBetweenRooms(CN); //������ͨͼCN������
        AddExitDoors(filename, layoutname);
    }

    public void LoadRoomDataFromJson(string filename, string layoutname)
    {
        //Debug.Log("��json�ļ��м��ط������������");
        TextAsset jsonFile = Resources.Load<TextAsset>(filename); //����json���ݵ�jsonFile����
        if (jsonFile == null)
        {
            Debug.LogError("JSON �ļ�δ�ҵ������飺\n" +
                         "- �ļ��Ƿ��� Assets/Resources ��\n" +
                         "- �ļ����Ƿ���ȫƥ�䣨������Сд��\n" +
                         "- �ļ���չ���Ƿ�Ϊ .json");
            return;
        }
        // Debug.Log("ԭʼJSON����:\n" + jsonFile.text);
        // 2. ʹ�� Newtonsoft.Json ��������ȡjson�ļ���һ��û���⡣��Ӧ�þ��ǽ�����ʱ�������
        try
        {
            LayoutList data = JsonConvert.DeserializeObject<LayoutList>(jsonFile.text);
            if (data == null || data.Layouts == null)
            {
                Debug.LogError("JSON����ʧ�ܣ������ʽ�Ƿ���ȷ");
                return;
            }

            // ����ָ���� Layout
            Layout targetLayout = data.Layouts.Find(layouts => layouts.name == layoutname);
            if (targetLayout == null)
            {
                Debug.LogError("û���ҵ�Ŀ�겼�֣������ṩ�Ĳ��������Ƿ���ȷ");
                return;
            }
            foreach (Room room in targetLayout.rooms)
            {
                roomList.Add(room);
            }

            //!!!!!!!!!!!�����ã������������Ƿ����roomlist
            /*foreach (Room room in roomList)
            {
                print("!!!!!!!!!!!!!!!!!!!!!");
                Debug.Log("��������Ϊ��"+room.roomName);
                Debug.Log("�������½�����Ϊ��"+room.xzPosition);
                Debug.Log("���䳤��ֱ�Ϊ��"+room.width+","+room.height);
            }*/
            //!!!!!!!!!!!�����ã������������Ƿ����roomlist
        }

        catch (System.Exception e)
        {
            Debug.LogError($"����ʧ��: {e.Message}\nJSON ����:\n{jsonFile.text}");
        }


    }
    public Room[][] LoadDoorDataFromJson(string filename, string layoutname)
    {
        //��һ����û����ģ��Ѿ������ˣ���Ӧ�þ���������㷨����������
        /* Debug.Log("�����ŵ����� ");*/
        // 1. ���ز�����JSON�ļ�
        TextAsset jsonFile = Resources.Load<TextAsset>(filename); //����json���ݵ�jsonFile����
        if (jsonFile == null)
        {
            Debug.LogError("JSON �ļ�δ�ҵ������飺\n" +
                         "- �ļ��Ƿ��� Assets/Resources ��\n" +
                         "- �ļ����Ƿ���ȫƥ�䣨������Сд��\n" +
                         "- �ļ���չ���Ƿ�Ϊ .json");
            return null;
        }
        LayoutList data = JsonConvert.DeserializeObject<LayoutList>(jsonFile.text);
        if (data == null || data.Layouts == null)
        {
            Debug.LogError("JSON����ʧ�ܣ������ʽ�Ƿ���ȷ");
            return null;
        }

        // 2. ����Ŀ�겼��
        Layout targetLayout = data.Layouts.Find(layouts => layouts.name == layoutname);
        if (targetLayout == null)
        {
            Debug.LogError("û���ҵ�Ŀ�겼�֣������ṩ�Ĳ��������Ƿ���ȷ");
            return null;
        }

        // 3. ������������Room������ֵ䡣
        // �ֵ��ʹ�ã���֪ʶ����������������Ҳ������֪ʶ�����������java�����mapӳ��
        Dictionary<string, Room> nameToRoom = new Dictionary<string, Room>();
        foreach (Room room in roomList)
        {
            nameToRoom[room.roomName] = room;
            //print("���ڽ������ֵ�����ֵ䣺"+room.roomName+"��Ӧ��ʵ����"+room);
        }

        // 4. ��ʼ����ά��������
        Room[][] connection = new Room[roomList.Count][];
        for (int i = 0; i < roomList.Count; i++)//��������ڽӷ���
        {
            Room currentRoom = roomList[i];

            if (currentRoom.ConnectedRoom == null || currentRoom.ConnectedRoom.Length == 0)// ���ڽӷ��䣬һ�㲻�������������������������Ǹ�������
            {
                connection[i] = new Room[0]; // ���ڽӷ���
                continue;
            }

            // 5. ת���ڽӷ������б�ΪRoom��������
            List<Room> connectedRooms = new List<Room>();
            foreach (string connectedName in currentRoom.ConnectedRoom)
            {
                //��ǰ�ķ�����
                if (nameToRoom.TryGetValue(connectedName, out Room connectedRoom))
                {
                    // Debug.Log("��ǰ�����ǣ�"+currentRoom.roomName+",���ڽӷ���Ϊ"+connectedRoom.roomName);
                    connectedRooms.Add(connectedRoom);
                }
                else
                {
                    Debug.LogWarning($"���� '{currentRoom.roomName}' ���ڽӷ��� '{connectedName}' ������");
                }
            }

            connection[i] = connectedRooms.ToArray();
        }

        return connection;
    }
    private void CreateDoorBetweenRooms(Room[][] cN)
    {
        float distanceThreshold = 0.1f;

        for (int i = 0; i < cN.Length; i++)
        {
            Room currentRoom = roomList[i];

            foreach (Room connectedRoom in cN[i])
            {
                // ����ҷ�����
                if (Mathf.Abs(currentRoom.xzPosition.x + currentRoom.width - connectedRoom.xzPosition.x) < distanceThreshold)
                {
                    TryCreateHorizontalDoor(currentRoom, connectedRoom, true);
                }
                // ���������
                else if (Mathf.Abs(currentRoom.xzPosition.x - (connectedRoom.xzPosition.x + connectedRoom.width)) < distanceThreshold)
                {
                    TryCreateHorizontalDoor(currentRoom, connectedRoom, false);
                }
                // ����Ϸ�����
                else if (Mathf.Abs(currentRoom.xzPosition.z + currentRoom.height - connectedRoom.xzPosition.z) < distanceThreshold)
                {
                    TryCreateVerticalDoor(currentRoom, connectedRoom, true);
                }
                // ����·�����
                else if (Mathf.Abs(currentRoom.xzPosition.z - (connectedRoom.xzPosition.z + connectedRoom.height)) < distanceThreshold)
                {
                    TryCreateVerticalDoor(currentRoom, connectedRoom, false);
                }
            }
        }
    }

    private void TryCreateHorizontalDoor(Room current, Room connected, bool isRight)
    {
        float overlapStart = Mathf.Max(current.xzPosition.z, connected.xzPosition.z);
        float overlapEnd = Mathf.Min(current.xzPosition.z + current.height, connected.xzPosition.z + connected.height);
        float overlap = overlapEnd - overlapStart;

        if (overlap >= 1f)
        { // ȷ�����㹻���ص��ռ�
            float doorX = isRight ? current.xzPosition.x + current.width : current.xzPosition.x;
            float doorZ = overlapStart + overlap / 2;

            Vector3 doorPosition = new Vector3(doorX, y / 2, doorZ);
            CreateDoor(doorPosition, doorWidth, true, "Door");
        }
    }

    private void TryCreateVerticalDoor(Room current, Room connected, bool isTop)
    {
        float overlapStart = Mathf.Max(current.xzPosition.x, connected.xzPosition.x);
        float overlapEnd = Mathf.Min(current.xzPosition.x + current.width, connected.xzPosition.x + connected.width);
        float overlap = overlapEnd - overlapStart;

        if (overlap >= 1f)
        { // ȷ�����㹻���ص��ռ�
            float doorZ = isTop ? current.xzPosition.z + current.height : current.xzPosition.z;
            float doorX = overlapStart + overlap / 2;

            Vector3 doorPosition = new Vector3(doorX, y / 2, doorZ);
            CreateDoor(doorPosition, doorWidth, false, "Door");
        }
    }


    private void AddExitDoors(string filename, string layoutname)
    {
        Room EscapeRoom = null;

        TextAsset jsonFile = Resources.Load<TextAsset>(filename); //����json���ݵ�jsonFile����
        if (jsonFile == null)
        {
            Debug.LogError("JSON �ļ�δ�ҵ������飺\n" +
                         "- �ļ��Ƿ��� Assets/Resources ��\n" +
                         "- �ļ����Ƿ���ȫƥ�䣨������Сд��\n" +
                         "- �ļ���չ���Ƿ�Ϊ .json");
            return;
        }
        // Debug.Log("ԭʼJSON����:\n" + jsonFile.text);
        // 2. ʹ�� Newtonsoft.Json ��������ȡjson�ļ���һ��û���⡣��Ӧ�þ��ǽ�����ʱ�������
        try
        {
            LayoutList data = JsonConvert.DeserializeObject<LayoutList>(jsonFile.text);
            if (data == null || data.Layouts == null)
            {
                Debug.LogError("JSON����ʧ�ܣ������ʽ�Ƿ���ȷ");
                return;
            }

            // ����ָ���� Layout
            Layout targetLayout = data.Layouts.Find(layouts => layouts.name == layoutname);
            if (targetLayout == null)
            {
                Debug.LogError("û���ҵ�Ŀ�겼�֣������ṩ�Ĳ��������Ƿ���ȷ");
                return;
            }
            //Debug.Log(targetLayout.ExitRoom);
            //Debug.Log(targetLayout.ExitDoorPosition);

            // ����Ŀ����������
            foreach (Room room in roomList)
            {
                if (room.roomName == targetLayout.ExitRoom)
                {
                    EscapeRoom = room;
                    break;
                }
            }
            //�������ڸ÷����λ�ã�right left forward backward

            Vector3 DoorPosition = GetDoorPosition(EscapeRoom, targetLayout.ExitDoorPosition);
            string DoorPOS = targetLayout.ExitDoorPosition;

            if (DoorPOS == "right")
            {
                CreateDoor(DoorPosition, 0.1f, true, "Exit");
            }

            else if (DoorPOS == "left")
            {
                CreateDoor(DoorPosition, 0.1f, true, "Exit");
            }
            else if (DoorPOS == "forward")
            {
                CreateDoor(DoorPosition, 0.1f, false, "Exit");
            }
            else if (DoorPOS == "backward")
            {
                CreateDoor(DoorPosition, 0.1f, false, "Exit");
            }
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"����ʧ��: {e.Message}\nJSON ����:\n{jsonFile.text}");
            return;
        }
    }

    private Vector3 GetDoorPosition(Room escapeRoom, string doorPosition)
    {
        Vector3 doorPos = new Vector3();

        if (doorPosition == "right")
        {
            doorPos = new Vector3(
                escapeRoom.xzPosition.x + escapeRoom.width,  // �Ҳ�ǽ��Xλ��
                y / 2,                                      // �Ÿ߶ȣ�Yλ�ã�
                escapeRoom.xzPosition.z + escapeRoom.height / 2  // ��ǽ���м�λ��
            );
        }
        else if (doorPosition == "left")
        {
            doorPos = new Vector3(
                escapeRoom.xzPosition.x,                    // ���ǽ��Xλ��
                y / 2,
                escapeRoom.xzPosition.z + escapeRoom.height / 2
            );
        }
        else if (doorPosition == "forward")
        {
            doorPos = new Vector3(
                escapeRoom.xzPosition.x + escapeRoom.width / 2,  // ��ǽ���м�λ��
                y / 2,
                escapeRoom.xzPosition.z + escapeRoom.height      // ǰ��ǽ��Zλ��
            );
        }
        else if (doorPosition == "backward")
        {
            doorPos = new Vector3(
                escapeRoom.xzPosition.x + escapeRoom.width / 2,
                y / 2,
                escapeRoom.xzPosition.z                        // ���ǽ��Zλ��
            );
        }
        else
        {
            Debug.LogError("Unknown door position: " + doorPosition);
        }

        return doorPos;
    }

    private void GetMaxXZofLayout(List<Room> roomList)
    {
        float maxX = 0f;
        float maxZ = 0f;
        foreach (Room room in roomList)
        {
            // ���㷿������Ͻ����꣨���½�xzPosition + width/height��
            float roomMaxX = room.xzPosition.x + room.width;
            float roomMaxZ = room.xzPosition.z + room.height;

            // ����ȫ�����ֵ
            maxX = Mathf.Max(maxX, roomMaxX);
            maxZ = Mathf.Max(maxZ, roomMaxZ);
        }
        totalWidth = maxX;
        totalHeight = maxZ;
    }

}

