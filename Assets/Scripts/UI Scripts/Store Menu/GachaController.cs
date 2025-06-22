// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using model;
// using System;
// using Newtonsoft.Json.Linq;
// using WebSocket;

// public class GachaController : MonoBehaviour
// {
//     [Header("UI References")]
//     public GameObject gachaPage;
//     public GameObject block;
//     public Image blockImage;
//     public Image blockPart;
//     public GameObject grid;
//     public GameObject blockPrefab;
//     public GridLayoutGroup Grid;

//     [Header("Free Claim UI")]
//     public TextMeshProUGUI heroFreeButtonText;
//     public TextMeshProUGUI rareFreeButtonText;
//     public TextMeshProUGUI epicFreeButtonText;

//     [Header("Dependencies")]
//     [SerializeField] private StoreMenuController storeMenu;

//     // Constants
//     private const int BLOCKS_PER_ROW = 5;
//     private const int BLOCKS_PER_DRAW = 10;
    
//     // Diamond costs
//     private const int HERO_X1_DIAMOND_COST = 300;
//     private const int HERO_X10_DIAMOND_COST = 3000;
//     private const int RARE_X1_DIAMOND_COST = 180;
//     private const int RARE_X10_DIAMOND_COST = 1800;
//     private const int EPIC_X1_DIAMOND_COST = 200;
//     private const int EPIC_X10_DIAMOND_COST = 2000;

//     // Key requirements
//     private const int KEY_REQUIRED_FOR_X1 = 1;
//     private const int KEY_REQUIRED_FOR_X10 = 10;

//     // Free claim limits
//     private const int HERO_FREE_CLAIM_LIMIT = 1;
//     private const int RARE_FREE_CLAIM_LIMIT = 3;
//     private const int EPIC_FREE_CLAIM_LIMIT = 3;

//     // PlayerPrefs keys
//     private const string PREF_HERO_FREE_CLAIM_COUNT = "HeroFreeClaimCount";
//     private const string PREF_RARE_FREE_CLAIM_COUNT = "RareFreeClaimCount";
//     private const string PREF_EPIC_FREE_CLAIM_COUNT = "EpicFreeClaimCount";

//     // Resource paths
//     private const string SHARD_PATH = "UILoading/CharacterImages/Shard";
//     private const string GEM_STONE_PATH = "UILoading/Gem/Stone";
//     private const string GEM_PART_PATH = "UILoading/Gem/Part";

//     private enum GachaType { None, Shard, RareGem, EpicGem }
//     private GachaType lastDraw = GachaType.None;

//     private static PurchaseWebSocketApi _wsSocketApi;
//     [SerializeField] private DiamondStore diamondStore;

//     private void Start()
//     {
//         _wsSocketApi = PurchaseWebSocketApi.Instance;
//     }

//     #region Page Management
//     public void OpenGachaPage()
//     {
//         gachaPage.SetActive(true);
//     }

//     public void CloseGachaPage()
//     {
//         gachaPage.SetActive(false);
//         grid.SetActive(false);
//         block.SetActive(false);

//         ClearGrid();
//         PlayerProfile.Data.NotifyListeners("Player");
//     }

//     private void ClearGrid()
//     {
//         foreach (Transform child in grid.transform)
//         {
//             Destroy(child.gameObject);
//         }
//     }
//     #endregion

//     #region Hero Gacha Methods
//     public void DrawFreeShard()
//     {
//         int claimCount = PlayerPrefs.GetInt(PREF_HERO_FREE_CLAIM_COUNT, 0);

//         if (claimCount >= HERO_FREE_CLAIM_LIMIT)
//         {
//             heroFreeButtonText.text = $"{HERO_FREE_CLAIM_LIMIT}/{HERO_FREE_CLAIM_LIMIT}";
//             return;
//         }

//         heroFreeButtonText.text = $"{claimCount}/{HERO_FREE_CLAIM_LIMIT}";

//         if (storeMenu.IsMonthlyCardActive())
//         {
//             ExecuteFreeClaim(PREF_HERO_FREE_CLAIM_COUNT, claimCount, DrawOneShard, heroFreeButtonText, HERO_FREE_CLAIM_LIMIT);
//         }
//         else
//         {
//             ShowAdForFreeClaim("gacha_shard", PREF_HERO_FREE_CLAIM_COUNT, claimCount, DrawOneShard, heroFreeButtonText, HERO_FREE_CLAIM_LIMIT);
//         }
//     }

