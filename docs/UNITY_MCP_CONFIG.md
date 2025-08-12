⏺Unity MCP Config:

  1. Before my changes: Unity MCP was configured but connection was failing (port 6401/6500 wasn't working)
  2. After I removed/re-added the MCP config: You had to reconfigure Unity, which forced it to use port 6400
  3. Result: Now it works on port 6400

  So the complete fix procedure might actually be:

  Complete Unity MCP Reset (When stuck on wrong port)

  Claude Code side:
  claude mcp remove UnityMCP -s local
  # (Re-adding happens automatically from project config)

  Unity side:
  1. Stop Bridge completely
  2. Enable Auto-Connect (this forces port 6400 detection)
  3. Start Bridge (should connect on port 6400)
  4. Register with Claude Code

  The key insight: When Unity gets stuck on the wrong port (6401/6500), removing the MCP config from Claude
  Code side might force Unity to re-detect and use the correct port 6400 when you reconfigure.

  So maybe my "mistake" was actually the necessary first step to force the port reset! The full reset (both
  Claude and Unity sides) might be required when the connection gets stuck on the wrong port.



  🎯 Unity MCP Best Practices for AI Assistants

  🚀 Starting Battle System

  WRONG: Assuming battle objects are inactive and trying to activate them
  // DON'T DO THIS - Battle system is already active!
  battleObject.SetActive(true); // This will fail

  CORRECT: Use the proper BattleStarter script
  // Use BattleStarter.StartBattle() method instead
  // This hides main canvas and shows battle scene properly

  🎮 Unity Editor Mode vs Play Mode

  ⚠️ CRITICAL: Edit Mode vs Play Mode

  RULE: Changes made during Play Mode are TEMPORARY and will be lost when exiting play mode!

  WRONG: Making changes while play mode is active
  - Modifying GameObject positions during play
  - Changing component properties during runtime
  - These changes are NOT saved to the scene

  CORRECT: Always make persistent changes in Edit Mode
  - Stop play mode first
  - Make changes to GameObjects/components
  - Changes are saved to the scene file

  🔍 Unity MCP GameObject Investigation

  1. Hierarchy Understanding

  WRONG: Assuming activeInHierarchy=false means object is inactive
  "activeInHierarchy": false  // This could mean parent is inactive, not this object!

  CORRECT: Check both activeSelf and activeInHierarchy
  - activeSelf: This object's active state
  - activeInHierarchy: Whether object is active in the scene (includes parent state)

  2. Position Coordinate Systems

  CRITICAL: Always distinguish between local and world positions
  - localPosition: Position relative to parent
  - position: World position in scene
  - Use world positions for monster movement logic
  - Use local positions for UI/child object positioning

  3. Component Investigation

  Best Practice: Always get full component data before making assumptions
  // Use get_components action to see ALL component properties
  mcp__UnityMCP__manage_gameobject(action="get_components", target="ObjectName")

  🐛 Debugging with Unity Console

  1. Always Check Console First

  RULE: Read Assets/logs/console.log before making assumptions about what's broken

  2. Add Strategic Debug Logs

  // Add debug logs to trace logic flow
  Debug.Log($"[ClassName] {objectName} at Y={currentY:F2}, state={currentState}");


  🔧 Unity MCP Specific Tips

  1. GameObject Finding

  // Use specific search methods
  mcp__UnityMCP__manage_gameobject(action="find", search_term="ObjectName", search_method="by_name")

  2. Component Management

  // Get full component data before modifying
  mcp__UnityMCP__manage_gameobject(action="get_components", target="ObjectName")
  // Then modify with full context

  3. Scene State Verification

  // Always verify scene state before making assumptions
  mcp__UnityMCP__manage_scene(action="get_hierarchy")

  ⚡ Performance and Logic

  1. Avoid Infinite Loops

  Problem: Resume movement triggering immediately after stopping
  Solution: Use clear state boundaries and debug logging

  2. Coordinate System Precision

  Use precise Unity measurements instead of calculations:
  // Get exact positions from Unity instead of guessing
  WoodGroup.transform.position.y = -7.22  // From Unity
  SteelGroup.transform.position.y = -7.92 // From Unity