using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public class BuildingGeneratiion : MonoBehaviour
{

    /*........................һ�����������õ������ݽṹ.....................................*/
    public float[] roomAreas;// ����ķ���������飨��֪���飩
    public int num = 1;//��¼�Ѿ����ɵķ�������,���ڸ�������  
    private float totalArea; // �������С
    public int totalWidth;//���ڼ�¼��������Ŀ�
    public int totalHeight;//���ڼ�¼��������ĸ�
    float y = 3.0f;//ǽ��ĸ߶� ,
    float doorWidth = 1.5f;//�ŵĿ��
    private GameObject[] generatedRooms; //�洢���ɵķ����������������ڶ�����ɻ���ʱ��ɾ��֮ǰ���ɵ�object

    public Material Floor ;
    public Material Door;
    public Material Exit;
    public Material Wall;
    /*.............................��������֮���������õ������ݽṹ................................*/
    public class Room
    {
        //Ϊ��������һЩ���ԣ����緿������ꡢ��С���Լ������ھ��б����ڼ�¼���ڷ��䣩��
        public GameObject roomObject;  // �������Ϸ����
        public Vector3 ZXposition;       // ��������½�����
        public float width;            // ����Ŀ��
        public float height;           // ����ĸ߶�

        public Room(GameObject roomObject, Vector3 position, float width, float height)
        {
            this.roomObject = roomObject;
            this.ZXposition = position;
            this.width = width;
            this.height = height;
        }
        // �������������һ�������Ƿ����ڣ����ڷ���λ�úʹ�С��
        public bool IsAdjacentTo(Room other)  //�жϸ÷����Ƿ�������һ����������,�������������ڵĳ��ȴ���Distanceʱ�����ǲ���Ϊ��������������
        {
            // ���跿���Ǿ��εģ����Ǽ���Ƿ���һ�����ڵ���
            float Distance = 2.0f; 
            bool isAdjacent = false;

            // ����������ڣ�����xΪˮƽ����yΪ��ֱ����zΪ��ȷ���
            if (Mathf.Abs(this.ZXposition.x + this.width - other.ZXposition.x) < 0.1f || Mathf.Abs(other.ZXposition.x + other.width - this.ZXposition.x) < 0.1f)
            {
                // ������������� x �᷽��������,��������������ڲ�����z�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                //����ɷ�Ϊ�������
                if (other.ZXposition.z <= this.ZXposition.z && other.ZXposition.z+other.height>=this.ZXposition.z && other.ZXposition.z+other.height-this.ZXposition.z >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if(other.ZXposition.z <= this.ZXposition.z+height && other.ZXposition.z + other.height >= this.ZXposition.z+height && this.ZXposition.z + this.height - other.ZXposition.z >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if(other.ZXposition.z >= this.ZXposition.z && other.ZXposition.z + other.height <= this.ZXposition.z + this.height && other.height >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                if (other.ZXposition.z <= ZXposition.z || other.ZXposition.z + other.height >= ZXposition.z + height || height >= 0.2)
                {

                }
                else { return isAdjacent; }

            }
            // �����������
            else if (Mathf.Abs(this.ZXposition.z + this.height - other.ZXposition.z) < 0.1f || Mathf.Abs(other.ZXposition.z + other.height - this.ZXposition.z) < 0.1f)
            {
                // ������������� z �᷽�������ڣ���������������ڲ�����x�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                if (other.ZXposition.x<= this.ZXposition.x && other.ZXposition.x+other.width>=this.ZXposition.x && other.ZXposition.x + other.width-this.ZXposition.x>=Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if (other.ZXposition.x <= this.ZXposition.x+this.width && other.ZXposition.x + other.width >= this.ZXposition.x+this.width && ZXposition.x + width - other.ZXposition.x >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if (other.ZXposition.x >= this.ZXposition.x && other.ZXposition.x + other.width <= this.ZXposition.x+width && other.width  >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if (other.ZXposition.x <= ZXposition.x && other.ZXposition.x + other.width >= ZXposition.x + width && width >= 0.2)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }

                else { return isAdjacent; }
            }
            return isAdjacent;
        }

    }
    private List<Room> roomList = new List<Room>();  // �洢���ɵ����з������

    /*.....................................һ������UI�������������ͷ�����Ŀ��Ȼ����з���İڷźͷ���ǽ�ڵ�����.......................................*/
    public void GenerateRooms()
    {
        /*foreach (float part in roomAreas)
        {
            Debug.Log("�����������ܵ������ݣ�" + part);
        }*/
        num = 1;  //���÷��������
        ClearPreviousRooms(); // ����ϴ����ɵķ���
        // ���������
        totalArea = 0f;
        foreach (var area in roomAreas)
        {
            totalArea += area;
        }

        // �ҳ���ӽ������εĳ������
        FindBestDimensions(totalArea);
        // ����������������
        /* Debug.Log("Total Area: " + totalArea);
         Debug.Log("Total Width: " + totalWidth);
         Debug.Log("Total Height: " + totalHeight);*/

        // ʹ�÷�����ͼ���ɷ���
        CreateRoomRects(roomAreas, 0, roomAreas.Length, 0, 0, totalWidth, totalHeight, totalArea, (totalHeight / (float)totalWidth) > 1);
        // ��������֮����ţ�ȷ��������ͨ
        Room [][]CN= GenerateCN(roomList.ToArray());
        //CreateDoorBetweenRooms();
        CreateDoorBetweenRooms(roomList.ToArray(), CN); //������ͨͼCN������
        //�����һ��������������ǽ����������������Ϊ��������
        AddExitDoors(roomList[roomList.Count - 1]);
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
    void CreateRoomRects(float[] areas, int start, int end, float x, float z, float width, float height, float totalArea, bool isHorizontal)
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
            AddRoomToList(room);
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
                AddRoomToList(room);
                currentX += roomWidth; // ������һ�������y����
            }
            //�ݹ������ϰ벿��
            CreateRoomRects(areas, splitIndex + 1, end, x, z + splitHeight, width, height - splitHeight, totalArea - splitArea, !isHorizontal);
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
                AddRoomToList(room);
                currentZ += roomHeight; // ������һ�������x����
            }
            //�ݹ������Ұ벿��
            CreateRoomRects(areas, splitIndex + 1, end, x + splitWidth, z, width - splitWidth, height, totalArea - splitArea, !isHorizontal);
        }
    }
    public GameObject CreateRoom(float x, float z, float width, float height)
    {
        // �������������
        GameObject room = new GameObject("Room" + num);
        num++;

        // ���ɷ���ĵײ��������Ҫ�Ļ���������ӵײ�����Ϊ������棩
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "floor";
        floor.transform.parent = room.transform;
        floor.transform.position = new Vector3(x + width / 2, 0f, z + height / 2);
        floor.transform.localScale = new Vector3(width, 0.1f, height); // �ײ���Ⱥ͸߶�

        // ������ײ���ɫ Assets/Material/Floor.mat
        floor.GetComponent<Renderer>().material = Floor;

        AddRoomToList(floor); // ��������뵽�����б���
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
        leftWall.name = "leftWall";
        leftWall.transform.parent = room.transform;
        leftWall.transform.position = new Vector3(x + wallThickness / 2, y / 2, z + height / 2);
        leftWall.transform.localScale = new Vector3(wallThickness, y, height);
        leftWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ


        // 2. ��ǽ
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "rightWall";
        rightWall.transform.parent = room.transform;
        rightWall.transform.position = new Vector3(x + width - wallThickness / 2, y / 2, z + height / 2);
        rightWall.transform.localScale = new Vector3(wallThickness, y, height);
        rightWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ

        // 3. ��ǽ
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.name = "backWall";
        frontWall.transform.parent = room.transform;
        frontWall.transform.position = new Vector3(x + width / 2, y / 2, z + wallThickness / 2);
        frontWall.transform.localScale = new Vector3(width, y, wallThickness);
        frontWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ

        // 4. ǰǽ
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "frontWall";
        backWall.transform.parent = room.transform;
        backWall.transform.position = new Vector3(x + width / 2, y / 2, z + height - wallThickness / 2);
        backWall.transform.localScale = new Vector3(width, y, wallThickness);
        backWall.GetComponent<Renderer>().material = Wall; // ǽ����ɫ

    }
    public void AddRoomToList(GameObject room)// �����ɵķ�����ӵ������б�
    {
        // �������ɵķ�����뵽�����б���
        Array.Resize(ref generatedRooms, generatedRooms.Length + 1);
        generatedRooms[generatedRooms.Length - 1] = room;
    }
    void ClearPreviousRooms()// ����ϴ����ɵķ���
        {
            // ��� AllObjects �Ƿ�Ϊ null������ǣ���ʼ��Ϊһ��������
            if (generatedRooms == null)
            {
                generatedRooms = new GameObject[0];
            }

            // ���������Ѿ����ɵķ����������
            foreach (var room in generatedRooms)
            {
                if (room != null)
                {
                    Destroy(room);
                }
            }

            // ��շ����б�
            generatedRooms = new GameObject[0];
        }
  

    /*.......................................���������ɵķ���֮��������.....................................*/
    Room[][] GenerateCN(Room [] rooms) { //������ͨͼ
        Room[][] connection = new Room[rooms.Length][];
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

                    Debug.Log(rooms[i].roomObject.name + "��" + rooms[j].roomObject.name+"����");//������
                }
            }
        }
        return connection;
    }

    void CreateDoorBetweenRooms(Room[] rooms, Room[][] CN) //������ͨͼCN������
        {

        for (int i = 0; i < rooms.Length; i++)
        {
            for (int j = i; j < rooms.Length; j++)
            {
                if (CN[i][j] != null)//˵�������������ڽӵģ������ж������������ϡ��¡������������ڷ�ʽ�����ݲ�ͬ�����ڷ�ʽ���в�ͬ�Ĵ���
                {
                    Debug.Log(rooms[i].roomObject.name + "��" + rooms[j].roomObject.name + "����");//������
                    Vector3 DoorPosition;
                    // �ҷ�����
                    if (Mathf.Abs(rooms[i].ZXposition.x + rooms[i].width - rooms[j].ZXposition.x) < 0.1f)
                    {
                       // Debug.Log("�ڸ÷����ҷ�����");//������
                        // ������������� x �᷽��������,��������������ڲ�����z�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                        //����ɷ�Ϊ�������
                        if (rooms[j].ZXposition.z < rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height > rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height - rooms[i].ZXposition.z >= 0.2)
                        {
                           // Debug.Log("�ڸ÷����ҷ����ڣ����1");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x + rooms[i].width, y / 2, (rooms[i].ZXposition.z + rooms[j].ZXposition.z + rooms[j].height) / 2);
                           /* DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                        }
                        if (rooms[j].ZXposition.z < rooms[i].ZXposition.z + rooms[i].height && rooms[j].ZXposition.z + rooms[j].height > rooms[i].ZXposition.z + rooms[i].height && rooms[i].ZXposition.z + rooms[i].height - rooms[j].ZXposition.z >= 0.2)
                        {
                           // Debug.Log("�ڸ÷����ҷ����ڣ����2");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x + rooms[i].width, y / 2, (rooms[j].ZXposition.z + rooms[i].ZXposition.z + rooms[i].height) / 2);
                           /* DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                        }
                        if (rooms[j].ZXposition.z >= rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height <= rooms[i].ZXposition.z + rooms[i].height && rooms[j].height >= 0.2)
                        {
                          //  Debug.Log("�ڸ÷����ҷ����ڣ����3");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x + rooms[i].width, y / 2, rooms[j].ZXposition.z + rooms[j].height / 2);
                           /* DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                        }
                        if (rooms[j].ZXposition.z <rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height > rooms[i].ZXposition.z + rooms[i].height && rooms[i].height >= 0.2)
                        {
                         //   Debug.Log("�ڸ÷����ҷ����ڣ����4");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x + rooms[i].width, y / 2, rooms[i].ZXposition.z + rooms[i].height);
                          /*  DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                        }

                    }
                    //������
                    if (Mathf.Abs(rooms[i].ZXposition.x - rooms[j].width - rooms[j].ZXposition.x) < 0.1f)
                    {
                        // ������������� x �᷽��������,��������������ڲ�����z�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                       // Debug.Log("�ڸ÷���������");//������
                        //����ɷ�Ϊ�������
                        if (rooms[j].ZXposition.z <= rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height >= rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height - rooms[i].ZXposition.z >= 0.2)
                        {
                          //  Debug.Log("�����ڵ�һ�����");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x, y / 2, (rooms[i].ZXposition.z + rooms[j].ZXposition.z + rooms[j].height) / 2);
                         /*   DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                        }
                        if (rooms[j].ZXposition.z <= rooms[i].ZXposition.z + rooms[i].height && rooms[j].ZXposition.z + rooms[j].height >= rooms[i].ZXposition.z + rooms[i].height && rooms[i].ZXposition.z + rooms[i].height - rooms[j].ZXposition.z >= 0.2)
                        {
                           // Debug.Log("�����ڵڶ������");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x, y / 2, (rooms[j].ZXposition.z + rooms[i].ZXposition.z + rooms[i].height) / 2);
                            /*DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                        }
                        if (rooms[j].ZXposition.z >= rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height <= rooms[i].ZXposition.z + rooms[i].height && rooms[j].height >= 0.2)
                        {
                          //  Debug.Log("�����ڵ��������");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x, y / 2, rooms[j].ZXposition.z + rooms[j].height / 2);
                          /*  DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");
                        }
                        if (rooms[j].ZXposition.z < rooms[i].ZXposition.z && rooms[j].ZXposition.z + rooms[j].height > rooms[i].ZXposition.z + rooms[i].height && rooms[i].height >= 0.2)
                        {
                          //  Debug.Log("�����ڵ��������");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x, y / 2, rooms[i].ZXposition.z + rooms[i].height);
                           /* DivideWall(rooms[i].roomObject.transform.Find("RightWall"), DoorPosition, "RightWall");
                            DivideWall(rooms[j].roomObject.transform.Find("leftWall"), DoorPosition, "leftWall");*/
                            CreateDoor(DoorPosition, doorWidth, true, "Door");

                        }
                    }
                    //�Ϸ�����
                    if (Mathf.Abs(rooms[i].ZXposition.z + rooms[i].height - rooms[j].ZXposition.z) < 0.1f)
                    {
                      //  Debug.Log("�ڸ÷����Ϸ�����");//������
                        // ������������� z �᷽�������ڣ���������������ڲ�����x�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                        if (rooms[j].ZXposition.x < rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width > rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width - rooms[i].ZXposition.x >= 0.2)
                        {
                           // Debug.Log("�Ϸ����ڵ�һ�����");//������
                            DoorPosition = new Vector3((rooms[j].ZXposition.x + rooms[j].width + rooms[i].ZXposition.x) / 2, y / 2, rooms[i].ZXposition.z + rooms[i].height);
                         /*   DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                        }
                        else if (rooms[j].ZXposition.x < rooms[i].ZXposition.x + rooms[i].width && rooms[j].ZXposition.x + rooms[j].width > rooms[i].ZXposition.x + rooms[i].width && rooms[i].ZXposition.x + rooms[i].width - rooms[j].ZXposition.x >= 0.2)
                        {
                          // Debug.Log("�Ϸ����ڵڶ������");//������
                            DoorPosition = new Vector3((rooms[i].ZXposition.x + rooms[i].width + rooms[j].ZXposition.x) / 2, y / 2, rooms[i].ZXposition.z + rooms[i].height);
                            /*DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                        }
                        else if (rooms[j].ZXposition.x >= rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width <= rooms[i].ZXposition.x + rooms[i].width && rooms[j].width >= 0.2)
                        {
                           // Debug.Log("�Ϸ����ڵ��������");//������
                            DoorPosition = new Vector3(rooms[j].ZXposition.x + rooms[j].width / 2, y / 2, rooms[i].ZXposition.z + rooms[i].height);
                          /*  DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                        }
                        else if (rooms[j].ZXposition.x < rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width > rooms[i].ZXposition.x + rooms[i].width && rooms[i].width >= 0.2)
                        {
                           // Debug.Log("�Ϸ����ڵ��������");//������
                            DoorPosition = new Vector3(rooms[i].ZXposition.x + rooms[i].width / 2, y / 2, rooms[i].ZXposition.z + rooms[i].height);
                          /*  DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                        }
                    }
                    //�·�����
                    if (Mathf.Abs(rooms[i].ZXposition.z - rooms[j].height - rooms[j].ZXposition.z) < 0.1f)
                    {
                        // ������������� z �᷽�������ڣ���������������ڲ�����x�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                       // Debug.Log("�ڸ÷����·�����");//������
                        if (rooms[j].ZXposition.x < rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width > rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width - rooms[i].ZXposition.x >= 0.2)
                        {
                          //  Debug.Log("�ڸ÷����·����ڣ����1");//������
                            DoorPosition = new Vector3((rooms[j].ZXposition.x + rooms[j].width + rooms[i].ZXposition.x) / 2, y / 2, rooms[i].ZXposition.z);
                           /* DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                        }
                        else if (rooms[j].ZXposition.x < rooms[i].ZXposition.x + rooms[i].width && rooms[j].ZXposition.x + rooms[j].width > rooms[i].ZXposition.x + rooms[i].width && rooms[i].ZXposition.x + rooms[i].width - rooms[j].ZXposition.x >= 0.2)
                        {
                           // Debug.Log("�ڸ÷����·����ڣ����2");//������ 
                            DoorPosition = new Vector3((rooms[j].ZXposition.x + rooms[j].width + rooms[i].ZXposition.x) / 2, y / 2, rooms[i].ZXposition.z);
                           /* DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");

                        }
                        else if (rooms[j].ZXposition.x >= rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width <= rooms[i].ZXposition.x + rooms[i].width && rooms[j].width >= 0.2)
                        {
                           // Debug.Log("�ڸ÷����·����ڣ����3");//������
                            DoorPosition = new Vector3((rooms[j].ZXposition.x + rooms[j].width + rooms[i].ZXposition.x) / 2, y / 2, rooms[i].ZXposition.z);
                           /* DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
                        }
                        else if (rooms[j].ZXposition.x < rooms[i].ZXposition.x && rooms[j].ZXposition.x + rooms[j].width > rooms[i].ZXposition.x + rooms[i].width && rooms[i].width >= 0.2)
                        {
                          //  Debug.Log("�ڸ÷����·����ڣ����4");//������
                            DoorPosition = new Vector3((rooms[j].ZXposition.x + rooms[j].width + rooms[i].ZXposition.x) / 2, y / 2, rooms[i].ZXposition.z);
                            /*DivideWall(rooms[i].roomObject.transform.Find("frontWall"), DoorPosition, "frontWall");
                            DivideWall(rooms[j].roomObject.transform.Find("backWall"), DoorPosition, "backWall");*/
                            CreateDoor(DoorPosition, doorWidth, false, "Door");
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
            Vector3 RightDoorPosition = new Vector3(EscapeRoom.ZXposition.x + EscapeRoom.width, wallHeight / 2, EscapeRoom.ZXposition.z+ EscapeRoom.height / 2);  
            // �Ϸ��ŵ�λ��
            Vector3 FrontDoorPosition = new Vector3(EscapeRoom.ZXposition.x + EscapeRoom.width/2, wallHeight / 2, EscapeRoom.ZXposition.z + EscapeRoom.height);

        // �����ҷ���������
        DivideWall(EscapeRoom.roomObject.transform.Find("rightWall"), RightDoorPosition, "rightWall");
        CreateDoor(RightDoorPosition, 0.1f, true, "Exit");
        //�����Ϸ���������
        DivideWall(EscapeRoom.roomObject.transform.Find("frontWall"), FrontDoorPosition, "frontWall");
        CreateDoor(FrontDoorPosition, 0.1f, false, "Exit");
       
        //Transform child = transform.Find("ChildName");
    }
    void CreateDoor(Vector3 position, float width,  bool isHorizontal, String Tag)
        {//���������� �ŵ�λ�ã��ŵĿ�ȣ�����ΰڷţ��ŵı�ǩ���ŵĸ߶�Ĭ����ȫ�ֱ����е� y ֵ
            // ����һ���µ������壨�ţ�������������Ϊ������
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.GetComponent<BoxCollider>().isTrigger = true;
           // ������ӵ������б���
          AddRoomToList(door);
        // �����ŵ����ƺͱ�ǩ
        door.name = Tag;
            door.tag = Tag;

            // �����ŵ�λ��
            door.transform.position =new Vector3(position.x,position.y,position.z);

            // �ж��Ƿ��Ǻ�����
            if (isHorizontal)
            {
                // ����Ǻ����ţ������ŵĴ�С
                // 0.3f ���ŵĺ�ȣ�y-1 ���ŵĸ߶ȣ�1f ���ŵĿ��
                door.transform.localScale = new Vector3(0.4f,3 , doorWidth);

               // �����ŵ���ɫ
               if (Tag == "Exit"){
                door.GetComponent<Renderer>().material= Exit;  // ��ɫ
               }
               else{
                door.GetComponent<Renderer>().material = Door;  // ��ɫ
               }
            }
            else
            {
                // ����������ţ������ŵĴ�С
                // 1f ���ŵĿ�ȣ�y-1 ���ŵĸ߶ȣ�0.3f ���ŵĺ��
                door.transform.localScale = new Vector3(doorWidth, 3, 0.4f);

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

           
        }
    void DivideWall(Transform wall,Vector3 DoorPosition, String WallName)
    { //����ǽ�ڣ���ǽ�ڷ�Ϊ�����֣����ŵĲ�λ������������ǽ�ڵ�������������λ���
        // ����ǽ�����ƻ��ֲ�ͬ��ǽ
        if (WallName == "leftWall" || WallName == "rightWall")
        {
            // �����ϰ벿��ǽ��
            GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontWall.transform.parent = wall.transform.parent;
            frontWall.name = WallName;
            frontWall.transform.position = new Vector3(
                wall.position.x,
                wall.position.y,
               (wall.position.z + wall.localScale.z / 2 + DoorPosition.z + doorWidth / 2) / 2);
            frontWall.transform.localScale = new Vector3(wall.localScale.x, wall.localScale.y, (wall.localScale.z - doorWidth) / 2);

            // �����°벿��ǽ��
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.transform.parent = wall.transform.parent;
            backWall.name = WallName;
            backWall.transform.position = new Vector3(
                wall.position.x,
                wall.position.y,
               (DoorPosition.z - doorWidth / 2 + wall.position.z - wall.localScale.z / 2) / 2);
            backWall.transform.localScale = new Vector3(wall.localScale.x, wall.localScale.y, (wall.localScale.z - doorWidth) / 2);

            // ����ԭǽ
             wall.gameObject.SetActive(false);
        }
        else if (WallName == "frontWall" || WallName == "backWall")
        {
            // ������벿��ǽ��
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.transform.parent = wall.transform.parent;
            leftWall.name = WallName;
            //�е����깫ʽ
            leftWall.transform.position = new Vector3(
                (DoorPosition.x - doorWidth / 2 + wall.position.x - wall.localScale.x / 2) / 2,
                wall.position.y,
                wall.position.z);
            leftWall.transform.localScale = new Vector3((wall.localScale.x - doorWidth) / 2, wall.localScale.y, wall.localScale.z);

            // �����Ұ벿��ǽ��
            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.transform.parent = wall.transform.parent;
            rightWall.name = WallName;
            rightWall.transform.position = new Vector3(
                (wall.position.x + wall.localScale.x / 2 + DoorPosition.x + doorWidth / 2) / 2,
                wall.position.y,
                wall.position.z);
            rightWall.transform.localScale = new Vector3((wall.localScale.x - doorWidth) / 2, wall.localScale.y, wall.localScale.z);
            // ����ԭǽ
             wall.gameObject.SetActive(false);
        }
    }

        /*  //����������,��UImanage���洴����һ��BuildingGeneration�ֱ࣬�ӵ����˸����GenerateRoom����
          void Start()
          {
              // ����Ƿ��Ѿ�ͨ�� UImanager �����˷������
              if (roomAreas != null && roomAreas.Length > 0)
              {
                  GenerateRooms();
              }
              else
              {
                  Debug.LogError("�����������Ϊ�գ��޷����ɷ��䣡");
              }
          }*/
    }
