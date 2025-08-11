using System.Collections.Generic;
using UnityEngine;

public class Binary : MonoBehaviour
{
    public int n = 10; // �ָ�����
    public float minSize = 5f; // ��С���
    public float minWidth = 3f; // ��С���
    public float minHeight = 3f; // ��С�߶�
    public Vector2 startPoint = new Vector2(0, 0); // �������½����
    public float width = 100f; // ���ο��
    public float height = 100f; // ���θ߶�

    private List<Rect> rectangles = new List<Rect>();

    void Start()
    {
        // ���÷ָ��
        rectangles = SplitSpace(new Rect(startPoint.x, startPoint.y, width, height), n, minSize);
    }

    void OnDrawGizmos()
    {
        // �������о���
        Gizmos.color = Color.green;
        foreach (var rect in rectangles)
        {
            DrawRect(rect);
        }
    }

    // �ݹ�ָ��
    List<Rect> SplitSpace(Rect rect, int n, float minSize)
    {
        List<Rect> result = new List<Rect>();

        // �ݹ���ֹ����
        if (n == 1)
        {
            if (IsValidRectangle(rect, minSize))
            {
                result.Add(rect);
            }
            return result;
        }

        // ��鵱ǰ�����Ƿ���Ч
        if (!IsValidRectangle(rect, minSize))
        {
            return result;
        }

        // ѡ��ָ��
        bool splitVertical = Random.Range(0, 2) == 0; // ���ѡ��ֱ��ˮƽ�ָ�

        if (splitVertical)  //��ֱ�ָͼ�κܳ�
        {
            // ��ֱ�ָ���ѡ��ָ��
            float minSplit = +minWidth; // ��С�ָ���
            float maxSplit = rect.width - minWidth; // ���ָ���
            float splitWidth = Random.Range(minSplit, maxSplit); // ����ָ��

            Rect leftRect = new Rect(rect.x, rect.y, splitWidth, rect.height);
            Rect rightRect = new Rect(rect.x + splitWidth, rect.y, rect.width - splitWidth, rect.height);

            result.AddRange(SplitSpace(leftRect, n / 2, minSize));
            result.AddRange(SplitSpace(rightRect, n - n / 2, minSize));
        }
        else
        {
            // ˮƽ�ָ���ѡ��ָ��
            float minSplit = minHeight; // ��С�ָ�߶�
            float maxSplit = rect.height - minHeight; // ���ָ�߶�
            float splitHeight = Random.Range(minSplit, maxSplit); // ����ָ��

            Rect bottomRect = new Rect(rect.x, rect.y, rect.width, splitHeight);
            Rect topRect = new Rect(rect.x, rect.y + splitHeight, rect.width, rect.height - splitHeight);

            result.AddRange(SplitSpace(bottomRect, n / 2, minSize));
            result.AddRange(SplitSpace(topRect, n - n / 2, minSize));
        }

        return result;
    }

    // �������Ƿ���������
    bool IsValidRectangle(Rect rect, float minSize)
    {
        float area = rect.width * rect.height;
        float aspectRatio = rect.width / rect.height;

        return area >= minSize &&
               rect.width >= minWidth &&
               rect.height >= minHeight &&
               aspectRatio >= 0.5f && aspectRatio <= 2f;
    }

    // ���ƾ���
    void DrawRect(Rect rect)
    {
        Vector3 bottomLeft = new Vector3(rect.x, rect.y, 0);
        Vector3 topLeft = new Vector3(rect.x, rect.y + rect.height, 0);
        Vector3 topRight = new Vector3(rect.x + rect.width, rect.y + rect.height, 0);
        Vector3 bottomRight = new Vector3(rect.x + rect.width, rect.y, 0);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
}