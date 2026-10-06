# ROOMBREAKERS — judge instructions template

**DRAFT — NOT READY TO PUBLISH.** No verified APK or completed XR Simulator integration is currently evidenced. Replace verification fields only after executing the corresponding checks. Development policy: [simulator-first, no physical headset dependency](../docs/SIMULATOR_FIRST.md).

## Build, access and validation scope

Build/version/commit/APK hash: [NOT AVAILABLE UNTIL BUILT]
Development host / Unity / SDK / XR Simulator versions: [VERIFY]
Simulator device profiles actually exercised: [VERIFY; not physical devices]
Physical-headset validation by the team: **Not performed; outside the development plan.**
Competition-channel invitation: [PROVIDE IN SUBMISSION, NOT PUBLIC GIT]
Required room configuration: [VERIFY DOCUMENTED PROCEDURE AND IMPLEMENTATION]
Known limitations: [LIST OBSERVED LIMITATIONS AND UNVERIFIED HARDWARE AREAS]

Planned disclosure, usable only after the simulator work has actually run:

> Developed and demonstrated using Meta XR Simulator. Physical-headset validation has not been performed by the team.

The video is a capture of the declared simulator runtime, not proof that the Android APK executed on the development computer. State any meaningful platform/fixture differences. An APK build and a simulator run are separate artifacts.

## Intended route on the evaluator's Meta VR device

Verify the implementation and retain only supported steps before publication. These are evaluator instructions, not a requirement that the author acquire hardware.

1. Obtain the APK through the authorized Competition-channel invitation. No source compilation or developer credentials should be needed.
2. Use the supported hand interaction mode without pairing a controller.
3. Sit in a stationary position with clear space for small hand movements. Do not reach toward or strike real walls/furniture.
4. Follow the application's room-access explanation and the documented room setup procedure. An unsuitable room should produce an explicit recovery route, not a silently substituted synthetic room.
5. Follow the implemented miniature-placement instructions. Only describe adjustment controls that the build actually provides; do not assume height/size settings exist.
6. Pinch the miniature Mote, move it gently to the return zone and release. Observe the full-sized consequence.
7. After the Motes, orient the fixed-stand reflector to return Shell's pulse, capture Shell while vulnerable and return it to finish.
8. View the result and use the implemented pause/resume and confirmed restart controls.

## Recovery

Tracking loss, focus loss or room invalidation: [VERIFY IMPLEMENTED MESSAGE AND RESUME/RELOAD STEPS]. No unintended throw or damage should be produced by stale input.

Permission denied / unsuitable geometry: [INSERT IMPLEMENTED RECOVERY]. Storage unavailable: [INSERT IMPLEMENTED LOCAL/MEMORY STATUS]. Never ask judges to disable safety systems or use the author's credentials.

## What the demonstration can establish

Two visual scales consume one state; geometry alters a route or reflector configuration; the intended loop can be completed with simulated hands in the recorded runtime. Keep evidence and synthetic-room identifiers available.

Do not claim physical comfort, sensor accuracy, registration stability, Android persistence or device frame rate from simulator tests. The submitted APK must retain genuine device input/room adapters; no required mouse, autoplay, external MCP agent or local fixture path.
