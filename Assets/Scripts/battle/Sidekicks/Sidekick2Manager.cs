using System.Numerics;
using Vector3 = UnityEngine.Vector3;

namespace battle
{
    public class Sidekick2Manager : SidekickManager
    {
        public override Vector3 getSkillPosition()
        {
            return ReleaseSkillPosition.position;
        }
    }
}