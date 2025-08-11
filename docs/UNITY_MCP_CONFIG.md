⏺ You might be right! Looking back at the sequence:

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