//     public void DrawOneShard()
//     {
//         if (!ValidateBlockReferences()) return;

//         var player = PlayerProfile.Data.Player;
//         var result = DeductResources(player, "heroKey", KEY_REQUIRED_FOR_X1, HERO_X1_DIAMOND_COST);
        
//         if (result.success)
//         {
//             ShowSingleBlock(SHARD_PATH, false);
//             storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson["heroKey"], HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Items/heroKey");
//             lastDraw = GachaType.Shard;
//         }
//         else if (result.needDiamonds)
//         {
//             diamondStore.Open();
//         }
//     }

//     public void DrawTenShard()
//     {
//         var player = PlayerProfile.Data.Player;
//         var result = DeductResources(player, "heroKey", KEY_REQUIRED_FOR_X10, HERO_X10_DIAMOND_COST);
        
//         if (result.success)
//         {
//             GenerateGachaBlocks(SHARD_PATH, false);
//             storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson["heroKey"], HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Items/heroKey");
//             lastDraw = GachaType.Shard;
//         }
//         else if (result.needDiamonds)
//         {
//             diamondStore.Open();
//         }
//     }
//     #endregion

//     #region Rare Gacha Methods
//     public void DrawFreeRareGem()
//     {
//         int claimCount = PlayerPrefs.GetInt(PREF_RARE_FREE_CLAIM_COUNT, 0);

//         if (claimCount >= RARE_FREE_CLAIM_LIMIT)
//         {
//             rareFreeButtonText.text = $"{RARE_FREE_CLAIM_LIMIT}/{RARE_FREE_CLAIM_LIMIT}";
//             return;
//         }

//         rareFreeButtonText.text = $"{claimCount}/{RARE_FREE_CLAIM_LIMIT}";

//         if (storeMenu.IsMonthlyCardActive())
//         {
//             ExecuteFreeClaim(PREF_RARE_FREE_CLAIM_COUNT, claimCount, DrawOneRareGem, rareFreeButtonText, RARE_FREE_CLAIM_LIMIT);
//         }
//         else
//         {
//             ShowAdForFreeClaim("gacha_rare", PREF_RARE_FREE_CLAIM_COUNT, claimCount, DrawOneRareGem, rareFreeButtonText, RARE_FREE_CLAIM_LIMIT);
//         }
//     }

//     public void DrawOneRareGem()
//     {
//         if (!ValidateBlockReferences()) return;

//         var player = PlayerProfile.Data.Player;
//         var result = DeductResources(player, "rareKey", KEY_REQUIRED_FOR_X1, RARE_X1_DIAMOND_COST);
        
//         if (result.success)
//         {
//             ShowSingleBlock(GEM_STONE_PATH, true);
//             storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Items/rareKey");
//             lastDraw = GachaType.RareGem;
//         }
//         else if (result.needDiamonds)
//         {
//             diamondStore.Open();
//         }
//     }

//     public void DrawTenRareGem()
//     {
//         var player = PlayerProfile.Data.Player;
//         var result = DeductResources(player, "rareKey", KEY_REQUIRED_FOR_X10, RARE_X10_DIAMOND_COST);
        
//         if (result.success)
//         {
//             GenerateGachaBlocks(GEM_STONE_PATH, true);
//             storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Items/rareKey");
//             lastDraw = GachaType.RareGem;
//         }
//         else if (result.needDiamonds)
//         {
//             diamondStore.Open();
//         }
//     }
//     #endregion

//     #region Epic Gacha Methods
//     public void DrawFreeEpicGem()
//     {
//         int claimCount = PlayerPrefs.GetInt(PREF_EPIC_FREE_CLAIM_COUNT, 0);

//         if (claimCount >= EPIC_FREE_CLAIM_LIMIT)
//         {
//             epicFreeButtonText.text = $"{EPIC_FREE_CLAIM_LIMIT}/{EPIC_FREE_CLAIM_LIMIT}";
//             return;
//         }

//         epicFreeButtonText.text = $"{claimCount}/{EPIC_FREE_CLAIM_LIMIT}";

//         if (storeMenu.IsMonthlyCardActive())
//         {
//             ExecuteFreeClaim(PREF_EPIC_FREE_CLAIM_COUNT, claimCount, DrawOneEpicGem, epicFreeButtonText, EPIC_FREE_CLAIM_LIMIT);
//         }
//         else
//         {
//             ShowAdForFreeClaim("gacha_epic", PREF_EPIC_FREE_CLAIM_COUNT, claimCount, DrawOneEpicGem, epicFreeButtonText, EPIC_FREE_CLAIM_LIMIT);
//         }
//     }

