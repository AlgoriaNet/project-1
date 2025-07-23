# Energy/Stamina Claim System

## Overview
This system provides robust, backend-synced energy claiming for both daily and hourly energy through DG's WebSocket API.

## Features

### Daily Energy Claims
- **Location**: `BaseMenuController.ClaimEnergy()`
- **API Call**: `daily_claim` with `{type: "daily"}`
- **Trigger**: Manual UI button press
- **Behavior**: Single claim per day, replaces local-only logic

### Hourly Energy Claims
- **Location**: `HourlyEnergyManager` component
- **API Call**: `hourly_claim` with `{type: "hourly"}`
- **Triggers**: 
  - **Regular**: Every hour (e.g., 13:00:01)
  - **Catch-up**: On login, claims all missed hours
- **Behavior**: Multiple API calls for missed hours (e.g., offline for 8 hours = 8 API calls)

## System Architecture

### Core Components

1. **PlayerWebSocketApi**: Handles all WebSocket communication
2. **HourlyEnergyManager**: Manages automatic hourly claims
3. **BaseMenuController**: Handles manual daily claims
4. **PlayerProfile.Data**: Updated on every successful claim

### API Specification (DG's Backend)

```json
// Daily Claim Request
{
  "action": "daily_claim",
  "params": {"type": "daily"}
}

// Hourly Claim Request  
{
  "action": "hourly_claim",
  "params": {"type": "hourly"}
}

// Response Format
{
  "player": { /* updated player object */ },
  "type": "daily" | "hourly",
  "amount_added": 50
}
```

### Automatic Setup
- `StartGame.cs` automatically attaches `HourlyEnergyManager` if not present
- Manager persists across scenes and handles all timing logic
- Uses PlayerPrefs to track last claim time across app restarts

## Key Behaviors

### Login Catch-up
When the player logs in after being offline:
1. Calculate hours missed since last claim
2. Send one API call per missed hour (up to 24 hour cap)
3. Each successful claim updates PlayerProfile.Data
4. UI automatically refreshes from updated data

### Regular Hourly Tick
- Checks every 5 minutes if a new hour has started
- Sends single API call with `{type: "hourly"}`
- Updates timestamp only on successful response

### Error Handling
- API errors are logged but don't break the system
- Failed claims don't update the timestamp (will retry next check)
- Button states are restored on errors

## Configuration

### Constants (HourlyEnergyManager)
- `HOURLY_CLAIM_INTERVAL_SECONDS = 3600` (1 hour)
- `CHECK_INTERVAL_SECONDS = 300` (check every 5 minutes)
- `MAX_CLAIMS_PER_SESSION = 24` (cap offline catch-up)

### WebSocket URL (Config.cs)
```csharp
public static string WEBSOCKET_URL = "ws://localhost:8080";
```

## Testing

### Manual Testing
1. **Daily Claims**: Press energy claim buttons in UI
2. **Hourly Claims**: Call `HourlyEnergyManager.ManualHourlyClaim()`
3. **Catch-up**: Clear PlayerPrefs, restart app, check logs

### Debug Logs
All claims include detailed logging:
- Request parameters sent
- Response data received  
- Stamina changes (before → after)
- Error messages

## UI Integration
- All UI updates happen automatically through PlayerProfile.Data changes
- No manual UI refresh needed - other scripts handle display updates
- Button states managed during API calls to prevent double-clicks

## Backend Requirements
- DG's WebSocket server running on configured URL
- Proper player authentication/session handling
- Stamina capping logic (recommended: 300 max stamina)
- Response format matching specification above

---

*Last Updated: System fully implemented and tested with DG's API specification*
