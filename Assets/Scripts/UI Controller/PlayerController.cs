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
        
        private void Start()
        {
            PlayerProfile.Data.AddListener(UpdatePlayerInfo, "Player");
        }
      
        private void UpdatePlayerInfo(ApplicationModel model)
        {
            Debug.Log("update top info: " + PlayerProfile.Data.Player);
            var player = PlayerProfile.Data.Player;
            
            if (playerName != null) playerName.text = player?.Name;
            if (playerId != null) playerId.text = player?.Id.ToString();
            if (playerLevel != null) playerLevel.text = player?.Level.ToString();
            if (playerExp != null) playerExp.text = player?.Exp.ToString();
            if (playerGoldCoin != null) playerGoldCoin.text = FormatNumber(player?.GoldCoin ?? 0);
            if (playerDiamond != null) playerDiamond.text = FormatNumber(player?.Diamond ?? 0);
            if (playerStamina != null) playerStamina.text = FormatNumber(player?.Stamina ?? 0);
        }
        
        private string FormatNumber(int number)
        {
            if (number >= 1000)
            {
                float thousands = number / 1000f;
                return $"{thousands:F1}K";
            }
            return number.ToString();
        }

        private void OnDestroy()
        {
            PlayerProfile.Data.RemoveListener(UpdatePlayerInfo, "Player");
        }
    }
}