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
            foreach (var equipment in equipments)
            {
                switch (equipment.Part)
                {
                    case "Helm":
                        helmTransform.GetComponent<EquipmentColumnManager>().Init(equipment, new List<Gemstone>());
                        break;
                    case "Shoulder":
                        shoulderTransform.GetComponent<EquipmentColumnManager>().Init(equipment, new List<Gemstone>());
                        break;
                    case "Chest":
                        chestTransform.GetComponent<EquipmentColumnManager>().Init(equipment, new List<Gemstone>());
                        break;
                    case "Pants":
                        pantsTransform.GetComponent<EquipmentColumnManager>().Init(equipment, new List<Gemstone>());
                        break;
                    case "Gloves":
                        glovesTransform.GetComponent<EquipmentColumnManager>().Init(equipment, new List<Gemstone>());
                        break;
                    case "Boots":
                        bootsTransform.GetComponent<EquipmentColumnManager>().Init(equipment, new List<Gemstone>());
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