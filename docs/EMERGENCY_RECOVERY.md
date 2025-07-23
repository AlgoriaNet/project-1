# 🚨 EMERGENCY DATA RECOVERY GUIDE

## ⚠️ CRITICAL SITUATION
The HourlyEnergyManager caused WebSocket errors that resulted in **data corruption** - diamonds and gold data are missing!

## ✅ IMMEDIATE ACTIONS TAKEN
1. **DISABLED HourlyEnergyManager** - prevents further corruption
2. **RESTORED StartGame.cs** - removed corrupted code
3. **EMERGENCY BRAKE** - no more automatic energy claims

## 🔍 DATA RECOVERY STEPS

### **Step 1: Check Current Data Status**
```csharp
// In Unity Console - Check what data remains:
Debug.Log($"Current Stamina: {PlayerProfile.Data?.Player?.Stamina ?? -1}");
Debug.Log($"Current Gold: {PlayerProfile.Data?.Player?.Gold ?? -1}");
Debug.Log($"Current Diamonds: {PlayerProfile.Data?.Player?.Diamonds ?? -1}");
```

### **Step 2: Check PlayerPrefs (Backup Data)**
```csharp
// Check if any backup data exists in PlayerPrefs:
Debug.Log($"PlayerPrefs Keys: {string.Join(", ", PlayerPrefs.GetString("").Split(','))}");
```

### **Step 3: Backend Data Recovery**
1. **Login again** - this should reload player data from DG's backend
2. **Check if backend has correct data** 
3. **If backend data is correct**, the frontend will restore automatically

### **Step 4: Manual Recovery (if needed)**
If backend data is also corrupted, you may need to:
1. Contact DG to restore from database backup
2. Manually set player data via admin tools
3. Use test/admin WebSocket commands to restore resources

## 🛠️ DEBUGGING TOOLS

### **Check WebSocket Status:**
```csharp
// Check if WebSocket is connected properly:
try {
    bool wsReady = WebSocketManager.Instance != null;
    Debug.Log($"WebSocket Ready: {wsReady}");
} catch (Exception ex) {
    Debug.Log($"WebSocket Error: {ex.Message}");
}
```

### **Test WebSocket Connection:**
```csharp
// Use BaseMenuController's test method:
BaseMenuController controller = FindObjectOfType<BaseMenuController>();
controller?.TestEnergyConnection();
```

## ⚡ EMERGENCY RESTORE COMMANDS

### **Force Data Reload from Backend:**
```csharp
// If you have access to player login/refresh:
// Re-login or refresh player data from backend
```

### **Temporary Resource Grant (Admin Only):**
```csharp
// If you have admin access, use WebSocket to grant resources:
// Admin command to restore gold/diamonds
```

## 🚦 NEXT STEPS

### **BEFORE Re-enabling Energy System:**
1. ✅ **Verify WebSocket is stable** - no null reference errors
2. ✅ **Test with small amounts** - verify no data corruption  
3. ✅ **Backup player data** - save current state before testing
4. ✅ **Monitor logs closely** - watch for any WebSocket issues

### **Safe Energy System Re-activation:**
1. **Fix WebSocket initialization** - ensure proper startup order
2. **Add data validation** - verify data before/after API calls
3. **Add rollback mechanism** - restore data if corruption detected
4. **Test on dev/staging first** - never test on production data

## 🔧 CURRENT SYSTEM STATUS

- ❌ **HourlyEnergyManager**: DISABLED (preventing further damage)
- ✅ **Daily Energy Claims**: Still working (BaseMenuController)  
- ✅ **WebSocket**: Available but potentially unstable
- ❓ **Player Data**: CORRUPTED (gold/diamonds missing)

## 📞 IMMEDIATE PRIORITIES

1. **Restore player gold/diamonds** from backend/backup
2. **Identify root cause** of WebSocket null reference
3. **Implement safety measures** before re-enabling hourly system
4. **Test thoroughly** on non-production data

---

**⚠️ DO NOT re-enable HourlyEnergyManager until WebSocket issues are completely resolved and data is safely restored!**
