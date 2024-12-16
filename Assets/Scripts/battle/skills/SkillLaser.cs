using System.Collections.Generic;
using UnityEngine;
using utils;

namespace battle
{
    public class SkillLaser : BaseSkillController
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Noise = Shader.PropertyToID("_Noise");
        public GameObject hitEffect;
        public float hitOffset = 0;

        private float _maxLength;
        private LineRenderer _laser;

        public float mainTextureLength = 1f;
        public float noiseTextureLength = 1f;

        private Vector4 Length = new(1, 1, 1, 1);

        private ParticleSystem[] _effects;
        private ParticleSystem[] _hits;

        private float _lastAttackTime = 0f;
        private MonsterManager _currentTarget;
        private readonly float _attackInterval = 0.2f;
        private readonly List<Vector2> _linePoints = new();
        private bool _changeTarget = false;
        private bool _isFirst = true;

        public override void Start()
        {
            base.Start();
            _maxLength = BattleGridManager.Instance.BattlegroundOppositeLength;
            _laser = GetComponentInChildren<LineRenderer>();
            _effects = GetComponentsInChildren<ParticleSystem>();
            _hits = hitEffect.GetComponentsInChildren<ParticleSystem>();

            _currentTarget = GetTarget();
            SetLaserDirection();
            UpdateLinePosition();

            foreach (var allPs in _effects)
                if (!allPs.isPlaying)
                    allPs.Play();
            foreach (var allPs in _hits)
                if (!allPs.isPlaying)
                    allPs.Play();
        }

        void Update()
        {
            UpdateTarget();
            UpdateEndPos();
            CheckAttack();
        }

        private void UpdateTarget()
        {
            // 需要开启技能追踪 以及 当前目标为空时 才会更新目标
            if (!Skill.IsTraceMonster || (_currentTarget && !_currentTarget.isDead)) return;
            _currentTarget = GetTarget();
            _changeTarget = true;
            SetLaserDirection();
            UpdateLinePosition();
        }

        private void UpdateLinePosition()
        {
            _linePoints.Clear();
            _linePoints.Add(transform.position);
            if (!Skill.IsImpenetrability)
            {
                _linePoints.Add((Vector2)transform.position + TargetDirection * _maxLength);
            }
            else
            {
                int refractionCount = 1;
                var ac = Skill.ActiveCharacter.Find(ac => ac.StartsWith("MonsterRefraction"));
                Debug.Log($"ActiveCharacter {ac}");
                if (ac != null) refractionCount = int.Parse(ac.Split('_')[3]);
                Debug.Log($"RefractionCount {refractionCount}");
                Vector2 start = transform.position;
                Vector2 direction = TargetDirection;
                RaycastHit2D curHit  = new RaycastHit2D();
                for (int i = 0; i < refractionCount; i++)
                {
                    RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction, Mathf.Infinity, Consts.MonsterLayer);
                    
                    var hit = LatestHit(hits, start, curHit);
                    if (hit.collider)
                    {
                        _linePoints.Add(hit.point);
                        start = hit.point;
                        direction = ((Vector2)BattleGridManager.Instance.GetTargetPosition(SkillTargetType.Random) - start).normalized;
                        curHit = hit;
                    }
                    else
                    {
                        _linePoints.Add(start + direction * _maxLength);
                        break;
                    }
                }
            }
        }
        
        private RaycastHit2D LatestHit(RaycastHit2D[] hits, Vector2 start, RaycastHit2D excludeHit)
        {
            RaycastHit2D latestHit = new RaycastHit2D();
            float latestDistance = Mathf.Infinity;
            foreach (var hit in hits)
            {
                if (hit.collider == excludeHit.collider) continue;
                float distance = Vector2.Distance(start, hit.point);
                if (distance < latestDistance)
                {
                    latestDistance = distance;
                    latestHit = hit;
                }
            }
            return latestHit;
        }
        private void CheckAttack()
        {
            _lastAttackTime += Time.deltaTime;
            if (_lastAttackTime >= _attackInterval)
            {
                _lastAttackTime -= _attackInterval;
                var hits = Physics2D.RaycastAll(transform.position, TargetDirection, _maxLength, Consts.MonsterLayer);
                foreach (var hit in hits) Attack(hit.collider.GetComponent<MonsterManager>());
            }
        }

        private void SetLaserDirection()
        {
            if (_currentTarget)
            {
                var rotation = Utils.DirectionQuaternion(transform.position, _currentTarget.transform.position,
                    Vector3.right);
                transform.rotation = rotation;
                hitEffect.transform.rotation = rotation;
                TargetDirection = (_currentTarget.transform.position - transform.position).normalized;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, 90);
                TargetDirection = Vector2.up.normalized;
            }
        }

        private void UpdateEndPos()
        {
            _laser.positionCount = _linePoints.Count;
            float maxLen = 0;
            var start = _linePoints[0];
            _laser.SetPosition(0, _linePoints[0]);
            for (var i = 1; i < _linePoints.Count; i++)
            {
                _laser.SetPosition(i, _linePoints[i]);
                maxLen += Vector2.Distance(_linePoints[i], start);
                start = _linePoints[i];
            }

            hitEffect.transform.position = _linePoints[^1];
            Length[0] = mainTextureLength * maxLen;
            Length[2] = noiseTextureLength * maxLen;
            _laser.material.SetTextureScale(MainTex, new Vector2(Length[0], Length[1]));
            _laser.material.SetTextureScale(Noise, new Vector2(Length[2], Length[3]));
        }
    }
}