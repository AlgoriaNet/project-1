# Item Type Detection and Data Structure Documentation

## Current Server Data Structure

The game receives item data from the server in the following format:

### PlayerProfile.Data.Player.ItemsJson
```csharp
Dictionary<string, int> ItemsJson
// Key: Item identifier string
// Value: Quantity (integer)
```

**Important**: The server data does NOT include explicit item type information. Item types must be inferred from the key naming patterns.

## Item Type Detection Logic

### Current Implementation
Located in `HeroBlockSetup.UpdateOthersTab()` method, item types are determined by exact key matching:

```csharp
// Item type detection based on exact naming patterns
if (rawKey.StartsWith("SKb_"))
{
    type = "skillbook";
}
else if (rawKey == "Hammer")
{
    type = "hammer";
}
else if (rawKey == "Nail")
{
    type = "nail";
}
else if (rawKey == "Crystal")
{
    type = "crystal";
}
else if (rawKey == "equipScroll")
{
    type = "equipScroll";
}
else
{
    type = "shard";  // Default fallback for character shards (xx_name format)
}
```

## Supported Item Types

| Type | Key Pattern | Example Keys | Resource Path |
|------|-------------|--------------|---------------|
| **Skillbook** | `SKb_xx_name` | `SKb_20_Nyx`, `SKb_15_Thor` | `UILoading/CharacterImages/Skillbook/` |
| **Hammer** | `Hammer` (exact) | `Hammer` | `UILoading/Other/Hammer` |
| **Nail** | `Nail` (exact) | `Nail` | `UILoading/Other/Nail` |
| **Crystal** | `Crystal` (exact) | `Crystal` | `UILoading/Other/Crystal` |
| **Equipment Scroll** | `equipScroll` (exact) | `equipScroll` | `UILoading/Other/equipScroll` |
| **Shard** | `xx_name` format | `20_Nyx`, `15_Thor` | `UILoading/CharacterImages/Shard/` |

## Excluded Items
Keys and other items not stored in the pack are excluded from the "Other" tab display:
- `heroKey`, `rareKey`, `epicKey` - These won't appear in the pack UI

## Implementation Details

### Files Modified
1. **HeroBlockSetup.cs** - `UpdateOthersTab()` method
   - Enhanced item type detection logic
   - Updated sprite loading logic

2. **OtherDetailBox.cs** - Multiple methods
   - `LoadOtherItemSprite()` - Resource path logic
   - `GetDisplayName()` - Name display logic
   - `GetItemDescription()` - Description text logic

### Display Name Logic
Each item type has specific logic for extracting display names:

- **Skillbooks**: Remove `SKb_` prefix (`SKb_20_Nyx` → `20_Nyx`)
- **Hammer**: Display as "Hammer"
- **Nail**: Display as "Nail"
- **Crystal**: Display as "Crystal"
- **Equipment Scroll**: Display as "Equipment Scroll"
- **Shards**: Extract character name from `xx_name` format (`20_Nyx` → `Nyx`)

### Resource Organization
Expected folder structure under `Resources/`:
```
UILoading/
├── CharacterImages/
│   ├── Skillbook/
│   └── Shard/
└── Other/
    ├── Hammer (single file)
    ├── Nail (single file)
    ├── Crystal (single file)
    └── equipScroll (single file)
```

**Note**: The 4 other item types (Hammer, Nail, Crystal, equipScroll) are single sprite files, not folders with multiple items.

## Adding New Item Types

To add a new item type to the pack:

1. **Add detection logic** in `HeroBlockSetup.UpdateOthersTab()`:
   ```csharp
   else if (rawKey == "NewItemName")
   {
       fileName = rawKey;
       type = "newitem";
   }
   ```

2. **Add sprite loading** in both files:
   ```csharp
   case "newitem":
       path = $"UILoading/Other/NewItemName";
       break;
   ```

3. **Add display name logic** in `OtherDetailBox.GetDisplayName()`:
   ```csharp
   case "newitem":
       return "New Item Display Name";
   ```

4. **Add description** in `OtherDetailBox.GetItemDescription()`:
   ```csharp
   case "newitem":
       return "Description for the new item type.";
   ```

**Note**: Since these are single items (not collections), use exact key matching rather than prefix patterns.

## Future Improvements

### Option 1: Server-Side Type Information
If possible, request the server team to include explicit type information:
```csharp
public class ItemData
{
    public string Id { get; set; }
    public string Type { get; set; }  // "skillbook", "shard", "hammer", etc.
    public int Quantity { get; set; }
}
```

### Option 2: Static Configuration
Create a static configuration file mapping item IDs to types:
```csharp
public static Dictionary<string, string> ItemTypeMap = new Dictionary<string, string>
{
    { "SKb_FireBall", "skillbook" },
    { "20_Nyx", "shard" },
    // ... etc
};
```

### Option 3: Asset Database
Store item metadata in ScriptableObjects or JSON configuration files within the project.

