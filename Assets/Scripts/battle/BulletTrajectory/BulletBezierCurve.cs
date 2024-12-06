using battle;
using entity;
using UnityEngine;
using utils;

public class BezierBullet : BaseSkillController
{
    public float besierRatio = 0.5f;
    private Vector2 _startPos;
    private MonsterManager _currentTarget;
    private float _speed;
    private float _percent;
    private float _percentSpeed;
    private Vector2 _middlePoint;
    private Vector2 _lastPos;
    

    protected override void Start()
    {
        base.Start();
        _currentTarget = GetTarget();

        _speed = Skill.Speed;
        _startPos = transform.position;
        _lastPos = transform.position;
        _percent = 0f;

        Vector2 initialDestination;
        if (_currentTarget != null)
            initialDestination = _currentTarget.transform.position;
        else
            initialDestination = _startPos + (Vector2)transform.up * 10;
        _middlePoint = GetMiddlePoint(_startPos, initialDestination);
        _percentSpeed = _speed / (initialDestination - _startPos).magnitude;
    }


    void Update()
    {
        if (!SkillSetting.IsDynamic) return;
        if (_currentTarget != null && !_currentTarget.isDead)
        {
            _percent += _percentSpeed * Time.deltaTime;
            if (_percent >= 1f)
                _percent = 1f;
            Vector2 currentPos = transform.position;
            transform.position = Utils.Bezier(_percent, _startPos, _middlePoint, _currentTarget.transform.position);
            transform.rotation = Utils.DirectionQuaternion(currentPos, transform.position, Vector3.left);
            _lastPos = currentPos;
        }
        else
        {
            Vector2 currentPos = transform.position;
            Vector2 direction = (currentPos - _lastPos).normalized;
            transform.position += _speed * Time.deltaTime * (Vector3)direction;
            _lastPos = currentPos;
            if (Skill.IsTraceMonster)
            {
                var overlapCircle = FindLatestMonster(currentPos);
                if (overlapCircle == null) return;
                _currentTarget = overlapCircle;
                _startPos = currentPos;
                _middlePoint = GetMiddlePosition(_startPos, direction, _speed);
                _percent = 0;
                _percentSpeed = _speed / ((Vector2)_currentTarget.transform.position - _startPos).magnitude;
            }
        }
    }


    Vector2 GetMiddlePoint(Vector2 a, Vector2 b)
    {
        Vector2 m = Vector2.Lerp(a, b, 0.1f);
        Vector2 normal = Vector2.Perpendicular(a - b).normalized;
        float rd = Random.Range(-1f, 1f);
        float curveRatio = 0.3f;
        return m + (a - b).magnitude * curveRatio * rd * normal;
    }

    Vector2 GetMiddlePosition(Vector2 pos, Vector2 direction, float v)
    {
        return pos + besierRatio * v * direction;
    }

    private MonsterManager FindLatestMonster(Vector2 currentPos)
    {
        float closestDistance = float.PositiveInfinity;
        MonsterManager closestCollider = null;
        foreach (MonsterManager monster in BattleGridManager.Instance.monsters)
        {
            float distance = Vector2.Distance(monster.transform.position, currentPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = monster;
            }
        }
        return closestCollider;
    }
}