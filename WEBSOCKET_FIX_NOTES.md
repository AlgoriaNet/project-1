# WebSocket Null Reference Fix

## Problem
The `HourlyEnergyManager` was trying to make WebSocket API calls before the WebSocket connection was established, causing a `NullReferenceException`.

## Root Cause
- `HourlyEnergyManager` starts in `Start()` and immediately calls `CheckAndClaimHourlyEnergy()`
- WebSocket connection is only established after user login/authentication
- This created a race condition where energy claims were attempted before WebSocket was ready

## Solution Applied

### 1. **Delayed Initial Check**
```csharp
// Instead of immediate check in Start(), delay it
StartCoroutine(DelayedInitialCheck());

private IEnumerator DelayedInitialCheck()
{
    yield return new WaitForSeconds(3f); // Wait for WebSocket
    CheckAndClaimHourlyEnergy();
}
```

### 2. **WebSocket Readiness Checks**
```csharp
private void ClaimHourlyEnergy(int hoursToClaim = 1)
{
    // Check if API is initialized
    if (_energyApi == null) return;
    
    // Check if WebSocketManager is ready
    if (WebSocketManager.Instance == null) return;
    
    // Proceed with claims...
}
```

### 3. **Continuous Monitoring**
```csharp
void Update()
{
    // Try initial check when WebSocket becomes ready
    if (!_hasPerformedInitialCheck && WebSocketIsReady())
    {
        CheckAndClaimHourlyEnergy();
        _hasPerformedInitialCheck = true;
    }
    
    // Regular interval checks...
}
```

### 4. **Enhanced Debugging**
```csharp
public void DebugStatus()
{
    Debug.Log($"WebSocket API Ready: {(_energyApi != null)}");
    Debug.Log($"WebSocketManager Ready: {WebSocketManager.Instance != null}");
    // ... other debug info
}
```

## Testing the Fix

### **Test 1: Check Logs**
After applying the fix, you should see:
```
[HourlyEnergyManager] Started. Last claim: 2025-07-05 08:00:00
[HourlyEnergyManager] Performing delayed initial energy check...
[HourlyEnergyManager] WebSocket ready, performing initial check...
```

### **Test 2: Manual Debug**
```csharp
// In Unity Console or Inspector
HourlyEnergyManager manager = FindObjectOfType<HourlyEnergyManager>();
manager.DebugStatus();
```

### **Test 3: Verify No More Null Reference**
- Start the game
- Watch for the previous error - it should be gone
- Energy claims should work once user is logged in

## Expected Behavior

### **Before Login:**
- No energy claims attempted
- WebSocket readiness checked every frame
- No errors thrown

### **After Login:**
- Automatic initial energy check performed
- Regular interval checks every 5 minutes
- Successful API calls to backend

### **On Error:**
- Graceful fallback with warning logs
- System continues to function
- Retries when WebSocket becomes available

## Verification Commands

```csharp
// Check if manager is working
HourlyEnergyManager.DebugStatus();

// Force a manual claim (after login)
HourlyEnergyManager.ManualHourlyClaim();

// Reset for testing
HourlyEnergyManager.ResetLastClaimTime();
```

---

**The fix ensures that hourly energy claims work reliably without WebSocket-related crashes.**
