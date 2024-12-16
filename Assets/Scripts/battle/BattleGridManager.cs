using UnityEngine;
using System.Collections.Generic;
using battle;
using model;

public class BattleGridManager : MonoBehaviour
{
    public static BattleGridManager Instance;
    [Header("postions")] public Transform topLeft; // 左上角
    public Transform bottomRight; // 右下角
    public Transform heroLocation; // 主角位置
    private float _gridSize;
    private int _gridCols;
    private int _gridRows;
    private List<MonsterManager>[,] MonsterGrid { get; set; }
    public List<MonsterManager> monsters = new();
    public List<Sidekick> Sidekicks = new();
    public float BattlegroundWidth { get; private set; }
    public float BattlegroundHeight { get; private set; }
    public float BattlegroundOppositeLength { get; private set; }
    public Vector3 CentralPoint { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    void Start()
    {
        BattlegroundWidth = Vector3.Distance(topLeft.position,
            new Vector3(bottomRight.position.x, topLeft.position.y, topLeft.position.z));
        BattlegroundHeight = Vector3.Distance(topLeft.position,
            new Vector3(topLeft.position.x, bottomRight.position.y, topLeft.position.z));
        BattlegroundOppositeLength = Vector3.Distance(topLeft.position, bottomRight.position);

        _gridSize = BattlegroundWidth / 10.0f; // X 轴的 1/10 用于网格宽度

        _gridCols = Mathf.CeilToInt(BattlegroundWidth / _gridSize);
        _gridRows = Mathf.CeilToInt(BattlegroundWidth / _gridSize);
        CentralPoint = GetCentralPoint();
        MonsterGrid = new List<MonsterManager>[_gridRows, _gridCols];

        for (int i = 0; i < _gridRows; i++)
        for (int j = 0; j < _gridCols; j++)
            MonsterGrid[i, j] = new List<MonsterManager>();
    }

    void Update()
    {
        // 每帧更新网格，清空并重新分配怪物
        ClearGrid();
        AssignMonstersToGrid();
    }

    void ClearGrid()
    {
        for (int i = 0; i < _gridRows; i++)
        for (int j = 0; j < _gridCols; j++)
            MonsterGrid[i, j].Clear();
    }

    void AssignMonstersToGrid()
    {
        monsters.Clear();
        MonsterManager[] existentMonsters = FindObjectsOfType<MonsterManager>();
        if (existentMonsters.Length <= 0) return;
        foreach (var monster in existentMonsters)
        {
            if (monster.isDead) continue;
            Vector3 position = monster.transform.position;
            AssignToGrid(position, monster);
            monsters.Add(monster);
        }

        if (monsters.Count > 1)
            monsters.Sort((x, y) =>
                Vector3.Distance(x.transform.position, heroLocation.transform.position)
                    .CompareTo(Vector3.Distance(y.transform.position, heroLocation.transform.position)));
    }


    void AssignToGrid(Vector3 position, MonsterManager monster)
    {
        // 计算怪物在长方形中的相对位置
        float xOffset = position.x - topLeft.position.x;
        float yOffset = topLeft.position.y - position.y; // 注意这里的计算是从上到下

        // 计算怪物所在的网格单元
        int col = Mathf.Clamp(Mathf.FloorToInt(xOffset / _gridSize), 0, _gridCols - 1);
        int row = Mathf.Clamp(Mathf.FloorToInt(yOffset / _gridSize), 0, _gridRows - 1);

        // 将怪物添加到相应的网格单元中
        MonsterGrid[row, col].Add(monster);
    }

    private Vector3 GetCentralPoint()
    {
        float x = (topLeft.position.x + bottomRight.position.x) / 2;
        float y = (topLeft.position.y + bottomRight.position.y) / 2;
        return new Vector3(x, y, 0);
    }

    public MonsterManager FarmostMonster(int index = 0)
    {
        if (monsters.Count < index + 1) return null;
        return monsters[monsters.Count - 1 - index];
    }

    public MonsterManager LatestMonster(int index = 0)
    {
        if (monsters.Count < index + 1) return null;
        return monsters[index];
    }

    public MonsterManager RandomMonster()
    {
        if (monsters.Count == 0) return null;
        return monsters[Random.Range(0, monsters.Count)];
    }

    public Vector3 GetTargetPosition(SkillTargetType targetType, int index = 0)
    {
        MonsterManager monster = null;
        switch (targetType)
        {
            case SkillTargetType.Farmost:
                monster = FarmostMonster();
                break;
            case SkillTargetType.FarmostMultiple:
                monster = FarmostMonster(index);
                break;
            case SkillTargetType.Random:
                monster = RandomMonster();
                break;
            case SkillTargetType.LatestMultiple:
                monster = LatestMonster(index);
                break;
            case SkillTargetType.LatestNearby:
                monster = LatestMonster();
                break;
        }
        if (monster) return monster.transform.position;
        return CentralPoint;
    }

    public MonsterManager GetTarget(SkillTargetType targetType, int index = 0)
    {
        switch (targetType)
        {
            case SkillTargetType.Farmost:
                return FarmostMonster();
            case SkillTargetType.FarmostMultiple:
                return FarmostMonster(index);
            case SkillTargetType.Random:
                return RandomMonster();
            case SkillTargetType.Latest:
                return LatestMonster();
            case SkillTargetType.LatestMultiple:
                return LatestMonster(index);
        }

        return null;
    }

    public bool TryReflectBulletIfOutOfBounds(GameObject @object)
    {
        // 获取子弹当前位置
        Vector2 position = @object.transform.position;

        // 检查子弹是否超出长方形的边界，并计算调整后的位置和反射后的速度
        Vector2 adjustedPosition = position;

        if (position.x < topLeft.position.x)
        {
            adjustedPosition.x = topLeft.position.x; // 调整回边界内
        }
        else if (position.x > bottomRight.position.x)
        {
            adjustedPosition.x = bottomRight.position.x; // 调整回边界内
        }
        else if (position.y > topLeft.position.y)
        {
            adjustedPosition.y = topLeft.position.y; // 调整回边界内
        }
        else if (position.y < bottomRight.position.y)
        {
            adjustedPosition.y = bottomRight.position.y; // 调整回边界内
        }
        else
            return false;

        @object.transform.position = adjustedPosition;
        return true;
    }

    public Vector2 GetRandomPositionInBattleGrid()
    {
        return new Vector2(
            Random.Range(topLeft.position.x, bottomRight.position.x),
            Random.Range(topLeft.position.y, bottomRight.position.y)
        );
    }
    
    public bool IsPositionInBattleGrid(Vector2 position)
    {
        return position.x >= topLeft.position.x && position.x <= bottomRight.position.x &&
               position.y <= topLeft.position.y && position.y >= bottomRight.position.y;
    }
}