//     public void DrawOneEpicGem()
//     {
//         if (!ValidateBlockReferences()) return;

//         var player = PlayerProfile.Data.Player;
//         var result = DeductResources(player, "epicKey", KEY_REQUIRED_FOR_X1, EPIC_X1_DIAMOND_COST);
        
//         if (result.success)
//         {
//             ShowSingleBlock(GEM_STONE_PATH, true);
//             storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Items/epicKey");
//             lastDraw = GachaType.EpicGem;
//         }
//         else if (result.needDiamonds)
//         {
//             diamondStore.Open();
//         }
//     }

//     public void DrawTenEpicGem()
//     {
//         var player = PlayerProfile.Data.Player;
//         var result = DeductResources(player, "epicKey", KEY_REQUIRED_FOR_X10, EPIC_X10_DIAMOND_COST);
        
//         if (result.success)
//         {
//             GenerateGachaBlocks(GEM_STONE_PATH, true);
//             storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Items/epicKey");
//             lastDraw = GachaType.EpicGem;
//         }
//         else if (result.needDiamonds)
//         {
//             diamondStore.Open();
//         }
//     }
//     #endregion

//     #region Utility Methods
//     private bool ValidateBlockReferences()
//     {
//         if (blockImage == null || blockPart == null)
//         {
//             Debug.LogError("❌ Block Image or Part reference is missing!");
//             return false;
//         }
//         return true;
//     }

//     private void ExecuteFreeClaim(string prefKey, int currentCount, System.Action drawAction, TextMeshProUGUI buttonText, int limit)
//     {
//         PlayerPrefs.SetInt(prefKey, currentCount + 1);
//         drawAction();
//         buttonText.text = $"{currentCount + 1}/{limit}";
//         storeMenu?.UpdateFreeClaimStatus();
//     }

//     private void ShowAdForFreeClaim(string adType, string prefKey, int currentCount, System.Action drawAction, TextMeshProUGUI buttonText, int limit)
//     {
//         if (GoogleMobileAdsScript.This.CheckRewardedAd())
//         {
//             GoogleMobileAdsScript.This.ShowRewardedAd(adType, () =>
//             {
//                 ExecuteFreeClaim(prefKey, currentCount, drawAction, buttonText, limit);
//             });
//         }
//         else
//         {
//             Debug.Log("Rewarded ad not available, showing interstitial ad instead.");
//             GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
//             {
//                 ExecuteFreeClaim(prefKey, currentCount, drawAction, buttonText, limit);
//             });
//         }
//     }

//     // Updated DeductResources method to return a result struct
//     private (bool success, bool needDiamonds) DeductResources(Player player, string keyType, int keyRequired, int diamondCost)
//     {
//         int keyCount = player.ItemsJson.TryGetValue(keyType, out int key) ? key : 0;

//         if (keyCount >= keyRequired)
//         {
//             player.ItemsJson[keyType] = keyCount - keyRequired;
//             Debug.Log($"Deducted {keyRequired} {keyType}.");
//             PlayerProfile.Data.NotifyListeners("Player");
//             return (true, false);
//         }
//         else if (player.Diamond >= diamondCost)
//         {
//             player.Diamond -= diamondCost;
//             Debug.Log($"Deducted {diamondCost} diamonds locally.");
//             PlayerProfile.Data.NotifyListeners("Player");
//             _wsSocketApi.Action("add_gold", new { type = diamondCost.ToString() }, AfterGachaAction);
//             return (true, false);
//         }
//         else
//         {
//             Debug.Log($"💎 Not enough {keyType} or diamonds - opening diamond store.");
//             return (false, true);
//         }
//     }

//     private void ShowSingleBlock(string imagePath, bool isGem)
//     {
//         OpenGachaPage();
//         block.SetActive(true);

//         // Load main image
//         Sprite[] images = Resources.LoadAll<Sprite>(imagePath);
//         if (images.Length > 0)
//         {
//             blockImage.sprite = images[UnityEngine.Random.Range(0, images.Length)];
//             blockImage.gameObject.SetActive(true);
//         }
//         else
//         {
//             Debug.LogError($"❌ No images found at {imagePath}!");
//         }

