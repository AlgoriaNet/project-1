using UnityEngine;
using System.Collections.Generic;

public class MonsterGridManager : MonoBehaviour
{
    public static MonsterGridManager Instance;
    public Transform topLeft;    // 左上角
    public Transform bottomRight; // 右下角

    private float gridSize;
    private int gridCols;
    private int gridRows;
    private List<MonsterManager>[,] grid;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    void Start()
    {
        float width = Vector3.Distance(topLeft.position, new Vector3(bottomRight.position.x, topLeft.position.y, topLeft.position.z));
        float height = Vector3.Distance(topLeft.position, new Vector3(topLeft.position.x, bottomRight.position.y, topLeft.position.z));

        gridSize = width / 10.0f; // X 轴的 1/10 用于网格宽度

        gridCols = Mathf.CeilToInt(width / gridSize);
        gridRows = Mathf.CeilToInt(height / gridSize);

        grid = new List<MonsterManager>[gridRows, gridCols];

        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridCols; j++)
            {
                grid[i, j] = new List<MonsterManager>();
            }
        }
    }
    
    void Update()
    {
        // 每帧更新网格，清空并重新分配怪物
        ClearGrid();
        AssignMonstersToGrid();
    }

    void ClearGrid()
    {
        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridCols; j++)
            {
                grid[i, j].Clear();
            }
        }
    }

    void AssignMonstersToGrid()
    {
        MonsterManager[] monsters = FindObjectsOfType<MonsterManager>();

        foreach (var monster in monsters)
        {
            Vector3 position = monster.transform.position;
            AssignToGrid(position, monster);
        }
    }

    void AssignToGrid(Vector3 position, MonsterManager monster)
    {
        // 计算怪物在长方形中的相对位置
        float xOffset = position.x - topLeft.position.x;
        float yOffset = topLeft.position.y - position.y; // 注意这里的计算是从上到下

        // 计算怪物所在的网格单元
        int col = Mathf.Clamp(Mathf.FloorToInt(xOffset / gridSize), 0, gridCols - 1);
        int row = Mathf.Clamp(Mathf.FloorToInt(yOffset / gridSize), 0, gridRows - 1);

        // 将怪物添加到相应的网格单元中
        grid[row, col].Add(monster);
    }
    
    public Vector3 CentralPoint()
    {
        float x = (topLeft.position.x + bottomRight.position.x) / 2;
        float y = (topLeft.position.y + bottomRight.position.y) / 2;
        return new Vector3(x, y, 0);
    }
}