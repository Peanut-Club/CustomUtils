using BetterCommands;
using BetterCommands.Permissions;
using Compendium;
using MapGeneration;
using NorthwoodLib.Pools;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace CustomUtils.commands {
    public static class PrintLayersCommand {

        [Command("printlayers", CommandType.RemoteAdmin, CommandType.GameConsole)]
        [Description("Print all layers.")]
        [Permission(PermissionLevel.Lowest)]
        public static string PrintLayersCmd(ReferenceHub sender) {
            var sb = StringBuilderPool.Shared.Rent();

            for (int layer = 0; layer <= 64; layer++) {
                var layerName = LayerMask.LayerToName(layer);
                if (string.IsNullOrEmpty(layerName)) continue;

                int tLayerMask = 1 << layer;
                sb.AppendLine($"Layer {layer} ({tLayerMask}): {layerName}");
            }

            return StringBuilderPool.Shared.ToStringReturn(sb);
        }
    }
}