//         // Handle part image for gems (but disable as per original code)
//         if (isGem)
//         {
//             Sprite[] partSprites = Resources.LoadAll<Sprite>(GEM_PART_PATH);
//             if (partSprites.Length > 0)
//             {
//                 blockPart.sprite = partSprites[UnityEngine.Random.Range(0, partSprites.Length)];
//             }
//         }
        
//         // Disable part for all types as per original code
//         blockPart.gameObject.SetActive(false);
//     }

//     private void GenerateGachaBlocks(string imagePath, bool isGem)
//     {
//         OpenGachaPage();
//         grid.SetActive(true);
//         ClearGrid();

//         Sprite[] images = Resources.LoadAll<Sprite>(imagePath);
//         Sprite[] partImages = isGem ? Resources.LoadAll<Sprite>(GEM_PART_PATH) : null;

//         for (int i = 0; i < BLOCKS_PER_DRAW; i++)
//         {
//             GameObject newBlock = Instantiate(blockPrefab, grid.transform);
//             newBlock.name = $"Block_{i + 1}";

//             SetupBlockImages(newBlock, images, partImages, isGem);
//         }

//         AdjustGridLayout();
//     }

//     private void SetupBlockImages(GameObject block, Sprite[] images, Sprite[] partImages, bool isGem)
//     {
//         Image blockImage = block.transform.Find("Image")?.GetComponent<Image>();
//         Transform partTransform = block.transform.Find("Part");
//         Image partImage = partTransform?.GetComponent<Image>();

//         // Set main image
//         if (blockImage != null && images.Length > 0)
//         {
//             blockImage.sprite = images[UnityEngine.Random.Range(0, images.Length)];
//             blockImage.color = Color.white;
//         }

//         // Set part image for gems but keep disabled as per original code
//         if (isGem && partImage != null && partImages != null && partImages.Length > 0)
//         {
//             partImage.sprite = partImages[UnityEngine.Random.Range(0, partImages.Length)];
//             partImage.color = Color.white;
//         }

//         // Always disable part as per original code
//         if (partTransform != null)
//         {
//             partTransform.gameObject.SetActive(false);
//         }
//     }

//     private void AdjustGridLayout()
//     {
//         RectTransform gridRect = grid.GetComponent<RectTransform>();
//         float panelWidth = gridRect.rect.width;

//         // Calculate block width and padding dynamically
//         float blockWidth = panelWidth / (BLOCKS_PER_ROW + 1);
//         float leftPadding = blockWidth * 0.25f;
//         float rightPadding = blockWidth * 0.25f;
//         float spacingX = blockWidth * 0.125f;
//         float spacingY = blockWidth * 0.125f;

//         // Adjust GridLayoutGroup settings
//         Grid.cellSize = new Vector2(blockWidth, blockWidth);
//         Grid.spacing = new Vector2(spacingX, spacingY);
//         Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//         Grid.constraintCount = BLOCKS_PER_ROW;

//         // Apply padding to the grid
//         Grid.padding.left = Mathf.RoundToInt(leftPadding);
//         Grid.padding.right = Mathf.RoundToInt(rightPadding);
//         Grid.padding.top = Mathf.RoundToInt(spacingY);
//         Grid.padding.bottom = Mathf.RoundToInt(spacingY);

//         // Calculate total rows dynamically (2 rows for 10 blocks)
//         int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)BLOCKS_PER_DRAW / BLOCKS_PER_ROW));

//         // Adjust grid size to fit the blocks without moving its position
//         float contentHeight = totalRows * (blockWidth + spacingY) - spacingY;
//         gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
//     }
//     #endregion

//     #region Repeat Draw Methods
//     public void DrawOneAgain()
//     {
//         CloseGachaPage();
//         grid.SetActive(false);

//         switch (lastDraw)
//         {
//             case GachaType.Shard:
//                 DrawOneShard();
//                 break;
//             case GachaType.RareGem:
//                 DrawOneRareGem();
//                 break;
//             case GachaType.EpicGem:
//                 DrawOneEpicGem();
//                 break;
//             default:
//                 Debug.LogWarning("⚠ No previous draw detected!");
//                 break;
//         }
//     }

//     public void DrawTenAgain()
//     {
//         CloseGachaPage();
//         block.SetActive(false);

//         switch (lastDraw)
//         {
//             case GachaType.Shard:
//                 DrawTenShard();
//                 break;
//             case GachaType.RareGem:
//                 DrawTenRareGem();
//                 break;
//             case GachaType.EpicGem:
//                 DrawTenEpicGem();
//                 break;
//             default:
//                 Debug.LogWarning("⚠ No previous draw detected!");
//                 break;
//         }
//     }
//     #endregion

