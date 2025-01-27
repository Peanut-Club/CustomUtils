using Compendium.Features;
using CustomPlayerEffects;
using HarmonyLib;
using helpers.Configuration;
using helpers.Patching;
using PlayerRoles;
using RemoteAdmin.Interfaces;
using SmartOverlays;
using System.Collections.Generic;

using EffectClass = CustomPlayerEffects.StatusEffectBase.EffectClassification;

namespace CustomUtils.utils {
    public static class EffectKeeper {
        [Config(Name = nameof(EscapedEffectClasses), Description = "Effect classes to keep while Escaping")]
        public static HashSet<EffectClass> EscapedEffectClasses { get; set; } = new HashSet<EffectClass>() {
            EffectClass.Positive,
            EffectClass.Mixed
        };

        [Config(Name = nameof(EscapedDisabledEffects), Description = "Effects to ignore while Escaping")]
        public static HashSet<string> EscapedDisabledEffects { get; set; } = new HashSet<string>() {
            nameof(Scp1344),
            nameof(Blindness)
        };

        public static string EffectKeptMessage { get; set; } = "<size=75%>Effect kept: <color=green>{0}</color></size> <size=50%>(for {1})</size>";
        public static string LongEffectKeptMessage { get; set; } = "<size=75%>Effect kept: <color=green>{0}</color></size>";
        public static int VOffset { get; set; } = -12;
        public static int HintDuration { get; set; } = 8;



        [Patch(typeof(StatusEffectBase), nameof(StatusEffectBase.OnRoleChanged), PatchType.Prefix)]
        public static bool OnRoleChanged(StatusEffectBase __instance, PlayerRoleBase newRole) {
            if (newRole.ServerSpawnReason != RoleChangeReason.Escaped) {
                return true;
            }
            if (!EscapedEffectClasses.Contains(__instance.Classification) || EscapedDisabledEffects.Contains(__instance.GetType().Name)) {
                return true;
            }

            if (__instance.Intensity > 0) {
                string effect_name = __instance.GetType().Name;
                if (__instance is ICustomRADisplay item) {
                    effect_name = item.DisplayName;
                }
                string message;
                if (__instance.TimeLeft > 0) {
                    message = string.Format(EffectKeptMessage, effect_name, formatSecondsToString((int)__instance.TimeLeft));
                } else {
                    message = string.Format(LongEffectKeptMessage, effect_name);
                }
                __instance.Hub.AddTempHint(message, HintDuration, VOffset);
            }

            return false;
        }

        public static string formatSecondsToString(int totalSeconds) {
            int minutes = (int)totalSeconds / 60;
            int seconds = (int)totalSeconds % 60;
            return $"{minutes}m:{seconds}s";
        }
    }
}
