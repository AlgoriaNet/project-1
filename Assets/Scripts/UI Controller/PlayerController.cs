using model;
using TMPro;
using UnityEngine;

namespace UI_Controller
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text playerId;
        [SerializeField] private TMP_Text playerLevel;
        [SerializeField] private TMP_Text playerExp;
        [SerializeField] private TMP_Text playerGoldCoin;
        [SerializeField] private TMP_Text playerDiamond;
        [SerializeField] private TMP_Text playerStamina;

        private const int MaxStamina = 100;

        private void Start()
        {
            PlayerProfile.Data.AddListener(UpdateTopInfo, "Player");
        }
      
        private void UpdateTopInfo(ApplicationModel model)
        {
            var player = (model as PlayerProfile)?.Player;
            if (player == null) return;

            Debug.Log("update top info: " + player.Name);

            if (playerName != null) playerName.text = player.Name;
            if (playerId != null) playerId.text = player.Id.ToString();
            if (playerLevel != null) playerLevel.text = player.Level.ToString();
            if (playerExp != null) playerExp.text = player.Exp.ToString();
            if (playerGoldCoin != null) playerGoldCoin.text = player.GoldCoin.ToString();
            if (playerDiamond != null) playerDiamond.text = player.Diamond.ToString();
            if (playerStamina != null) playerStamina.text = $"{player.Stamina}";
        }
    }
}