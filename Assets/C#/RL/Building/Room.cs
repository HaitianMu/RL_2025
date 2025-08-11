using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour//Ϊ��������һЩ���ԣ����緿������ꡢ��С���Լ������ھ��б����ڼ�¼���ڷ��䣩��
{
    public Vector3 xzPosition;          // Lower-left corner coordinates of the room
    public float width;                // Width of the room
    public float height;               // Height of the room
    /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Added on 3.12 */
    public float roomSize;            // Size of the room (area)
    /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Added on 4.4 */
    public String roomName;          // Name of the room
    public string[] ConnectedRoom;    // Names of adjacent rooms

    public Room() { }
    public Room(GameObject roomObject, Vector3 position, float width, float height)
    {
        this.xzPosition = position;
        this.width = width;
        this.height = height;
    }

    public Room(Vector3 position, float width, float height, float roomsize)
    {
        this.xzPosition = position;
        this.width = width;
        this.height = height;
        roomSize = roomsize;
    }
    // ���㷿��ĶԽ��߳��ȣ�Ȩ�أ�

    // �������������һ�������Ƿ����ڣ����ڷ���λ�úʹ�С��
    public bool IsAdjacentTo(Room other)  //�жϸ÷����Ƿ�������һ����������,�������������ڵĳ��ȴ���Distanceʱ�����ǲ���Ϊ��������������
    {
        // ���跿���Ǿ��εģ����Ǽ���Ƿ���һ�����ڵ���
        float Distance = 5.0f;
        bool isAdjacent = false;

        // ����������ڣ�����xΪˮƽ����yΪ��ֱ����zΪ��ȷ���
        if (Mathf.Abs(this.xzPosition.x + this.width - other.xzPosition.x) < 0.1f || Mathf.Abs(other.xzPosition.x + other.width - this.xzPosition.x) < 0.1f)
        {
            // ������������� x �᷽��������,��������������ڲ�����z�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
            //����ɷ�Ϊ�������
            if (other.xzPosition.z <= this.xzPosition.z && other.xzPosition.z + other.height >= this.xzPosition.z && other.xzPosition.z + other.height - this.xzPosition.z >= Distance)
            {
                isAdjacent = true;
                return isAdjacent;
            }
            else if (other.xzPosition.z <= this.xzPosition.z + height && other.xzPosition.z + other.height >= this.xzPosition.z + height && this.xzPosition.z + this.height - other.xzPosition.z >= Distance)
            {
                isAdjacent = true;
                return isAdjacent;
            }
            else if (other.xzPosition.z >= this.xzPosition.z && other.xzPosition.z + other.height <= this.xzPosition.z + this.height && other.height >= Distance)
            {
                isAdjacent = true;
                return isAdjacent;
            }
            if (other.xzPosition.z <= xzPosition.z || other.xzPosition.z + other.height >= xzPosition.z + height || height >= 0.2)
            {

            }
            else { return isAdjacent; }

        }
        // �����������
        else if (Mathf.Abs(this.xzPosition.z + this.height - other.xzPosition.z) < 0.1f || Mathf.Abs(other.xzPosition.z + other.height - this.xzPosition.z) < 0.1f)
        {
            // ������������� z �᷽�������ڣ���������������ڲ�����x�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
            if (other.xzPosition.x <= this.xzPosition.x && other.xzPosition.x + other.width >= this.xzPosition.x && other.xzPosition.x + other.width - this.xzPosition.x >= Distance)
            {
                isAdjacent = true;
                return isAdjacent;
            }
            else if (other.xzPosition.x <= this.xzPosition.x + this.width && other.xzPosition.x + other.width >= this.xzPosition.x + this.width && xzPosition.x + width - other.xzPosition.x >= Distance)
            {
                isAdjacent = true;
                return isAdjacent;
            }
            else if (other.xzPosition.x >= this.xzPosition.x && other.xzPosition.x + other.width <= this.xzPosition.x + width && other.width >= Distance)
            {
                isAdjacent = true;
                return isAdjacent;
            }
            else if (other.xzPosition.x <= xzPosition.x && other.xzPosition.x + other.width >= xzPosition.x + width && width >= 0.2)
            {
                isAdjacent = true;
                return isAdjacent;
            }

            else { return isAdjacent; }
        }
        return isAdjacent;
    }
}
