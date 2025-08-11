using UnityEngine;

public class WFC : MonoBehaviour
{
    public int gridSize = 20; // �����С
    public int maxRooms = 10;  // ��󷿼�����
    public int roomMinSize = 5;  // ������С�ߴ�
    public int roomMaxSize = 40;  // �������ߴ�
    public int maxDoors = 10;  // ���������
    public int maxExits = 1;  // �̶�һ������

    private enum CellType { Floor, Wall, Door, Exit, Collapsed }
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

        // ��ʼ����������Ϊ�ذ�
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                grid[x, y] = CellType.Floor;
            }
        }

        // ��������Ϊǽ
        for (int x = 0; x < gridSize; x++)
        {
            grid[x, 0] = CellType.Wall;
            grid[x, gridSize - 1] = CellType.Wall;
        }
        for (int y = 0; y < gridSize; y++)
        {
            grid[0, y] = CellType.Wall;
            grid[gridSize - 1, y] = CellType.Wall;
        }
    }

    // ���ɲ���
    void GenerateLayout()
    {
        // �̶�һ������
        Vector2Int exitPos = GetRandomEdgePosition();
        grid[exitPos.x, exitPos.y] = CellType.Exit;

        // ���÷��䣨�ذ壩��������ǽ�ͷָ�
        for (int i = 0; i < maxRooms; i++)
        {
            Vector2Int roomPos = GetRandomPosition();
            Vector2Int roomSize = new Vector2Int(Random.Range(roomMinSize, roomMaxSize), Random.Range(roomMinSize, roomMaxSize));
            if (CanPlaceRoom(roomPos, roomSize))
            {
                PlaceRoom(roomPos, roomSize);
            }
        }

        // �ڷ���֮������ǽ�ڣ��ָ����䣩
        GenerateWalls();

        // ������
        for (int i = 0; i < maxDoors; i++)
        {
            Vector2Int doorPos = GetRandomPosition();
            if (IsAdjacentToFloor(doorPos) && CanPlaceDoor(doorPos))
            {
                grid[doorPos.x, doorPos.y] = CellType.Door;
            }
        }
    }

    // ��ȡ���λ��
    Vector2Int GetRandomPosition()
    {
        int x = Random.Range(1, gridSize - 1); // �������������ܵ�ǽ��
        int y = Random.Range(1, gridSize - 1);
        return new Vector2Int(x, y);
    }

    // ��ȡ�����Եλ��
    Vector2Int GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        int x = 0, y = 0;
        switch (edge)
        {
            case 0: x = 0; y = Random.Range(1, gridSize - 1); break; // ���Ե
            case 1: x = gridSize - 1; y = Random.Range(1, gridSize - 1); break; // �ұ�Ե
            case 2: x = Random.Range(1, gridSize - 1); y = 0; break; // �±�Ե
            case 3: x = Random.Range(1, gridSize - 1); y = gridSize - 1; break; // �ϱ�Ե
        }
        return new Vector2Int(x, y);
    }

    // ����Ƿ��ܷ��÷���
    bool CanPlaceRoom(Vector2Int position, Vector2Int roomSize)
    {
        for (int x = position.x; x < position.x + roomSize.x; x++)
        {
            for (int y = position.y; y < position.y + roomSize.y; y++)
            {
                if (x >= gridSize || y >= gridSize || grid[x, y] != CellType.Floor)
                    return false;
            }
        }
        return true;
    }

    // ���÷���
    void PlaceRoom(Vector2Int position, Vector2Int roomSize)
    {
        // ��������������Ϊ�ذ�
        for (int x = position.x; x < position.x + roomSize.x; x++)
        {
            for (int y = position.y; y < position.y + roomSize.y; y++)
            {
                grid[x, y] = CellType.Floor;
            }
        }

        // ���÷�������Ϊǽ
        for (int x = position.x - 1; x < position.x + roomSize.x + 1; x++)
        {
            for (int y = position.y - 1; y < position.y + roomSize.y + 1; y++)
            {
                if (x >= 0 && y >= 0 && x < gridSize && y < gridSize)
                {
                    if (x == position.x - 1 || x == position.x + roomSize.x || y == position.y - 1 || y == position.y + roomSize.y)
                    {
                        grid[x, y] = CellType.Wall; // ǽ
                    }
                }
            }
        }
    }

    // ����������ǽ�ڣ��ָ�����
    void GenerateWalls()
    {
        // �������һЩǽ�����ָ����䣬ʹ�÷��䲼�ָ��Ӹ���
        for (int x = 1; x < gridSize - 1; x++)
        {
            for (int y = 1; y < gridSize - 1; y++)
            {
                if (grid[x, y] == CellType.Floor && Random.Range(0, 10) > 7) // �������ǽ��
                {
                    grid[x, y] = CellType.Wall;
                }
            }
        }
    }

    // ����Ƿ���ذ�����
    bool IsAdjacentToFloor(Vector2Int pos)
    {
        int x = pos.x;
        int y = pos.y;
        if (x > 0 && grid[x - 1, y] == CellType.Floor) return true;
        if (x < gridSize - 1 && grid[x + 1, y] == CellType.Floor) return true;
        if (y > 0 && grid[x, y - 1] == CellType.Floor) return true;
        if (y < gridSize - 1 && grid[x, y + 1] == CellType.Floor) return true;
        return false;
    }

    // ����ŵķ�������
    bool CanPlaceDoor(Vector2Int doorPos)
    {
        // ����ŵ��ĸ���������������������ǽ����������Ե�ǽ
        bool topWall = doorPos.y < gridSize - 1 && grid[doorPos.x, doorPos.y + 1] == CellType.Wall;
        bool bottomWall = doorPos.y > 0 && grid[doorPos.x, doorPos.y - 1] == CellType.Wall;
        bool leftWall = doorPos.x > 0 && grid[doorPos.x - 1, doorPos.y] == CellType.Wall;
        bool rightWall = doorPos.x < gridSize - 1 && grid[doorPos.x + 1, doorPos.y] == CellType.Wall;

        // ����������������ǽ���ұ�������Ե�ǽ
        return (topWall && bottomWall) || (leftWall && rightWall);
    }

    // ��Ⱦ����
    void RenderGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = position;

                switch (grid[x, y])
                {
                    case CellType.Floor:
                        tile.GetComponent<Renderer>().material.color = Color.green; // �ذ�
                        break;
                    case CellType.Wall:
                        tile.GetComponent<Renderer>().material.color = Color.gray; // ǽ
                        break;
                    case CellType.Door:
                        tile.GetComponent<Renderer>().material.color = Color.blue; // ��
                        break;
                    case CellType.Exit:
                        tile.GetComponent<Renderer>().material.color = Color.red; // ����
                        break;
                    case CellType.Collapsed:
                        tile.GetComponent<Renderer>().material.color = Color.black; // ̮��
                        break;
                }
            }
        }
    }
}
