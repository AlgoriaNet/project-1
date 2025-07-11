# Frontend Integration Guide: Sidekick Level Up System

## Overview
This guide explains how to integrate the sidekick level-up functionality with the backend APIs. The system allows players to spend gold and skillbooks to level up their sidekicks from level 1 to 20.

## Required APIs

### 1. Check Level Up Cost and Status
**Endpoint**: `GET /api/allies/:ally_id/level_up_cost`  
**Auth**: Required (JWT token)

**Purpose**: Get current level, cost for next level, and player's resources to determine if level up is possible.

**Request Example**:
```javascript
const response = await fetch(`${Config.BaseUrl}/api/allies/04_Aurelia/level_up_cost`, {
  headers: {
    'Authorization': `Bearer ${playerToken}`
  }
});
const data = await response.json();
```

**Response Examples**:

**Normal case (can level up)**:
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

**At max level**:
```json
{
  "ally_id": "04_Aurelia",
  "current_level": 20,
  "max_level": 20,
  "can_level_up": false,
  "message": "Already at maximum level"
}
```

### 2. Execute Level Up
**Endpoint**: `POST /api/allies/:ally_id/level_up`  
**Auth**: Required (JWT token)

**Purpose**: Execute the level up, deduct resources, increment level.

**Request Example**:
```javascript
const response = await fetch(`${Config.BaseUrl}/api/allies/04_Aurelia/level_up`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${playerToken}`,
    'Content-Type': 'application/json'
  }
});
const result = await response.json();
```

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
// Insufficient gold
{
  "error": "Insufficient gold",
  "required": 5000,
  "available": 3000
}

// Insufficient skillbooks
{
  "error": "Insufficient skillbooks",
  "required": 40,
  "available": 25,
  "skillbook_name": "SKb_04_Aurelia"
}

// At max level
{
  "error": "Already at maximum level"
}

// Player doesn't own this ally
{
  "error": "Player doesn't own this ally"
}
```

## Implementation Steps

### Step 1: Check Level Up Status
When displaying the ally upgrade screen:

```javascript
async function loadAllyLevelUpInfo(allyId) {
  try {
    const response = await fetch(`${Config.BaseUrl}/api/allies/${allyId}/level_up_cost`, {
      headers: {
        'Authorization': `Bearer ${getPlayerToken()}`
      }
    });
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    
    const data = await response.json();
    updateLevelUpUI(data);
    
  } catch (error) {
    console.error('Failed to load level up info:', error);
    showError('Failed to load level up information');
  }
}
```

### Step 2: Update UI Based on Status
```javascript
function updateLevelUpUI(data) {
  // Update level display
  document.getElementById('currentLevel').textContent = data.current_level;
  
  if (!data.can_level_up) {
    // At max level - hide level up button
    document.getElementById('levelUpButton').style.display = 'none';
    document.getElementById('maxLevelMessage').style.display = 'block';
    return;
  }
  
  // Show next level and costs
  document.getElementById('nextLevel').textContent = data.next_level;
  document.getElementById('goldCost').textContent = data.cost.gold_cost;
  document.getElementById('skillbookCost').textContent = data.cost.skillbook_cost;
  
  // Show player resources
  document.getElementById('playerGold').textContent = data.player_resources.gold;
  document.getElementById('playerSkillbooks').textContent = data.player_resources.skillbooks;
  
  // Enable/disable level up button based on resources
  const levelUpButton = document.getElementById('levelUpButton');
  const canAfford = data.has_enough_resources;
  
  levelUpButton.disabled = !canAfford;
  levelUpButton.style.display = 'block';
  
  if (!canAfford) {
    levelUpButton.textContent = 'Insufficient Resources';
    levelUpButton.classList.add('disabled');
  } else {
    levelUpButton.textContent = 'Level Up';
    levelUpButton.classList.remove('disabled');
  }
}
```

### Step 3: Handle Level Up Action
```javascript
async function performLevelUp(allyId) {
  // Disable button to prevent double-clicks
  const levelUpButton = document.getElementById('levelUpButton');
  const originalText = levelUpButton.textContent;
  levelUpButton.disabled = true;
  levelUpButton.textContent = 'Leveling Up...';
  
  try {
    const response = await fetch(`${Config.BaseUrl}/api/allies/${allyId}/level_up`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getPlayerToken()}`,
        'Content-Type': 'application/json'
      }
    });
    
    const result = await response.json();
    
    if (response.ok && result.success) {
      // Success - show level up animation/feedback
      showLevelUpSuccess(result);
      
      // Refresh the UI with new data
      await loadAllyLevelUpInfo(allyId);
      
    } else {
      // Handle error
      showError(result.error || 'Level up failed');
    }
    
  } catch (error) {
    console.error('Level up failed:', error);
    showError('Network error during level up');
    
  } finally {
    // Re-enable button
    levelUpButton.disabled = false;
    levelUpButton.textContent = originalText;
  }
}
```

### Step 4: Success Feedback
```javascript
function showLevelUpSuccess(result) {
  // Show success message with details
  const message = `
    ${result.ally_id} leveled up!
    ${result.old_level} → ${result.new_level}
    
    Costs paid:
    - Gold: ${result.costs_paid.gold}
    - Skillbooks: ${result.costs_paid.skillbooks}
    
    Remaining:
    - Gold: ${result.remaining_resources.gold}
    - Skillbooks: ${result.remaining_resources.skillbooks}
  `;
  
  showSuccessPopup(message);
  
  // Play level up animation/sound
  playLevelUpAnimation();
}
```

## Complete Integration Example

```javascript
class AllyLevelUpManager {
  constructor(allyId) {
    this.allyId = allyId;
    this.bindEvents();
  }
  
