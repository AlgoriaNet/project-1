using UnityEngine;
using UnityEngine.UI;

public class GachaController : MonoBehaviour
{
    public GameObject gachaPage;  // Assign GachaPage in Inspector
    public GameObject block;      // Assign Block in Inspector
    public Image blockImage;      // Assign Block → Image in Inspector
    public Image blockPart; // Assign Block → Part in Inspector

    public int blocksPerRow = 5; // Fixed columns (5 per row)
    public GameObject grid; // Assign grid in Inspector
    public GameObject blockPrefab; // Assign BlockItem Prefab in Inspector
    public GridLayoutGroup Grid; // Assign GridLayoutGroup in Inspector

    private enum GachaType { None, Shard, Gem }
    private GachaType lastDraw = GachaType.None;


    public void OpenGachaPage()
    {
        gachaPage.SetActive(true);
    }

    public void DrawOneShard()
    {
        if (blockImage == null || blockPart == null)
        {
            Debug.LogError("❌ Block Image or Part reference is missing!");
            return;
        }

        // Show the block
        block.SetActive(true);

        // Load a random shard image
        string shardPath = "UILoading/CharacterImages/Shard";
        Sprite[] shardSprites = Resources.LoadAll<Sprite>(shardPath);

        if (shardSprites.Length > 0)
        {
            blockImage.sprite = shardSprites[Random.Range(0, shardSprites.Length)];
            blockImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ No shard images found in Resources folder!");
        }

        // Disable Part for shards
        blockPart.gameObject.SetActive(false);

        lastDraw = GachaType.Shard; // Track last draw type
    }

    public void DrawOneGem()
    {
        if (blockImage == null || blockPart == null)
        {
            Debug.LogError("❌ Block Image or Part reference is missing!");
            return;
        }

        // Show the block
        block.SetActive(true);

        // Load a random gem image
        string gemPath = "UILoading/Gem/Stone";
        Sprite[] gemSprites = Resources.LoadAll<Sprite>(gemPath);

        if (gemSprites.Length > 0)
        {
            blockImage.sprite = gemSprites[Random.Range(0, gemSprites.Length)];
            blockImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ No gem images found in Resources folder!");
        }

        // Load a random part image
        string partPath = "UILoading/Gem/Part";
        Sprite[] partSprites = Resources.LoadAll<Sprite>(partPath);

        if (partSprites.Length > 0)
        {
            blockPart.sprite = partSprites[Random.Range(0, partSprites.Length)];
            blockPart.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ No part images found in Resources folder!");
        }
    
        lastDraw = GachaType.Gem; // Track last draw type
    }

    public void DrawFreeShard()
    {
        // 🔹 Placeholder for future implementation (e.g., watch ad logic)
        Debug.Log("🔹 DrawFreeShard method called! Future ad logic goes here.");

        // Call DrawOneShard at the end
        DrawOneShard();
    }

    public void DrawFreeGem()
    {
        // 🔹 Placeholder for future implementation (e.g., watch ad logic)
        Debug.Log("🔹 DrawFreeShard method called! Future ad logic goes here.");

        // Call DrawOneShard at the end
        DrawOneGem();
    }

    public void DrawTenShard()
    {
        GenerateGachaBlocks("UILoading/CharacterImages/Shard", false);
        lastDraw = GachaType.Shard; // Track last draw type
    }

    public void DrawTenGem()
    {
        GenerateGachaBlocks("UILoading/Gem/Stone", true);
        lastDraw = GachaType.Gem; // Track last draw type
    }

    private void GenerateGachaBlocks(string path, bool isGem)
    {
        grid.SetActive(true);
        
        // Clear existing blocks
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }

        // Load shard or gem images
        Sprite[] images = Resources.LoadAll<Sprite>(path);
        Sprite[] partImages = isGem ? Resources.LoadAll<Sprite>("UILoading/Gem/Part") : null;

        for (int i = 0; i < 10; i++)
        {
            GameObject newBlock = Instantiate(blockPrefab, grid.transform);
            newBlock.name = $"Block_{i + 1}";

            Image blockImage = newBlock.transform.Find("Image")?.GetComponent<Image>();
            Transform partTransform = newBlock.transform.Find("Part");
            Image partImage = partTransform?.GetComponent<Image>();

            if (blockImage != null && images.Length > 0)
            {
                blockImage.sprite = images[Random.Range(0, images.Length)];
                blockImage.color = Color.white;
            }

            if (isGem && partImage != null && partImages != null && partImages.Length > 0)
            {
                partImage.sprite = partImages[Random.Range(0, partImages.Length)];
                partImage.color = Color.white;
                partTransform.gameObject.SetActive(true);
            }
            else if (partTransform != null)
            {
                partTransform.gameObject.SetActive(false);
            }
        }

        AdjustGridLayout();
    }

    private void AdjustGridLayout()
    {
        float panelWidth = grid.GetComponent<RectTransform>().rect.width;

        // Calculate block width and padding dynamically
        float blockWidth = panelWidth / (blocksPerRow + 1);
        float leftPadding = blockWidth * 0.25f;
        float rightPadding = blockWidth * 0.25f;

        float spacingX = blockWidth * 0.125f;
        float spacingY = blockWidth * 0.125f;
        Grid.padding.top = Mathf.RoundToInt(spacingY);

        // Adjust GridLayoutGroup settings
        Grid.cellSize = new Vector2(blockWidth, blockWidth);
        Grid.spacing = new Vector2(spacingX, spacingY);
        Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Grid.constraintCount = blocksPerRow;

        // Apply padding to the grid
        Grid.padding.left = Mathf.RoundToInt(leftPadding);
        Grid.padding.right = Mathf.RoundToInt(rightPadding);
        Grid.padding.top = Mathf.RoundToInt(spacingY);
        Grid.padding.bottom = Mathf.RoundToInt(spacingY);

        // ✅ **Fix: Do not modify grid's position**  
        RectTransform gridRect = grid.GetComponent<RectTransform>();

        // Calculate total rows dynamically (2 rows for 10 blocks)
        int totalRows = Mathf.Max(1, Mathf.CeilToInt((float)10 / blocksPerRow));

        // Adjust grid size to fit the blocks **without moving its position**
        float contentHeight = totalRows * (blockWidth + spacingY) - spacingY;
        // gridRect.sizeDelta = new Vector2(gridRect.sizeDelta.x, contentHeight);

        // ✅ Use SetSizeWithCurrentAnchors to avoid shifting
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

    }

    public void DrawOneAgain()
    {
        grid.SetActive(false);

        if (lastDraw == GachaType.Shard)
        {
            DrawOneShard();
        }
        else if (lastDraw == GachaType.Gem)
        {
            DrawOneGem();
        }
        else
        {
            Debug.LogWarning("⚠ No previous draw detected!");
        }
    }

    public void DrawTenAgain()
    {
        block.SetActive(false);

        if (lastDraw == GachaType.Shard)
        {
            DrawTenShard();
        }
        else if (lastDraw == GachaType.Gem)
        {
            DrawTenGem();
        }
        else
        {
            Debug.LogWarning("⚠ No previous draw detected!");
        }
    }

    public void CloseGachaPage()
    {
        gachaPage.SetActive(false);
        grid.SetActive(false);
        block.SetActive(false);

        // Clear previous blocks
        foreach (Transform child in grid.transform)
        {
            Destroy(child.gameObject);
        }
    }
}