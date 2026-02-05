# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Reference Documents:**
- Project governance: [.specify/memory/constitution.md](.specify/memory/constitution.md)
- Project overview: [README.md](README.md)

## Project Overview

MWorldsTech is a Unity 6 game project with integrated MCP (Model Context Protocol) support, enabling AI assistants to directly interact with Unity Editor. The architecture follows a two-tier design:

**AI Assistant** ⇄ (stdio) ⇄ **Node.js MCP Server** ⇄ (WebSocket) ⇄ **Unity Editor** ⇄ **Game Objects/Scenes**

## Setup Requirements

### Asset Store Packages (Required)
Install these packages before running the project:
1. **Synty Basic Animation**

After installation:
- **Window > Render Pipeline > URP Converter**: Run "Built-in to URP" → "Material Upgrade"

### Folder Structure for Assets
```
Assets/
└── Asset Packs/
    └── (Asset Store content goes here)
```

## Development Commands

### Unity Operations
- Open Unity Editor and work with the project directly
- **Tools > MCP Unity > Server Window**: Start/stop MCP server and configure settings
- **Window > General > Test Runner**: Run Unity Test Framework tests (EditMode/PlayMode)
- Development scene: [DesktopRig.unity](Assets/Scenes/DesktopRig.unity)

### MCP Node.js Server
Navigate to: `Library/PackageCache/com.gamelovers.mcp-unity@*/Server~`

```bash
npm install          # Install dependencies
npm run build        # Compile TypeScript to build/
npm start            # Run MCP server
npm test             # Run tests
```

### MCP Tools Available
Use via natural language prompts with AI assistants:

**GameObjects**: `get_gameobject`, `update_gameobject`, `duplicate_gameobject`, `delete_gameobject`, `reparent_gameobject`, `select_gameobject`
**Transforms**: `move_gameobject`, `rotate_gameobject`, `scale_gameobject`, `set_transform`
**Components**: `update_component`
**Assets**: `create_prefab`, `add_asset_to_scene`, `add_package`
**Scenes**: `create_scene`, `load_scene`, `save_scene`, `delete_scene`, `unload_scene`, `get_scene_info`
**Materials**: `create_material`, `assign_material`, `modify_material`, `get_material_info`
**Testing**: `run_tests`, `recompile_scripts`
**Utilities**: `execute_menu_item`, `send_console_log`, `get_console_logs`, `batch_execute`

**Resources (read-only)**: `unity://menu-items`, `unity://scenes-hierarchy`, `unity://gameobject/{id}`, `unity://logs`, `unity://packages`, `unity://assets`

## Project Structure

```
Assets/
├── Scenes/
│   └── DesktopRig.unity            # Desktop FPS development scene
├── Game/Player/DesktopFps/         # FPS player controller
│   ├── CameraController.cs         # Mouse look with crouch offsets
│   ├── PlayerAnimationController.cs # Locomotion state machine
│   ├── PlayerCapsuleController.cs  # Height/collision management
│   └── InputSystem/
│       ├── InputReader.cs          # Event dispatcher
│       └── Controls.cs             # Input actions
├── Settings/                       # URP configs, build profiles
├── Prefabs/AnimatedPlayer.prefab   # FPS player prefab
└── Resources/                      # Runtime-loaded assets

Library/PackageCache/com.gamelovers.mcp-unity@*/  # MCP integration
ProjectSettings/                                   # Unity config
.specify/                                          # Spec Kit planning
```

## Tech Stack

- **Engine**: Unity 6.3+ with URP 17.3.0
- **Language**: C# 9.0 (.NET 4.7.1)
- **Platforms**: Windows Standalone (primary), Linux Dedicated Server, Android/Mobile

**Key Packages:**
- Unity Test Framework 1.6.0
- Unity Input System 1.17.0
- Unity AI Navigation 2.0.9
- TextMesh Pro
- Newtonsoft Json 3.2.2
- Timeline 1.8.10
- com.gamelovers.mcp-unity (GitHub package)

## Input System Configuration

Pre-configured Player action map:
- **Move**: Vector2 (WASD/Analog stick)
- **Look**: Vector2 (Mouse/Right analog stick)
- **Interact**: Button with Hold interaction
- **Crouch**: Toggle button

## MCP Server Configuration

- **Endpoint**: `ws://localhost:8090/McpUnity`
- **Config**: `ProjectSettings/McpUnitySettings.json` (auto-managed)
- **Timeout**: 10 seconds (configurable)
- **Remote Access**: Disabled by default (enable in Server Window to bind 0.0.0.0)

**Common Workflows:**
1. Start server: Tools > MCP Unity > Server Window → "Start Server"
2. Execute commands via natural language with AI assistant
3. Debug: `npm run inspector` in Server~ directory or check Unity console
4. Use `batch_execute` for atomic multi-step operations with rollback

## Player Controller Architecture

Component-based desktop FPS controller:

- **CameraController** - Mouse look (yaw/pitch) with crouch camera offsets
- **PlayerAnimationController** - Locomotion state machine (walk, crouch, jump, fall)
- **PlayerCapsuleController** - CharacterController height management with ceiling detection
- **InputReader** - Event-based input dispatcher (walk, sprint, crouch, aim, jump)

## Development Guidelines

### Governance
- No direct commits to main (PR required, CI must pass)
- Claude Code is authorized AI tool
- Spec Kit is planning source of truth
- Constitution supersedes local preferences
- Exceptions require written rationale in PR with minimal scope

### Adding New Systems
- Define clear state owner type
- Define public contract (interfaces, events, commands)
- Make systems easy to delete
- Start with Core layer logic, add Unity adapters last
- Avoid implicit discovery or inspector-driven behavior

### Definition of Done
- State flow is explicit
- Dependencies wired explicitly
- No inspector-driven behavior
- No hidden coupling to scene order or hierarchy
- Test coverage (unit tests in Core, Play Mode tests only when necessary)
- Clear PR description with intent
- No unrelated refactors bundled with feature work
