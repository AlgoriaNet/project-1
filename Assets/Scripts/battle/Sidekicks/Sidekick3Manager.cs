using UnityEngine;

namespace battle
{
    public class Sidekick3Manager : SidekickManager
    {
        public int skillCount = 3;
        public float skillReleaseInterval = 2f;
        public int releasedSkillCount = 0;
        public float waitIntervalTime = 0;


        protected override void HandleSkillRelease()
        {
            if (isReleasing) return;
            waitIntervalTime += Time.deltaTime;
            if (releasedSkillCount > 0 && waitIntervalTime < skillReleaseInterval) return;
            releasedSkillCount++;
            StartCoroutine(ReleaseSkill());
        }

        protected override void OnSkillReleased()
        {
            isReleasing = false;
            if (releasedSkillCount == skillCount)
            {
                waitingTime = 0;
                releasedSkillCount = 0;
            }
            waitIntervalTime = 0;
        }

        public override Vector3 getSkillPosition()
        {
            int maxCount = BattleManager.Instance.monsters.Count;
            int randomIndex = Random.Range(0, maxCount);
            return BattleManager.Instance.monsters[randomIndex].transform.position;
        }
    }
}