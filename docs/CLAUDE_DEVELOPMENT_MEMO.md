# Claude Development Memo

This document records changes, issues, and solutions made during development sessions to help future Claude agents understand the codebase evolution and avoid repeating mistakes.

## Session: 2025-01-10 - Sidekicks Upgrade API Integration

### Context
Working on integrating frontend AlliesGridSetup with backend API to load real sidekick upgrade data instead of hardcoded mock data.

### Changes Made

#### 1. Frontend API Integration
**Files Modified:**
- `Assets/Scripts/UI Scripts/Allies Menu/UpgradePanelManager.cs`
- `Assets/Scripts/UI Scripts/Allies Menu/AlliesGridSetup.cs`

**Changes:**
- Replaced mock data with API calls using `Config.BaseUrl`
- Updated data structures to match backend API format
- Added proper error handling and debug logging
- Integrated UpgradePanelManager with AlliesGridSetup for ally selection

**Important Pattern Learned:**
- Project uses `Config.BaseUrl` for all API calls, NOT inspector-configured URLs
- Follow existing patterns in `AuthService.cs` for API integration

#### 2. Backend Database Issue (CRITICAL BUG AND FIX)

**Problem:** 
All sidekicks shared `skill_id: 1`, causing the API to return ALL upgrade effects (100 entries) for every sidekick query instead of 5 unique levels per sidekick.

**Root Cause:**
- Initial populate script `populate_correct_sidekick_upgrades.rb` created upgrade effects for all 20 sidekicks
- All sidekicks used the same `skill_id: 1` 
- API controller queried by `skill_id` only: `BaseSkillLevelUpEffect.where(skill_id: sidekick.skill_id)`
- This returned ALL upgrade effects for ALL sidekicks (20 sidekicks × 5 levels = 100 duplicates)

**Solution Applied:**
1. **Database Fix:** 
   - Created `fix_sidekick_upgrades_v2.rb` script
   - Added sidekick identification to `effects` JSON field:
     ```ruby
     effects: {
       sidekick_id: sidekick.id,
       sidekick_fragment_name: sidekick.fragment_name
     }.to_json
     ```

2. **API Controller Fix:**
   - Modified `/app/controllers/api/allies_controller.rb`
   - Updated query to filter by specific sidekick:
     ```ruby
     BaseSkillLevelUpEffect.where(skill_id: sidekick.skill_id)
                           .select { |upgrade| 
                             effects = JSON.parse(upgrade.effects || '{}')
                             effects['sidekick_fragment_name'] == ally_id
                           }
     ```

**Files Modified:**
- `/Volumes/WD_SSD/UnityProjects/Rogue/project1-server/fix_sidekick_upgrades_v2.rb` (new)
- `/Volumes/WD_SSD/UnityProjects/Rogue/project1-server/app/controllers/api/allies_controller.rb`

**Why This Solution:**
- Couldn't change `skill_id` due to foreign key constraints to `BaseSkill` table
- Only one `BaseSkill` exists (id=1, name="222")
- Storing sidekick info in JSON field allows proper filtering without schema changes

#### 3. Data Validation

**Before Fix:** API returned 36+ duplicate entries for one sidekick
**After Fix:** API returns exactly 5 unique levels (L02, L06, L08, L15, L20) per sidekick

**Test Commands:**
```bash
curl -s "http://localhost:3000/api/allies/04_Aurelia/upgrade_levels" | jq '.'
curl -s "http://localhost:3000/api/allies/02_Gideon/upgrade_levels" | jq '.'
```

### Important Notes for Future Agents

1. **Mock Data Disclaimer:** 
   - All upgrade descriptions are TEMPORARY mock data
   - Will be replaced with real content later
   - Do NOT hardcode these descriptions in application logic

2. **Database Constraint:** 
   - All sidekicks must use `skill_id: 1` due to foreign key to `BaseSkill`
   - Use JSON fields or other methods for sidekick-specific data

3. **API Pattern:**
   - Backend: `/api/allies/:ally_id/upgrade_levels`
   - Frontend: Uses `Config.BaseUrl` + endpoint
   - Response format: 5 levels with L02, L06, L08, L15, L20

4. **Testing:**
   - Always verify API returns exactly 5 unique levels per sidekick
   - Check logs for duplicate entries if issues arise

### Mistakes Made & Lessons

1. **Initial Wrong Approach:** Tried to change `skill_id` values without understanding foreign key constraints
2. **Learning:** Always check database relationships before modifying key fields
3. **Better Solution:** Use flexible JSON fields for additional metadata when schema changes are restricted

