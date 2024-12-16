using battle;
using model;
using UnityEngine;
using utils;

public class RefractionBullet : BaseSkillController
{
    
    public Vector2 topLeftCorner; // 长方形的左上角
    public Vector2 bottomRightCorner; // 长方形的右下角
    public int maxReflections = 7; // 最大反射次数
    private int _reflectionCount = 0; // 当前反射次数
    private Rigidbody2D _rb; // 子弹的Rigidbody2D组件

    void Start()
    {
        base.Start();
        _rb = GetComponent<Rigidbody2D>();
        topLeftCorner = BattleGridManager.Instance.topLeft.position;
        bottomRightCorner = BattleGridManager.Instance.bottomRight.position;
        Destroy(gameObject, 4);
    }
    
    protected override void WhenAttackAfter()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if (BattleGridManager.Instance.TryReflectBulletIfOutOfBounds(gameObject))
        {
            ReflectBullet();
            // 增加反射次数
            _reflectionCount++;
            // 如果反射次数达到最大限制，则停止更新速度或销毁子弹
            if (_reflectionCount >= maxReflections)
            {
                Destroy(gameObject); // 销毁子弹对象
            }
        }
    }
    
 
    void ReflectBullet()
    {
        
        Vector2 position = _rb.position;
        Vector2 velocity = _rb.velocity;
        Vector2 normal;
        
        // 根据子弹越界的边来确定法向量
        if (position.x < topLeftCorner.x || position.x > bottomRightCorner.x)
            // 碰到左右边界，法向量为水平方向
            normal = new Vector2(Mathf.Sign(position.x - topLeftCorner.x), 0);
        else
            normal = new Vector2(0, Mathf.Sign(position.y - topLeftCorner.y));
 
        Vector2 reflectedVelocity = velocity - 2 * Vector2.Dot(velocity, normal) * normal;
        _rb.velocity = reflectedVelocity;
        Vector2 direction = reflectedVelocity.normalized;
        transform.rotation =  Utils.DirectionQuaternion(direction, Vector2.left);
    }
}