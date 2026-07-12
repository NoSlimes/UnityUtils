#if USE_NGO
using NoSlimes.UnityUtils.Common.Attributes;
using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NoSlimes.UnityUtils.Runtime.ActionStacks.Netcode.NGO
{
    public abstract class NetworkAction : NetworkBehaviour, IAction
    {
        [field: SerializeField, ReadOnlyField]
        public byte ActionId { get; private set; }

        public virtual bool IsDone { get; set; }
        public virtual bool UpdateOnClient => false;

        public virtual void OnInitialize() { }
        public virtual void OnBegin() { }

        public virtual void OnInterrupt() { }
        public virtual void OnResume() { }


        public virtual void OnLateUpdate() { }
        public virtual void OnUpdate() { }

        public virtual void OnFinish()
        {
            IsDone = false;
        }

#if UNITY_EDITOR
        internal void SetId_EDITOR(byte id)
        {
            ActionId = id;
        }
#endif
    }

    public class NetworkActionStack : NetworkBehaviour, IActionStack<NetworkAction>
    {
        [SerializeField, ReadOnlyField] private NetworkAction[] actions = Array.Empty<NetworkAction>();

        private readonly Dictionary<NetworkAction, byte> idLookup = new();
        private readonly Dictionary<byte, NetworkAction> actionLookup = new();
        private readonly Dictionary<Type, NetworkAction> typeLookup = new();

        private readonly ActionStack<NetworkAction> actionStack = new();
        private readonly NetworkList<byte> netActionIds = new();

        public IReadOnlyCollection<NetworkAction> PredefinedActions => actions;
        public IReadOnlyList<IAction> Actions => actionStack.Actions;
        public IAction CurrentAction => actionStack.CurrentAction;
        public NetworkAction CurrentActionTyped => actionStack.CurrentActionTyped;

        private void Awake()
        {
            for (int i = 0; i < actions.Length; i++)
            {
                NetworkAction action = actions[i];
                if (action == null) continue;

                if (action.ActionId == 0)
                {
                    Debug.LogError($"Action {action} on {gameObject} has an ActionId of 0, which is reserved for uninitialized actions. Something is wrong in the setup.");
                    continue;
                }

                idLookup[action] = action.ActionId;
                actionLookup[action.ActionId] = action;

                if (!typeLookup.TryAdd(action.GetType(), action))
                {
                    Debug.LogWarning($"Multiple actions of type {action.GetType()} found in predefined actions array. Type-based lookup will only work for the first one.");
                }
            }

            actionStack.OnActionPushed += _ => SyncToNetList();
            actionStack.OnActionPopped += _ => SyncToNetList();
        }

        private void Update()
        {
            if (IsServer)
            {
                actionStack.Update();
            }
            else if (IsClient)
            {
                if (CurrentAction == null)
                    actionStack.ResolveCurrentAction();

                if (actionStack.CurrentAction != null && actionStack.CurrentActionTyped.UpdateOnClient)
                {
                    actionStack.Update();
                }
            }
        }

        private void LateUpdate()
        {
            if (IsServer)
            {
                actionStack.LateUpdate();
            }
            else if (IsClient)
            {
                if (actionStack.CurrentAction != null && (actionStack.CurrentAction as NetworkAction).UpdateOnClient)
                {
                    actionStack.LateUpdate();
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsClient)
            {
                netActionIds.OnListChanged += OnSyncedActionIdsChanged;
                RebuildStackFromNetList();
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
            {
                netActionIds.OnListChanged -= OnSyncedActionIdsChanged;
            }

            if (!IsServer)
                return;

            actionStack.Clear();
        }

        public void PushAction<T>() where T : NetworkAction
        {
            if (typeLookup.TryGetValue(typeof(T), out NetworkAction foundAction))
            {
                PushAction(foundAction);
            }
            else
            {
                Debug.LogError($"Attempted to push an action of type {typeof(T)} which is not in the predefined actions array.");
            }
        }

        public void PushAction(NetworkAction action, bool reinitializeAction = true)
        {
            if (!IsServer || !CheckReady())
                return;

            if (!actions.Contains(action))
            {
                Debug.LogError("Attempted to push an action that is not in the predefined actions array.");
                return;
            }

            actionStack.PushAction(action, reinitializeAction);
        }

        public void Pop(NetworkAction action)
        {
            if (!IsServer || !CheckReady())
                return;

            if (!actions.Contains(action))
            {
                Debug.LogError("Attempted to pop an action that is not in the predefined actions array.");
                return;
            }

            actionStack.Pop(action);
        }

        public void Pop()
        {
            if (!IsServer || !CheckReady())
                return;

            if (Actions.Count == 0)
                return;

            Pop(Actions[0] as NetworkAction);
        }

        public void Clear()
        {
            if (!IsServer || !CheckReady())
                return;

            actionStack.Clear();
        }

        public T GetAction<T>() where T : NetworkAction
        {
            if (!CheckReady())
                return null;

            if (typeLookup.TryGetValue(typeof(T), out NetworkAction foundAction))
            {
                return foundAction as T;
            }
            else
            {
                Debug.LogError($"Attempted to get an action of type {typeof(T)} which is not in the predefined actions array.");
                return null;
            }
        }

        private bool CheckReady()
        {
            if (!IsSpawned)
            {
                Debug.LogException(new InvalidOperationException("NetworkActionStack is not spawned."));
                return false;
            }

            return true;
        }

        private void SyncToNetList()
        {
            if (!IsServer)
                return;

            netActionIds.Clear();
            foreach (var action in actionStack.Actions)
            {
                if (idLookup.TryGetValue(action as NetworkAction, out byte id))
                {
                    netActionIds.Add(id);
                }
            }
        }

        private void OnSyncedActionIdsChanged(NetworkListEvent<byte> changeEvent)
        {
            if (IsServer)
                return;

            RebuildStackFromNetList();
        }

        private void RebuildStackFromNetList()
        {
            if (IsServer) return;

            actionStack.Clear();
            for (int i = netActionIds.Count - 1; i >= 0; i--)
            {
                byte actionId = netActionIds[i];
                if (actionLookup.TryGetValue(actionId, out NetworkAction action))
                {
                    actionStack.PushAction(action, false);
                }
                else
                {
                    Debug.LogError($"Received unknown Action ID {actionId} from server!");
                }
            }
        }

#if UNITY_EDITOR
        internal void RefreshActions()
        {
            actions = GetComponentsInChildren<NetworkAction>();

            var usedIds = new HashSet<byte>();
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                if (action == null)
                    continue;

                if (action.ActionId != 0)
                    usedIds.Add(action.ActionId);
            }

            byte nextId = 1;
            foreach (var action in actions)
            {
                if (action == null)
                    continue;
                if (action.ActionId == 0)
                {
                    while (usedIds.Contains(nextId))
                    {
                        nextId++;
                        if (nextId == 0)
                        {
                            Debug.LogError("Ran out of available Action IDs. Maximum of 255 actions allowed.");
                            return;
                        }
                    }
                    action.SetId_EDITOR(nextId);
                    usedIds.Add(nextId);
                }
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}
#endif