using System;
using System.Collections.Generic;
using RoomBreakers.Core;
using UnityEngine;
using NVector = System.Numerics.Vector3;

namespace RoomBreakers.UnityInput
{
    // Presentation only. All input, arbitration and confirmation live in the C# core.
    // These are pinchable tokens, not poke buttons. They deliberately use the same
    // pinch/release vocabulary as the probe and do not implement an extra system-menu gesture.
    public sealed class HandsControlPanel : IDisposable
    {
        private readonly Transform root;
        private readonly Transform[] shapes = new Transform[3];
        private readonly Renderer[] renderers = new Renderer[3];
        private readonly TextMesh[] labels = new TextMesh[3];
        private readonly TextMesh instruction;
        private readonly ControlTarget[] normal = new ControlTarget[3];
        private readonly ControlTarget[] confirmation = new ControlTarget[2];
        private readonly Material idle, hover, pressed, disabled;
        private readonly List<Material> materials = new List<Material>();
        private readonly Font font;
        private bool disposed;
        public IReadOnlyList<ControlTarget> Targets { get; private set; }
        private const float Radius = .034f;

        public HandsControlPanel(Transform parent, Font font, Shader shader)
        {
            if (parent == null || font == null || shader == null)
                throw new ArgumentException("Panel requires a root, font and renderer shader.");
            this.font = font;
            root = new GameObject("Hands-only controls - pinch then open").transform;
            root.SetParent(parent, false);
            idle = MakeMaterial(shader, new Color(.16f, .22f, .29f));
            hover = MakeMaterial(shader, new Color(.12f, .50f, .65f));
            pressed = MakeMaterial(shader, new Color(.05f, .66f, .40f));
            disabled = MakeMaterial(shader, new Color(.09f, .11f, .14f));
            for (int i = 0; i < 3; i++)
            {
                var shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shape.name = "Pinch control token " + i;
                shapes[i] = shape.transform;
                shapes[i].SetParent(root, false);
                shapes[i].localPosition = new Vector3((i - 1) * .095f, 0, 0);
                shapes[i].localScale = new Vector3(.07f, .048f, .025f);
                shape.GetComponent<Collider>().enabled = false;
                renderers[i] = shape.GetComponent<Renderer>();
                renderers[i].sharedMaterial = idle;
                labels[i] = CreateText("Control label " + i, .0023f);
                labels[i].transform.localPosition = shapes[i].localPosition + Vector3.back * .018f;
            }
            instruction = CreateText("Control instructions", .0021f);
            instruction.transform.localPosition = new Vector3(0, -.055f, 0);
            Targets = normal;
        }

        // Use the ACCEPTED frame, not mutable scene Transform data awaiting confirmation.
        // Physical button sizes stay constant when the miniature changes scale.
        public void Refresh(SpatialFrame miniature, HarnessControls controls)
        {
            if (disposed) return;
            Vector3 origin = UnitySpatialFrame.Vector(miniature.Origin.Position);
            Quaternion rotation = new Quaternion(miniature.Origin.Rotation.X, miniature.Origin.Rotation.Y,
                miniature.Origin.Rotation.Z, miniature.Origin.Rotation.W);
            root.SetPositionAndRotation(origin + rotation * new Vector3(0, -.055f, -.035f), rotation);
            root.localScale = Vector3.one;
            bool confirming = controls.Prompt != ControlPrompt.None;
            if (confirming)
            {
                confirmation[0] = Target(ControlAction.Confirm, 0);
                confirmation[1] = Target(ControlAction.Cancel, 2);
                Targets = confirmation;
                Display(0, ControlAction.Confirm, "CONFIRM", controls);
                Display(2, ControlAction.Cancel, "CANCEL", controls);
                shapes[1].gameObject.SetActive(false);
                labels[1].gameObject.SetActive(false);
                instruction.text = controls.Prompt == ControlPrompt.Restart
                    ? "Restart this probe?\nPinch, then open to choose."
                    : "Move miniature in front of you?\nRoom content stays in place.";
            }
            else
            {
                ControlAction pause = (controls.AllowedActions & ControlAction.Pause) != 0
                    ? ControlAction.Pause : ControlAction.Resume;
                normal[0] = Target(pause, 0);
                normal[1] = Target(ControlAction.Restart, 1);
                normal[2] = Target(ControlAction.Reposition, 2);
                Targets = normal;
                Display(0, pause, pause == ControlAction.Pause ? "PAUSE" : "RESUME", controls);
                Display(1, ControlAction.Restart, "RESTART", controls);
                Display(2, ControlAction.Reposition, "MOVE", controls);
                instruction.text = "Pinch a token, then open to choose.\nMove away to cancel.";
            }
        }
        private ControlTarget Target(ControlAction action, int index)
        {
            Vector3 p = shapes[index].position;
            return new ControlTarget(action, new NVector(p.x, p.y, p.z), Radius);
        }
        private void Display(int index, ControlAction action, string label, HarnessControls controls)
        {
            shapes[index].gameObject.SetActive(true);
            labels[index].gameObject.SetActive(true);
            bool enabled = (controls.AllowedActions & action) != 0;
            bool selected = controls.Menu.PressedAction == action;
            bool highlighted = controls.Menu.HoverAction == action;
            renderers[index].sharedMaterial = !enabled ? disabled : selected ? pressed : highlighted ? hover : idle;
            shapes[index].localScale = new Vector3(.07f, .048f, .025f) * (selected ? 1.08f : 1f);
            labels[index].text = label;
            labels[index].color = enabled ? Color.white : new Color(.55f, .58f, .62f);
        }
        private TextMesh CreateText(string name, float size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(root, false);
            var text = obj.AddComponent<TextMesh>();
            text.font = font;
            text.fontSize = 40;
            text.characterSize = size;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            obj.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return text;
        }
        private Material MakeMaterial(Shader shader, Color color)
        {
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            materials.Add(material);
            return material;
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (root != null) UnityEngine.Object.Destroy(root.gameObject);
            foreach (var material in materials)
                if (material != null) UnityEngine.Object.Destroy(material);
        }
    }
}
