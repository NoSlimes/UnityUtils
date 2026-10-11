using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.GameSystems
{
    /// <summary>
    /// Marker base for ScriptableObject configs. Assign instances on the system
    /// manager, pull them in Initialize with GetConfig.
    /// </summary>
    public abstract class GameConfig : ScriptableObject
    {
    }
}