  bindEvents() {
    document.getElementById('levelUpButton').addEventListener('click', () => {
      this.performLevelUp();
    });
  }
  
  async initialize() {
    await this.loadLevelUpInfo();
  }
  
  async loadLevelUpInfo() {
    try {
      const response = await fetch(`${Config.BaseUrl}/api/allies/${this.allyId}/level_up_cost`, {
        headers: { 'Authorization': `Bearer ${getPlayerToken()}` }
      });
      
      const data = await response.json();
      this.updateUI(data);
      
    } catch (error) {
      console.error('Failed to load level up info:', error);
    }
  }
  
  updateUI(data) {
    const elements = {
      currentLevel: document.getElementById('currentLevel'),
      nextLevel: document.getElementById('nextLevel'),
      goldCost: document.getElementById('goldCost'),
      skillbookCost: document.getElementById('skillbookCost'),
      playerGold: document.getElementById('playerGold'),
      playerSkillbooks: document.getElementById('playerSkillbooks'),
      levelUpButton: document.getElementById('levelUpButton'),
      maxLevelMessage: document.getElementById('maxLevelMessage')
    };
    
    elements.currentLevel.textContent = data.current_level;
    
    if (!data.can_level_up) {
      elements.levelUpButton.style.display = 'none';
      elements.maxLevelMessage.style.display = 'block';
      return;
    }
    
    elements.nextLevel.textContent = data.next_level;
    elements.goldCost.textContent = data.cost.gold_cost.toLocaleString();
    elements.skillbookCost.textContent = data.cost.skillbook_cost;
    elements.playerGold.textContent = data.player_resources.gold.toLocaleString();
    elements.playerSkillbooks.textContent = data.player_resources.skillbooks;
    
    elements.levelUpButton.disabled = !data.has_enough_resources;
    elements.levelUpButton.style.display = 'block';
    elements.maxLevelMessage.style.display = 'none';
  }
  
  async performLevelUp() {
    const button = document.getElementById('levelUpButton');
    button.disabled = true;
    button.textContent = 'Leveling Up...';
    
    try {
      const response = await fetch(`${Config.BaseUrl}/api/allies/${this.allyId}/level_up`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getPlayerToken()}`,
          'Content-Type': 'application/json'
        }
      });
      
      const result = await response.json();
      
      if (response.ok && result.success) {
        this.showSuccess(result);
        await this.loadLevelUpInfo(); // Refresh UI
      } else {
        this.showError(result.error);
      }
      
    } catch (error) {
      this.showError('Network error');
    } finally {
      button.disabled = false;
      button.textContent = 'Level Up';
    }
  }
  
  showSuccess(result) {
    // Implement your success feedback here
    console.log('Level up successful:', result);
  }
  
  showError(message) {
    // Implement your error display here
    console.error('Level up error:', message);
  }
}

// Usage
const levelUpManager = new AllyLevelUpManager('04_Aurelia');
levelUpManager.initialize();
```

## Important Notes

1. **Authentication**: Both APIs require JWT token in Authorization header
2. **Error Handling**: Always check response status and handle specific error messages
3. **Button State**: Disable level up button when at max level or insufficient resources
4. **Resource Display**: Show both costs and player's current resources
5. **Feedback**: Provide clear success/error feedback to users
6. **Refresh**: Reload level up info after successful upgrade to show updated state

## UI Recommendations

- **Current Level**: Display prominently 
- **Next Level**: Show what level they'll reach
- **Costs**: Show both gold and skillbook requirements clearly
- **Player Resources**: Display current gold and skillbook counts
- **Button States**: 
  - Enabled: "Level Up" (green)
  - Disabled: "Insufficient Resources" (gray)
  - Hidden: When at max level
- **Max Level**: Show "MAX LEVEL" indicator when at level 20
- **Loading**: Show loading state during API calls

## Testing

Test these scenarios:
1. Normal level up with sufficient resources
2. Insufficient gold
3. Insufficient skillbooks  
4. At max level (level 20)
5. Player doesn't own the ally
6. Network errors
7. Invalid auth token