using System.Collections;
using battle.bullets;
using entity;
using UnityEngine;
using UnityEngine.Diagnostics;
using utils;
using Utils = utils.Utils;

namespace battle
{
    public class SkillFlameRay : BaseSkillController
    {
        private LineRenderer _lineRenderer;
        private LaserManager _laserManager;
        private Vector2 _startPos;
        private float _length;
        private Vector2 _direction;
        private readonly float _attackInterval = 0.2f;
        private float _lastAttackTime = 0f;
        private MonsterManager _target; 

        protected override void Start()
        {
           base.Start();
           Destroy(gameObject, Skill.Duration);
            _lineRenderer = GetComponentInChildren<LineRenderer>();
            _laserManager = GetComponentInChildren<LaserManager>();

            _startPos = transform.position;
            _length = BattleGridManager.Instance.BattlegroundOppositeLength;
            _target = GetTarget();
            SetLaserQuat();
            ComputeDirection();
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
            UpdateEndPos();
            if (_lastAttackTime >= _attackInterval)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(_startPos, _direction, _length, Consts.MonsterLayer);
                Debug.Log("Hit Count: " + hits.Length);
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
            }
            else
                transform.rotation = Quaternion.Euler(0, 0, 90);
        }

        private void UpdateEndPos()
        {
            Vector2 endPos = _startPos + _direction * _length;
            _lineRenderer.SetPosition(1, new Vector2(_length, 0));
            _laserManager.startVFX.transform.position = _startPos;
            _laserManager.endVFX.transform.position = endPos;
        }
    }
}