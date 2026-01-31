# unity-social-vr
An experimental social VR library for Unity, released under the MIT License.

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

## Running locally

*TODO: add steps when code is added*

---

### System Architectural Proof of Concept (POC) high-level requirements

- [ ] PCVR support
- [ ] Flat-screen support for desktop PC
- [ ] Able to experience custom 3D scene data as Unity levels (e.g. FBX import)
- [ ] P2P networking for 3 or more simultaneous users
- [ ] Each user has an avatar
    - [ ] Users control their avatar's position
    - [ ] Everyone can clearly see each other's avatars when convening in a location in a level (can be as simple as a dot with a username label)
    - [ ] Everyone can clearly hear each user's microphone audio signal accurately placed in the 3D space (e.g. if an avatar moves away from you while they are talking then their voice should get quieter) 
- [ ] Each user spawns in their own "home level" that they can invite other users to
    - [ ] Users can choose from a default home level or a custom level they've added locally by adding the right files to their Unity project.
    - [ ] If a user selects a custom level pointing to files they have added locally then any user they invite to join them there should be able to have access to that data via P2P
- [ ] Users can freely teleport between levels they are invited to
- [ ] Data privacy and security
    - [ ] End-to-end encryption
    - [ ] Authentication
    - [ ] Access control
    - [ ] Audit controls
    - [ ] Integrity
