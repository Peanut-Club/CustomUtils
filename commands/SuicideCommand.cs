using BetterCommands;
using System;
using Compendium;
using Footprinting;
using HarmonyLib;
using helpers;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Events;
using UnityEngine;
using static PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers.FeetStabilizerSubcontroller;
using static InventorySystem.Items.Firearms.Modules.CylinderAmmoModule;
using Compendium.Features;

namespace CustomUtils.commands {
    public class SuicideCommand {
        private static readonly string SuicideMessage = "Committed suicide";


        [Command("suicide", CommandType.PlayerConsole, CommandType.RemoteAdmin)]
        [CommandAliases("kms")]
        [Description("Allows you to suicide with gun")]
        public static string SuicideCmd(ReferenceHub sender) {
            if (!sender.IsHuman()) {
                return "You cannot suicide";
            }

            Inventory inv = sender.inventory;
            if (!(inv.CurInstance is Firearm firearm)) {
                return "You aren't holding a gun!";
            }

            if (!FireGun(firearm)) {
                return "No ammo";
            }


            Vector3 forceVector = sender.transform.rotation * Vector3.back;
            forceVector.y = 1f;
            forceVector.Normalize();
            forceVector *= 5f;
            forceVector.y *= 2f;
            Plugin.Debug($"Suicide rotation: {sender.Rotation()}, back vector: {forceVector}, player: {sender}");

            StandardDamageHandler handler;
            if (firearm.Modules.TryGetFirst<DisruptorHitregModule>(out var disruptor)) {
                handler = new DisruptorDamageHandler(disruptor.DisruptorShotData, forceVector.normalized, float.MaxValue);
            } else {
                handler = new CustomReasonDamageHandler(SuicideMessage, -1f, "");
            }

            if (sender.characterClassManager.GodMode) {
                return "Cannot suicide, You have God mode!";
            }

            if (!EventManager.ExecuteEvent(new PlayerDamageEvent(sender, sender, handler))) {
                return "Cannot suicide, PlayerDamageEvent was cancelled";
            }
            handler.ApplyDamage(sender);
            handler.StartVelocity = forceVector;
            //PlayerStats.OnAnyPlayerDamaged?.(_hub, handler);
            //_hub.playerStats.OnThisPlayerDamaged?.Invoke(handler);
            if (!EventManager.ExecuteEvent(new PlayerDyingEvent(sender, sender, handler))) {
                return "Cannot suicide, PlayerDyingEvent was cancelled";
            }

            sender.playerStats.KillPlayer(handler);
            EventManager.ExecuteEvent(new PlayerDeathEvent(sender, sender, handler));
            return "You have committed suicide!";
        }

        public static bool FireGun(Firearm firearm) {
            if (firearm.TryGetModules<CylinderAmmoModule, DoubleActionModule>(out var cylinderModule, out var doubleActionModule)) {
                int ammoMax = cylinderModule.AmmoMax;
                var chambers = new ReadOnlySpan<Chamber>(GetChambersArrayForSerial(cylinderModule.ItemSerial, ammoMax), 0, ammoMax);
                if (chambers.Length <= 0) {
                    return false;
                }

                var currentChamber = doubleActionModule.Cocked ? chambers[0] : chambers[1];
                if (currentChamber.ContextState == CylinderAmmoModule.ChamberState.Live) {
                    //doubleActionModule.Fire(null);
                    currentChamber.ContextState = ChamberState.Discharged;
                    cylinderModule.RotateCylinder(1);
                } else {
                    doubleActionModule.FireDry();
                    cylinderModule.RotateCylinder(1);
                    cylinderModule.ServerResync();
                    return false;
                }
            } else if (firearm.Modules.TryGetFirst<MagazineModule>(out var magazineModule)) {

                if (firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule)) {
                    if (!magazineModule.MagazineInserted || magazineModule.AmmoStored <= 0) {
                        automaticActionModule.PlayDryFire();
                        return false;
                    } else automaticActionModule.PlayFire(1);
                } else if (firearm.TryGetModule<PumpActionModule>(out var pumpActionModule)) {
                    if (pumpActionModule.MagazineAmmo <= 0) {
                        return false;
                    }// else doubleActionModule1.Fire(null);

                } else if (firearm.TryGetModule<DisruptorAudioModule>(out var disruptorAudioModule)) {
                    disruptorAudioModule.PlayDisruptorShot(true, magazineModule.AmmoStored <= 1);
                } else return false;
                magazineModule.AmmoStored = magazineModule.AmmoStored - 1;
                //FirearmStatus status = firearm.Status;
                //firearm.Status = new FirearmStatus((byte)(status.Ammo - 1), status.Flags, status.Attachments);

                //PlayerStats.OnAnyPlayerDied?.Invoke(_hub, handler);
                //_hub.playerStats.OnThisPlayerDied?.Invoke(handler);
            } else return false;

            return true;
        }
    }
}
