using System.Collections;
using battle.bullets;
using UnityEngine;
using utils;
using Utils = utils.Utils;

namespace battle
{
    public class SkillBlazingRay : BaseSkillController
    {
        private Vector2 _startPos;
        private float _length;
        private Vector2 _direction;
        private readonly float _attackInterval = 0.2f;
        private float _lastAttackTime = 0f;
        private MonsterManager _target;


        public override void Start()
        {
           base.Start();

            _startPos = transform.position;
            _length = BattleGridManager.Instance.BattlegroundOppositeLength;
            _target = GetTarget();
            SetLaserQuat();
            ComputeDirection();
            releasingAudio.Play();
            
        }

        private void Update()
        {
            _lastAttackTime += Time.deltaTime;
            if (Skill.IsTraceMonster &&  _target == null)
            {
                _target = GetTarget();
                SetLaserQuat();
                ComputeDirection();
            }
            if (Skill.IsImpenetrability) UpdateMaxLength();
            
            if (_lastAttackTime >= _attackInterval)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(_startPos, _direction, _length, Consts.MonsterLayer);
                _lastAttackTime -= _attackInterval;
                StartCoroutine(AttackDelay(hits));
            }
        }

        private void UpdateMaxLength()
        {
            RaycastHit2D hit = Physics2D.Raycast(_startPos, _direction, Mathf.Infinity, Consts.MonsterLayer);
            UpdateMaxLength(hit);
        }

        private IEnumerator AttackDelay(RaycastHit2D[] hits)
        {
            foreach (var hit in hits)
            {
                var monsterManager = hit.collider.GetComponent<MonsterManager>();
                Attack(monsterManager);
            }
            yield return null;
        }

        private void UpdateMaxLength(RaycastHit2D hit)
        {
            _length = hit ? (hit.point - (Vector2)transform.position).magnitude + 0.2f : BattleGridManager.Instance.BattlegroundOppositeLength;
        }
        
        private void ComputeDirection()
        {
            var rotationZ = transform.rotation.eulerAngles.z;
            rotationZ *= Mathf.Deg2Rad;
            _direction = new Vector2(Mathf.Cos(rotationZ), Mathf.Sin(rotationZ));
        }

        private void SetLaserQuat()
        {
            if (_target)
            {
                var direction = Utils.DirectionQuaternion(_startPos, _target.transform.position, Vector3.right);
                transform.rotation = direction;
                TargetDirection = (_target.transform.position - transform.position).normalized;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 90);
                TargetDirection = Vector2.up.normalized;
            }
        }
        
    }
}