## Notes
- The current implementation handles exactly 6 item types as specified
- Each of the 4 "other" items (Hammer, Nail, Crystal, equipScroll) is a single item, not a collection
- Skillbooks and shards can have multiple variants based on character names/numbers
- Keys and other non-pack items are not handled in this UI system
- All item types display correctly in both the main "Other" tab and the `OtherDetailBox` popup
- Error logging is included for missing sprites
- The system gracefully handles unknown item types by defaulting to "shard"

---

# Equipment and Gemstone Data Structures

## Equipment Data Structure

Equipment items use the `Equipment` model class with comprehensive data for combat, enhancement, and management.

### Equipment Model (Assets/Scripts/model/Equipment.cs)

```csharp
public class Equipment : ApplicationModel
{
    // Core Identity
    public int Id;                              // Unique equipment ID
    public string Name;                         // Equipment name (e.g., "Helm_05", "Chest_03")
    public int Quality;                         // Quality: 1=Common, 2=Uncommon, 3=Rare, 4=Epic, 5=Legendary, 6=Mythic
    public string Part;                         // Equipment slot: "Helm", "Shoulder", "Chest", "Pants", "Gloves", "Boots"
    public string Description;                  // Equipment description text

    // Combat Stats
    [JsonProperty("base_atk")]
    public int BaseAtk;                         // Base attack value
    [JsonProperty("growth_atk")]
    public int GrowthAtk;                       // Attack gained per intensify level
    public int Attack => BaseAtk + IntensifyLevel * GrowthAtk;  // Calculated total attack

    // Enhancement System
    [JsonProperty("intensify_level")]
    public int IntensifyLevel = 0;              // Current enhancement level (0-10+)
    [JsonProperty("nearby_attributes")]
    public Dictionary<string, int> NearbyAttributes;  // Additional stats (Critical, Defense, Health, etc.)
    [JsonProperty("additional_attributes")]
    public string AdditionalAttributes;         // Special attributes as string

    // Equipment Status
    [JsonProperty("equip_with_hero_id")]
    public int? EquipWithHeroId = 0;           // ID if equipped to hero (null/0 if not)
    [JsonProperty("equip_with_sidekick_id")]
    public int? EquipWithSidekickId = 0;       // ID if equipped to sidekick (null/0 if not)
    
    // Utility Methods
    public bool IsEquipped() => EquipWithHeroId != 0 || EquipWithSidekickId != 0;
}
```

### Example Equipment: "Helm_05" (Legendary Helmet)

```csharp
Equipment exampleHelm = {
    // Core Identity
    Id = 1234,
    Name = "Helm_05",                          // Equipment name (type_tier)
    Quality = 5,                               // Legendary quality
    Part = "Helm",                             // Helmet slot
    Description = "Legendary helmet forged with ancient magic",

    // Combat Stats  
    BaseAtk = 50,                              // Base attack value
    GrowthAtk = 8,                             // +8 attack per enhancement level
    Attack = 98,                               // Calculated: 50 + (6 * 8) = 98

    // Enhancement Data
    IntensifyLevel = 6,                        // Enhanced 6 times
    
    // Additional Attributes
    NearbyAttributes = {
        "Critical" = 15,                       // +15 Critical Hit
        "Defense" = 22,                        // +22 Defense  
        "Health" = 180,                        // +180 Health Points
        "Speed" = 8                            // +8 Speed
    },
    AdditionalAttributes = "Fire Resistance +10%",

    // Equipment Status
    EquipWithHeroId = null,                    // Not equipped to hero
    EquipWithSidekickId = 42,                  // Equipped to sidekick ID 42
    IsEquipped() = true                        // Method returns true
}
```

### Equipment UI Integration

- **Icon Loading**: `Resources.Load<Sprite>($"UILoading/Equipment/{equipment.Name}")` → "UILoading/Equipment/Helm_05"
- **Quality Color**: `ItemLoader.quantityColor[equipment.Quality]` → Legendary gold background
- **Attack Display**: `equipment.Attack.ToString()` → Shows calculated total "98"
- **Enhancement Level**: `equipment.IntensifyLevel` → Shows current level "6"
- **Additional Stats**: Iterates through `NearbyAttributes` dictionary for UI display

---

## Gemstone Data Structure

Gemstones use the `Gemstone` model class with focused stat bonuses and inlay system.

### Gemstone Model (Assets/Scripts/model/Gemstone.cs)

