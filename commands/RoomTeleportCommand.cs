using BetterCommands;
using BetterCommands.Permissions;
using Compendium;
using MapGeneration;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace CustomUtils.commands {
    public static class RoomTeleportCommand {

        [Command("roomtpo", CommandType.RemoteAdmin)]
        [Description("Room Teleport with Offset")]
        [Permission(PermissionLevel.Lowest)]
        public static string RoomTpOffsetCmd(ReferenceHub sender, ReferenceHub target, RoomName roomName, Vector3 offset = default) {
            if(!RoomIdentifier.AllRoomIdentifiers.TryGetFirst(room => room.Name == roomName, out var room)) {
                return "Room not found";
            }

            string message = "";
            Vector3 newPos = room.transform.position;
            if (offset != Vector3.zero) {
                newPos += room.transform.rotation * offset;
                message += "Offset: " + offset;
            }

            message += "\nRoom Position: " + room.transform.position;
            message += "\n New Position: " + newPos;

            target.Position(newPos);

            return message;
        }

    }
}
