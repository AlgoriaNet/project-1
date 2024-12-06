using UnityEngine;

namespace utils
{
    public class Utils
    {
        public static Vector2 Bezier(float t, Vector2 a, Vector2 b, Vector2 c)
        {
            var ab = Vector2.Lerp(a, b, t);
            var bc = Vector2.Lerp(b, c, t);
            return Vector2.Lerp(ab, bc, t);
        }
        
        public static Quaternion DirectionQuaternion(Vector2 self, Vector2 target, Vector2 initialDirection)
        {
           return DirectionQuaternion((target - self).normalized, initialDirection);
        }
        
        public static Quaternion DirectionQuaternion(Vector2 direction,  Vector2 initialDirection)
        {
            
            float angle = Vector2.SignedAngle(initialDirection, direction);
            return Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        public static Vector2 RotateVector(Vector2 vector, float angle)
        {
            float radian = angle * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radian);
            float cos = Mathf.Cos(radian);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos).normalized;
        }
        
        public static Vector2 AngleOffsetDirection(Vector2 direction, float angle, int maxCount, int index)
        {
            float angleStep =  angle / (maxCount - 1);
            float currentAngle = -angle / 2 + angleStep * index;
            
            Debug.Log("currentAngle: " + currentAngle);
            return RotateVector(direction, currentAngle);
        }
        
        public static Vector2[] AngleOffsetDirections(Vector2 direction, float angle, int maxCount)
        {
            Vector2[] directions = new Vector2[maxCount];
            float angleStep = angle / (maxCount - 1);
            for (int i = 0; i < maxCount; i++)
            {
                float currentAngle = -angle / 2 + angleStep * i;
                directions[i] = RotateVector(direction, currentAngle);
            }
            return directions;
        }
    }
}