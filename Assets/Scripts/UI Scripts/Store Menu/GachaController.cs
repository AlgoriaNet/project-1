using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using model;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using WebSocket;
using UI_Controller;
using System.Collections.Generic;

public class GachaController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gachaPage;
    public GameObject block;
    public Image blockImage;
    public Image blockPart;
    public TextMeshProUGUI qntyText; // New field for Qnty
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

    private const int BLOCKS_PER_ROW = 5;
    private const int BLOCKS_PER_DRAW = 10;

    private const int HERO_X1_DIAMOND_COST = 300;
    private const int HERO_X10_DIAMOND_COST = 3000;
    private const int RARE_X1_DIAMOND_COST = 180;
    private const int RARE_X10_DIAMOND_COST = 1800;
    private const int EPIC_X1_DIAMOND_COST = 200;
    private const int EPIC_X10_DIAMOND_COST = 2000;

    private const int KEY_REQUIRED_FOR_X1 = 1;
    private const int KEY_REQUIRED_FOR_X10 = 10;

    private const int HERO_FREE_CLAIM_LIMIT = 1;
    private const int RARE_FREE_CLAIM_LIMIT = 3;
    private const int EPIC_FREE_CLAIM_LIMIT = 3;

    private const string PREF_HERO_FREE_CLAIM_COUNT = "HeroFreeClaimCount";
    private const string PREF_RARE_FREE_CLAIM_COUNT = "RareFreeClaimCount";
    private const string PREF_EPIC_FREE_CLAIM_COUNT = "EpicFreeClaimCount";

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
            PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, claimCount + 1);
            DrawController.Instance.Draw("hero", "ad", 1);
            heroFreeButtonText.text = $"{claimCount + 1}/{HERO_FREE_CLAIM_LIMIT}";
            storeMenu?.UpdateFreeClaimStatus();
        }
        else
        {
            if (GoogleMobileAdsScript.This.CheckRewardedAd())
            {
                GoogleMobileAdsScript.This.ShowRewardedAd("gacha_shard", () =>
                {
                    PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, claimCount + 1);
                    DrawController.Instance.Draw("hero", "ad", 1);
                    heroFreeButtonText.text = $"{claimCount + 1}/{HERO_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
            else
            {
                GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
                {
                    PlayerPrefs.SetInt(PREF_HERO_FREE_CLAIM_COUNT, claimCount + 1);
                    DrawController.Instance.Draw("hero", "ad", 1);
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
                DrawController.Instance.Draw("hero", "key", 1);
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("hero", "diamond", 1);
            }
        }
        else
        {
            diamondStore.Open();
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
                DrawController.Instance.Draw("hero", "key", 10);
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("hero", "diamond", 10);
            }
        }
        else
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

        if (startGame.IsMonthlyCardActive())
        {
            PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, claimCount + 1);
            DrawController.Instance.Draw("rare gem", "ad", 1);
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
                    rareFreeButtonText.text = $"{claimCount + 1}/{RARE_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
            else
            {
                GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
                {
                    PlayerPrefs.SetInt(PREF_RARE_FREE_CLAIM_COUNT, claimCount + 1);
                    DrawController.Instance.Draw("rare gem", "ad", 1);
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
                DrawController.Instance.Draw("rare gem", "key", 1);
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("rare gem", "diamond", 1);
            }
        }
        else
        {
            diamondStore.Open();
        }
    }

    public void DrawTenRareGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = ResourceCheck(player, "rareKey", KEY_REQUIRED_FOR_X10, RARE_X10_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("rare gem", "key", 10);
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("rare gem", "diamond", 10);
            }
        }
        else
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

        if (startGame.IsMonthlyCardActive())
        {
            PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, claimCount + 1);
            DrawController.Instance.Draw("epic gem", "ad", 1);
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
                    epicFreeButtonText.text = $"{claimCount + 1}/{EPIC_FREE_CLAIM_LIMIT}";
                    storeMenu?.UpdateFreeClaimStatus();
                });
            }
            else
            {
                GoogleMobileAdsScript.This.ShowInterstitialAd(() =>
                {
                    PlayerPrefs.SetInt(PREF_EPIC_FREE_CLAIM_COUNT, claimCount + 1);
                    DrawController.Instance.Draw("epic gem", "ad", 1);
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
                DrawController.Instance.Draw("epic gem", "key", 1);
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("epic gem", "diamond", 1);
            }
        }
        else
        {
            diamondStore.Open();
        }
    }

    public void DrawTenEpicGem()
    {
        var player = PlayerProfile.Data.Player;
        var result = ResourceCheck(player, "epicKey", KEY_REQUIRED_FOR_X10, EPIC_X10_DIAMOND_COST);

        if (result.isSuccess)
        {
            if (result.isKey)
            {
                DrawController.Instance.Draw("epic gem", "key", 10);
            }
            else if (result.isDiamond)
            {
                DrawController.Instance.Draw("epic gem", "diamond", 10);
            }
        }
        else
        {
            diamondStore.Open();
        }
    }
    #endregion

    #region Core Draw Execution Methods
    public void ExecuteShardDraw()
    {
        if (!ValidateBlockReferences()) return;

        var lastItems = DrawController.Instance.GetLastDrawnItems();
        if (lastItems == null || lastItems.Count == 0)
        {
            return;
        }

        var orderedKey = lastItems.Keys.FirstOrDefault(k => k.StartsWith("SKb_") || k.Contains("_"));
        if (string.IsNullOrEmpty(orderedKey))
        {
            return;
        }

        // Extract the actual item name from the ordered key format "00_ItemName_Quantity"
        string[] keyParts = orderedKey.Split('_');
        if (keyParts.Length < 3)
        {
            return;
        }

        // Reconstruct the original item name (everything except the first part which is the index and last part which is quantity)
        string fileName = string.Join("_", keyParts.Skip(1).Take(keyParts.Length - 2));
        int quantity = lastItems[orderedKey]; // Get the quantity of the drawn item
        
        string imagePath = fileName.StartsWith("SKb_")
            ? $"UILoading/CharacterImages/Skillbook/{fileName}"
            : $"UILoading/CharacterImages/Shard/{fileName}";

        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite == null)
        {
            return;
        }

        blockImage.sprite = sprite;
        blockImage.gameObject.SetActive(true);
        blockPart.gameObject.SetActive(false);

        // Set the Qnty text
        TextMeshProUGUI qntyText = block.transform.Find("Qnty")?.GetComponent<TextMeshProUGUI>();
        if (qntyText != null)
        {
            qntyText.text = quantity.ToString();
            qntyText.gameObject.SetActive(true);
        }

        OpenGachaPage();
        block.SetActive(true);

        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson.TryGetValue("heroKey", out var heroKey) ? heroKey : 0, HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Keys/heroKey");
        lastDraw = GachaType.Shard;
    }

    public void ExecuteShardTenDraw()
    {
        var lastItems = DrawController.Instance.GetLastDrawnItems();
        if (lastItems == null || lastItems.Count == 0)
        {
            return;
        }

        OpenGachaPage();
        grid.SetActive(true);
        ClearGrid();

        // Sort by the order index we embedded in the key (00_, 01_, 02_, etc.)
        var sortedItems = lastItems.OrderBy(kvp => kvp.Key).ToList();

        int blockIndex = 0;
        foreach (var item in sortedItems)
        {
            // Extract the actual item name from our ordered key format "00_ItemName_Quantity"
            string[] keyParts = item.Key.Split('_');
            if (keyParts.Length < 3)
            {
                continue;
            }

            // Reconstruct the original item name (everything except the first part which is the index)
            string fileName = string.Join("_", keyParts.Skip(1).Take(keyParts.Length - 2));
            int quantity = item.Value;

            // Create one block for this item (regardless of quantity, since server sent it as one entry)
            string imagePath = fileName.StartsWith("SKb_")
                ? $"UILoading/CharacterImages/Skillbook/{fileName}"
                : $"UILoading/CharacterImages/Shard/{fileName}";

            Sprite sprite = Resources.Load<Sprite>(imagePath);
            if (sprite == null)
            {
                continue;
            }

            GameObject newBlock = Instantiate(blockPrefab, grid.transform);
            if (newBlock == null)
            {
                continue;
            }
            newBlock.name = $"Block_{blockIndex + 1}_{fileName}";

            Image blockImage = newBlock.transform.Find("Image")?.GetComponent<Image>();
            if (blockImage != null)
            {
                blockImage.sprite = sprite;
                blockImage.color = Color.white;
            }
            else
            {
                Destroy(newBlock);
                continue;
            }

            // Hide Part for shards/skillbooks
            Image partImage = newBlock.transform.Find("Part")?.GetComponent<Image>();
            if (partImage != null)
            {
                partImage.gameObject.SetActive(false);
            }

            // Set the quantity text
            TextMeshProUGUI qntyText = newBlock.transform.Find("Qnty")?.GetComponent<TextMeshProUGUI>();
            if (qntyText != null)
            {
                qntyText.text = quantity.ToString();
                qntyText.gameObject.SetActive(true);
            }

            blockIndex++;
        }

        AdjustGridLayout();

        var player = PlayerProfile.Data.Player;
        storeMenu.UpdateGachaPanels("heroKey", player.ItemsJson.TryGetValue("heroKey", out var heroKey) ? heroKey : 0, HERO_X1_DIAMOND_COST, HERO_X10_DIAMOND_COST, "UILoading/Keys/heroKey");
        lastDraw = GachaType.Shard;
    }

    public void ExecuteRareGemDraw()
    {
        if (!ValidateBlockReferences()) return;

        var gem = DrawController.Instance.GetLastDrawnGem();
        if (gem == null)
        {
            return;
        }

        string imageName = $"Gem_{gem.Level:00}";
        Sprite gemSprite = Resources.Load<Sprite>($"UILoading/Gem/Stone/{imageName}");

        if (gemSprite != null)
        {
            blockImage.sprite = gemSprite;
            blockImage.gameObject.SetActive(true);
        }

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
        storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Keys/rareKey");
        lastDraw = GachaType.RareGem;
    }

    public void ExecuteEpicGemDraw()
    {
        if (!ValidateBlockReferences()) return;

        var gem = DrawController.Instance.GetLastDrawnGem();
        if (gem == null)
        {
            Debug.LogError("❌ No newly drawn gem found!");
            return;
        }

        Debug.Log($"🎯 Displaying newly drawn gem: ID={gem.Id}, Level={gem.Level}, Part={gem.Part}");

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
        storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Keys/epicKey");
        lastDraw = GachaType.EpicGem;
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
        storeMenu.UpdateGachaPanels("rareKey", player.ItemsJson["rareKey"], RARE_X1_DIAMOND_COST, RARE_X10_DIAMOND_COST, "UILoading/Keys/rareKey");
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
        storeMenu.UpdateGachaPanels("epicKey", player.ItemsJson["epicKey"], EPIC_X1_DIAMOND_COST, EPIC_X10_DIAMOND_COST, "UILoading/Keys/epicKey");
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
            return (true, true, false);
        }
        else if (player.Diamond >= diamondCost)
        {
            return (true, false, true);
        }
        else
        {
            return (false, false, false);
        }
    }

    private void GenerateTenGems()
    {
        OpenGachaPage();
        grid.SetActive(true);
        ClearGrid();

        var gems = DrawController.Instance.GetLastDrawnGems();
        if (gems.Count < 10)
        {
            Debug.LogError("❌ Not enough gems returned from server to display 10.");
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            var gem = gems[i];

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

        float blockWidth = panelWidth / (BLOCKS_PER_ROW + 1);
        float leftPadding = blockWidth * 0.25f;
        float rightPadding = blockWidth * 0.25f;
        float spacingX = blockWidth * 0.125f;
        float spacingY = blockWidth * 0.125f;

        Grid.cellSize = new Vector2(blockWidth, blockWidth);
        Grid.spacing = new Vector2(spacingX, spacingY);
        Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Grid.constraintCount = BLOCKS_PER_ROW;

        Grid.padding.left = Mathf.RoundToInt(leftPadding);
        Grid.padding.right = Mathf.RoundToInt(rightPadding);
        Grid.padding.top = Mathf.RoundToInt(spacingY);
        Grid.padding.bottom = Mathf.RoundToInt(spacingY);

        int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)BLOCKS_PER_DRAW / BLOCKS_PER_ROW));
        float contentHeight = totalRows * (blockWidth + spacingY) - spacingY;
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
    }
    #endregion

    #region Repeat Draw Methods
    public void DrawOneAgain()
    {
        IEnumerator DelayAndDraw()
        {
            CloseGachaPage();
            grid.SetActive(false);

            yield return new WaitForSeconds(0.01f);

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

        StartCoroutine(DelayAndDraw());
    }

    public void DrawTenAgain()
    {
        IEnumerator DelayAndDraw()
        {
            CloseGachaPage();
            block.SetActive(false);

            yield return new WaitForSeconds(0.01f);

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

        StartCoroutine(DelayAndDraw());
    }
    #endregion
}