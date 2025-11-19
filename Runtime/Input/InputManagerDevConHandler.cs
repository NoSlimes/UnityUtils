#if DEVCON
using NoSlimes.Util.DevCon;
using UnityEngine;

namespace NoSlimes.Utils.Input.Compatibility
{
    public class InputManagerDevConHandler : MonoBehaviour
    {
        [SerializeField] private string uiActionMapName = "UI";

        private InputManager inputManager;

        private void OnEnable()
        {
            inputManager = InputManager.Instance;

            DeveloperConsoleUI.OnConsoleToggled += HandleConsoleToggled;
        }

        private void OnDisable()
        {
            DeveloperConsoleUI.OnConsoleToggled -= HandleConsoleToggled;
        }

        private void HandleConsoleToggled(bool isActive)
        {
            if (inputManager == null || inputManager.PreviousActionMap == null) return;
            inputManager.SwitchActionMap(isActive ? uiActionMapName : inputManager.PreviousActionMap.name);
        }
    }
}
#endif