# Claude Agent Guide

## Critical Architecture Patterns

### ❌ ANTI-PATTERN: Split Data Operations
**Never do this:**
```
1. Primary API call (e.g., star_upgrade) returns minimal data
2. Frontend makes separate API call (e.g., profile) to get updated state
3. Hope both APIs are synchronized
```

**Why this fails:**
- Race conditions between API calls
- Database may not be fully updated when second call executes
- Caching layers can return stale data
- Network timing creates unpredictable behavior

### ✅ CORRECT PATTERN: Atomic Complete Updates
**Always do this:**
```json
// star_upgrade response should include EVERYTHING needed
{
  "data": {
    "ally_id": "14_Kaelith",
    "new_star": 2,
    "gold": 28500,
    "shards": 72
  },
  "updated_sidekick": {
    "player_id": 4,
    "star": 2,
    "base_id": 14,
    "skill_level": 1,
    "id": 39,
    // ... complete sidekick object
  }
}
```

## API Design Checklist

### Backend Requirements
- [ ] Primary operation returns complete updated object state
- [ ] No separate API calls required for data consistency
- [ ] Response includes all data needed to update frontend state
- [ ] Document exact response structure location of each field

### Frontend Requirements  
- [ ] Log complete response structure during development
- [ ] Verify data extraction path matches actual response
- [ ] Update local state immediately from primary response
- [ ] Add comprehensive debug logging for data flow tracking

## Data Synchronization Rules

### 1. **Single Source of Truth Principle**
One API operation = One complete state update

### 2. **Immediate Update Principle** 
Never delay UI updates hoping "eventual consistency" will fix things

### 3. **Fail Fast Principle**
If data parsing fails, log everything and fail immediately - don't silently continue with stale data

### 4. **Complete Context Principle**
Return full object state, not just the changed fields

## Debugging Guidelines

### When UI doesn't update after backend operations:

1. **Log the complete backend response**
   ```csharp
   Debug.Log($"Complete response: {response}");
   ```

2. **Verify data extraction path**
   ```csharp
   // Check if data is at root or nested
   var data1 = response["field"];
   var data2 = response["data"]["field"];
   ```

3. **Log before/after state**
   ```csharp
   Debug.Log($"BEFORE: {PlayerProfile.Data.Sidekick}");
   // ... update logic
   Debug.Log($"AFTER: {PlayerProfile.Data.Sidekick}");
   ```

4. **Add timestamps to track timing**
   ```csharp
   Debug.Log($"Update at {System.DateTime.Now:HH:mm:ss.fff}");
   ```

## Common Gotchas

### Response Structure Mismatches
- Data might be at root level vs nested in "data" object
- Field names might differ between documentation and implementation
- Data types might differ (string vs int for IDs)

### Unity-Specific Issues
- GameObject lifecycle can interrupt coroutines
- Canvas updates may need manual refresh
- PlayerProfile listeners may not trigger properly

## Prevention Checklist

Before implementing any data update feature:

- [ ] Design API to return complete updated state
- [ ] Document exact response structure
- [ ] Create integration test for complete data flow
- [ ] Add comprehensive logging throughout the pipeline
- [ ] Verify UI updates immediately without additional API calls

## Unity Console Logging System

### Console to File Script
- **Location**: `Assets/Scripts/AIDebug/ConsoleToFile.cs`
- **Log Output Path**: `Assets/Logs/console.log`
- **Behavior**: Automatically captures all Unity console output (Debug.Log, Debug.LogError, etc.) to file
- **Active When**: Editor mode or Development builds only
- **Format**: `[HH:mm:ss] LogType: Message`
- **Clear Policy**: Log file is cleared on game start in Editor mode

### Usage for Claude Agents
```bash
# To read current Unity console logs:
cat "/Volumes/WD_SSD/UnityProjects/project-1-equip/Assets/Logs/console.log"

# To monitor logs in real-time:
tail -f "/Volumes/WD_SSD/UnityProjects/project-1-equip/Assets/Logs/console.log"
```

### Debug Logging Best Practices
- All debug logs automatically saved to file for analysis
- Use descriptive log messages with context
- Include timestamps for timing analysis
- Log complete API responses during development
- Add component/system prefixes: `[PlayerProfile]`, `[API]`, `[WebSocket]`

---

**Remember**: If you need to make a second API call to "refresh" data after a primary operation, your architecture is wrong. Fix the primary operation to return complete data instead.

## Project Structure
- **API Reference**: See `API_REFERENCE.md` for all endpoints
- **Console Logs**: `Assets/Logs/console.log` (auto-captured)
- **Development History**: See `DEVELOPMENT_HISTORY.md` for implementation context
- **Documentation Rules**: See `DOC_RULES.md` for update guidelines

## Key Scripts
- `Assets/Scripts/utils/Config.cs` - API configuration
- `Assets/Scripts/AuthService.cs` - API pattern reference  
- `Assets/Scripts/AIDebug/ConsoleToFile.cs` - Console logging

## Agent Guidelines
- Fix bugs freely
- Update documentation after changes
- Use atomic API responses (never split operations)
- Log complete responses during development
- Follow existing patterns in codebase