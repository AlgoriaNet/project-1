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
