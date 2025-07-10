# Development Guide for Unity Project

## Project Overview
This is a Unity game project with backend API integration. The project uses WebSocket connections and REST APIs for real-time features.

## API Architecture

### Base Configuration
- **API Base URL**: Configured in `Assets/Scripts/utils/Config.cs`
- **Current Setup**: `Config.BaseUrl = "http://127.0.0.1:3000"`
- **WebSocket**: `Config.websocket = "ws://127.0.0.1:3000/cable"`

### API Usage Pattern
All API calls in the project follow this pattern:
```csharp
string apiUrl = $"{Config.BaseUrl}/api/endpoint";
using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
{
    yield return webRequest.SendWebRequest();
    // Handle response...
}
```

**IMPORTANT**: Never create custom base URL fields in scripts! Always use `Config.BaseUrl`.

### Example API Implementation
See `Assets/Scripts/AuthService.cs` for reference implementation of API calls.

## Ally/Sidekick System

### Data Structure
- **Ally ID Format**: `"XX_Name"` (e.g., "01_Zorath", "02_Gideon")
- **Character Names**: 20 total characters from "01_Zorath" to "20_Nyx"
- **Unlock Status**: Managed via `PlayerProfile.Data.IsAllyUnlocked(allyName)`

### Key Scripts
- **AlliesGridSetup.cs**: Main ally grid management, handles UI and navigation
- **UpgradePanelManager.cs**: Handles ally upgrade levels display via API

### Upgrade Levels API
- **Endpoint**: `GET /api/allies/{ally_id}/upgrade_levels`
- **Example**: `GET /api/allies/02_Gideon/upgrade_levels`
- **Response Format**:
```json
{
  "ally_id": "02_Gideon",
  "name": "Gideon",
  "current_level": 1,
  "upgrade_levels": [
    {"level": "L02", "description": "Attack damage +20%", "cost": 0, "is_unlocked": false},
    {"level": "L06", "description": "Multi-shot capability", "cost": 0, "is_unlocked": false}
  ]
}
```

### Data Models
```csharp
[System.Serializable]
public class UpgradeLevel
{
    public string level;
    public string description; 
    public int cost;
    public bool is_unlocked;
}

[System.Serializable]
public class AllyUpgradeResponse
{
    public string ally_id;
    public string name;
    public string cn_name;
    public int current_level;
    public List<UpgradeLevel> upgrade_levels;
}
```

## UI Architecture

### Menu System
- **MenuController**: Manages main menu navigation
- **AlliesGridSetup**: Step 1 (grid) and Step 2 (detail) panels
- **UpgradePanelManager**: Dynamic upgrade level panels in scrollview

### Integration Points
- AlliesGridSetup calls UpgradePanelManager when ally is selected
- UpgradePanelManager creates dynamic UI panels based on API response
- System handles any number of upgrade levels (not hardcoded to 5)

## Common Patterns

### Player Data Access
```csharp
// Check if ally is unlocked
bool isUnlocked = PlayerProfile.Data.IsAllyUnlocked(allyName);

// Listen for player data changes
PlayerProfile.Data.AddListener(OnPlayerDataChanged, "Player");
PlayerProfile.Data.AddListener(OnPlayerDataChanged, "Sidekicks");
```

### Resource Loading
```csharp
// Character images
string imagePath = $"UILoading/CharacterImages/CardDisplay/C_{index}_{name}";
Sprite sprite = Resources.Load<Sprite>(imagePath);
```

## Error Handling

### API Error Patterns
- Always log detailed error information
- Include response codes for debugging
- Provide meaningful error messages
- Never use fallback mock data - fix the real issue

### Debug Information
- Log API URLs being called
- Log API responses for troubleshooting
- Use descriptive error messages with context

## Development Notes

### Recent Changes
- Replaced hardcoded ally upgrade data with real API calls
- Integrated UpgradePanelManager with AlliesGridSetup
- Fixed API base URL configuration issues

### Important Reminders
1. Always use `Config.BaseUrl` for API calls
2. Never hardcode API endpoints or mock data as fallbacks
3. Follow existing patterns in AuthService.cs for new API implementations
4. Test with actual backend server, don't mask problems with fake data

## File Locations

### Key Scripts
- `Assets/Scripts/utils/Config.cs` - API configuration
- `Assets/Scripts/AuthService.cs` - API pattern reference
- `Assets/Scripts/UI Scripts/Allies Menu/AlliesGridSetup.cs` - Main ally UI
- `Assets/Scripts/UI Scripts/Allies Menu/UpgradePanelManager.cs` - Upgrade levels

### Resources Structure
- `Resources/UILoading/CharacterImages/` - Character assets
  - `CardDisplay/C_{index}_{name}` - Grid thumbnails
  - `Stand_Illustration/P_{index}_{name}` - Detail illustrations
  - `Shard/{index}_{name}` - Shard images
  - `Skillbook/Skb_{index}_{name}` - Skillbook images