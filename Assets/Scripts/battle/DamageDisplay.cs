namespace battle
{
    using UnityEngine;
    using UnityEngine;

    public class DamageDisplay : MonoBehaviour
    {
        public float initialSpeed = 5f; // 初始速度
        public float lifetime = 1.5f; // 存活时间
        private float elapsedTime = 0f; // 已经过的时间
        private Vector3 randomDirection; // 随机移动方向
        public float scaleLimit = 0.8f;
        private static readonly float[] Scales = { 1f, 1f, 10f, 9f, 8f, 1f, 1f, 4f, 4f, 1f, 1f, 3f, 3f, 1f, 1f, 1f };
        private int _count = 0;
        private Vector3 _initialScale;

        void Start()
        {
            _initialScale = transform.localScale;
            // 随机生成一个方向（单位向量）
            float angle = Random.Range(45f, 135f) * Mathf.Deg2Rad;
            // randomDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            randomDirection = Vector3.up;
            //自动销毁对象
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            if (_count < Scales.Length)
            {
                transform.localScale = _initialScale * Scales[_count];
            }

            _count++;
            // 计算已生存时间
            elapsedTime += Time.deltaTime;
            // 计算当前速度：前快后慢，使用平方根曲线
            float progress = elapsedTime / lifetime; // 当前进度（0 到 1）
            float speed = Random.Range(1, initialSpeed);
            float currentSpeed = speed * (1 - Mathf.Sqrt(progress)); // 使用平方根

            if (elapsedTime < lifetime / 2)
            {
                transform.position += randomDirection * (currentSpeed * Time.deltaTime);
            }
        }
    }
}