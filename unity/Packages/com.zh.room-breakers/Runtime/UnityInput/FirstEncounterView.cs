using System;
using System.Collections.Generic;
using System.Text;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;

namespace RoomBreakers.UnityInput
{
    // Procedural presentation only; one authoritative gameplay simulation supplies both scales.
    public sealed class FirstEncounterView : IDisposable
    {
        private readonly GameObject root;
        private readonly Transform world, mini, largeMote, smallMote, largePortal, smallPortal, largePip, smallPip, hand;
        private readonly Renderer largeBody, smallBody;
        private readonly Transform[,] refugeLights = new Transform[2, 3];
        private readonly Material gold, active, structure, portal, dark, white, pipMaterial;
        private readonly List<Material> materials = new List<Material>();
        private readonly List<AudioClip> clips = new List<AudioClip>();
        private readonly AudioSource audio;
        private readonly AudioClip returnSound, missSound;
        private readonly TextMesh instructions;
        private readonly ShellDuelView shellView;
        private readonly PipGuidanceView pipView;
        private readonly EncounterExperience experience;
        private int captured, missed;
        private string lastCopy;
        public HandsControlPanel Panel { get; }
        public Vector3 MiniatureMotePosition => smallMote.position;
        public FirstEncounterView(Transform parent, FirstEncounter game, Font font, Shader shader, EncounterExperience experience = null)
        {
            this.experience = experience;
            root = new GameObject("Encounter views - one shared simulation"); root.transform.SetParent(parent, false);
            gold = Material(shader, new Color(1, .64f, .18f)); active = Material(shader, new Color(.2f, .9f, .9f));
            structure = Material(shader, new Color(.3f, .4f, .5f)); portal = Material(shader, new Color(.48f, .3f, 1));
            dark = Material(shader, new Color(.04f, .05f, .10f)); white = Material(shader, Color.white);
            pipMaterial = Material(shader, new Color(.30f, .80f, .64f));
            world = Child(root.transform, "Room-scale view"); mini = Child(root.transform, "Miniature view");
            BuildRoom(mini, game.Plan.Room);
            if (game.Plan.Room.IsSynthetic) BuildRoom(world, game.Plan.Room);
            largeMote = Mote(world); smallMote = Mote(mini);
            largeBody = largeMote.GetChild(0).GetComponent<Renderer>(); smallBody = smallMote.GetChild(0).GetComponent<Renderer>();
            largePortal = Rift(world, game.Plan.Portal); smallPortal = Rift(mini, game.Plan.Portal);
            largePip = Pip(world, game.Plan.Refuge); smallPip = Pip(mini, game.Plan.Refuge);
            for (int view = 0; view < 2; view++) for (int i = 0; i < 3; i++)
                refugeLights[view, i] = Primitive(view == 0 ? largePip : smallPip, "Refuge light " + (i + 1),
                    PrimitiveType.Sphere, new Vector3((i - 1) * .11f, -.10f, -.16f), Vector3.one * .055f, pipMaterial);
            hand = Child(root.transform, "Enlarged schematic pinch hand - not articulated joint tracking");
            Primitive(hand, "Palm", PrimitiveType.Cube, Vector3.zero, new Vector3(.18f, .045f, .15f), active);
            Primitive(hand, "Index", PrimitiveType.Capsule, new Vector3(.05f, .03f, .10f), new Vector3(.035f, .10f, .035f), active).localRotation = Quaternion.Euler(90, 0, 0);
            Primitive(hand, "Thumb", PrimitiveType.Capsule, new Vector3(-.06f, 0, .09f), new Vector3(.035f, .07f, .035f), active).localRotation = Quaternion.Euler(70, 35, 0);
            var label = Child(root.transform, "Pip instructions"); instructions = label.gameObject.AddComponent<TextMesh>();
            instructions.font = font; instructions.fontSize = 40; instructions.characterSize = .0028f;
            instructions.anchor = TextAnchor.MiddleCenter; instructions.alignment = TextAlignment.Center;
            label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            Panel = new HandsControlPanel(root.transform, font, shader);
            shellView = new ShellDuelView(world, mini, largeMote, smallMote, shader);
            pipView = new PipGuidanceView(world, mini, largePip, smallPip, shader);
            var speaker = Child(root.transform, "Spatial outcome speaker"); audio = speaker.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false; audio.spatialBlend = 1; audio.volume = .20f; audio.minDistance = .5f; audio.maxDistance = 7;
            returnSound = Tone(660, .13f); missSound = Tone(180, .20f);
        }
        private static Transform Child(Transform parent, string name)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
        private Material Material(Shader shader, Color color)
        {
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            materials.Add(m); return m;
        }
        private static Transform Primitive(Transform parent, string name, PrimitiveType type, Vector3 p, Vector3 size, Material material)
        {
            var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(parent, false);
            g.transform.localPosition = p; g.transform.localScale = size; g.GetComponent<Renderer>().sharedMaterial = material;
            g.GetComponent<Collider>().enabled = false; return g.transform;
        }
        private static LineRenderer Line(Transform parent, string name, Vector3[] points, Material material, float width, bool loop = false)
        {
            var line = Child(parent, name).gameObject.AddComponent<LineRenderer>(); line.useWorldSpace = false;
            line.sharedMaterial = material; line.positionCount = points.Length; line.SetPositions(points); line.loop = loop;
            line.startWidth = line.endWidth = width; return line;
        }
        private void BuildRoom(Transform parent, RoomSnapshot room)
        {
            var floor = new Vector3[room.Floor.Points.Count];
            for (int i = 0; i < floor.Length; i++) floor[i] = new Vector3(room.Floor.Points[i].X, 0, room.Floor.Points[i].Y);
            Line(parent, "Floor outline", floor, structure, .018f, true);
            foreach (WallSpan wall in room.Walls)
                Line(parent, "Detected vertical wall", new[] { new Vector3(wall.A.X, wall.Bottom, wall.A.Y),
                    new Vector3(wall.A.X, wall.Top, wall.A.Y), new Vector3(wall.B.X, wall.Top, wall.B.Y),
                    new Vector3(wall.B.X, wall.Bottom, wall.B.Y) }, structure, .012f, true);
            foreach (RoomObstacle box in room.Obstacles)
            {
                var corners = new Vector3[8];
                for (int i = 0; i < 8; i++) corners[i] = new Vector3((i & 1) == 0 ? box.Min.X : box.Max.X,
                    (i & 2) == 0 ? box.Min.Y : box.Max.Y, (i & 4) == 0 ? box.Min.Z : box.Max.Z);
                for (int i = 0; i < 8; i++) for (int bit = 1; bit <= 4; bit *= 2)
                    if ((i & bit) == 0) Line(parent, "Conservative obstacle edge", new[] { corners[i], corners[i | bit] }, structure, .012f);
            }
        }
        private Transform Mote(Transform parent)
        {
            Transform mote = Child(parent, "Mote / Shell core");
            Primitive(mote, "Body", PrimitiveType.Sphere, Vector3.zero, new Vector3(.24f, .20f, .20f), gold);
            for (int side = -1; side <= 1; side += 2)
            {
                Primitive(mote, "Eye", PrimitiveType.Sphere, new Vector3(side * .045f, .03f, -.09f), Vector3.one * .065f, white);
                Primitive(mote, "Pupil", PrimitiveType.Sphere, new Vector3(side * .045f, .03f, -.12f), Vector3.one * .029f, dark);
            }
            return mote;
        }
        private Transform Pip(Transform parent, NVector refuge)
        {
            var pip = Child(parent, "Pip and refuge"); pip.localPosition = UnitySpatialFrame.Vector(refuge);
            Primitive(pip, "Refuge", PrimitiveType.Cylinder, Vector3.down * .14f, new Vector3(.40f, .055f, .40f), structure);
            Primitive(pip, "Pip body", PrimitiveType.Cube, Vector3.zero, new Vector3(.17f, .19f, .14f), pipMaterial);
            Primitive(pip, "Face", PrimitiveType.Cube, new Vector3(0, .025f, -.075f), new Vector3(.14f, .07f, .01f), dark);
            for (int side = -1; side <= 1; side += 2)
                Primitive(pip, "Pip eye", PrimitiveType.Sphere, new Vector3(side * .037f, .025f, -.085f), Vector3.one * .03f, white);
            return pip;
        }
        private Transform Rift(Transform parent, Pose3 pose)
        {
            var rift = Child(parent, "Rift"); rift.localPosition = UnitySpatialFrame.Vector(pose.Position);
            rift.localRotation = new Quaternion(pose.Rotation.X, pose.Rotation.Y, pose.Rotation.Z, pose.Rotation.W);
            Primitive(rift, "Rift interior", PrimitiveType.Sphere, Vector3.zero, new Vector3(.60f, .60f, .025f), dark);
            var points = new Vector3[48];
            for (int i = 0; i < points.Length; i++) { float a = i * Mathf.PI * 2 / points.Length; points[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * .32f; }
            Line(rift, "Rift edge", points, portal, .03f, true); return rift;
        }
        private AudioClip Tone(float frequency, float duration)
        {
            const int rate = 22050; int count = Mathf.RoundToInt(rate * duration); var samples = new float[count];
            for (int i = 0; i < count; i++) samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / rate) * Mathf.Sin(Mathf.PI * i / count) * .3f;
            var clip = AudioClip.Create("Original generated interaction tone", count, 1, rate, false); clip.SetData(samples, 0); clips.Add(clip); return clip;
        }
        private static void Frame(Transform t, SpatialFrame frame)
        { UnitySpatialFrame.ApplyPose(t, frame.Origin); t.localScale = Vector3.one * frame.Scale; }
        public void Refresh(FirstEncounter game, Camera camera, SpatialFrame uiFrame)
        {
            Frame(world, game.Session.Map.Room); Frame(mini, game.Session.Map.Miniature);
            Pose3 creature = game.CreaturePose;
            largeMote.localPosition = smallMote.localPosition = UnitySpatialFrame.Vector(creature.Position);
            var q = creature.Rotation;
            largeMote.localRotation = smallMote.localRotation = new Quaternion(q.X, q.Y, q.Z, q.W);
            largeMote.gameObject.SetActive(game.CreatureVisible); smallMote.gameObject.SetActive(game.CreatureVisible);
            bool held = game.Session.State == ProbeState.Held;
            bool selected = !game.ActiveIsReflector && (held || game.Controls.Capture.HoverHand != HandId.None);
            largeBody.sharedMaterial = smallBody.sharedMaterial = selected ? active : gold;
            hand.gameObject.SetActive(held && game.Session.HasValidCaptureSample && game.CreatureVisible);
            if (held) { UnitySpatialFrame.ApplyPose(hand, game.Session.LastHandWorld); hand.localScale = Vector3.one / game.Session.Map.Miniature.Scale; }
            float riftSize = game.Phase == EncounterPhase.Won ? .01f : 1 - game.Captured / (float)game.CaptureGoal * .80f;
            largePortal.localScale = smallPortal.localScale = Vector3.one * riftSize;
            // Do not animate the entire refuge using an unpaused wall clock.
            largePip.localScale = smallPip.localScale = Vector3.one;
            for (int v = 0; v < 2; v++) for (int i = 0; i < 3; i++) refugeLights[v, i].gameObject.SetActive(i < game.Integrity);
            if (game.Captured > captured) { audio.transform.position = largePortal.position; audio.PlayOneShot(returnSound); }
            if (game.Missed > missed) { audio.transform.position = largePip.position; audio.PlayOneShot(missSound); }
            captured = game.Captured; missed = game.Missed;
            shellView.Refresh(game); pipView.Refresh(game, experience);
            string copy = Copy(game);
            if (copy != lastCopy) { instructions.text = Wrap(copy, 52); lastCopy = copy; }
            // Text stays at the confirmed UI frame instead of translating with each head tilt.
            instructions.transform.position = UnitySpatialFrame.Vector(uiFrame.Origin.Position) + Vector3.up * .20f;
            var uiRotation = uiFrame.Origin.Rotation;
            instructions.transform.rotation = new Quaternion(uiRotation.X, uiRotation.Y, uiRotation.Z, uiRotation.W);
            Panel.Refresh(uiFrame, game.Controls);
        }
        private string Copy(FirstEncounter game)
        {
            string prefix = game.Plan.Room.IsSynthetic ? "SYNTHETIC ROOM / PRACTICE\n" : experience != null && experience.Practice ? "DEVICE ROOM / DEVELOPMENT\n" : "";
            if (experience == null) return prefix + "ROOMBREAKERS\nReturned " + game.Captured + "/" + game.CaptureGoal + " | Refuge " + game.Integrity + "/3";
            EncounterCoach coach = experience.Coach;
            bool result = game.Phase == EncounterPhase.Won || game.Phase == EncounterPhase.Lost;
            string body = prefix + coach.Chapter + "\n" + coach.Headline + "\n" + coach.Instruction;
            if (result) return body + "\n\n" + experience.ResultSummary + "\n" + experience.ProgressLine + "\n" + experience.SaveStatus;
            return prefix + coach.Chapter + "\nReturned " + game.Captured + "/" + game.CaptureGoal + " | Refuge " + game.Integrity + "/3\n\n" +
                coach.Headline + "\n" + coach.Instruction;
        }
        private static string Wrap(string text, int columns)
        {
            var output = new StringBuilder();
            foreach (string paragraph in text.Split('\n'))
            {
                int length = 0;
                foreach (string word in paragraph.Split(' '))
                {
                    if (length > 0 && length + 1 + word.Length > columns) { output.Append('\n'); length = 0; }
                    if (length > 0) { output.Append(' '); length++; }
                    output.Append(word); length += word.Length;
                }
                output.Append('\n');
            }
            return output.ToString().TrimEnd('\n');
        }
        public void Dispose()
        {
            pipView?.Dispose(); shellView?.Dispose(); Panel?.Dispose(); if (root != null) UnityEngine.Object.Destroy(root);
            foreach (var material in materials) if (material != null) UnityEngine.Object.Destroy(material);
            foreach (var clip in clips) if (clip != null) UnityEngine.Object.Destroy(clip);
        }
    }
}
