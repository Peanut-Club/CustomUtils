using Compendium.Features;
using Compendium.Input;
using CustomUtils.keybinds;

namespace CustomUtils
{
    public class CustomUtilsFeature : ConfigFeatureBase {
        public override string Name => "CustomUtils";

        public override bool IsPatch => true;

        public override void Load() {
            base.Load();
            if (!InputManager.TryGetHandler<SuicideKeybind>(out var _)) {
                InputManager.Register<SuicideKeybind>();
            }
        }

        public override void Unload() {
            base.Unload();
            if (InputManager.TryGetHandler<SuicideKeybind>(out var _)) {
                InputManager.Unregister<SuicideKeybind>();
            }
        }
    }
}
