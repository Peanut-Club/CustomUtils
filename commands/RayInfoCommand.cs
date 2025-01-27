using BetterCommands;
using BetterCommands.Permissions;
using NorthwoodLib.Pools;
using System;
using System.Text;
using UnityEngine;

namespace CustomUtils.commands {
    public static class RayInfoCommand {

        [Command("rayinfo", CommandType.RemoteAdmin)]
        [Description("Print information about fired ray.")]
        [Permission(PermissionLevel.Lowest)]
        public static string RayInfoCmd(
            ReferenceHub sender,
            float distance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers,
            bool parentInfo = false
        ) {
            if (!Physics.Raycast(sender.PlayerCameraReference.position, sender.PlayerCameraReference.forward, out var hit, distance, layerMask)) {
                return "Nothing hit";
            }

            Transform transform = hit.collider.transform;

            var parents = ListPool<GameObject>.Shared.Rent();
            Transform t = transform.parent;
            while (t != null) {
                parents.Add(t.gameObject);
                t = t.parent;
            }

            var sb = StringBuilderPool.Shared.Rent();
            sb.AppendLine($"Raycast settings: distance={distance}, layerMask={layerMask}, printParentsChildren={parentInfo}");

            sb.AppendLine($"Found: <u>{transform.name}</u>");

            if (parents.Count > 0) {
                sb.AppendLine("Parents:");
                for (int i = parents.Count - 1; i >= 0; i--) {
                    var gameObject = parents[i];
                    AddTextAndPrintComponents("- " + gameObject.name, gameObject.transform, sb);
                }
                sb.AppendLine("- " + transform.name);
            }
            ListPool<GameObject>.Shared.Return(parents);

            sb.AppendLine();
            sb.AppendLine("Childrens:");
            PrintChildrens(parentInfo ? transform.parent ?? transform : transform, sb);

            int layer = transform.gameObject.layer;
            int tLayerMask = 1 << layer;
            sb.AppendLine();
            sb.AppendLine($"Layer {layer} ({tLayerMask}): {LayerMask.LayerToName(layer)}");

            return StringBuilderPool.Shared.ToStringReturn(sb);
        }

        public static void PrintNetworkComponents(Transform t, StringBuilder sb) {
            foreach (var component in t.GetComponents<System.Object>()) {
                var type = component.GetType();
                sb.Append($" <u>({type.Assembly.GetName().Name}:{type.Name})</u>");
            }
        }

        public static void PrintChildrens(Transform t, StringBuilder sb, int index = 0) {
            int childCount = t.childCount;
            if (childCount <= 0) {
                AddTextAndPrintComponents($"{new String(' ', index)}- {t.name}", t, sb);
                return;
            }

            AddTextAndPrintComponents($"{new String(' ', index)}- {t.name}:", t, sb);
            for (int child = 0; child < childCount; child++) {
                PrintChildrens(t.GetChild(child), sb, index + 1);
            }
        }

        private static void AddTextAndPrintComponents(string text, Transform t, StringBuilder sb) {
            sb.Append(text);
            PrintNetworkComponents(t, sb);
            sb.AppendLine();
        }
    }
}