### Files to Reference
- `docs/DEVELOPMENT_GUIDE.md` - General project patterns
- `Assets/Scripts/utils/Config.cs` - API configuration
- `Assets/Scripts/AuthService.cs` - API call examples

### Current Backend Database Tables & APIs

#### Database Tables
1. **`base_sidekicks`** - Sidekick template data
   - Fields: `id`, `name`, `cn_name`, `skill_id`, `fragment_name`, `description`, stats, display assets
   - 20 sidekicks total
   - All use `skill_id: 1` due to foreign key constraint

2. **`base_skill_level_up_effects`** - Upgrade descriptions (benchmark levels only)
   - Fields: `skill_id`, `level`, `description`, `effect_name`, `effects` (JSON)
   - Contains 100 records (20 sidekicks × 5 levels: L02, L06, L08, L15, L20)
   - **NOTE**: `weight` and `gold_cost` fields cleaned up - costs now come from CSV

3. **`base_skills`** - Skill definitions
   - Single record: `id=1, name="222"`
   - All sidekicks reference this

4. **`players`** - Player data
   - Fields: `gold_coin`, `diamond`, `stamina`, `exp`, `items_json`, etc.
   - Contains player resources and inventories

5. **`sidekicks`** - Player-owned sidekick instances
   - Fields: `base_id`, `player_id`, `skill_level`, `star`, `is_deployed`
   - Links to `base_sidekicks` template

6. **`battle_formations`** - Team composition
   - Fields: `player_id`, `sidekick1_id` through `sidekick4_id`
   - Up to 4 sidekicks per formation

7. **`equipments`** - Equipment system
   - Fields: `base_equipment_id`, `intensify_level`, `player_id`, `equip_with_sidekick_id`
   - Links to sidekicks for stat bonuses

#### CSV Configuration Files
1. **`level_up_costs.csv`** - **NEW** Universal level progression costs
   - Fields: `level`, `skillbook_cost`, `gold_cost`
   - Covers levels 1-20 with linear progression
   - Used by all sidekicks (universal costs)

2. **`base_sidekicks.csv`** - Sidekick template data
3. **`draw_cost.csv`** - Gacha system costs
4. **`base_items.csv`** - Items including skillbooks
5. **`product.csv`** - Monetization/IAP data
6. **`washing_config.csv`** - Equipment upgrade costs

#### API Endpoints

1. **`GET /api/allies/:ally_id/upgrade_levels`**
   - Purpose: Get benchmark upgrade descriptions for specific sidekick
   - Input: `ally_id` (e.g., "04_Aurelia")
   - Output: 5 benchmark levels (L02, L06, L08, L15, L20) with descriptions
   - Example: `curl "http://localhost:3000/api/allies/04_Aurelia/upgrade_levels"`

2. **`GET /api/level_up_costs`** - **NEW**
   - Purpose: Get universal level-up costs for all sidekicks
   - Input: None
   - Output: 20 levels with skillbook and gold costs
   - Example: `curl "http://localhost:3000/api/level_up_costs"`

3. **`GET /api/allies/:ally_id/level_up_cost`** - **NEW** 🔐
   - Purpose: Get current level and cost to upgrade to next level
   - Input: `ally_id` (e.g., "04_Aurelia") + JWT token
   - Output: Current level, next level cost, player resources, can_level_up status
   - Example: `curl -H "Authorization: Bearer <token>" "http://localhost:3000/api/allies/04_Aurelia/level_up_cost"`

4. **`POST /api/allies/:ally_id/level_up`** - **NEW** 🔐
   - Purpose: Level up a specific sidekick (deduct resources, increment level)
   - Input: `ally_id` + JWT token
   - Output: Success status, old/new levels, costs paid, remaining resources
   - Example: `curl -X POST -H "Authorization: Bearer <token>" "http://localhost:3000/api/allies/04_Aurelia/level_up"`

5. **`POST /api/login`** - Player authentication
6. **`POST /api/guest_login`** - Guest login system

#### Key Patterns
- **CSV Loading**: Use `CsvConfig.load_*` methods for configuration data
- **API Base URL**: Frontend uses `Config.BaseUrl` for all API calls
- **Resource Management**: Player currencies handled through `ResourceService`
- **Cost System**: Universal costs in CSV, specific descriptions in database

#### Important Notes
- **Benchmark vs Level Costs**: Benchmark levels (L02, L06, etc.) have descriptions only, NOT costs
- **Universal Costs**: All sidekicks use same level-up costs from `level_up_costs.csv`
- **Mock Data**: All upgrade descriptions are temporary and will be replaced
- **Foreign Key Constraint**: Cannot change `skill_id` in `base_sidekicks` - must stay as `1`

