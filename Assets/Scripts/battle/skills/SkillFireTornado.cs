using System;
using System.Collections.Generic;
using entity;
using UnityEngine;

namespace battle
{
    public class SkillFireTornado : BaseSkillController
    {
        public GameObject skill;
        public Transform peat;
        private int _speed = 10;
        private BoxCollider2D _boxCollider;
        private float _radius;
        private float _angle = 0;
        private Dictionary<Guid, int> _stayTimes = new();
        public bool canMove;
        public Vector2 distance;
        public int separateNumber = 12;
        public void Start()
        {
            SetSkillSetting(GetComponentInParent<SkillSetting>());
            Init();
            Destroy(gameObject, Skill.Duration);
            _boxCollider = GetComponent<BoxCollider2D>();
            _radius = (_boxCollider.size.x / 2f * 0.9f);

            if (canMove)
            {
                Destroy(gameObject, 4F);
                transform.localScale *= 0.4f;
            }
        }

        public void Update()
        {
            if (canMove)
            {
                transform.position += (Vector3)distance * (Time.deltaTime * _speed * 0.7f);
            }
        }
        private void OnDestroy()
        {
            if(canMove || !Skill.HasCharacter("split")) return;
            float maxOffset = 360f;
            float angleStep = 2 * maxOffset / (separateNumber - 1);
            for (int i = 0; i < separateNumber; i++)
            {
                float angle = -maxOffset + (angleStep * i);
                Vector2 direction = Quaternion.Euler(0, 0, angle) * transform.up;
                
                GameObject small = Instantiate(skill, transform.position, Quaternion.identity);
                var skillSetting = small.GetComponent<SkillSetting>();
                skillSetting.Living = Living;
                skillSetting.targetIndex = TargetIndex;
                var skillTornado = small.GetComponentInChildren<SkillFireTornado>();
                skillTornado.distance = direction;
                skillTornado.canMove = true;
                small.SetActive(true);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Monster"))
            {
                _angle += _speed * Time.deltaTime * 0.3f;
                if(_angle > 360) _angle -= 360;

                // 计算圆周上的x和z位置
                float x = _radius * Mathf.Cos(_angle);
                float z = _radius * Mathf.Sin(_angle);

                var monsterManager = other.GetComponent<MonsterManager>();
                monsterManager.canMove = false;
                // var distance = (peat.position - monsterManager.transform.position).normalized;
                Vector3 newPosition = transform.position + new Vector3(x, peat.position.y - transform.position.y, z);
                monsterManager.transform.position = newPosition;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Monster"))
            {
                other.GetComponent<MonsterManager>().canMove = true;
            }
        }
    }
}