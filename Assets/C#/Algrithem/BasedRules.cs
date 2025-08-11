using System.Collections.Generic;
using UnityEngine;

public class BasedRules: MonoBehaviour
{
    public int gridSize = 10; // �����С
    public int roomCount = 5; // ��������
    public int doorCount = 4; // ������
    public int exitCount = 2; // ��������
    public GameObject roomPrefab; // ����Ԥ����
    public GameObject doorPrefab; // ��Ԥ����
    public GameObject wallPrefab; // ǽԤ����
    public GameObject exitPrefab; // ����Ԥ����

    private enum CellType { Room, Door, Wall, Exit }
    private CellType[,] grid; // ����

    void Start()
    {
        InitializeGrid();
        GenerateLayout();
        RenderGrid();
    }

    // ��ʼ������
    void InitializeGrid()
    {
        grid = new CellType[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = CellType.Wall; // ��ʼ��Ϊǽ
            }
        }
    }

    // ���ɲ���
    void GenerateLayout()
    {
        // ���÷���
        for (int i = 0; i < roomCount; i++)
        {
            Vector2Int roomPos = GetRandomPosition();
            grid[roomPos.x, roomPos.y] = CellType.Room;
        }

        // ������
        for (int i = 0; i < doorCount; i++)
        {
            Vector2Int doorPos = GetRandomPosition();
            if (IsAdjacentToRoom(doorPos))
            {
                grid[doorPos.x, doorPos.y] = CellType.Door;
            }
        }

        // ���ó���
        for (int i = 0; i < exitCount; i++)
        {
            Vector2Int exitPos = GetRandomEdgePosition();
            grid[exitPos.x, exitPos.y] = CellType.Exit;
        }
    }

    // ��ȡ���λ��
    Vector2Int GetRandomPosition()
    {
        int x = Random.Range(0, gridSize);
        int y = Random.Range(0, gridSize);
        return new Vector2Int(x, y);
    }

    // ��ȡ�����Եλ��
    Vector2Int GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        int x = 0, y = 0;
        switch (edge)
        {
            case 0: x = 0; y = Random.Range(0, gridSize); break; // ���Ե
            case 1: x = gridSize - 1; y = Random.Range(0, gridSize); break; // �ұ�Ե
            case 2: x = Random.Range(0, gridSize); y = 0; break; // �±�Ե
            case 3: x = Random.Range(0, gridSize); y = gridSize - 1; break; // �ϱ�Ե
        }
        return new Vector2Int(x, y);
    }

    // ����Ƿ��뷿������
    bool IsAdjacentToRoom(Vector2Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        if (x > 0 && grid[x - 1, y] == CellType.Room) return true;
        if (x < gridSize - 1 && grid[x + 1, y] == CellType.Room) return true;
        if (y > 0 && grid[x, y - 1] == CellType.Room) return true;
        if (y < gridSize - 1 && grid[x, y + 1] == CellType.Room) return true;
        return false;
    }

    // ��Ⱦ����
    void RenderGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                switch (grid[x, y])
                {
                    case CellType.Room:
                        Instantiate(roomPrefab, position, Quaternion.identity);
                        break;
                    case CellType.Door:
                        Instantiate(doorPrefab, position, Quaternion.identity);
                        break;
                    case CellType.Wall:
                        Instantiate(wallPrefab, position, Quaternion.identity);
                        break;
                    case CellType.Exit:
                        Instantiate(exitPrefab, position, Quaternion.identity);
                        break;
                }
            }
        }
    }
}