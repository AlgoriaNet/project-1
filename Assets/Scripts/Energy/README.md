# Energy (Stamina) Claim System

This document describes the implementation of the backend-synced energy/stamina claim system in Unity, supporting both daily and hourly energy claims.

## Overview

The energy system consists of two main components:

1. **Daily Energy Claims** - Player can claim energy at 12PM and 7PM each day
2. **Hourly Energy Regeneration** - Automatic stamina regeneration every hour (when stamina < 100)

## Architecture

### Core Components

- **`EnergyWebSocketApi.cs`** - WebSocket API for energy/stamina operations
- **`BaseMenuController.cs`** - Handles daily energy claim UI and logic  
- **`HourlyEnergyManager.cs`** - Manages automatic hourly energy regeneration
- **`EnergySystemInitializer.cs`** - Helper for initializing the energy system across scenes

### Backend Integration

The system uses WebSocket API with the following actions:

- **Daily Claims**: Action `"daily_claim"` with parameter `{type: "daily"}`
- **Hourly Claims**: Action `"hourly_claim"` with parameter `{type: "hourly", hours: 1}`

Backend response format:
```json
{
  "success": true,
  "player": { ...updated player data... },
  "type": "daily" | "hourly",
  "amount_added": 50
}
```

## Setup Instructions

### 1. Add Scripts to Your Project

All scripts should already be in place:
- `/Assets/Scripts/WebSocket/EnergyWebSocketApi.cs`
- `/Assets/Scripts/Energy/HourlyEnergyManager.cs`
- `/Assets/Scripts/Energy/EnergySystemInitializer.cs`

### 2. Configure WebSocket Backend

In `/Assets/Scripts/utils/Config.cs`, ensure the WebSocket URL points to your backend:

```csharp
// For local development
public static string WebSocketURL = "ws://localhost:5000";

// For production  
public static string WebSocketURL = "wss://your-production-server.com";
```

### 3. Set Up Hourly Energy Manager

**Option A: Add to an existing persistent GameObject**
1. Find a GameObject that persists across scenes (like a GameManager or StartGame object)
2. Add the `HourlyEnergyManager` component to it

**Option B: Create a dedicated Energy Manager object**
1. Create a new GameObject in your main scene
2. Name it "EnergyManager" 
3. Add the `HourlyEnergyManager` component
4. Check "Don't Destroy On Load" (or use `EnergySystemInitializer`)

**Option C: Use the EnergySystemInitializer**
1. Add `EnergySystemInitializer` component to any GameObject in your main scene
2. It will automatically create and manage the `HourlyEnergyManager`

### 4. Configure Hourly Energy Settings

In the Inspector for `HourlyEnergyManager`, you can adjust:
- **Enable Hourly Regeneration**: On/off switch for the system
- **Max Stamina For Hourly Regen**: Stamina cap for regeneration (default: 100)
- **Check Interval Seconds**: How often to check for regeneration (default: 300s = 5 minutes)

## How It Works

### Daily Energy Claims

1. Player sees claim buttons at 12PM and 7PM if they haven't claimed yet that day
2. Clicking the button sends a WebSocket request to backend
3. Backend validates the claim and returns updated player data
4. Frontend updates UI only after backend confirmation
5. Button disappears until next eligible time

### Hourly Energy Regeneration

1. `HourlyEnergyManager` runs continuously in the background
2. Every 5 minutes (configurable), it checks if an hour has passed since last regeneration
3. If player's stamina < 100, it sends a regeneration request to backend
4. Backend processes the request and returns updated stamina
5. Works even when player is offline (catches up on next login)

## Testing

### Testing Daily Claims

1. Use browser dev tools or backend logs to verify WebSocket messages
2. Check that the correct action (`daily_claim`) and parameters are sent
3. Verify player stamina updates after successful claim

### Testing Hourly Regeneration

The `HourlyEnergyManager` provides several testing tools:

**In Inspector Context Menu:**
- "Force Check Hourly Energy" - Immediately checks for regeneration
- "Reset Hourly Timer" - Sets last claim time to 2 hours ago for immediate testing

**Debug Information:**
```csharp
// Get current system state
HourlyEnergyManager manager = FindObjectOfType<HourlyEnergyManager>();
string info = manager.GetHourlyEnergyInfo();
Debug.Log(info);
```

**Manual Testing Steps:**
1. Reduce player stamina below 100
2. Use "Reset Hourly Timer" to simulate time passage
3. Use "Force Check Hourly Energy" to trigger regeneration
4. Verify backend receives `hourly_claim` request
5. Check that stamina increases after response

### Switching Between Local and Production

1. **Clear PlayerPrefs**: When switching servers, clear PlayerPrefs to ensure clean registration:
   ```csharp
   PlayerPrefs.DeleteAll();
   PlayerPrefs.Save();
   ```

2. **Update Config**: Change `Config.WebSocketURL` to point to the desired backend

3. **Re-register**: Create a new guest account or login to sync with the new backend

## Troubleshooting

### Common Issues

1. **Energy claims not working**:
   - Check WebSocket connection in browser dev tools
   - Verify backend is running and accessible
   - Ensure player is properly registered with the backend

2. **Hourly regeneration not triggering**:
   - Verify `HourlyEnergyManager` is active in the scene
   - Check that stamina is below the regeneration threshold
   - Use debug tools to verify timer state

3. **Backend 500 errors**:
   - Often indicates player not found in backend database
   - Clear PlayerPrefs and re-register
   - Check backend logs for specific error details

### Debug Logging

All energy system components include comprehensive debug logging:
- `[ClaimEnergy]` - Daily claim operations
- `[HourlyEnergyManager]` - Hourly regeneration system
- `[EnergyWebSocketApi]` - WebSocket communications

Enable Unity's Console to see detailed operation logs.

## Future Extensibility

The system is designed to easily support additional energy claim types:

### Adding New Claim Types

1. **Frontend**: Add new action calls with appropriate parameters:
   ```csharp
   var apiParams = new { type = "weekly" };
   _energyApi.Action("weekly_claim", apiParams, OnResponseCallback, OnErrorCallback);
   ```

2. **Backend**: Add new action handlers following the same response format

3. **UI**: Create appropriate UI elements and timing logic

### Possible Extensions

- **Weekly energy claims**
- **Event-based energy rewards**
- **Watch-ad-for-energy system**
- **Energy purchase with premium currency**
- **Social energy sharing**

All would follow the same pattern as daily/hourly claims, ensuring consistency and maintainability.

## Code Organization

```
Assets/Scripts/
├── WebSocket/
│   ├── EnergyWebSocketApi.cs          # WebSocket API for energy operations
│   └── WebSocketManager.cs           # Core WebSocket management
├── Energy/
│   ├── HourlyEnergyManager.cs         # Hourly regeneration system
│   └── EnergySystemInitializer.cs     # Scene setup helper
├── UI Scripts/
│   └── Base Menu/
│       └── BaseMenuController.cs      # Daily claim UI & logic
├── model/
│   ├── Player.cs                      # Player data model (Stamina property)
│   └── PlayerProfile.cs              # Player data management
└── utils/
    └── Config.cs                      # WebSocket URL configuration
```

This organization keeps energy-related code organized and separated while maintaining clean integration with the existing codebase.
