# Project Improvement Wishlist

## Tab/Panel State Management Issues

### Issue: Equipment/Gem Tab State Chaos
**Problem**: Multiple scripts manage tab state independently causing mismatched UI states
- `ItemLoader.currentItemType` - Content display
- `LoadButtonController.activeButtonIndex` - Tab visual highlighting  
- `SwitchPanels.GetCurrentTab()` - Button visibility logic

**Specific Bug**: After auto embed, gem content shows with equipment tab highlighted

### Proposed Solutions:

#### Option 1: Single State Manager ⭐ (Recommended)
**What**: Create `TabStateManager` as single source of truth
**How**: 
- Centralized state management
- Event-driven updates to all UI components
- Single `SetCurrentTab(ItemType)` method
**Cost**: Medium refactoring effort
**Benefits**: Clean architecture, no state conflicts
**Risks**: Requires touching multiple existing scripts

#### Option 2: Event-Driven Sync
**What**: Keep existing structure, add event system
**How**: `TabStateChanged` events, all components subscribe
**Cost**: Low effort
**Benefits**: Minimal changes to existing code
**Risks**: Still multiple sources of truth

#### Option 3: UI State Persistence
**What**: Save/restore tab state around profile refreshes
**How**: Store current tab before refresh, restore after
**Cost**: Very low effort
**Benefits**: Quick fix
**Risks**: 
- Race conditions (user clicks during restore)
- Stale state problems
- Multi-layer conflicts
- Timing issues
- Edge cases with invalid saved state

**Conclusion**: Option 1 is best long-term solution, but only worth it during major UI refactoring

---

## Future Issues

*Add more improvement items here...*