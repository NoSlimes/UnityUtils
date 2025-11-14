using UnityEngine;
using Object = UnityEngine.Object;

namespace NoSlimes.Utils
{
    /// <summary>
    /// Utility extension methods for <see cref="Transform"/> operations.
    /// </summary>
    public static class TransformUtils
    {
        /// <summary>
        /// Clones the specified transform's GameObject.
        /// </summary>
        /// <param name="transform">The transform to clone.</param>
        /// <param name="newParent">Optional parent for the cloned transform. If null, the clone will have no parent.</param>
        /// <param name="newPosition">Optional world position for the clone. Defaults to the original transform's position.</param>
        /// <param name="name">Optional name for the cloned GameObject. Defaults to "OriginalName (Clone)".</param>
        /// <returns>The cloned <see cref="Transform"/>.</returns>
        public static Transform Clone(this Transform transform, Transform newParent = null, Vector3 newPosition = default, string name = null)
        {
            GameObject clone = Object.Instantiate(
                transform.gameObject, newPosition, transform.rotation, newParent);

            clone.name = !string.IsNullOrEmpty(name) ? name : $"{transform.name} (Clone)";
            return clone.transform;
        }

        /// <summary>
        /// Clones the specified transform's GameObject at a new position.
        /// </summary>
        /// <param name="transform">The transform to clone.</param>
        /// <param name="newPosition">World position for the clone.</param>
        /// <param name="name">Optional name for the cloned GameObject. Defaults to "OriginalName (Clone)".</param>
        /// <returns>The cloned <see cref="Transform"/>.</returns>
        public static Transform Clone(this Transform transform, Vector3 newPosition, string name = null) =>
            Clone(transform, null, newPosition, name);

        /// <summary>
        /// Clones the specified transform's GameObject with an optional name.
        /// </summary>
        /// <param name="transform">The transform to clone.</param>
        /// <param name="name">Optional name for the cloned GameObject. Defaults to "OriginalName (Clone)".</param>
        /// <returns>The cloned <see cref="Transform"/>.</returns>
        public static Transform Clone(this Transform transform, string name = null) =>
            Clone(transform, null, default, name);

        /// <summary>
        /// Clones the specified transform's GameObject.
        /// </summary>
        /// <param name="transform">The transform to clone.</param>
        /// <returns>The cloned <see cref="Transform"/>.</returns>
        public static Transform Clone(this Transform transform) =>
            Clone(transform, null, default, null);

        /// <summary>
        /// Destroys all child GameObjects of the specified transform.
        /// </summary>
        /// <param name="transform">The transform whose children will be destroyed.</param>
        /// <param name="immediate">
        /// If true, uses <see cref="GameObject.DestroyImmediate"/> instead of <see cref="GameObject.Destroy"/>.
        /// Useful for editor scripts.
        /// </param>
        public static void DestroyAllChildren(this Transform transform, bool immediate = false)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (immediate)
                    Object.DestroyImmediate(child);
                else
                    Object.Destroy(child);
            }
        }

    }
}