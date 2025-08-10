# Skill System Documentation

## Overview

The skill system is a core gameplay mechanic that allows sidekicks to cast various abilities during battle. Each sidekick has one skill that can be upgraded and customized through the level-up system.

## Architecture

### Core Classes

#### `Skill` Class (`Assets/Scripts/battle/skills/Skill.cs`)
The main data structure that defines all skill properties and behaviors.

#### `SkillSetting` Class (`Assets/Scripts/battle/skills/SkillSetting.cs`)
MonoBehaviour component that connects skills to GameObjects in the scene.

#### `SkillFactory` Class (`Assets/Scripts/battle/skills/SkillFactory.cs`)
Factory pattern implementation for creating skill instances at runtime.

#### `BaseSkillController` Class (`Assets/Scripts/battle/skills/BaseSkillController.cs`)
Abstract base class for all skill behavior implementations.

## Skill Data Structure

### Basic Information
```csharp
public class Skill
{
    public string Name;           // Unique identifier for the skill
    public string Icon;           // Icon sprite name from "skills/ICON/skill_icon"
    public string Description;    // Human-readable description
}
```

### Numerical Properties
```csharp
// Release Behavior
public int ReleaseCount = 1;          // Times skill activates per cast
public int LaunchesCount = 1;         // Projectiles launched per release
public int ReleaseUltimateCount = 10; // Ultimate threshold
public int SplitCount = 3;            // Projectile split count
public int MaxAngle = 60;             // Spread angle in degrees
public int Speed = 20;                // Movement speed

// Timing
public float DestroyDelay;            // Destruction delay
public float LaunchesInterval;        // Launch interval
public float Duration = 5f;           // Active duration (seconds)
public float Cd = 10f;               // Cooldown (seconds)
public float Scope = 1;              // Effect range multiplier

// Damage
public float DamageRatio = 1;         // Base damage multiplier
public float TwoStageDamageRatio = 0; // Stage 2 damage
public float ThreeStageDamageRatio = 0; // Stage 3 damage
public float ExtraDamageGain;         // Bonus damage from upgrades
```

### Behavior Flags
```csharp
public bool IsDynamic;                // Moves after creation
public bool IsLivingPositionRelease;  // Spawns at caster position
public bool IsImpenetrability = true; // Penetrates targets
public bool IsTraceMonster = false;   // Tracks enemies
public bool IsCdRestByReleased = false; // Cooldown resets on release
```

### Type System
```csharp
public DamageType DamageType;         // Physical, Fire, Ice, Thunder, etc.
public SkillTargetType SkillTargetType = SkillTargetType.LatestMultiple;
public List<string> ActiveCharacter = new(); // Special modifiers
```

## Damage Types

Available damage types (defined in `DamageType` enum):
- `Physical` - Standard physical damage
- `Fire` - Fire elemental damage
- `Ice` - Ice elemental damage
- `Thunder` - Lightning elemental damage
- `Light` - Holy/light damage
- `Dark` - Shadow/dark damage
- `Wind` - Wind elemental damage

## Target Types

Available targeting modes (`SkillTargetType` enum):
- `LatestMultiple` - Targets most recent enemies (multiple)
- `Nearest` - Closest enemy
- `Random` - Random enemy selection
- `All` - All enemies in range

## Available Skills

### Fire Skills
- **Skill_Fireball** - Basic projectile attack
- **Skill_FireTornado** - Spinning fire vortex
- **Skill_Blazing_Ray** - Continuous laser beam
- **Skill_Inferno** - Area fire explosion
- **Skill_SmallFireTornado** - Smaller tornado variant

### Ice Skills
- **Skill_IceCrackBullet** - Projectile that splits on impact
- **Skill_Ice_Spike** - Ice projectile attack

### Thunder Skills
- **Skill_Thunder_Punishment** - Lightning area attack
- **Skill_leidian** - Lightning bolt

### Wind Skills
- **Skill_Wind_Feather** - Wind-based projectile
- **Skill_Storm_Blade** - Wind blade attack

### Dark Skills
- **Skill_Dark_Touch** - Dark magic attack
- **Skill_Undead_Summoning** - Necromancy skill

## Implementation Guide

### Creating a New Skill

1. **Create Skill Data**
```csharp
var newSkill = new Skill
{
    Name = "MyNewSkill",
    Icon = "skill_icon_xxx",
    Description = "Skill description",
    Duration = 8f,
    Cd = 12f,
    DamageType = DamageType.Fire,
    ReleaseCount = 2,
    Speed = 25
};
```

2. **Create Skill Prefab**
   - Create GameObject in `Assets/Resources/skills/`
   - Name it `Skill_MyNewSkill.prefab`
   - Add `SkillSetting` component
   - Add custom skill controller (inherit from `BaseSkillController`)

