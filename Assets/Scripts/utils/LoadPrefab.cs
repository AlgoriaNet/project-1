using UnityEngine;

namespace utils
{
    public class LoadPrefab : MonoBehaviour
    {
        // 根据文件名加载预制件的方法
        public static GameObject Load(string prefabName)
        {
            // 使用Resources.Load方法加载预制件
            GameObject prefab = Resources.Load<GameObject>(prefabName);

            // 检查是否成功加载
            if (prefab != null)
            {
                // 实例化预制件并返回新创建的GameObject
                return prefab;
            }
            return null;
        }
    }
}