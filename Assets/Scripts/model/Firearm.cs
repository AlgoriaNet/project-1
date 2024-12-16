using UnityEngine;

namespace model
{
    public abstract class Firearm : MonoBehaviour
    {
        public string name;
        public GameObject buttet;
        public float bulletSpeed;
        public int magazineCapacity;
        public float fireRate;

        public  void Fire(GameObject self, GameObject target, Transform firePoint)
        {
            if (target != null)
            {
                if (target.gameObject.GetComponent <MonsterManager>().isDead)
                {
                    return;
                }
                firePoint.GetChild(0).gameObject.GetComponent<ParticleSystem>().Play();
                self.GetComponent<AudioSource>().Play();
                Vector2 direction = target.transform.position - firePoint.position;
                direction.Normalize();
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                GameObject bullet = Instantiate(buttet, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));
                Rigidbody2D bulletRigidbody = bullet.GetComponent<Rigidbody2D>();
                bulletRigidbody.velocity = direction * bulletSpeed;
                Destroy(bullet, 4);
            }
        }
    }
}