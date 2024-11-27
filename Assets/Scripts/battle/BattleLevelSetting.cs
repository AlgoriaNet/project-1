using System;
using System.Collections.Generic;
using UnityEngine;

namespace battle
{
    public class BattleLevelSetting : MonoBehaviour
    {
        public int CurrentLevel { get; set; } = 1;
        public int CurrentExp { get; set; }
        public List<int> RequiredExp { get; set; }

        public void ExpUp(int exp)
        {
            CurrentExp += exp;
            if (CurrentExp >= RequiredExp[CurrentLevel - 1])
            {
                onLevelUp();
                CurrentExp -= RequiredExp[CurrentLevel - 1];
                CurrentLevel++;
            }
        }

        private void onLevelUp()
        {
            throw new NotImplementedException();
        }
    }
}