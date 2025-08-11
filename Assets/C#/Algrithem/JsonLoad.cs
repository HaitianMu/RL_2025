using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using TMPro;

public class JsonLoad : MonoBehaviour
{
    public Material lineMaterial; // �������ʣ���Inspector��ָ����
    private Dictionary<string, RoomData> roomsDict = new Dictionary<string, RoomData>();

    [System.Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [System.Serializable]
    public class RoomData
    {
        public string id;
        public string name;
        public Vector3Serializable xzPosition;
        public float width;
        public float height;
        public float roomSize;
        public string[] connectedRooms;
    }

    [System.Serializable]
    public class Layout
    {
        public string name;
        public RoomData[] rooms;
    }

    [System.Serializable]
    public class LayoutCollection
    {
        public List<Layout> layouts;
    }

    void Start()
    {
        string[] names = { "layout1" , "layout2", "office_layout2", "supermarket_layout", "university_library", "apartment_complex", "hospital_floor", "SEU_LiWenzheng_Library_Floor1" }; // ����һ������Ϊ10���ַ�������

        LoadAndDrawRooms("layout_1");
    }

    private void LoadAndDrawRooms(string targetLayoutName)
    {
        // 1. ����JSON�ļ���·�������� StreamingAssets �� Resources��
        TextAsset jsonFile = Resources.Load<TextAsset>("Layout");
        if (jsonFile == null)
        {
            Debug.LogError("JSON �ļ�δ�ҵ������飺\n" +
                         "- �ļ��Ƿ��� Assets/Resources ��\n" +
                         "- �ļ����Ƿ���ȫƥ�䣨������Сд��\n" +
                         "- �ļ���չ���Ƿ�Ϊ .json");
            return;
        }

        // 2. ʹ�� Newtonsoft.Json ��������ȡjson�ļ���һ��û���⡣��Ӧ�þ��ǽ�����ʱ�������
        try
        {
            LayoutCollection data = JsonConvert.DeserializeObject<LayoutCollection>(jsonFile.text);
           // Debug.Log("ԭʼJSON����:\n" + jsonFile.text);
            if (data == null || data.layouts == null)
            {
                Debug.LogError("JSON����ʧ�ܣ������ʽ�Ƿ���ȷ");
                return;
            }


            // ����ָ���� Layout
            Layout targetLayout = data.layouts.Find(layout => layout.name == targetLayoutName);
            if (targetLayout == null )
            {
                Debug.LogError("û���ҵ�Ŀ�겼�֣������ṩ�Ĳ��������Ƿ���ȷ");
                return;
            }


            // �������ֵı߽���
                foreach (RoomData room in targetLayout.rooms)
                {
                    if (!roomsDict.ContainsKey(room.id))
                    {
                        roomsDict.Add(room.id, room);
                        DrawRoom(room);
                        DrawRoomLabel(room); // ������ǩ���ƺ���
                }

                    /*// ������Ϣ
                    Debug.Log($"����ID: {room.id}, ����: {room.name}");
                    Debug.Log($"λ��: X={room.xzPosition.x}, Z={room.xzPosition.z}");
                    Debug.Log($"�ߴ�: ��={room.width}, ��={room.height}");
                    Debug.Log($"���ӷ���: {string.Join(", ", room.connectedRooms)}");*/
                }
            }

        catch (System.Exception e)
        {
            Debug.LogError($"����ʧ��: {e.Message}\nJSON ����:\n{jsonFile.text}");
        }
    }


    private void DrawRoom(RoomData data)
    {
        Vector3[] corners = new Vector3[5] {
            new Vector3(data.xzPosition.x, 0, data.xzPosition.z),
            new Vector3(data.xzPosition.x + data.width, 0, data.xzPosition.z),
            new Vector3(data.xzPosition.x + data.width, 0, data.xzPosition.z + data.height),
            new Vector3(data.xzPosition.x, 0, data.xzPosition.z + data.height),
            new Vector3(data.xzPosition.x, 0, data.xzPosition.z) // �պ�
        };

        GameObject lineObj = new GameObject($"Room_{data.id}_Border");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;

        lr.startWidth = lr.endWidth = 0.3f;
        lr.positionCount = corners.Length;
        lr.SetPositions(corners);
    }

    private void DrawRoomLabel(RoomData room)
    {
        // �����ǩλ�ã��������ĵ㣩
        Vector3 labelPosition = new Vector3(
            room.xzPosition.x + room.width / 2f,
            0,
            room.xzPosition.z + room.height / 2f
        );

        // �����ı���ǩ
        GameObject labelObj = new GameObject($"Label_{room.id}");
       
        labelObj.transform.position = labelPosition;
        labelObj.transform.SetParent(this.transform); // ��Ϊ��ǰ�����������
                                                      // ǿ����תΪ���ϣ�Y�����ϣ�X/Z����㣩
        labelObj.transform.rotation = Quaternion.Euler(90, 0, 0); // X����ת90�ȣ�3D�ı�Ĭ�ϳ�ǰ��
        // ����ı����

        // ʹ��Ԥ������������ʲ�
        TMP_FontAsset chineseFont = Resources.Load<TMP_FontAsset>("Resources/KaiTi SDF");
        TextMeshPro labelText = labelObj.AddComponent<TextMeshPro>();
        labelText.font = chineseFont;
        labelText.text = room.id;
        labelText.fontSize = 15f;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        
        
    }
}