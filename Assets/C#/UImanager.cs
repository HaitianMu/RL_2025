using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    // Start is called before the first frame update
    // ������������
    public TMP_InputField inputField1;  //�����С
    public TMP_InputField inputField2;  //��������
    public float MINROOMAREA = 5f; //��С�ķ������

    // ���ð�ť���
    public Button submitButton;

    // ���ڴ洢���������
    private float number1; //������������������С��
    private int number2;  //���ֵķ�������һ��������

    public BuildingGeneratiion buildingGeneration;  // ���� BuildingGeneratiion �ű�

    // ��ť����¼�������
    public void OnSubmit() //����Ϊpublic���Զ����ɼ������ú����밴ť��onclick�������������ʾ�����ť�������������
    {
        // ���Խ���������ַ���Ϊ����
        if (float.TryParse(inputField1.text, out number1) && int.TryParse(inputField2.text, out number2))
        {
            // �����ȡ������
            Debug.Log("�����СΪ: " + number1);
            Debug.Log("��������Ϊ: " + number2);
            // �������������к����ļ������������
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
                buildingGeneration.roomAreas = result; // ���÷����������
                buildingGeneration.GenerateRooms();  // ���÷������ɷ���
            }
            else
            {
                Debug.LogError("���ֵ���������>=1");
            }
        }
        else
        {
            // �������ʧ�ܣ���ʾ�û�������Ч
            Debug.LogError("������Ч����������Ч�����֣�");
        }
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
            float maxAdjustment = T*number1 /((number2-1)*number2);
            float minAdjustment = baseArea-5;

            // ������ɵ���ֵ�����Ƶ�������
            //rand.NextDouble()���ɵķ�Χ�ǣ�0,1��   rand.NextDouble()*2-1 �󽫷�Χ��Ϊ��-1,1��
            //����ÿ����������ƫ��ֵ
            float change= (float)(rand.NextDouble()*2-1);
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
}