```csharp
public class Gemstone : ApplicationModel
{
    // Core Identity
    [JsonProperty("id")]
    public int Id { get; set; }                 // Unique gemstone ID
    [JsonProperty("name")]
    public string Name { get; set; }            // Gem name/type
    [JsonProperty("level")]
    public int Level { get; set; }              // Gem level (1-7+, determines quality)
    [JsonProperty("quality")]
    public int? Quality { get; set; }           // Quality tier (usually matches level)
    [JsonProperty("description")]
    public string Description { get; set; }      // Gem description
    [JsonProperty("part")]
    public string Part { get; set; }            // Equipment part compatibility ("Helm", "Chest", etc.)

    // Gem Stats & Effects
    [JsonProperty("entry_id")]
    public int EntryId { get; set; }            // Stat type ID (maps to attack, defense, etc.)
    [JsonProperty("entry_value")]
    public double EntryValue { get; set; }      // Stat value (supports decimals for percentages)

    // Inlay Status
    [JsonProperty("inlay_with_hero_id")]
    public int? InlayWithHeroId { get; set; }   // ID if inlaid into hero equipment
    [JsonProperty("inlay_with_sidekick_id")]
    public int? InlayWithSidekickId { get; set; } // ID if inlaid into sidekick equipment
    [JsonProperty("is_locked")]
    public bool IsLocked { get; set; }          // Lock status (prevents removal/use)
}
```

### Example Gemstone: Level 5 "Legendary Gem" for Helmet

```csharp
Gemstone exampleGem = {
    // Core Identity
    Id = 5678,
    Name = "Fire Ruby",
    Level = 5,                                  // Level 5 = Legendary
    Quality = 5,                                // Matches level
    Part = "Helm",                              // For helmet equipment
    Description = "A fiery gem that increases attack power",

    // Gem Stats & Effects
    EntryId = 101,                              // Stat type (attack, defense, etc.)
    EntryValue = 45.5,                          // +45.5 attack (double precision)

    // Inlay Status  
    InlayWithHeroId = null,                     // Not inlaid into hero equipment
    InlayWithSidekickId = 42,                   // Inlaid into sidekick 42's equipment
    IsLocked = false                            // Can be removed/used
}
```

### Gemstone Level to Quality Mapping

The game uses a localization system for gem quality names:

```csharp
private Dictionary<string, string> gemNameLocalization = new Dictionary<string, string>
{
    { "Gem_01", "Common Gem" },        // Level 1 - Gray
    { "Gem_02", "Superior Gem" },      // Level 2 - Green  
    { "Gem_03", "Rare Gem" },          // Level 3 - Blue
    { "Gem_04", "Epic Gem" },          // Level 4 - Purple
    { "Gem_05", "Legendary Gem" },     // Level 5 - Orange/Gold
    { "Gem_06", "Mythic Gem" },        // Level 6 - Red
    { "Gem_07", "Ultimate Gem" }       // Level 7 - Rainbow/Special
};
```

### Gemstone UI Integration

- **Gem Icon**: `Resources.Load<Sprite>($"UILoading/Gem/Stone/Gem_{gemstone.Level:D2}")` → "UILoading/Gem/Stone/Gem_05"
- **Gem Name**: Uses localization → "Gem_05" becomes "Legendary Gem"  
- **Part Icon**: `Resources.Load<Sprite>($"UILoading/Gem/Part/{gemstone.Part}")` → Equipment part icon
- **Level Display**: `gemstone.Level.ToString()` → Shows "5"
- **Stat Value**: `gemstone.EntryValue.ToString()` → Shows "45.5"
- **Quality Color**: Based on level for background colors

### Key Differences: Equipment vs Gemstones

| Aspect | Equipment | Gemstones |
|--------|-----------|-----------|
| **Primary Purpose** | Direct combat items | Stat enhancement for equipment |
| **Attachment** | Equipped to character | Inlaid into equipment |
| **Stats** | Multiple attributes (BaseAtk, NearbyAttributes, etc.) | Single focused stat (EntryValue) |
| **Enhancement** | IntensifyLevel system | Level-based quality tiers |
| **Complexity** | Complex with multiple stat systems | Simple focused bonuses |
| **Slot System** | 6 equipment slots per character | Part-specific inlay compatibility |

---

## Data Usage Patterns

### Equipment Access
```csharp
// Get hero's equipped items
List<Equipment> heroEquipments = PlayerProfile.Data.GetHeroEquipments();

// Get sidekick's equipped items  
List<Equipment> sidekickEquipments = PlayerProfile.Data.GetSidekickEquipments(sidekickId);

// Get unequipped items in pack
List<Equipment> packEquipments = PlayerProfile.Data.GetEquipmentsInPack();
```

### Gemstone Access
```csharp
// Get hero's gemstones for specific part
List<Gemstone> heroGems = PlayerProfile.Data.GetHeroGemstones("Helm");

// Get sidekick's gemstones for specific part
List<Gemstone> sidekickGems = PlayerProfile.Data.GetSidekickGemstones(sidekickId, "Helm");

// Get unslotted gems in pack
List<Gemstone> packGems = PlayerProfile.Data.GetGemstonesInPack();
```

### Equipment Quality Colors
Equipment and gemstone UI uses `ItemLoader.quantityColor[]` array for quality-based background colors matching their tier levels.
