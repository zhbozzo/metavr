using System;
using System.Collections.Generic;
using RoomBreakers.Core;
using UnityEngine;

namespace RoomBreakers.UnityInput
{
    // Decorative feedback only. No colliders, grabbing, damage, input or gameplay authority.
    public sealed class PipGuidanceView : IDisposable
    {
        private readonly Transform[] roots = new Transform[2];
        private readonly LineRenderer[] guides = new LineRenderer[2], markers = new LineRenderer[2], returns = new LineRenderer[2];
        private readonly Transform[,] beacon = new Transform[2, 3];
        private readonly Transform[,] eyes = new Transform[2, 2];
        private readonly Vector3[] curve = new Vector3[16];
        private readonly List<Material> materials = new List<Material>();
        private readonly Material guideMaterial, successMaterial, beaconMaterial;
        private bool disposed;

        public PipGuidanceView(Transform world, Transform miniature, Transform pip, Transform smallPip, Shader shader)
        {
            guideMaterial = MakeMaterial(shader, new Color(.45f, .87f, 1f));
            successMaterial = MakeMaterial(shader, new Color(.6f, 1f, .8f));
            beaconMaterial = MakeMaterial(shader, new Color(1f, .83f, .4f));
            Transform[] parents = { world, miniature }, pips = { pip, smallPip };
            for (int v = 0; v < 2; v++)
            {
                roots[v] = Child(parents[v], "Pip guidance and local beacon");
                guides[v] = Line(roots[v], "Guide arc - no interaction", guideMaterial, 16, false);
                markers[v] = Ring(roots[v], "Target emphasis", guideMaterial, .18f);
                returns[v] = Ring(roots[v], "Return acknowledgment", successMaterial, .28f);
                int eye = 0;
                foreach (Transform part in pips[v])
                    if (part.name == "Pip eye" && eye < 2) eyes[v, eye++] = part;
                for (int tier = 0; tier < 3; tier++)
                {
                    Transform piece = Child(roots[v], "Beacon milestone " + (tier + 1)); beacon[v, tier] = piece;
                    var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = "Earned beacon crystal";
                    g.transform.SetParent(piece, false);
                    g.transform.localScale = new Vector3(.045f, .09f + tier * .025f, .045f);
                    g.transform.localRotation = Quaternion.Euler(0, 45, 12);
                    g.GetComponent<Renderer>().sharedMaterial = beaconMaterial;
                    g.GetComponent<Collider>().enabled = false;
                }
            }
        }
        private static Transform Child(Transform parent, string name)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); return t; }
        private Material MakeMaterial(Shader shader, Color color)
        {
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            materials.Add(m); return m;
        }
        private static LineRenderer Line(Transform parent, string name, Material material, int count, bool loop)
        {
            var line = Child(parent, name).gameObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material; line.useWorldSpace = false; line.loop = loop;
            line.positionCount = count; line.startWidth = line.endWidth = .012f; return line;
        }
        private static LineRenderer Ring(Transform parent, string name, Material material, float radius)
        {
            var line = Line(parent, name, material, 32, true);
            for (int i = 0; i < 32; i++)
            { float a = i * Mathf.PI * 2 / 32; line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius)); }
            return line;
        }
        public void Refresh(FirstEncounter game, EncounterExperience experience)
        {
            if (disposed || experience == null) return;
            EncounterCoach coach = experience.Coach;
            Vector3 focus = Vector3.zero;
            switch (coach.Target)
            {
                case GuideTarget.Creature: focus = UnitySpatialFrame.Vector(game.CreaturePose.Position); break;
                case GuideTarget.Reflector: focus = UnitySpatialFrame.Vector(game.ReflectorPose.Position); break;
                case GuideTarget.ReturnZone: focus = UnitySpatialFrame.Vector(game.Plan.ReturnCenter); break;
            }
            bool guide = game.CreatureVisible && !game.Session.IsPaused && coach.Target != GuideTarget.None &&
                (coach.HintLevel > 0 || coach.BeatRemaining > 0);
            Vector3 start = UnitySpatialFrame.Vector(game.Plan.Refuge) + Vector3.up * .17f;
            Vector3 end = focus + Vector3.up * .05f;
            Vector3 bend = (start + end) * .5f + Vector3.up * .25f;
            for (int i = 0; i < curve.Length; i++)
            { float t = i / (float)(curve.Length - 1); curve[i] = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * bend + t * t * end; }
            float clock = (float)game.ActiveSeconds;
            float targetScale = 1f + .08f * Mathf.Sin(clock * 2f); // Slow emphasis, not flashing.
            for (int v = 0; v < 2; v++)
            {
                float scale = v == 0 ? game.Session.Map.Room.Scale : game.Session.Map.Miniature.Scale;
                guides[v].gameObject.SetActive(guide); markers[v].gameObject.SetActive(guide);
                guides[v].startWidth = guides[v].endWidth = v == 0 ? .01f : .0015f / scale;
                markers[v].startWidth = markers[v].endWidth = v == 0 ? .012f : .002f / scale;
                if (guide)
                {
                    guides[v].SetPositions(curve);
                    markers[v].transform.localPosition = end;
                    markers[v].transform.localScale = Vector3.one * targetScale;
                }
                // Terminal results use static trophies rather than an indefinitely frozen pulse.
                bool pulse = game.CreatureVisible && coach.ReturnPulseRemaining > 0;
                returns[v].gameObject.SetActive(pulse);
                if (pulse)
                {
                    returns[v].transform.localPosition = UnitySpatialFrame.Vector(game.Plan.ReturnCenter);
                    returns[v].transform.localScale = Vector3.one * (1f + .5f * (1f - (float)coach.ReturnPulseRemaining / .8f));
                }
                for (int tier = 0; tier < 3; tier++)
                {
                    beacon[v, tier].gameObject.SetActive(tier < experience.Journal.Current.BeaconTier);
                    beacon[v, tier].localPosition = UnitySpatialFrame.Vector(game.Plan.Refuge) +
                        new Vector3((tier - 1) * .11f, .22f, .08f);
                }
                float eyeHeight = coach.Mood == PipMood.Worried ? .5f : coach.Mood == PipMood.Celebrating ? .75f : 1;
                for (int i = 0; i < 2; i++)
                    if (eyes[v, i] != null) eyes[v, i].localScale = new Vector3(.03f, .03f * eyeHeight, .03f);
            }
        }
        public void Dispose()
        {
            if (disposed) return; disposed = true;
            foreach (Transform root in roots) if (root != null) UnityEngine.Object.Destroy(root.gameObject);
            foreach (Material material in materials) if (material != null) UnityEngine.Object.Destroy(material);
        }
    }
}
