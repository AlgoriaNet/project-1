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

### Backend APIs

#### 1. Upgrade Levels (Benchmark Descriptions)
- **Endpoint**: `GET /api/allies/{ally_id}/upgrade_levels`
- **Purpose**: Get benchmark upgrade descriptions (L02, L06, L08, L15, L20)
- **Auth**: Not required
- **Example**: `GET /api/allies/02_Gideon/upgrade_levels`
- **Response Format**:
```json
{
  "ally_id": "02_Gideon",
  "name": "Gideon",
  "current_level": 1,
  "upgrade_levels": [
    {"level": "L02", "description": "Thunder damage +20%", "cost": 0, "is_unlocked": false},
    {"level": "L06", "description": "Thunder multi-strike: 2 bolts", "cost": 0, "is_unlocked": false},
    {"level": "L08", "description": "Thunder triple strike capability", "cost": 0, "is_unlocked": false},
    {"level": "L15", "description": "Thunder ultimate: devastating storm", "cost": 0, "is_unlocked": false},
    {"level": "L20", "description": "Thunder legendary form: all stats +100%", "cost": 0, "is_unlocked": false}
  ]
}
```

#### 2. Universal Level Up Costs
- **Endpoint**: `GET /api/level_up_costs`
- **Purpose**: Get universal level progression costs (1-20) for all sidekicks
- **Auth**: Not required
- **Example**: `GET /api/level_up_costs`
- **Response Format**:
```json
{
  "level_up_costs": [
    {"level": 1, "skillbook_cost": 20, "gold_cost": 1000},
    {"level": 2, "skillbook_cost": 25, "gold_cost": 2000},
    ...
  ]
}
```

#### 3. Check Level Up Status 🔐
- **Endpoint**: `GET /api/allies/{ally_id}/level_up_cost`
- **Purpose**: Get current level and cost to upgrade to next level
- **Auth**: Required (JWT token)
- **Example**: `GET /api/allies/04_Aurelia/level_up_cost`
- **Response Format**:
```json
{
  "ally_id": "04_Aurelia",
  "current_level": 5,
  "next_level": 6,
  "max_level": 20,
  "can_level_up": true,
  "cost": {
    "skillbook_cost": 40,
    "gold_cost": 5000,
    "skillbook_name": "SKb_04_Aurelia"
  },
  "player_resources": {
    "gold": 15000,
    "skillbooks": 50
  },
  "has_enough_resources": true
}
```

#### 4. Execute Level Up 🔐
- **Endpoint**: `POST /api/allies/{ally_id}/level_up`
- **Purpose**: Level up sidekick, deduct resources, increment level
- **Auth**: Required (JWT token)
- **Example**: `POST /api/allies/04_Aurelia/level_up`
- **Success Response**:
```json
{
  "success": true,
  "ally_id": "04_Aurelia",
  "old_level": 5,
  "new_level": 6,
  "costs_paid": {
    "gold": 5000,
    "skillbooks": 40,
    "skillbook_name": "SKb_04_Aurelia"
  },
  "remaining_resources": {
    "gold": 10000,
    "skillbooks": 10
  },
  "can_level_up_again": true
}
```

#### 5. WebSocket Ally APIs 🔐
**Channel**: `PlayerChannel`

**Get Upgrade Levels:**
- **Action**: `get_upgrade_levels`
- **Parameters**: `{"ally_name": "04_Aurelia"}`
- **Response**:
```json
{
  "action": "get_upgrade_levels",
  "success": true,
  "data": {
    "ally_id": "04_Aurelia",
    "name": "Aurelia", 
    "cn_name": "奥蕾莉亚",
    "current_level": 1,
    "upgrade_levels": [
      {"level": "L02", "description": "Attack damage +20%", "cost": 0, "is_unlocked": false},
      {"level": "L06", "description": "Multi-shot: fires 2 projectiles", "cost": 0, "is_unlocked": false},
      {"level": "L08", "description": "Triple shot capability", "cost": 0, "is_unlocked": false},
      {"level": "L15", "description": "Ultimate ability: devastating blast", "cost": 0, "is_unlocked": false},
      {"level": "L20", "description": "Legendary form: all stats +100%", "cost": 0, "is_unlocked": false}
    ]
  }
}
```

**Get Level Up Cost:**
- **Action**: `get_level_up_cost` 
- **Parameters**: `{"ally_name": "04_Aurelia"}`
- **Response**:
```json
{
  "action": "get_level_up_cost",
  "success": true,
  "data": {
    "ally_id": "04_Aurelia",
    "current_level": 5,
    "next_level": 6,
    "max_level": 20,
    "can_level_up": true,
    "cost": {
      "skillbook_cost": 40,
      "gold_cost": 5000
    },
    "player_resources": {
      "gold": 15000,
      "skillbooks": 50
    },
    "has_enough_resources": true
  }
}
```

**Level Up Ally:**
- **Action**: `level_up_ally`
- **Parameters**: `{"ally_id": "04_Aurelia"}`
- **Response**:
```json
{
  "action": "level_up_ally", 
  "success": true,
  "data": {
    "ally_id": "04_Aurelia",
    "old_level": 5,
    "new_level": 6,
    "costs_paid": {
      "gold": 5000,
      "skillbooks": 40
    },
    "remaining_resources": {
      "gold": 10000,
      "skillbooks": 10
    },
    "can_level_up_again": true
  }
}
```

#### 6. Authentication APIs
- **Login**: `POST /api/login` - Player authentication
- **Guest Login**: `POST /api/guest_login` - Guest login system

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