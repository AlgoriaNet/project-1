using System.Collections.Generic;
using model;
using PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.PopUpBox;
using UnityEngine;

namespace PimDeWitte.UnityMainThreadDispatcher.UI_Scripts.Hero_Menu
{
    public class HeroEquipments : MonoBehaviour
    {
        [SerializeField] private Transform helmTransform;
        [SerializeField] private Transform shoulderTransform;
        [SerializeField] private Transform chestTransform;
        [SerializeField] private Transform pantsTransform;
        [SerializeField] private Transform glovesTransform;
        [SerializeField] private Transform bootsTransform;

        public void Start()
        {
            Init();
            PlayerProfile.Data.AddListener(UpdateUI, "Player");
        }

        public void Init()
        {
            List<Equipment> equipments = PlayerProfile.Data.GetHeroEquipments();
            Debug.Log($"equipments count: {equipments.Count}");
            foreach (var equipment in equipments)
            {
                Debug.Log(equipment.Part);
                switch (equipment.Part)
                {
                    case "Helm":
                        var helmGems = PlayerProfile.Data.GetHeroGemstones(equipment.Part) ?? new List<Gemstone>();
                        helmTransform.GetComponent<EquipmentColumnManager>().Init(equipment, helmGems);
                        break;
                    case "Shoulder":
                        var shoulderGems = PlayerProfile.Data.GetHeroGemstones(equipment.Part) ?? new List<Gemstone>();
                        shoulderTransform.GetComponent<EquipmentColumnManager>().Init(equipment, shoulderGems);
                        break;
                    case "Chest":
                        var chestGems = PlayerProfile.Data.GetHeroGemstones(equipment.Part) ?? new List<Gemstone>();
                        chestTransform.GetComponent<EquipmentColumnManager>().Init(equipment, chestGems);
                        break;
                    case "Pants":
                        var pantsGems = PlayerProfile.Data.GetHeroGemstones(equipment.Part) ?? new List<Gemstone>();
                        pantsTransform.GetComponent<EquipmentColumnManager>().Init(equipment, pantsGems);
                        break;
                    case "Gloves":
                        var glovesGems = PlayerProfile.Data.GetHeroGemstones(equipment.Part) ?? new List<Gemstone>();
                        glovesTransform.GetComponent<EquipmentColumnManager>().Init(equipment, glovesGems);
                        break;
                    case "Boots":
                        var bootsGems = PlayerProfile.Data.GetHeroGemstones(equipment.Part) ?? new List<Gemstone>();
                        bootsTransform.GetComponent<EquipmentColumnManager>().Init(equipment, bootsGems);
                        break;
                }
            }
        }

        private void UpdateUI(ApplicationModel model)
        {
            Init();
        }
    }
}