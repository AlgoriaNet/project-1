# Ally Merge System - Design Specification

## Overview
This document outlines the design for implementing the ally merge system where 10 shards can be merged into one ally character. The system requires backend support to track unlocked allies and provide merge functionality.

## Current State Analysis

### ✅ What's Already in Place
1. **Shard Display**: `OtherDetailBox.cs` shows shard quantities and has placeholder merge button
2. **Ally Grid UI**: `AlliesGridSetup.cs` and `LineupController.cs` display allies with unlock status
3. **Item Tracking**: `Player.ItemsJson` tracks shard quantities (e.g., "nyx_shard": 15)
4. **WebSocket API**: `PlayerWebSocketApi.cs` ready for new actions

### ❌ What's Missing (Backend Support Needed)
1. **Ally Unlock Tracking**: No field in Player model to track which allies are unlocked
2. **Merge API**: No backend endpoint to convert 10 shards → 1 ally
3. **Frontend Integration**: Hardcoded ally unlock data needs to use backend data

## Required Backend Changes

### 1. Update Player Model
Add a new field to track unlocked/summoned allies:

```json
// Player object should include:
{
  "id": 12345,
  "name": "PlayerName",
  "stamina": 150,
  "items_json": {
    "nyx_shard": 15,
    "aurelia_shard": 8,
    // ... other items
  },
  "summoned_allies": ["nyx", "aurelia"], // NEW FIELD
  // ... other existing fields
}
```

### 2. Implement Merge API Endpoint

**Action**: `"summon_ally"`

**Request Format**:
```json
{
  "action": "summon_ally",
  "params": {
    "ally_name": "nyx",
    "shards_used": 10
  }
}
```

**Backend Logic**:
1. Validate player has ≥10 shards for the specified ally
2. Deduct 10 shards from `items_json["{ally_name}_shard"]`
3. Add ally name to `summoned_allies` array (if not already present)
4. Return updated player data

**Response Format**:
```json
{
  "success": true,
  "player": { 
    // Updated player object with:
    // - Reduced shard count
    // - Ally added to summoned_allies
  },
  "ally_summoned": "nyx",
  "shards_used": 10
}
```

**Error Cases**:
- Insufficient shards: `{"error": "Not enough shards. Need 10, have 5"}`
- Already summoned: `{"error": "Ally already summoned"}`
- Invalid ally: `{"error": "Unknown ally name"}`

## Frontend Implementation

### 1. Update Player Model
```csharp
// In Player.cs, add:
[JsonProperty("summoned_allies")] 
public List<string> SummonedAllies { get; set; } = new List<string>();
```

### 2. Implement Merge Logic
```csharp
// In OtherDetailBox.cs
private void OnMergeClicked()
{
    string allyName = GetAllyNameFromFileName(currentFileName); // e.g., "nyx"
    var player = PlayerProfile.Data.Player;
    
    // Check if player has enough shards
    string shardKey = $"{allyName}_shard";
    int shardCount = player.ItemsJson.TryGetValue(shardKey, out int count) ? count : 0;
    
    if (shardCount < 10)
    {
        ShowError($"Need 10 shards to summon {allyName}. You have {shardCount}.");
        return;
    }
    
    // Check if already summoned
    if (player.SummonedAllies.Contains(allyName))
    {
        ShowError($"{allyName} has already been summoned!");
        return;
    }
    
    // Send merge request
    var apiParams = new { ally_name = allyName, shards_used = 10 };
    PlayerWebSocketApi.Instance.Action("summon_ally", apiParams, OnSummonSuccess, OnSummonError);
}

private void OnSummonSuccess(JObject response)
{
    // Update player data
    PlayerProfile.Data.SetPlayer(response["player"].ToObject<Player>());
    
    string allyName = response["ally_summoned"].ToString();
    Debug.Log($"✅ Successfully summoned {allyName}!");
    
    // Show celebration popup
    ShowSummonCelebration(allyName);
    
    // Refresh UI
    RefreshOtherTab();
    RefreshAlliesGrid();
}
```

