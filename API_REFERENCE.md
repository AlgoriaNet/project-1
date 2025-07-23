# API Reference

## Configuration
- **Base URL**: `Config.BaseUrl = "http://127.0.0.1:3000"`
- **WebSocket**: `Config.websocket = "ws://127.0.0.1:3000/cable"`
- **Pattern**: Always use `Config.BaseUrl` + endpoint path

## Authentication APIs

### Login
- **POST** `/api/login` - Player authentication
- **POST** `/api/guest_login` - Guest login system

## Sidekick/Ally APIs

### Get Upgrade Descriptions
- **GET** `/api/allies/{ally_id}/upgrade_levels`
- **Auth**: Not required
- **Purpose**: Get benchmark descriptions (L02, L06, L08, L15, L20)

```json
{
  "ally_id": "02_Gideon",
  "name": "Gideon",
  "current_level": 1,
  "upgrade_levels": [
    {"level": "L02", "description": "Thunder damage +20%", "cost": 0, "is_unlocked": false}
  ]
}
```

### Check Level Up Cost 🔐
- **GET** `/api/allies/{ally_id}/level_up_cost`
- **Auth**: Required (JWT token)
- **Purpose**: Get current level, cost for next level, player resources

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

### Execute Level Up 🔐
- **POST** `/api/allies/{ally_id}/level_up`
- **Auth**: Required (JWT token)
- **Purpose**: Level up sidekick, deduct resources

**Success:**
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

**Errors:**
```json
{"error": "Insufficient gold", "required": 5000, "available": 3000}
{"error": "Insufficient skillbooks", "required": 40, "available": 25}
{"error": "Already at maximum level"}
```

### Universal Level Costs
- **GET** `/api/level_up_costs`
- **Auth**: Not required
- **Purpose**: Get universal level progression costs (1-20) for all sidekicks

```json
{
  "level_up_costs": [
    {"level": 1, "skillbook_cost": 20, "gold_cost": 1000},
    {"level": 2, "skillbook_cost": 25, "gold_cost": 2000}
  ]
}
```

## WebSocket APIs 🔐

**Channel**: `PlayerChannel`

### Ally Upgrade Levels
```javascript
// Get upgrade levels via WebSocket
{
  "action": "get_upgrade_levels",
  "ally_name": "04_Aurelia"
}
```

### Level Up Cost Check
```javascript
{
  "action": "get_level_up_cost", 
  "ally_name": "04_Aurelia"
}
```

### Level Up Execution
```javascript
{
  "action": "level_up_ally",
  "ally_id": "04_Aurelia"
}
```

## Energy System APIs

### Daily Energy Claims
```javascript
// WebSocket action
{
  "action": "daily_claim",
  "type": "daily"
}
```

### Hourly Energy Regeneration
```javascript
// WebSocket action - automatic
{
  "action": "hourly_claim",
  "type": "hourly", 
  "hours": 1
}
```

## Frontend Integration Examples

### API Call Pattern
```csharp
string apiUrl = $"{Config.BaseUrl}/api/allies/{allyId}/level_up_cost";
using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
{
    webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
    yield return webRequest.SendWebRequest();
    
    if (webRequest.result == UnityWebRequest.Result.Success)
    {
        var response = JsonUtility.FromJson<LevelUpResponse>(webRequest.downloadHandler.text);
        // Update UI with response data
    }
}
```

### Resource Display Pattern
```csharp
// Always use API response data, never make separate calls
if (data.can_level_up && data.has_enough_resources)
{
    levelUpButton.SetActive(true);
    levelUpButton.interactable = true;
}
else if (!data.can_level_up)
{
    levelUpButton.SetActive(false); // At max level
}
else
{
    levelUpButton.SetActive(true);
    levelUpButton.interactable = false; // Insufficient resources
}
```

## Key Implementation Rules

1. **Always use `Config.BaseUrl`** - Never hardcode API URLs
2. **Follow AuthService.cs pattern** - For consistent API implementation
3. **Update UI from API responses** - Never make separate refresh calls
4. **Log complete responses** during development
5. **Handle all error cases** with user-friendly messages

## Data Models

```csharp
[System.Serializable]
public class LevelUpResponse
{
    public string ally_id;
    public int current_level;
    public int next_level;
    public int max_level;
    public bool can_level_up;
    public LevelUpCost cost;
    public PlayerResources player_resources;
    public bool has_enough_resources;
}

[System.Serializable]
public class LevelUpCost
{
    public int skillbook_cost;
    public int gold_cost;
    public string skillbook_name;
}
```