3. **Implement Skill Controller**
```csharp
public class SkillMyNewSkill : BaseSkillController
{
    public override void BaseInit()
    {
        // Initialize skill-specific logic
    }

    protected override void WhenAttackAfter()
    {
        // Custom behavior after attack
    }
}
```

### Adding Skill to Sidekick

```csharp
var sidekick = new Sidekick
{
    Name = "MySidekick",
    Skill = newSkill,
    // ... other properties
};
```

## Skill Enhancement System

### Available Enhancement Functions

Located in `SkillLevelUpUtils.cs`:

```csharp
// Damage enhancements
SkillLevelUpUtils.ExtraDamageGain(skill, 0.2f);

// Timing enhancements
SkillLevelUpUtils.AddDuration(skill, 2f);
SkillLevelUpUtils.ReduceCd(skill, 1f);

// Range enhancement
SkillLevelUpUtils.AddScope(skill, 0.3f);

// Release enhancements
SkillLevelUpUtils.AddReleaseCount(skill, 1);
SkillLevelUpUtils.AddLaunchesCount(skill, 1);

// Special effects
SkillLevelUpUtils.ActiveCharacter(skill, "SPECIAL_EFFECT_ID");
```

### Creating Custom Enhancement Effects

```csharp
var enhancementEffect = new SkillLevelUpEffect
{
    Id = "damage_boost",
    SkillName = "Fireball",
    EffectName = "Damage Boost",
    Description = "Increases fireball damage by 20%",
    Weight = 10,           // Selection probability weight
    MaxCount = 5,          // Maximum times this can be selected
    Effects = new Dictionary<string, string> 
    { 
        { "ExtraDamageGain", "0.2" } 
    }
};
```

## Resource Management

### Icon System
- Icons stored in `Assets/Resources/skills/ICON/skill_icon.png`
- Use sprite names like `skill_icon_117` for Icon property
- Icons automatically loaded via `LoadIconSprite()` method

### Prefab Naming Convention
- All skill prefabs must be named `Skill_{SkillName}.prefab`
- Stored in `Assets/Resources/skills/` directory
- Loaded dynamically using `Path.SkillPrefab` pattern

### Animation Resources
- Skill animations in `Assets/Resources/skills/{SkillName}/`
- Animation controllers for complex skills
- Particle effects and visual components

## Active Character Modifiers

Special effects that can be applied to skills:

### Buff System
- `ADD_BUFF_{BuffType}` - Adds status effect to target
- `MonsterRefraction_{Count}_{Angle}_{Distance}` - Projectile refraction

### Examples
```csharp
skill.ActiveCharacter.Add("ADD_BUFF_BURN_5");           // 5-second burn
skill.ActiveCharacter.Add("MonsterRefraction_2_45_3");  // Refract twice, 45° angle, 3 units
```

## Testing and Debug

### Test Utilities
- `ShowAllSidekicks.cs` - Displays all sidekicks with test skills
- Console logging for skill activation and effects
- Visual debug information in Scene view

### Performance Considerations
- Skills use object pooling for projectiles
- Destroy delays prevent memory leaks
- Efficient targeting algorithms for large enemy counts

## Integration Points

### Battle System Integration
- Skills triggered by `SidekickManager`
- Cooldown management via UI components
- Damage calculation through `Living.Attack()`

### UI Integration
- Skill icons displayed in battle UI
- Cooldown visualization with masks
- Upgrade selection through `SkillLevelUpController`

### Save System Integration
- Skill states saved with sidekick data
- Enhancement effects persisted between battles
- Unlock states managed by progression system

## Best Practices

### Performance
- Use `DestroyDelay` to clean up skill objects
- Implement efficient collision detection
- Pool frequently used projectiles

### Design
- Keep skill durations reasonable (5-15 seconds)
- Balance cooldowns with skill power
- Provide clear visual feedback for all skills

### Code Organization
- One controller class per unique skill behavior
- Shared logic in `BaseSkillController`
- Consistent naming conventions

### Testing
- Test all combinations of enhancements
- Verify skill interactions don't break gameplay
- Performance test with multiple skills active

## Common Issues and Solutions

### Skill Not Appearing
- Check prefab naming: must be `Skill_{Name}.prefab`
- Verify prefab location: `Assets/Resources/skills/`
- Ensure `SkillSetting` component is attached

### Icon Not Loading
- Confirm icon exists in `skills/ICON/skill_icon` sprite sheet
- Check Icon property matches sprite name exactly
- Verify sprite import settings

### Performance Issues
- Implement proper `DestroyDelay` values
- Use object pooling for high-frequency skills
- Optimize collision detection loops

### Enhancement Not Working
- Verify enhancement function name in Effects dictionary
- Check if skill reference is valid in `SkillLevelUpController`
- Ensure MaxCount allows multiple selections

---

*Last updated: [Current Date]*
*For questions or clarifications, contact the development team.*