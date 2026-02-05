# unity-social-vr
An experimental social VR library for Unity, released as-is under the MIT License.

## Hardware and software requirements
- Windows PC
- X86 CPU
- Dedicated GPU (equivalent of Nvidia 3080 or above)
- Unity dev environment
- Computer Monitor (e.g. LG 27" panel or above) for testing flat screen experiences
- PCVR-capable HMD (e.g. Steam Frame) for testing VR experiences
- Microphone for testing user avatar voice in 3D spatial audio
- Speakers or headphones to hear other users' voices

---

## What this code is
A Unity 6 game project with MCP (Model Context Protocol) integration, enabling AI assistants to directly interact with Unity Editor for automated development workflows.

## Tech
- Unity 6.3LTS
- C# 9.0 (.NET 4.7.1)
- Universal Render Pipeline (URP)
- Unity Input System
- Node.js 18+ (for MCP server)
- Target platforms: Windows Standalone, Android/Mobile

In order to run, add the following asset packs from unity store or local into the correct folder as:

Assets

- / Asset Packs
  - / Synty/AnimationBaseLocomotion (https://syntystore.com/products/animation-base-locomotion)

Packages

- / TextMesh Pro (free package)

After installing asset packs, run window/render/render pipeline converter -> 'built in to urp' -> 'material upgrade'

## How to run
1. Open project in Unity 6 Editor
2. Start MCP server: **Tools > MCP Unity > Server Window** → "Start Server"
3. Open scene: **Assets/Scenes/DesktopRig.unity**
4. Press Play in Unity Editor

## Current Scene Layout
- **DesktopRig.unity**: Desktop FPS player development/testing scene

### Common Gotchas

- Wrong build profile selected
- Server scene included in client build
- TLS accidentally enabled for Telepathy
- Using 7777 directly instead of PlayFlow external port
- Forgetting to copy the server IP from PlayFlow

---

### Debugging

- Check PlayFlow dashboard logs for server output
- Confirm transport type matches PlayFlow port config
- Ensure firewall is not blocking the connection

---

### References

Official PlayFlow Mirror guide:
https://docs.playflowcloud.com/quickstart/mirror

---

## AI usage
- Claude Code is allowed
- Spec Kit is allowed
