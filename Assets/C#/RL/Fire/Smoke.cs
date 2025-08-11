using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Smoke : MonoBehaviour
{
    public float initialRadius = 1f;  // ��ʼ�뾶
    public float growthSpeed = 0.5f;  // �뾶�����ٶȣ���λ/�룩
    public int segments = 64;         // Բ�ķֶ�����Խ��Խƽ����

    private Mesh mesh;
    private float currentRadius;

    void Start()
    {
        currentRadius = initialRadius;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateCircleMesh();
    }

    void Update()
    {
        // �뾶��ʱ������
        currentRadius += growthSpeed * Time.deltaTime;
        UpdateCircleMesh();
    }

    void UpdateCircleMesh()
    {
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        // ���ĵ�
        vertices[0] = Vector3.zero;

        // ����Բ���ϵĶ���
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i - 1) / (float)segments * 2 * Mathf.PI;
            vertices[i] = new Vector3(
                Mathf.Cos(angle) * currentRadius,
                0,
                Mathf.Sin(angle) * currentRadius
            );
        }

        // ����������
        for (int i = 0; i < segments; i++)
        {
            int startIndex = i * 3;
            triangles[startIndex] = 0;
            triangles[startIndex + 1] = i + 1;
            triangles[startIndex + 2] = (i + 1) % segments + 1;
        }

        // ����Mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}