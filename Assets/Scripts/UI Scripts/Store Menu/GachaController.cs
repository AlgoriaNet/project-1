using UnityEngine;
using UnityEngine.UI;
using TMPro;
using model;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using WebSocket;
using UI_Controller;

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
    [SerializeField] private StartGame startGame;
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

    [SerializeField] private DiamondStore diamondStore;

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

        if (startGame.IsMonthlyCardActive())
        {
            // Direct execution for monthly card users
            PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, claimCount + 1);
            DrawController.Instance.Draw("hero", "ad", 1);
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
                    DrawController.Instance.Draw("hero", "ad", 1);
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
                    DrawController.Instance.Draw("hero", "ad", 1);
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
        var result = ResourceCheck(player, "heroKey", KEY_REQUIRED_FOR_X1, HERO_X1_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("hero", "key", 1); // Use keys
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("hero", "diamond", 1); // Use diamonds
            }

            ExecuteShardDraw();
        }
        else
        {
            diamondStore.Open(); // Open diamond store when there aren't enough resources
        }
    }

    public void DrawTenShard()
    {
        var player = PlayerProfile.Data.Player;
        var result = ResourceCheck(player, "heroKey", KEY_REQUIRED_FOR_X10, HERO_X10_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("hero", "key", 10); // Use keys
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("hero", "diamond", 10); // Use diamonds
            }

            ExecuteShardTenDraw();
        }
        else
        {
            diamondStore.Open(); // Open diamond store when there aren't enough resources
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

        if (startGame.IsMonthlyCardActive())
        {
            PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, claimCount + 1);
            DrawController.Instance.Draw("rare gem", "ad", 1);
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
                    DrawController.Instance.Draw("rare gem", "ad", 1);
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
                    DrawController.Instance.Draw("rare gem", "ad", 1);
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
        var result = ResourceCheck(player, "rareKey", KEY_REQUIRED_FOR_X1, RARE_X1_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("rare gem", "key", 1); // Use keys
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("rare gem", "diamond", 1); // Use diamonds
            }

            ExecuteRareGemDraw();
        }
        else
        {
            diamondStore.Open(); // Open diamond store when there aren't enough resources
        }
    }

    public void DrawTenRareGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = ResourceCheck(player, "rareKey", KEY_REQUIRED_FOR_X1, RARE_X1_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("rare gem", "key", 10); // Use keys
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("rare gem", "diamond", 10); // Use diamonds
            }

            ExecuteRareGemTenDraw();
        }
        else
        {
            diamondStore.Open(); // Open diamond store when there aren't enough resources
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

        if (startGame.IsMonthlyCardActive())
        {
            PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, claimCount + 1);
            DrawController.Instance.Draw("epic gem", "ad", 1);
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
                    DrawController.Instance.Draw("epic gem", "ad", 1);
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
                    DrawController.Instance.Draw("epic gem", "ad", 1);
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
        var result = ResourceCheck(player, "epicKey", KEY_REQUIRED_FOR_X1, EPIC_X1_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("epic gem", "key", 1); // Use keys
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("epic gem", "diamond", 1); // Use diamonds
            }

            ExecuteEpicGemDraw();
        }
        else
        {
            diamondStore.Open(); // Open diamond store when there aren't enough resources
        }
    }


    public void DrawTenEpicGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = ResourceCheck(player, "epicKey", KEY_REQUIRED_FOR_X1, EPIC_X1_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("epic gem", "key", 10); // Use keys
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("epic gem", "diamond", 10); // Use diamonds
            }

            ExecuteEpicGemTenDraw();
        }
        else
        {
            diamondStore.Open(); // Open diamond store when there aren't enough resources
        }
    }
    #endregion

    #region Core Draw Execution Methods
    public void ExecuteShardDraw()
    {
        if (!ValidateBlockReferences()) return;

        var items = PlayerProfile.Data.Player.ItemsJson;

        string fileName = items
            .Keys
            .FirstOrDefault(k => k.StartsWith("SKb_") || char.IsDigit(k[0]));

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.Log("❌ No valid file name for shard or skillbook item!");
            return;
        }

        string imagePath = fileName.StartsWith("SKb_")
            ? $"UILoading/CharacterImages/Skillbook/{fileName}"
            : $"UILoading/CharacterImages/Shard/{fileName}";

        Sprite sprite = Resources.Load<Sprite>(imagePath);

        if (sprite != null)
        {
            blockImage.sprite = sprite;
            blockImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"❌ Image not found at {imagePath}");
        }

        blockPart.gameObject.SetActive(false);
        OpenGachaPage();
        block.SetActive(true);

        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson["heroKey"], HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Items/heroKey");
        lastDraw = GachaType.Shard;
    }


    public void ExecuteRareGemDraw()
    {
        if (!ValidateBlockReferences()) return;

        var gems = PlayerProfile.Data.Player.Gemstones;
        if (gems == null || gems.Count == 0)
        {
            Debug.LogError("❌ No gemstone data found!");
            return;
        }

        var gem = gems[gems.Count - 1];  // Last drawn gem

        // Load gem image by Level → Gem_01, Gem_02, ...
        string imageName = $"Gem_{gem.Level:00}";
        Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/{imageName}");

        if (gemSprite != null)
        {
            blockImage.sprite = gemSprite;
            blockImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"❌ Gem image not found: {imageName}");
        }

        // Load part image using Part name
        Sprite partSprite = Resources.Load<Sprite>($"UILoading/Gem/Part/{gem.Part}");
        if (partSprite != null)
        {
            blockPart.sprite = partSprite;
            blockPart.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"❌ Part image not found: {gem.Part}");
        }

        OpenGachaPage();
        block.SetActive(true);

        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Items/rareKey");
        lastDraw = GachaType.RareGem;
    }

    public void ExecuteEpicGemDraw()
    {
        if (!ValidateBlockReferences()) return;

        var gems = PlayerProfile.Data.Player.Gemstones;
        if (gems == null || gems.Count == 0)
        {
            Debug.LogError("❌ No gemstone data found!");
            return;
        }

        var gem = gems[gems.Count - 1];  // Last drawn gem

        // Load gem image by Level → Gem_01, Gem_02, ...
        string imageName = $"Gem_{gem.Level:00}";
        Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/{imageName}");

        if (gemSprite != null)
        {
            blockImage.sprite = gemSprite;
            blockImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"❌ Gem image not found: {imageName}");
        }

        // Load part image using Part name
        Sprite partSprite = Resources.Load<Sprite>($"UILoading/Gem/Part/{gem.Part}");
        if (partSprite != null)
        {
            blockPart.sprite = partSprite;
            blockPart.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"❌ Part image not found: {gem.Part}");
        }

        OpenGachaPage();
        block.SetActive(true);

        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Items/epicKey");
        lastDraw = GachaType.EpicGem;
    }

    public void ExecuteShardTenDraw()
    {
        GenerateTenShards();
        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson["heroKey"], HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Items/heroKey");
        lastDraw = GachaType.Shard;
    }

    public void ExecuteRareGemTenDraw()
    {
        if (PlayerProfile.Data.Player.Gemstones == null || PlayerProfile.Data.Player.Gemstones.Count < 10)
        {
            Debug.LogError("❌ Not enough gems returned from server to display 10.");
            return;
        }

        GenerateTenGems();

        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Items/rareKey");
        lastDraw = GachaType.RareGem;
    }


    public void ExecuteEpicGemTenDraw()
    {
        if (PlayerProfile.Data.Player.Gemstones == null || PlayerProfile.Data.Player.Gemstones.Count < 10)
        {
            Debug.LogError("❌ Not enough gems returned from server to display 10.");
            return;
        }

        GenerateTenGems();

        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Items/epicKey");
        lastDraw = GachaType.EpicGem;
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

    private (bool isSuccess, bool isKey, bool isDiamond) ResourceCheck(Player player, string keyType, int keyRequired, int diamondCost)
    {
        int keyCount = player.ItemsJson.TryGetValue(keyType, out int key) ? key : 0;

        string drawType = keyType.Contains("epic") ? "epic gem" :
                        keyType.Contains("rare") ? "rare gem" :
                        keyType.Contains("hero") ? "hero" : "shard";

        if (keyCount >= keyRequired)
        {
            // Enough keys, return success and that it uses keys
            return (true, true, false);
        }
        else if (player.Diamond >= diamondCost)
        {
            // Not enough keys, but enough diamonds, return success and that it uses diamonds
            return (true, false, true);
        }
        else
        {
            // Not enough keys or diamonds
            return (false, false, false);
        }
    }

    private void GenerateTenShards()
    {
        OpenGachaPage();
        grid.SetActive(true);
        ClearGrid();

        var items = PlayerProfile.Data.Player.ItemsJson;
        var keys = items.Keys.Where(k => k.StartsWith("SKb_") || char.IsDigit(k[0])).TakeLast(10).ToList();

        for (int i = 0; i < keys.Count; i++)
        {
            string fileName = keys[i];

            GameObject newBlock = Instantiate(blockPrefab, grid.transform);
            newBlock.name = $"Block_{i + 1}";

            Image blockImage = newBlock.transform.Find("Image")?.GetComponent<Image>();
            Image partImage = newBlock.transform.Find("Part")?.GetComponent<Image>();

            string imagePath = fileName.StartsWith("SKb_")
                ? $"UILoading/CharacterImages/Skillbook/{fileName}"
                : $"UILoading/CharacterImages/Shard/{fileName}";

            Sprite sprite = Resources.Load<Sprite>(imagePath);

            if (blockImage != null && sprite != null)
            {
                blockImage.sprite = sprite;
                blockImage.color = Color.white;
            }
        }

        AdjustGridLayout();
    }


    private void GenerateTenGems()
    {
        OpenGachaPage();
        grid.SetActive(true);
        ClearGrid();

        var gems = PlayerProfile.Data.Player.Gemstones;
        for (int i = 0; i < 10; i++)
        {
            var gem = gems[gems.Count - 10 + i];

            GameObject newBlock = Instantiate(blockPrefab, grid.transform);
            newBlock.name = $"Block_{i + 1}";

            Image blockImage = newBlock.transform.Find("Image")?.GetComponent<Image>();
            Image partImage = newBlock.transform.Find("Part")?.GetComponent<Image>();

            string levelStr = gem.Level.ToString("D2");
            Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{levelStr}");
            Sprite partSprite = Resources.Load<Sprite>($"UILoading/Gem/Part/{gem.Part}");

            if (blockImage != null && gemSprite != null)
            {
                blockImage.sprite = gemSprite;
                blockImage.color = Color.white;
            }

            if (partImage != null && partSprite != null)
            {
                partImage.sprite = partSprite;
                partImage.color = Color.white;
                partImage.gameObject.SetActive(true);
            }
        }

        AdjustGridLayout();
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
}