---

## Session: 2025-01-10 - Level Up Costs System

### Context
Created universal level-up cost system after realizing benchmark costs were incorrectly implemented.

### Changes Made

#### 1. Created Universal Cost System
**Files Created:**
- `/lib/config/level_up_costs.csv` - Universal level progression costs (1-20)
- `/cleanup_benchmark_costs.rb` - Script to remove wrong benchmark costs

**Files Modified:**
- `/lib/csv_config.rb` - Added `load_level_up_costs` method
- `/config/routes.rb` - Added `/api/level_up_costs` endpoint
- `/app/controllers/api/allies_controller.rb` - Added `level_up_costs` action

#### 2. Cost Structure
- **Universal Costs**: Same for all sidekicks (level 1-20)
- **Benchmark Descriptions**: Specific per sidekick (L02, L06, L08, L15, L20)
- **Separation**: Costs come from CSV, descriptions from database

#### 3. API Format
```json
{
  "level_up_costs": [
    {"level": 1, "skillbook_cost": 20, "gold_cost": 1000},
    {"level": 2, "skillbook_cost": 25, "gold_cost": 2000},
    ...
  ]
}
```

### Validation
- CSV loads successfully with 20 cost entries
- API endpoint returns proper JSON format
- Cleaned up wrong benchmark costs from database

---

## Session: 2025-01-10 - Level Up Action System

### Context
Implemented complete level-up functionality with resource management and validation.

### Design Decisions Made

#### Max Level Button Logic
- **Frontend**: Should hide level-up button when at max level (better UX)
- **Backend**: Must validate and reject level-up attempts past max level (security)
- **Max Level**: Set to 20 (based on CSV data)

#### Resource Management
- **Gold**: Universal currency stored in `player.gold_coin`
- **Skillbooks**: Per-sidekick items stored in `player.items_json` with names like `SKb_01_Zorath`
- **Level**: Stored in `player_sidekick.skill_level`

### APIs Implemented

#### 1. `GET /api/allies/:ally_id/level_up_cost`
**Purpose**: Get current level and cost for next level upgrade

**Response Format**:
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

**When at max level**:
```json
{
  "ally_id": "04_Aurelia",
  "current_level": 20,
  "max_level": 20,
  "can_level_up": false,
  "message": "Already at maximum level"
}
```

#### 2. `POST /api/allies/:ally_id/level_up`
**Purpose**: Execute level up action with resource deduction

**Success Response**:
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

**Error Responses**:
```json
// Insufficient resources
{"error": "Insufficient gold", "required": 5000, "available": 3000}
{"error": "Insufficient skillbooks", "required": 40, "available": 25, "skillbook_name": "SKb_04_Aurelia"}

// At max level
{"error": "Already at maximum level"}

// Player doesn't own sidekick
{"error": "Player doesn't own this ally"}
```

### Implementation Details

#### Authentication
- Uses JWT tokens in `Authorization: Bearer <token>` header
- Validates through `authenticate_user` before action filter
- Gets current player via `@current_user.player`

#### Resource Validation
1. **Check ownership**: Player must own the sidekick
2. **Check max level**: Current level must be < 20
3. **Check gold**: Player gold >= required gold cost
4. **Check skillbooks**: Player's specific skillbook count >= required skillbooks

#### Transaction Safety
- All changes wrapped in `ApplicationRecord.transaction`
- Rollback on any failure
- Atomic: either all succeed or all fail

#### Error Handling
- Proper HTTP status codes (400 for client errors, 500 for server errors)
- Detailed error messages with required vs available resources
- Transaction rollback on any exception

### Frontend Integration Notes

#### Level Up Button Logic
```javascript
// Get cost info first
const costInfo = await fetch(`/api/allies/${allyId}/level_up_cost`);
const data = await costInfo.json();

// Hide button if at max level or insufficient resources
if (!data.can_level_up || !data.has_enough_resources) {
  hidelevelUpButton();
} else {
  showLevelUpButton(data.cost);
}

// On button click
const result = await fetch(`/api/allies/${allyId}/level_up`, {method: 'POST'});
```

#### Resource Display
- Show current level and next level costs
- Display player's gold and skillbook counts
- Enable/disable button based on `has_enough_resources`
- Show max level indicator when at level 20

### Key Files Modified
- `/config/routes.rb` - Added level up routes
- `/app/controllers/api/allies_controller.rb` - Added level up methods
- Uses existing `/lib/config/level_up_costs.csv` for cost data

---

*Last Updated: 2025-01-10*
*Session Duration: ~3 hours*
*Status: Level Up Costs System Complete*