using System;
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

        /// <summary>
        /// Calculates the world-space bounds of a GameObject, including all child renderers and optionally colliders.
        /// </summary>
        /// <param name="transform">The Transform of the object to calculate bounds for.</param>
        /// <param name="includeColliders">Whether to include Colliders in the bounds calculation.</param>
        /// <returns>A Bounds struct representing the combined bounds of all renderers (and colliders if requested).</returns>
        public static Bounds GetObjectBounds(this Transform transform, bool includeColliders = false)
        {
            var renderers = transform.GetComponentsInChildren<Renderer>();
            var colliders = includeColliders ? transform.GetComponentsInChildren<Collider>() : Array.Empty<Collider>();

            if (renderers.Length == 0 && renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.one);

            Bounds combined = new Bounds(transform.position, Vector3.zero);
            bool hasBounds = false;

            foreach (var renderer in renderers)
            {
                if (!hasBounds)
                {
                    combined = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(renderer.bounds);
                }
            }

            if (includeColliders && colliders != null)
            {
                foreach (var collider in colliders)
                {
                    if (!hasBounds)
                    {
                        combined = collider.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combined.Encapsulate(collider.bounds);
                    }
                }
            }

            return combined;
        }

        /// <summary>
        /// Returns the world-space size of the object, including all child renderers and optionally colliders.
        /// </summary>
        /// <param name="transform">The Transform of the object.</param>
        /// <param name="includeColliders">Whether to include colliders in the size calculation. Defaults to false.</param>
        /// <returns>The size of the object as a <see cref="Vector3"/>.</returns>
        public static Vector3 GetObjectSize(this Transform transform, bool includeColliders = false)
        {
            return GetObjectBounds(transform, includeColliders).size;
        }

        /// <summary>
        /// Returns the world-space center of the object, including all child renderers and optionally colliders.
        /// </summary>
        /// <param name="transform">The Transform of the object.</param>
        /// <param name="includeColliders">Whether to include colliders in the center calculation. Defaults to false.</param>
        /// <returns>The center of the object as a <see cref="Vector3"/>.</returns>
        public static Vector3 GetObjectCenter(this Transform transform, bool includeColliders = false)
        {
            return GetObjectBounds(transform, includeColliders).center;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Bounds GetLocalBounds(this Transform transform, bool includeColliders = false)
        {
            var renderers = transform.GetComponentsInChildren<Renderer>();
            var colliders = includeColliders ? transform.GetComponentsInChildren<Collider>() : Array.Empty<Collider>();

            if (renderers.Length == 0 && colliders.Length == 0)
                return new Bounds(Vector3.zero, Vector3.one);

            Bounds combined = new Bounds();
            bool hasBounds = false;

            // Include renderers
            foreach (var r in renderers)
            {
                // Convert renderer's localBounds to this transform's local space
                Bounds b = new Bounds(
                    transform.InverseTransformPoint(r.transform.TransformPoint(r.localBounds.center)),
                    r.localBounds.size
                );

                if (!hasBounds)
                {
                    combined = b;
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(b);
                }
            }

            // Include colliders if requested
            if (includeColliders)
            {
                foreach (var c in colliders)
                {
                    Bounds b = new Bounds(
                        transform.InverseTransformPoint(c.bounds.center),
                        transform.InverseTransformVector(c.bounds.size)
                    );

                    if (!hasBounds)
                    {
                        combined = b;
                        hasBounds = true;
                    }
                    else
                    {
                        combined.Encapsulate(b);
                    }
                }
            }

            return combined;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Vector3 GetLocalSize(this Transform transform)
        {
            return GetLocalBounds(transform).size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Vector3 GetLocalCenter(this Transform transform)
        {
            return GetLocalBounds(transform).center;
        }
    }
}