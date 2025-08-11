using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    public string doorDirection; // �ŵĳ���

    public void Start()
    {
        DetermineDoorDirection();
    }
    void DetermineDoorDirection()//��ʼ���ŵĳ�����
    {
        Vector3 myPosition = transform.position;
        float checkRadius = 0.5f; // ͳһ�ļ��뾶
        float checkDistance = 1.5f; // ������

        // ������
        bool hasLeftCollider = Physics.CheckSphere(myPosition + Vector3.left * checkDistance, checkRadius);
        // ����Ҳ�
        bool hasRightCollider = Physics.CheckSphere(myPosition + Vector3.right * checkDistance, checkRadius);

        // �ж��ŵĳ���
        if (hasLeftCollider && hasRightCollider)
        {
            doorDirection = "Horizontal";//��ֱ
        }
        else
        {
            doorDirection = "Vertical";//ˮƽ
        }

    }


}

