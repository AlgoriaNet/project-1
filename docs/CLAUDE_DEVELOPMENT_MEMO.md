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

---

*Last Updated: 2025-01-10*
*Session Duration: ~2 hours*
*Status: Issue Resolved*