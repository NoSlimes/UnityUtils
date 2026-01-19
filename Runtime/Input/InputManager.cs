using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NoSlimes.UnityUtils.Input
{
    public struct ActionNames
    {
        public const string Move = "Move";
        public const string Look = "Look";
        public const string Jump = "Jump";
        public const string Crouch = "Crouch";
        public const string Sprint = "Sprint";
        public const string Attack = "Attack";
        public const string Interact = "Interact";
    }

    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        [Flags]
        public enum InputEventType
        {
            Started = 1 << 0,
            Performed = 1 << 1,
            Canceled = 1 << 2
        }

        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<InputManager>();

                if (_instance == null)
                {
                    return null;
                }

                return _instance;
            }
        }

        public InputAction this[string name] => GetAction(name);

        private PlayerInput playerInput;
        private string previousActionMapName;

        private readonly Dictionary<string, InputAction> inputActions = new();
        private readonly Dictionary<string, Action<InputAction.CallbackContext>> performedCallbacks = new();
        private readonly Dictionary<string, Action<InputAction.CallbackContext>> startedCallbacks = new();
        private readonly Dictionary<string, Action<InputAction.CallbackContext>> canceledCallbacks = new();

        public PlayerInput PlayerInput => playerInput;
        public bool IsGamepad => PlayerInput.currentControlScheme == "Gamepad";
        public InputActionMap PreviousActionMap
        {
            get
            {
                var map = string.IsNullOrEmpty(previousActionMapName) ? null : playerInput.actions.FindActionMap(previousActionMapName);
                return map;
            }
        }

        // Optional centralized events
        public event Action<InputAction.CallbackContext> OnMove;
        public event Action<InputAction.CallbackContext> OnJump;
        public event Action<InputAction.CallbackContext> OnCrouch;
        public event Action<InputAction.CallbackContext> OnSprint;
        public event Action<InputAction.CallbackContext> OnLook;
        public event Action<InputAction.CallbackContext> OnAttack;
        public event Action<InputAction.CallbackContext> OnInteract;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component is missing on the GameObject.", this);
                return;
            }

            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            playerInput.SwitchCurrentActionMap(playerInput.defaultActionMap);

            CacheActions();
        }

        private void OnEnable()
        {
            playerInput.onActionTriggered += HandleActionTriggered;
            playerInput.onControlsChanged += OnInputControlsChanged;

        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.onActionTriggered -= HandleActionTriggered;
                playerInput.onControlsChanged -= OnInputControlsChanged;
            }

        }

        private void OnInputControlsChanged(PlayerInput obj)
        {
            CacheActions();
        }

        private void CacheActions()
        {
            inputActions.Clear();
            foreach (InputAction action in playerInput.actions)
            {
                inputActions[action.name] = action;
            }
        }

        private void HandleActionTriggered(InputAction.CallbackContext context)
        {
            string name = context.action.name;

            // Centralized events (optional)
            switch (name)
            {
                case ActionNames.Move: OnMove?.Invoke(context); break;
                case ActionNames.Jump: OnJump?.Invoke(context); break;
                case ActionNames.Crouch: OnCrouch?.Invoke(context); break;
                case ActionNames.Sprint: OnSprint?.Invoke(context); break;
                case ActionNames.Look: OnLook?.Invoke(context); break;
                case ActionNames.Attack: OnAttack?.Invoke(context); break;
                case ActionNames.Interact: OnInteract?.Invoke(context); break;
            }

            // Phase-specific callbacks
            if (context.started && startedCallbacks.TryGetValue(name, out Action<InputAction.CallbackContext> started))
                started?.Invoke(context);

            if (context.performed && performedCallbacks.TryGetValue(name, out Action<InputAction.CallbackContext> performed))
                performed?.Invoke(context);

            if (context.canceled && canceledCallbacks.TryGetValue(name, out Action<InputAction.CallbackContext> canceled))
                canceled?.Invoke(context);
        }

        public InputAction GetAction(string name)
        {
            if (inputActions.TryGetValue(name, out InputAction action))
                return action;

            Debug.LogWarning($"Input action '{name}' not found in PlayerInput actions.", this);
            return null;
        }

        public bool TryGetAction(string name, out InputAction action)
        {
            return inputActions.TryGetValue(name, out action);
        }

        public void RegisterActionCallback(string actionName, Action<InputAction.CallbackContext> callback, InputEventType phase)
        {
            if(phase.HasFlag(InputEventType.Started))
                RegisterActionCallback(actionName, callback, InputActionPhase.Started);
            if (phase.HasFlag(InputEventType.Performed))
                RegisterActionCallback(actionName, callback, InputActionPhase.Performed);
            if (phase.HasFlag(InputEventType.Canceled))
                RegisterActionCallback(actionName, callback, InputActionPhase.Canceled);
        }

        public void RegisterActionCallback(string actionName, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
        {
            if (string.IsNullOrEmpty(actionName) || callback == null)
            {
                Debug.LogWarning("Invalid action name or callback provided.", this);
                return;
            }

            if (!inputActions.ContainsKey(actionName))
            {
                Debug.LogWarning($"Input action '{actionName}' not found. Cannot register callback.", this);
                return;
            }

            switch (phase)
            {
                case InputActionPhase.Started:
                    startedCallbacks.TryAdd(actionName, null);
                    startedCallbacks[actionName] += callback;
                    break;

                case InputActionPhase.Performed:
                    performedCallbacks.TryAdd(actionName, null);
                    performedCallbacks[actionName] += callback;
                    break;

                case InputActionPhase.Canceled:
                    canceledCallbacks.TryAdd(actionName, null);
                    canceledCallbacks[actionName] += callback;
                    break;

                default:
                    Debug.LogWarning("Unsupported input phase for registration.", this);
                    break;
            }
        }

        public void UnregisterActionCallback(string actionName, Action<InputAction.CallbackContext> callback, InputEventType phase)
        {
            if(phase.HasFlag(InputEventType.Started))
                UnregisterActionCallback(actionName, callback, InputActionPhase.Started);
            if (phase.HasFlag(InputEventType.Performed))
                UnregisterActionCallback(actionName, callback, InputActionPhase.Performed);
            if (phase.HasFlag(InputEventType.Canceled))
                UnregisterActionCallback(actionName, callback, InputActionPhase.Canceled);
        }

        public void UnregisterActionCallback(string actionName, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
        {
            if (string.IsNullOrEmpty(actionName) || callback == null)
            {
                Debug.LogWarning("Invalid action name or callback provided.", this);
                return;
            }

            switch (phase)
            {
                case InputActionPhase.Started:
                    if (startedCallbacks.TryGetValue(actionName, out Action<InputAction.CallbackContext> started))
                    {
                        started -= callback;
                        if (started == null) startedCallbacks.Remove(actionName);
                        else startedCallbacks[actionName] = started;
                    }
                    break;

                case InputActionPhase.Performed:
                    if (performedCallbacks.TryGetValue(actionName, out Action<InputAction.CallbackContext> performed))
                    {
                        performed -= callback;
                        if (performed == null) performedCallbacks.Remove(actionName);
                        else performedCallbacks[actionName] = performed;
                    }
                    break;

                case InputActionPhase.Canceled:
                    if (canceledCallbacks.TryGetValue(actionName, out Action<InputAction.CallbackContext> canceled))
                    {
                        canceled -= callback;
                        if (canceled == null) canceledCallbacks.Remove(actionName);
                        else canceledCallbacks[actionName] = canceled;
                    }
                    break;
            }
        }

        public void RegisterActionCallback(string[] actionNames, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
        {
            if (actionNames == null || callback == null)
            {
                Debug.LogWarning("Invalid action names or callback provided.", this);
                return;
            }

            foreach (string actionName in actionNames)
            {
                RegisterActionCallback(actionName, callback, phase);
            }
        }

        public void UnregisterActionCallback(string[] actionNames, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
        {
            if (actionNames == null || callback == null)
            {
                Debug.LogWarning("Invalid action names or callback provided.", this);
                return;
            }

            foreach (string actionName in actionNames)
            {
                UnregisterActionCallback(actionName, callback, phase);
            }
        }

        public void SwitchActionMap(string mapName)
        {
            if (playerInput.currentActionMap?.name != mapName)
            {
                previousActionMapName = playerInput.currentActionMap?.name;

                playerInput.SwitchCurrentActionMap(mapName);
                Debug.Log($"Switched input map to '{mapName}'", this);
                CacheActions();
            }
        }

        public InputActionMap GetActionMap(string mapName)
        {
            var actionMap = playerInput.actions.FindActionMap(mapName);
            if (actionMap == null)
            {
                Debug.LogWarning($"Input action map '{mapName}' not found in PlayerInput.", this);
                return null;
            }

            return playerInput.actions.FindActionMap(mapName);
        }

        public InputActionMap EnableActionMap(string mapName, bool enable)
        {
            var actionMap = GetActionMap(mapName);
            if (actionMap != null)
            {
                if (enable)
                    actionMap.Enable();
                else
                    actionMap.Disable();
            }

            return actionMap;
        }
    }
}
