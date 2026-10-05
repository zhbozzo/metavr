using System;
using System.Collections.Generic;
using RoomBreakers.Core;
using UnityEngine;

namespace RoomBreakers.UnityInput
{
    // Presentation only. No collider, rigidbody, damage callback or independent projectile simulation.
    public sealed class ShellDuelView : IDisposable
    {
        private readonly Transform[] roots = new Transform[2], shields = new Transform[2], mirrors = new Transform[2];
        private readonly Transform[] rotors = new Transform[2], pulses = new Transform[2], charges = new Transform[2];
        private readonly LineRenderer[] incoming = new LineRenderer[2], outgoing = new LineRenderer[2];
        private readonly Renderer[] faces = new Renderer[2], balls = new Renderer[2];
        private readonly Material armor, metal, glass, highlight, warning;
        private readonly List<Material> materials = new List<Material>();
        private readonly List<AudioClip> clips = new List<AudioClip>();
        private readonly GameObject speakerObject;
        private readonly AudioSource speaker;
        private readonly AudioClip chargeTone, reflectTone, breakTone;
        private ShellDuel observed;
        private int shots, reflections, breaks;

        public ShellDuelView(Transform world, Transform miniature, Transform largeCreature, Transform smallCreature, Shader shader)
        {
            armor = MakeMaterial(shader, new Color(.28f, .34f, .46f));
            metal = MakeMaterial(shader, new Color(.09f, .13f, .22f));
            glass = MakeMaterial(shader, new Color(.18f, .55f, .76f));
            highlight = MakeMaterial(shader, new Color(.26f, 1, .75f));
            warning = MakeMaterial(shader, new Color(1, .48f, .16f));
            for (int i = 0; i < 2; i++)
            {
                roots[i] = Child(i == 0 ? world : miniature, "Shell duel representation");
                shields[i] = Child(i == 0 ? largeCreature : smallCreature, "Visible segmented armor");
                for (int segment = 0; segment < 6; segment++)
                {
                    float a = segment * Mathf.PI / 3;
                    Transform plate = Shape(shields[i], "Armor plate", PrimitiveType.Cube,
                        new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * .16f, new Vector3(.12f, .08f, .23f), armor);
                    plate.localRotation = Quaternion.Euler(0, 0, segment * 60);
                }
                mirrors[i] = Child(roots[i], "Fixed reflector plinth");
                Shape(mirrors[i], "Base", PrimitiveType.Cylinder, Vector3.down * .16f, new Vector3(.36f, .04f, .36f), metal);
                Shape(mirrors[i], "Pivot", PrimitiveType.Sphere, Vector3.down * .08f, Vector3.one * .12f, armor);
                rotors[i] = Child(mirrors[i], "Orientation-only reflector");
                Transform face = Shape(rotors[i], "Two-sided reflecting disc", PrimitiveType.Cylinder, Vector3.zero, new Vector3(.5f, .015f, .5f), glass);
                face.localRotation = Quaternion.Euler(90, 0, 0); faces[i] = face.GetComponent<Renderer>();
                Shape(rotors[i], "Grip bar", PrimitiveType.Cube, new Vector3(0, -.19f, 0), new Vector3(.24f, .045f, .07f), highlight);
                Shape(rotors[i], "Surface normal marker", PrimitiveType.Cube, new Vector3(0, 0, .09f), new Vector3(.028f, .028f, .18f), highlight);
                pulses[i] = Shape(roots[i], "One pulse represented at this scale", PrimitiveType.Sphere, Vector3.zero, Vector3.one * .07f, warning);
                balls[i] = pulses[i].GetComponent<Renderer>();
                charges[i] = Child(roots[i], "Attack telegraph ring");
                var ring = Line(charges[i], "Growing charge cue", warning, 48);
                ring.loop = true;
                for (int j = 0; j < 48; j++)
                { float a = j * Mathf.PI * 2 / 48; ring.SetPosition(j, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * .25f); }
                incoming[i] = Line(roots[i], "Announced incoming trajectory", warning, 2);
                outgoing[i] = Line(roots[i], "Predicted reflected trajectory", glass, 2);
            }
            speakerObject = new GameObject("Shell spatial event audio");
            speakerObject.transform.SetParent(world.parent, false);
            speaker = speakerObject.AddComponent<AudioSource>(); speaker.playOnAwake = false;
            speaker.spatialBlend = 1; speaker.volume = .16f; speaker.minDistance = .5f; speaker.maxDistance = 8;
            chargeTone = Tone(320, .24f); reflectTone = Tone(880, .10f); breakTone = Tone(520, .28f);
        }
        private static Transform Child(Transform parent, string name)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
        private static Transform Shape(Transform parent, string name, PrimitiveType type, Vector3 p, Vector3 scale, Material m)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = p; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = m; obj.GetComponent<Collider>().enabled = false;
            return obj.transform;
        }
        private Material MakeMaterial(Shader shader, Color color)
        {
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            materials.Add(m); return m;
        }
        private static LineRenderer Line(Transform parent, string name, Material material, int count)
        {
            var line = Child(parent, name).gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false; line.sharedMaterial = material; line.positionCount = count;
            line.startWidth = line.endWidth = .012f; return line;
        }
        private AudioClip Tone(float frequency, float duration)
        {
            const int rate = 22050; var samples = new float[Mathf.RoundToInt(rate * duration)];
            for (int i = 0; i < samples.Length; i++)
            { float t = i / (float)rate; samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * Mathf.Sin(Mathf.PI * i / samples.Length) * .25f; }
            var clip = AudioClip.Create("Original procedural Shell cue", samples.Length, 1, rate, false);
            clip.SetData(samples, 0); clips.Add(clip); return clip;
        }
        public void Refresh(FirstEncounter game)
        {
            ShellDuel duel = game.Duel;
            bool visible = game.ShellVisible;
            for (int i = 0; i < 2; i++)
            { roots[i].gameObject.SetActive(visible); shields[i].gameObject.SetActive(visible && duel.Armored); }
            if (!ReferenceEquals(duel, observed))
            { observed = duel; shots = reflections = breaks = 0; }
            if (!visible) return;
            Pose3 pose = game.ReflectorPose;
            Quaternion orientation = new Quaternion(pose.Rotation.X, pose.Rotation.Y, pose.Rotation.Z, pose.Rotation.W);
            bool preview = game.ActiveIsReflector && (game.Session.State == ProbeState.Held || duel.Phase == ShellPhase.AwaitingOrientation || duel.Phase == ShellPhase.Charging);
            for (int i = 0; i < 2; i++)
            {
                float scale = i == 0 ? 1 : game.Session.Map.Miniature.Scale;
                mirrors[i].localPosition = UnitySpatialFrame.Vector(pose.Position); rotors[i].localRotation = orientation;
                faces[i].sharedMaterial = game.ActiveIsReflector && (game.Controls.Capture.HoverHand != HandId.None || game.Session.State == ProbeState.Held) ? highlight : glass;
                pulses[i].gameObject.SetActive(duel.ProjectileVisible);
                pulses[i].localPosition = UnitySpatialFrame.Vector(duel.ProjectilePosition);
                balls[i].sharedMaterial = duel.Phase == ShellPhase.Reflected ? highlight : warning;
                charges[i].gameObject.SetActive(duel.Phase == ShellPhase.Charging);
                charges[i].localPosition = UnitySpatialFrame.Vector(duel.Lane.ShellPosition);
                charges[i].localScale = Vector3.one * (.6f + duel.ChargeProgress * .6f);
                incoming[i].enabled = preview; outgoing[i].enabled = preview;
                incoming[i].widthMultiplier = outgoing[i].widthMultiplier = scale;
                incoming[i].SetPosition(0, UnitySpatialFrame.Vector(duel.Lane.ShellPosition));
                incoming[i].SetPosition(1, UnitySpatialFrame.Vector(duel.Lane.ReflectorPosition));
                outgoing[i].SetPosition(0, UnitySpatialFrame.Vector(duel.Lane.ReflectorPosition));
                outgoing[i].SetPosition(1, UnitySpatialFrame.Vector(duel.PreviewEnd));
                outgoing[i].sharedMaterial = duel.PreviewHitsShell ? highlight : glass;
            }
            if (duel.ShotSerial > shots) Play(game, duel.Lane.ShellPosition, chargeTone);
            if (duel.ReflectionCount > reflections) Play(game, duel.Lane.ReflectorPosition, reflectTone);
            if (duel.ShieldBreakCount > breaks) Play(game, duel.Lane.ShellPosition, breakTone);
            shots = duel.ShotSerial; reflections = duel.ReflectionCount; breaks = duel.ShieldBreakCount;
        }
        private void Play(FirstEncounter game, System.Numerics.Vector3 canonical, AudioClip clip)
        {
            speaker.transform.position = UnitySpatialFrame.Vector(game.Session.Map.RoomView(new Pose3(canonical, System.Numerics.Quaternion.Identity)).Position);
            speaker.PlayOneShot(clip);
        }
        public void Dispose()
        {
            for (int i = 0; i < 2; i++)
            { if (roots[i] != null) UnityEngine.Object.Destroy(roots[i].gameObject); if (shields[i] != null) UnityEngine.Object.Destroy(shields[i].gameObject); }
            if (speakerObject != null) UnityEngine.Object.Destroy(speakerObject);
            foreach (Material m in materials) if (m != null) UnityEngine.Object.Destroy(m);
            foreach (AudioClip clip in clips) if (clip != null) UnityEngine.Object.Destroy(clip);
        }
    }
}
