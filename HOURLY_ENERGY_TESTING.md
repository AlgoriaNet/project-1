# Hourly Energy System - Testing Guide

## System Overview
The hourly energy system tracks the last successful hourly claim and calculates how many hours have passed since then. It works based on **elapsed time from the last claim**, not from midnight.

## Key Components

### 1. **HourlyEnergyManager.cs**
- Tracks last claim time in PlayerPrefs: `"LastHourlyEnergyClaimTime"`
- Checks every 5 minutes if new claims are available
- Sends multiple API calls for missed hours
- API: `hourly_claim` with `{type: "hourly"}`

### 2. **BaseMenuController.cs** 
- Handles daily claims via UI buttons
- API: `daily_claim` with `{type: "daily"}`

### 3. **StartGame.cs**
- Automatically attaches HourlyEnergyManager on startup

## Testing Scenarios

### **Scenario 1: Login Catch-up**
```
Last claim: Yesterday 14:30:00
Login today: 08:05:01
Hours passed: ~18 hours
Expected: 18 API calls with {type: "hourly"}
```

### **Scenario 2: Regular Hourly Tick**
```
Playing since 08:00:00
Current time: 11:00:01
Expected: 3 claims (09:00, 10:00, 11:00)
```

### **Scenario 3: First Run**
```
Fresh install/cleared PlayerPrefs
Expected: Sets last claim to now, no retroactive claims
```

## Debug Commands

### **Console Commands** (call from script or inspector):
```csharp
// Show current status
HourlyEnergyManager manager = FindObjectOfType<HourlyEnergyManager>();
manager.DebugStatus();

// Reset last claim time (for testing)
manager.ResetLastClaimTime();

// Manual claim
manager.ManualHourlyClaim();
```

### **Testing Steps:**

1. **Clear PlayerPrefs** (to simulate first install):
   ```csharp
   PlayerPrefs.DeleteKey("LastHourlyEnergyClaimTime");
   ```

2. **Set artificial last claim time** (to simulate offline period):
   ```csharp
   DateTime testTime = DateTime.Now.AddHours(-5); // 5 hours ago
   PlayerPrefs.SetString("LastHourlyEnergyClaimTime", testTime.ToString("o"));
   ```

3. **Restart game or call**:
   ```csharp
   manager.DebugStatus(); // Shows how many claims are pending
   ```

## Expected Logs

### **Startup (with 3 missed hours):**
```
[HourlyEnergyManager] Loaded last claim time: 2025-07-05 05:00:00
[HourlyEnergyManager] Time check - Last claim: 2025-07-05 05:00:00, Now: 2025-07-05 08:05:01, Hours passed: 3
[HourlyEnergyManager] Making 3 hourly claim(s)...
[HourlyEnergyManager] Sending claim 1/3
[HourlyEnergyManager] Sending claim 2/3  
[HourlyEnergyManager] Sending claim 3/3
```

### **Each Successful Response:**
```
[HourlyEnergyManager] Stamina: 150 → 160 (+10)
[HourlyEnergyManager] Backend reported 10 stamina added
```

### **Regular Check (no claims needed):**
```
[HourlyEnergyManager] No claims needed - only 45.2 minutes since last claim
```

## API Specification

### **Hourly Claim Request:**
```json
{
  "action": "hourly_claim",
  "params": {"type": "hourly"}
}
```

### **Expected Response:**
```json
{
  "player": { /* updated player object with new stamina */ },
  "type": "hourly", 
  "amount_added": 10
}
```

## Configuration

### **Constants (editable in HourlyEnergyManager):**
- `HOURLY_CLAIM_INTERVAL_SECONDS = 3600` (1 hour)
- `CHECK_INTERVAL_SECONDS = 300` (check every 5 minutes)  
- `MAX_CLAIMS_PER_SESSION = 24` (max catch-up claims)

### **PlayerPrefs Keys:**
- `LastHourlyEnergyClaimTime` - Stores last successful claim timestamp

## Troubleshooting

### **No claims happening:**
1. Check if HourlyEnergyManager is attached to a GameObject
2. Verify WebSocket connection to backend
3. Check PlayerPrefs for last claim time
4. Call `DebugStatus()` to see calculated hours

### **Too many/few claims:**
1. Verify the calculation: `(now - lastClaim) / 3600 seconds`
2. Check if backend is capping stamina at 300
3. Ensure successful responses update `_lastClaimTime`

### **Claims not persisting across restarts:**
1. Verify PlayerPrefs.Save() is called after each successful claim
2. Check PlayerPrefs key format (ISO 8601: "o" format)

---

*The system is now fully implemented and ready for production use.*
