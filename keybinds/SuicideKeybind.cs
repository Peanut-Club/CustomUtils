using Compendium.Input;
using CustomUtils.commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomUtils.keybinds {
    public class SuicideKeybind : IInputHandler {
        public KeyCode Key => KeyCode.F4;

        public bool IsChangeable => true;

        public string Id => "suicide";
        public string Label => "Other - Suicide";

        public void OnPressed(ReferenceHub player) {
            player.gameConsoleTransmission.SendToClient(SuicideCommand.SuicideCmd(player), "yellow");
        }
    }
}
