using UnityEngine;
using UnityEngine.Serialization;
using utils;
using Random = UnityEngine.Random;

namespace battle.BulletTrajectory
{
    public class RandomMotion : BaseSkillController
    {
        public bool isChangeRotation;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Vector2 _midPosition;

        private float _percentSpeed;
        private float _percent;

        public override void Start()
        {
            base.Start();
            SelfInit();
            GenerateMiddlePoint();
        }

        private void Update()
        {
            if (!Skill.IsDynamic) return;
            if (_percent < 1f)
            {
                _percent += _percentSpeed * Time.deltaTime;
                Vector2 currentPos = transform.position;
                transform.position = Utils.Bezier(_percent, _startPosition, _midPosition, _endPosition);
                if (isChangeRotation)
                    transform.rotation = Utils.DirectionQuaternion(currentPos, transform.position, Vector3.up);
            }
            else
            {
                GenerateMiddlePointByTangentLine();
                SelfInit();
            }
        }

        private void SelfInit()
        {
            _percent = 0;
            _startPosition = transform.position;
            _endPosition = BattleGridManager.Instance.GetRandomPositionInBattleGrid();
            _percentSpeed = Skill.Speed / (_endPosition - _startPosition).magnitude;
        }

        void GenerateMiddlePoint()
        {
            var m = Vector2.Lerp(_startPosition, _endPosition, 0.1f);
            var normal = Vector2.Perpendicular(_startPosition - _endPosition).normalized;
            var rd = Random.Range(-1f, 1f);
            var curveRatio = 0.3f;
            _midPosition = m + (_startPosition - _endPosition).magnitude * curveRatio * rd * normal;
            if (BattleGridManager.Instance.IsPositionInBattleGrid(_midPosition)) return;
            GenerateMiddlePoint();
        }

        void GenerateMiddlePointByTangentLine()
        {
            _midPosition = _endPosition + (0.2f * Skill.Speed) * (_endPosition - _startPosition).normalized;
        }
    }
}