using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;

public class ComplexityControl : MonoBehaviour
{
    // Start is called before the first frame update
    public float MINROOMAREA = 5f; //��С�ķ������
    public BuildingControl buildingGeneration;  // ���� BuildingGeneratiion �ű�
    public NavMeshSurface surface;//���ɵ��������
    public GameObject HumanList;
    public GameObject HumanPrefab;
   /* void Update()
    {
       
        // ÿ֡���¼�ʱ��
        timer += Time.deltaTime;

        // �����ʱ���ﵽ 1 ��
        if (timer >=interval)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            // ���ü�ʱ��
            timer = 0f;

            // ��������������� BeginGeneration
            float number1 = 900; // ������������
            int number2 = UnityEngine.Random.Range(5, 21); // ���ֵķ�������

            BeginGeneration(number1, number2);//���ɻ�����

            surface.BuildNavMesh();//���ɵ���

            SetHuman();//��������

        }
    }*/

    public void BeginGenerationFangTree(float number1, int number2) 
    {
            if (number2 > 0)
            {
                float[] result = DivideNumberRandomly(number1, number2);

            //���Դ���
            /*// ������
            foreach (float part in result)
            {
                Debug.Log("����������ɣ�������������С�Ľ���� " + part);
            }*/

            // �����ֽ�����ݸ� BuildingGeneratiion

            buildingGeneration.ClearPreviousRooms();
            buildingGeneration.roomAreas = result; // ���÷����������
            buildingGeneration.GenerateRoomsFang();  // ���÷������ɷ���
            }
            else
            {
                Debug.LogError("���ֵ���������>=1");
            }
    }


    public void BeginGenerationBinary(float number1, int number2)
    {

       // Debug.Log("BeginGenerationBinary");
        if (number2 > 0)
        {
            buildingGeneration.ClearPreviousRooms();
            buildingGeneration.GenerateRoomsBinary(number1,number2);  // ���÷������ɷ���
        }
        else
        {
            Debug.LogError("���ֵ���������>=1");
        }
    }

    public void BeginGenerationJsonLoad(string filename,string layoutname)
    {
        // Debug.Log("BeginGenerationJson");
        
            buildingGeneration.ClearPreviousRooms();
            buildingGeneration.GenerateRoomsJsonLoad( filename,layoutname);  // ���÷������ɷ���
        
    }
    // ����� number1 ����Ϊ number2 ������
    //����ƽ��������ԣ�ÿ������Ļ������ baseArea �����ȼ���Ϊ��������Է���������Ȼ���ڴ˻�����Ӧ������ĵ���ֵ��ȷ���������ƽ�⡣
    //������Χ����������Χ maxAdjustment ������Ϊʣ�������һ�루remaining / 2f�����������Ա��⼫�˵�����仯������ͨ�� rand.NextDouble() ���Ʋ����ķ���
    public float[] DivideNumberRandomly(float number1, int number2)
    {
        if (number2 <= 0)
        {
            throw new ArgumentException("Number of parts must be greater than 0", nameof(number2));
        }

        float[] result = new float[number2];

        // �����������
        System.Random rand = new System.Random();

        // ����ÿ������ĳ�ʼƽ�����
        float baseArea = number1 / number2;

        // ����ʣ������
        float remaining = number1;

        // ������ɷ������
        for (int i = 0; i < number2 - 1; i++)
        {
            // �����������󲨶���Χ,������Χ�����ֵ�����ޣ�����Сֵû��
            //����������ֵ���������������ǰn-1������ȫ��ȡ���ƫ��ʱ�����һ������������Ȼ����0��
            //������Χ����Сֵ�������ݷ����������Сֵ�����㣬�涨������СӦ����5m2(�洢��)��������Χ��СֵӦ����ƽ�����-5
            //������ȡ���������ĸ���ͬ�������ŷ�����Ŀ���������С�����ǿ����ʵ���������仯������num2Խ��Խ�ӽ�ƽ������

            int T = Mathf.RoundToInt((number2 / 10));//����T���ʵ����󲨶���Χ
            if (T < 1) { T = 1; }
            float maxAdjustment = T * number1 / ((number2 - 1) * number2);
            float minAdjustment = baseArea - 5;

            // ������ɵ���ֵ�����Ƶ�������
            //rand.NextDouble()���ɵķ�Χ�ǣ�0,1��   rand.NextDouble()*2-1 �󽫷�Χ��Ϊ��-1,1��
            //����ÿ����������ƫ��ֵ
            float change = (float)(rand.NextDouble() * 2 - 1);
            float adjustment;//���ڼ�¼����Ĳ��������

            if (change >= 0) { adjustment = change * maxAdjustment; }
            else { adjustment = change * minAdjustment; }

            //Debug.Log("���������ƫ��Ϊ��" + adjustment);������
            // ����ÿ������������ȷ������Ϊ minValue
            float roomArea = baseArea + adjustment;

            result[i] = Mathf.Round(roomArea); // ��������Ϊ����

            remaining -= result[i]; // ����ʣ�����
        }

        // �������һ������������ȷ������������� number1���Ҵ���0
        result[number2 - 1] = remaining;
        return result;

    }

    //���λ������10�����࣬
    /*public void SetHuman()
    {
        //
        HumanList = GameObject.Find("HumanList");
        
        for (int i = 0; i < 10; i++)
        {
            float randomX = UnityEngine.Random.Range(1f, buildingGeneration.totalWidth);
            float randomZ = UnityEngine.Random.Range(1f, buildingGeneration.totalHeight);
            GameObject Person=  Instantiate(HumanPrefab,new Vector3(randomX,0.5f,randomZ), Quaternion.identity);
           Person.transform.parent = HumanList.transform;
        }
    }*/


}
