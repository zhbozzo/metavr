# ROOMBREAKERS — submission working draft

**NOT READY TO SUBMIT.** Rewrite capability claims to match the final observed runtime and build. There is currently no verified APK or completed XR Simulator run documented for this project. [Development policy](../docs/SIMULATOR_FIRST.md).

## Identity

Working title: ROOMBREAKERS
Proposed tagline: Hold your room in your hands. Defend it by moving its miniature.
Proposed track: Gaming
Proposed division: New, subject to actual eligibility
Build path: Unity
Target launch date: [OWNER DECISION]
Development host / Unity / SDK / simulator: [VERIFY]
Simulator profiles exercised: [NOT YET VERIFIED]
Physical headsets tested by the team: **None**
Build version / commit / APK hash: [NOT AVAILABLE]

Do not store account emails, personal team details, signing keys or private channel invitations in this public file.

## Inspiration

ROOMBREAKERS explores a small hand action with a large consequence in a familiar room. Instead of asking the player to walk toward a distant threat, it brings a simplified miniature within reach. Room-based MR and scale-changing games have precedents; the intended contribution is their combination with hands-first defense influenced by room geometry, not the invention of miniature worlds.

## Intended experience — audit against the candidate

Protect Pip by manipulating miniature creatures and a reflector. An enlarged hand connects each miniature capture with its room-scale consequence. Motes can be returned directly; Shell first requires a reflected attack to expose its armor. The current code includes a tutorial, outcome, restart, contextual guidance and local cosmetic progress. The audiovisual/runtime experience still requires execution and verification.

Room geometry should alter placement or trajectories. Prove this using identified synthetic environments before claiming it. Do not describe simulation data as a live scan. Recovery paths should handle tracking, focus, invalid rooms and storage errors visibly.

## Construction and validation

Unity/C#, Meta SDK adapters and one canonical simulation driving two views. Offline, without accounts, multiplayer or generative AI services. The team chose a computer-only development and demonstration plan, using Meta XR Simulator rather than buying or relying on a physical headset.

The final deliverable remains an Android APK for Meta VR. XR Simulator is an API-level runtime, not an Android hardware emulator; simulator execution and APK compilation are separate evidence. Keep real hand/room adapters in the APK. Do not turn desktop mouse controls or fixtures into release dependencies.

After the simulator work actually runs, include:

> Developed and demonstrated using Meta XR Simulator. Physical-headset validation has not been performed by the team.

Installed versions, executed checks, fixtures, profile differences and observed performance: [ADD ACTUAL EVIDENCE]. Never report host frame rate as Quest frame rate.

## Hand interactions

Demonstrate launch, placement, selection, movement, orientation, release, pause, ending and restart through simulated hand input in the XR runtime. Describe which adapter was exercised, ownership/selection stability, invalid targets and recovery. A mouse-only Unity scene or synthetic samples injected into .NET do not establish the runtime integration.

## Future work

More valid encounter layouts, improved accessibility and final art may follow the proven core. Cosmetic progress already exists in code; do not list it as wholly unimplemented, or describe it as finished on hardware. Cross-session spatial placement remains a distinct possible extension.

## Final audit

Every present-tense capability maps to observed evidence; planned work is future tense. Video and APK use the declared source revision, with meaningful platform differences disclosed. No unperformed device tests, hardware guarantees, invented metrics or sponsor endorsement. The owner reviews eligibility, access, rights and required fields before authorizing submission.