//     public void AfterGachaAction(JObject _object)
//     {
//         int diamond = _object.GetValue("diamond").Value<int>();
//         Debug.Log($"AfterGachaAction: diamond={diamond}");

//         PlayerProfile.Data.Player.Diamond = diamond;
//         PlayerProfile.Data.NotifyListeners("Player");
//     }
// }




using UnityEngine;
using UnityEngine.UI;
using TMPro;
using model;
using System;
using Newtonsoft.Json.Linq;
using WebSocket;

public class GachaController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gachaPage;
    public GameObject block;
    public Image blockImage;
    public Image blockPart;
    public GameObject grid;
    public GameObject blockPrefab;
    public GridLayoutGroup Grid;

    [Header("Free Claim UI")]
    public TextMeshProUGUI heroFreeButtonText;
    public TextMeshProUGUI rareFreeButtonText;
    public TextMeshProUGUI epicFreeButtonText;

    [Header("Dependencies")]
    [SerializeField] private StoreMenuController storeMenu;

    // Constants
    private const int BLOCKS_PER_ROW = 5;
    private const int BLOCKS_PER_DRAW = 10;
    
    // Diamond costs
    private const int HERO_X1_DIAMOND_COST = 300;
    private const int HERO_X10_DIAMOND_COST = 3000;
    private const int RARE_X1_DIAMOND_COST = 180;
    private const int RARE_X10_DIAMOND_COST = 1800;
    private const int EPIC_X1_DIAMOND_COST = 200;
    private const int EPIC_X10_DIAMOND_COST = 2000;

    // Key requirements
    private const int KEY_REQUIRED_FOR_X1 = 1;
    private const int KEY_REQUIRED_FOR_X10 = 10;

    // Free claim limits
    private const int HERO_FREE_CLAIM_LIMIT = 1;
    private const int RARE_FREE_CLAIM_LIMIT = 3;
    private const int EPIC_FREE_CLAIM_LIMIT = 3;

    // PlayerPrefs keys
    private const string PREF_HERO_FREE_CLAIM_COUNT = "HeroFreeClaimCount";
    private const string PREF_RARE_FREE_CLAIM_COUNT = "RareFreeClaimCount";
    private const string PREF_EPIC_FREE_CLAIM_COUNT = "EpicFreeClaimCount";

    // Resource paths
    private const string SHARD_PATH = "UILoading/CharacterImages/Shard";
    private const string GEM_STONE_PATH = "UILoading/Gem/Stone";
    private const string GEM_PART_PATH = "UILoading/Gem/Part";

    private enum GachaType { None, Shard, RareGem, EpicGem }
    private GachaType lastDraw = GachaType.None;

    private static PurchaseWebSocketApi _wsSocketApi;
    [SerializeField] private DiamondStore diamondStore;

    private void Start()
    {
        _wsSocketApi = PurchaseWebSocketApi.Instance;
    }

    #region Page Management
    public void OpenGachaPage()
    {
        gachaPage.SetActive(true);
    }

    public void CloseGachaPage()
    {
        gachaPage.SetActive(false);
        grid.SetActive(false);
        block.SetActive(false);

        ClearGrid();
        PlayerProfile.Data.NotifyListeners("Player");
    }

    private void ClearGrid()
    {
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region Core Draw Execution Methods
    private void ExecuteShardDraw()
    {
        if (!ValidateBlockReferences()) return;
        
        ShowSingleBlock(SHARD_PATH, false);
        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson["heroKey"], HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Items/heroKey");
        lastDraw = GachaType.Shard;
    }

    private void ExecuteRareGemDraw()
    {
        if (!ValidateBlockReferences()) return;
        
        ShowSingleBlock(GEM_STONE_PATH, true);
        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Items/rareKey");
        lastDraw = GachaType.RareGem;
    }

    private void ExecuteEpicGemDraw()
    {
        if (!ValidateBlockReferences()) return;
        
        ShowSingleBlock(GEM_STONE_PATH, true);
        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Items/epicKey");
        lastDraw = GachaType.EpicGem;
    }

    private void ExecuteShardTenDraw()
    {
        GenerateGachaBlocks(SHARD_PATH, false);
        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson["heroKey"], HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Items/heroKey");
        lastDraw = GachaType.Shard;
    }

    private void ExecuteRareGemTenDraw()
    {
        GenerateGachaBlocks(GEM_STONE_PATH, true);
        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Items/rareKey");
        lastDraw = GachaType.RareGem;
    }

    private void ExecuteEpicGemTenDraw()
    {
        GenerateGachaBlocks(GEM_STONE_PATH, true);
        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Items/epicKey");
        lastDraw = GachaType.EpicGem;
    }
    #endregion

    #region Hero Gacha Methods
    public void DrawFreeShard()
    {
        int claimCount = PlayerPrefs.GetInt(PREF_HERO_FREE_CLAIM_COUNT, 0);

        if (claimCount >= HERO_FREE_CLAIM_LIMIT)
        {
            heroFreeButtonText.text = $"{HERO_FREE_CLAIM_LIMIT}/{HERO_FREE_CLAIM_LIMIT}";
            return;
        }

        heroFreeButtonText.text = $"{claimCount}/{HERO_FREE_CLAIM_LIMIT}";

        if (storeMenu.IsMonthlyCardActive())
        {
            // Direct execution for monthly card users
            PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, claimCount + 1);
            ExecuteShardDraw();
            heroFreeButtonText.text = $"{claimCount + 1}/{HERO_FREE_CLAIM_LIMIT}";
            storeMenu?.UpdateFreeClaimStatus();
        }
        else
        {
            // Show ad first, then execute
            if (GoogleMobileAdsScript.This.CheckRewardedAd())
            {
                GoogleMobileAdsScript.This.ShowRewardedAd("gacha_shard", () =>
                {
                    PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, claimCount + 1);
                    ExecuteShardDraw();
                    heroFreeButtonText.text = $"{claimCount + 1}/{HERO_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
            else
            {
                Debug.Log("Rewarded ad not available, showing interstitial ad instead.");
                GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
                {
                    PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, claimCount + 1);
                    ExecuteShardDraw();
                    heroFreeButtonText.text = $"{claimCount + 1}/{HERO_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
        }
    }

    public void DrawOneShard()
    {
        var player = PlayerProfile.Data.Player;
        var result = DeductResources(player, "heroKey", KEY_REQUIRED_FOR_X1, HERO_X1_DIAMOND_COST);
        
        if (result.success)
        {
            ExecuteShardDraw();
        }
        else if (result.needDiamonds)
        {
            diamondStore.Open();
        }
    }

    public void DrawTenShard()
    {
        var player = PlayerProfile.Data.Player;
        var result = DeductResources(player, "heroKey", KEY_REQUIRED_FOR_X10, HERO_X10_DIAMOND_COST);
        
        if (result.success)
        {
            ExecuteShardTenDraw();
        }
        else if (result.needDiamonds)
        {
            diamondStore.Open();
        }
    }
    #endregion

    #region Rare Gacha Methods
    public void DrawFreeRareGem()
    {
        int claimCount = PlayerPrefs.GetInt(PREF_RARE_FREE_CLAIM_COUNT, 0);

        if (claimCount >= RARE_FREE_CLAIM_LIMIT)
        {
            rareFreeButtonText.text = $"{RARE_FREE_CLAIM_LIMIT}/{RARE_FREE_CLAIM_LIMIT}";
            return;
        }

        rareFreeButtonText.text = $"{claimCount}/{RARE_FREE_CLAIM_LIMIT}";

        if (storeMenu.IsMonthlyCardActive())
        {
            PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, claimCount + 1);
            ExecuteRareGemDraw();
            rareFreeButtonText.text = $"{claimCount + 1}/{RARE_FREE_CLAIM_LIMIT}";
            storeMenu?.UpdateFreeClaimStatus();
        }
        else
        {
            if (GoogleMobileAdsScript.This.CheckRewardedAd())
            {
                GoogleMobileAdsScript.This.ShowRewardedAd("gacha_rare", () =>
                {
                    PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, claimCount + 1);
                    ExecuteRareGemDraw();
                    rareFreeButtonText.text = $"{claimCount + 1}/{RARE_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
            else
            {
                Debug.Log("Rewarded ad not available, showing interstitial ad instead.");
                GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
                {
                    PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, claimCount + 1);
                    ExecuteRareGemDraw();
                    rareFreeButtonText.text = $"{claimCount + 1}/{RARE_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
        }
    }

    public void DrawOneRareGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = DeductResources(player, "rareKey", KEY_REQUIRED_FOR_X1, RARE_X1_DIAMOND_COST);
        
        if (result.success)
        {
            ExecuteRareGemDraw();
        }
        else if (result.needDiamonds)
        {
            diamondStore.Open();
        }
    }

    public void DrawTenRareGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = DeductResources(player, "rareKey", KEY_REQUIRED_FOR_X10, RARE_X10_DIAMOND_COST);
        
        if (result.success)
        {
            ExecuteRareGemTenDraw();
        }
        else if (result.needDiamonds)
        {
            diamondStore.Open();
        }
    }
    #endregion

    #region Epic Gacha Methods
    public void DrawFreeEpicGem()
    {
        int claimCount = PlayerPrefs.GetInt(PREF_EPIC_FREE_CLAIM_COUNT, 0);

        if (claimCount >= EPIC_FREE_CLAIM_LIMIT)
        {
            epicFreeButtonText.text = $"{EPIC_FREE_CLAIM_LIMIT}/{EPIC_FREE_CLAIM_LIMIT}";
            return;
        }

        epicFreeButtonText.text = $"{claimCount}/{EPIC_FREE_CLAIM_LIMIT}";

        if (storeMenu.IsMonthlyCardActive())
        {
            PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, claimCount + 1);
            ExecuteEpicGemDraw();
            epicFreeButtonText.text = $"{claimCount + 1}/{EPIC_FREE_CLAIM_LIMIT}";
            storeMenu?.UpdateFreeClaimStatus();
        }
        else
        {
            if (GoogleMobileAdsScript.This.CheckRewardedAd())
            {
                GoogleMobileAdsScript.This.ShowRewardedAd("gacha_epic", () =>
                {
                    PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, claimCount + 1);
                    ExecuteEpicGemDraw();
                    epicFreeButtonText.text = $"{claimCount + 1}/{EPIC_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
            else
            {
                Debug.Log("Rewarded ad not available, showing interstitial ad instead.");
                GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
                {
                    PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, claimCount + 1);
                    ExecuteEpicGemDraw();
                    epicFreeButtonText.text = $"{claimCount + 1}/{EPIC_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
        }
    }

    public void DrawOneEpicGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = DeductResources(player, "epicKey", KEY_REQUIRED_FOR_X1, EPIC_X1_DIAMOND_COST);
        
        if (result.success)
        {
            ExecuteEpicGemDraw();
        }
        else if (result.needDiamonds)
        {
            diamondStore.Open();
        }
    }

    public void DrawTenEpicGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = DeductResources(player, "epicKey", KEY_REQUIRED_FOR_X10, EPIC_X10_DIAMOND_COST);
        
        if (result.success)
        {
            ExecuteEpicGemTenDraw();
        }
        else if (result.needDiamonds)
        {
            diamondStore.Open();
        }
    }
    #endregion

    #region Utility Methods
    private bool ValidateBlockReferences()
    {
        if (blockImage == null || blockPart == null)
        {
            Debug.LogError("❌ Block Image or Part reference is missing!");
            return false;
        }
        return true;
    }

    // Updated DeductResources method to return a result struct
    private (bool success, bool needDiamonds) DeductResources(Player player, string keyType, int keyRequired, int diamondCost)
    {
        int keyCount = player.ItemsJson.TryGetValue(keyType, out int key) ? key : 0;

        if (keyCount >= keyRequired)
        {
            player.ItemsJson[keyType] = keyCount - keyRequired;
            Debug.Log($"Deducted {keyRequired} {keyType}.");
            PlayerProfile.Data.NotifyListeners("Player");
            return (true, false);
        }
        else if (player.Diamond >= diamondCost)
        {
            player.Diamond -= diamondCost;
            Debug.Log($"Deducted {diamondCost} diamonds locally.");
            PlayerProfile.Data.NotifyListeners("Player");
            _wsSocketApi.Action("add_gold", new { type = diamondCost.ToString() }, AfterGachaAction);
            return (true, false);
        }
        else
        {
            Debug.Log($"💎 Not enough {keyType} or diamonds - opening diamond store.");
            return (false, true);
        }
    }

    private void ShowSingleBlock(string imagePath, bool isGem)
    {
        OpenGachaPage();
        block.SetActive(true);

        // Load main image
        Sprite[] images = Resources.LoadAll<Sprite>(imagePath);
        if (images.Length > 0)
        {
            blockImage.sprite = images[UnityEngine.Random.Range(0, images.Length)];
            blockImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"❌ No images found at {imagePath}!");
        }

        // Handle part image for gems (but disable as per original code)
        if (isGem)
        {
            Sprite[] partSprites = Resources.LoadAll<Sprite>(GEM_PART_PATH);
            if (partSprites.Length > 0)
            {
                blockPart.sprite = partSprites[UnityEngine.Random.Range(0, partSprites.Length)];
            }
        }
        
        // Disable part for all types as per original code
        blockPart.gameObject.SetActive(false);
    }

    private void GenerateGachaBlocks(string imagePath, bool isGem)
    {
        OpenGachaPage();
        grid.SetActive(true);
        ClearGrid();

        Sprite[] images = Resources.LoadAll<Sprite>(imagePath);
        Sprite[] partImages = isGem ? Resources.LoadAll<Sprite>(GEM_PART_PATH) : null;

        for (int i = 0; i < BLOCKS_PER_DRAW; i++)
        {
            GameObject newBlock = Instantiate(blockPrefab, grid.transform);
            newBlock.name = $"Block_{i + 1}";

            SetupBlockImages(newBlock, images, partImages, isGem);
        }

        AdjustGridLayout();
    }

    private void SetupBlockImages(GameObject block, Sprite[] images, Sprite[] partImages, bool isGem)
    {
        Image blockImage = block.transform.Find("Image")?.GetComponent<Image>();
        Transform partTransform = block.transform.Find("Part");
        Image partImage = partTransform?.GetComponent<Image>();

        // Set main image
        if (blockImage != null && images.Length > 0)
        {
            blockImage.sprite = images[UnityEngine.Random.Range(0, images.Length)];
            blockImage.color = Color.white;
        }

        // Set part image for gems but keep disabled as per original code
        if (isGem && partImage != null && partImages != null && partImages.Length > 0)
        {
            partImage.sprite = partImages[UnityEngine.Random.Range(0, partImages.Length)];
            partImage.color = Color.white;
        }

        // Always disable part as per original code
        if (partTransform != null)
        {
            partTransform.gameObject.SetActive(false);
        }
    }

    private void AdjustGridLayout()
    {
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        float panelWidth = gridRect.rect.width;

        // Calculate block width and padding dynamically
        float blockWidth = panelWidth / (BLOCKS_PER_ROW + 1);
        float leftPadding = blockWidth * 0.25f;
        float rightPadding = blockWidth * 0.25f;
        float spacingX = blockWidth * 0.125f;
        float spacingY = blockWidth * 0.125f;

        // Adjust GridLayoutGroup settings
        Grid.cellSize = new Vector2(blockWidth, blockWidth);
        Grid.spacing = new Vector2(spacingX, spacingY);
        Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Grid.constraintCount = BLOCKS_PER_ROW;

        // Apply padding to the grid
        Grid.padding.left = Mathf.RoundToInt(leftPadding);
        Grid.padding.right = Mathf.RoundToInt(rightPadding);
        Grid.padding.top = Mathf.RoundToInt(spacingY);
        Grid.padding.bottom = Mathf.RoundToInt(spacingY);

        // Calculate total rows dynamically (2 rows for 10 blocks)
        int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)BLOCKS_PER_DRAW / BLOCKS_PER_ROW));

        // Adjust grid size to fit the blocks without moving its position
        float contentHeight = totalRows * (blockWidth + spacingY) - spacingY;
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
    }
    #endregion

    #region Repeat Draw Methods
    public void DrawOneAgain()
    {
        CloseGachaPage();
        grid.SetActive(false);

        switch (lastDraw)
        {
            case GachaType.Shard:
                DrawOneShard();
                break;
            case GachaType.RareGem:
                DrawOneRareGem();
                break;
            case GachaType.EpicGem:
                DrawOneEpicGem();
                break;
            default:
                Debug.LogWarning("⚠ No previous draw detected!");
                break;
        }
    }

    public void DrawTenAgain()
    {
        CloseGachaPage();
        block.SetActive(false);

        switch (lastDraw)
        {
            case GachaType.Shard:
                DrawTenShard();
                break;
            case GachaType.RareGem:
                DrawTenRareGem();
                break;
            case GachaType.EpicGem:
                DrawTenEpicGem();
                break;
            default:
                Debug.LogWarning("⚠ No previous draw detected!");
                break;
        }
    }
    #endregion

    public void AfterGachaAction(JObject _object)
    {
        int diamond = _object.GetValue("diamond").Value<int>();
        Debug.Log($"AfterGachaAction: diamond={diamond}");

        PlayerProfile.Data.Player.Diamond = diamond;
        PlayerProfile.Data.NotifyListeners("Player");
    }
}