### 3. Update Ally Display Logic
```csharp
// In AlliesGridSetup.cs, replace hardcoded unlocked logic:
private bool IsAllyUnlocked(string allyName)
{
    var player = PlayerProfile.Data.Player;
    return player?.SummonedAllies?.Contains(allyName.ToLower()) ?? false;
}

// Update LoadAllyItems() to use backend data:
foreach (var ally in allAllies)
{
    bool isUnlocked = IsAllyUnlocked(ally.name);
    // ... rest of ally setup logic
}
```

### 4. Dynamic Button Logic
```csharp
// In OtherDetailBox.cs
private void UpdateActionButton()
{
    string allyName = GetAllyNameFromFileName(currentFileName);
    bool isAlreadySummoned = HasCharacterBeenSummoned(allyName);
    int shardCount = GetShardCount(allyName);
    
    if (isAlreadySummoned)
    {
        // Show "Utilize" button for extra shards
        SetupUtilizeButton();
    }
    else if (shardCount >= 10)
    {
        // Show "Merge" button
        SetupMergeButton();
    }
    else
    {
        // Hide button or show "Need X more shards"
        HideActionButton();
    }
}

private bool HasCharacterBeenSummoned(string characterName)
{
    var player = PlayerProfile.Data.Player;
    return player?.SummonedAllies?.Contains(characterName.ToLower()) ?? false;
}
```

## Testing Strategy

### Backend Testing
1. **Valid Merge**: Player with 10+ shards successfully merges → ally added, shards deducted
2. **Insufficient Shards**: Player with <10 shards gets error message
3. **Already Summoned**: Attempt to merge already summoned ally gets error
4. **Edge Cases**: Invalid ally name, negative shards, etc.

### Frontend Testing
1. **UI Updates**: Verify ally grid shows newly summoned allies as unlocked
2. **Button States**: Merge button appears/disappears based on shard count and summon status
3. **Data Sync**: Confirm PlayerProfile.Data updates correctly after merge
4. **Error Handling**: Test error messages display properly

## Migration Strategy

### Phase 1: Backend Implementation
1. Add `summoned_allies` field to Player model
2. Implement `summon_ally` API endpoint
3. Test with existing player data (empty summoned_allies array)

### Phase 2: Frontend Integration
1. Update Player.cs model
2. Implement merge logic in OtherDetailBox.cs
3. Update ally display logic to use backend data
4. Replace hardcoded unlock data

### Phase 3: Testing & Polish
1. Test merge functionality end-to-end
2. Add celebration animations/popups
3. Implement "Utilize" functionality for extra shards
4. Performance testing with multiple allies

## Future Enhancements

### Star Promotion System
Once allies are summoned, extra shards can be used for star promotion:
- API: `"promote_ally"` with `{ally_name, promotion_level}`
- Cost: Varying shards per star level (e.g., 5 shards for 1→2 star, 10 for 2→3, etc.)

### Ally Management
- Ally collection screen showing all summoned allies
- Ally details with stats, skills, and upgrade options
- Ally sorting/filtering options

## Implementation Priority

### High Priority (Core Functionality)
1. ✅ Backend API for `summon_ally`
2. ✅ Frontend merge logic in OtherDetailBox
3. ✅ Update ally grid to use backend data

### Medium Priority (UX Improvements)
4. Celebration popup/animation on successful summon
5. Better error messaging and validation
6. "Utilize" functionality for extra shards

### Low Priority (Nice-to-Have)
7. Ally preview before merge
8. Batch operations for multiple merges
9. Undo functionality

---

## Summary

The ally merge system requires minimal frontend changes but significant backend support. The key missing piece is the `summoned_allies` field in the Player model and the `summon_ally` API endpoint. Once these are implemented, the frontend can easily integrate with the existing UI components.

**Next Steps:**
1. **Backend Developer**: Implement the `summoned_allies` field and `summon_ally` API
2. **Frontend Developer**: Update Player model and implement merge logic
3. **Testing**: Verify the system works end-to-end with real data

The design maintains consistency with your existing energy claim system architecture and WebSocket API patterns.
