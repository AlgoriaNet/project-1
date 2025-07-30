# Item Type and UI Button Documentation

## Allies Menu Button Visibility Logic

The Allies Menu uses a dynamic button system that shows/hides different action buttons based on the current UI state and selected tab.

### Button Visibility Rules

#### 1. Board visible (pack inactive):
- **Show**: "Go to Pack" button
- **Hide**: Auto Equip, Auto Embed

#### 2. Pack visible + Equipment tab:
- **Show**: "Auto Equip" button  
- **Hide**: Go to Pack, Auto Embed

#### 3. Pack visible + Gem tab:
- **Show**: "Auto Embed" button
- **Hide**: Go to Pack, Auto Equip

### Implementation Details

- **File**: `Assets/Scripts/UI Scripts/Allies Menu/SwitchPanels.cs`
- **Method**: `UpdateButtonVisibility()`
- **Trigger**: Called whenever tabs change in `ItemLoader.SwitchItemType()`

### Button Functions

- **Go to Pack**: Switches from board view to pack view
- **Auto Equip**: Automatically equips best available equipment for current ally
- **Auto Embed**: Automatically embeds best available gems into current ally's equipment

### Notes

- Allies Menu only has Equipment and Gem tabs (no "Other" tab like Hero Menu)
- Each button has a dedicated click handler to avoid timing/interference issues
- Button visibility logic exactly matches the legacy dynamic text logic for consistency