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

        void Start()
        {
            // 随机生成一个方向（单位向量）
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            randomDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            //自动销毁对象
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            // 计算已生存时间
            elapsedTime += Time.deltaTime;
            // 计算当前速度：前快后慢，使用平方根曲线
            float progress = elapsedTime / lifetime; // 当前进度（0 到 1）
            float speed = Random.Range(1, initialSpeed);
            float currentSpeed = speed * (1 - Mathf.Sqrt(progress)); // 使用平方根
            // 前一半时间，物体移动；后一半时间，物体缩小
            if (elapsedTime > lifetime * 2 / 3)
            {
                progress = (elapsedTime - lifetime * 1 / 2) / lifetime * 1 / 2;
                float scale = Mathf.Lerp(1, scaleLimit, progress);
                transform.localScale *= scale;
            }
            if (elapsedTime < lifetime / 2)
            {
                transform.position += randomDirection * (currentSpeed * Time.deltaTime);
            }
        }
    }
}