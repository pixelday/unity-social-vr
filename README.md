# unity-social-vr
Open-source experimental social VR library for Unity

---

This repo is the future home of code that can be downloaded and run locally on a Windows PC Unity dev environment. 

Proof of Concept (POC) high-level requirements
- [ ] PCVR support
- [ ] Flat-screen support for desktop PC
- [ ] Able to experience custom 3D scene data as Unity levels (e.g. FBX import)
- [ ] P2P networking for at least 3 simultaneous users
- [ ] Each user has an avatar
    - [ ] Users control their avatars position
    - [ ] Everyone can clearly see each other's avatars when convening in a location in a level (can be as simple as a dot with a username label)
    - [ ] Everyone can clearly hear each user's microphone audio signal accurately placed in the 3D space (e.g. if an avatar moves away from you while they are talking then their voice should get quieter) 
- [ ] Each user spawns in their own "home level" that they can invite other users to
    - [ ] Users can choose from a default home level or a custom level they've added locally by adding the right files to their Unity project.
    - [ ] If a user selects a custom home level pointing to files they have added locally then any user they invite to their should be able to have access to that data via P2P
