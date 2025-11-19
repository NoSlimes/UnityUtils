using UnityEngine;
using TMPro;

public static class TextMeshUtils
{
    private const int sortingOrderDefault = 5000;

    /// <summary>
    /// Creates a TextMeshPro object in world space.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="parent">Optional parent transform.</param>
    /// <param name="localPosition">Local position relative to parent.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="color">Text color. Defaults to white if null.</param>
    /// <param name="textAlignment">Text alignment. Defaults to TopLeft.</param>
    /// <param name="sortingOrder">Sorting order of the mesh renderer.</param>
    /// <param name="font">Optional TMP_FontAsset to use.</param>
    /// <param name="name">Optional name for the GameObject. Defaults to "World_Text_TMP".</param>
    /// <returns>The created TextMeshPro component.</returns>
    public static TextMeshPro CreateWorldText(
        string text,
        Transform parent = null,
        Vector3 localPosition = default,
        int fontSize = 40,
        Color? color = null,
        TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft,
        int sortingOrder = sortingOrderDefault,
        TMP_FontAsset font = null,
        string name = "World_Text_TMP")
    {
        if (color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAlignment, sortingOrder, font, name);
    }

    /// <summary>
    /// Internal method that handles creation of the TextMeshPro object.
    /// </summary>
    private static TextMeshPro CreateWorldText(
        Transform parent,
        string text,
        Vector3 localPosition,
        int fontSize,
        Color color,
        TextAlignmentOptions textAlignment,
        int sortingOrder,
        TMP_FontAsset font = null,
        string name = "World_Text_TMP")
    {
        GameObject gameObject = new GameObject(name, typeof(TextMeshPro));
        Transform transform = gameObject.transform;

        if (parent != null)
            transform.SetParent(parent, false);

        transform.localPosition = localPosition;

        TextMeshPro tmp = gameObject.GetComponent<TextMeshPro>();

        if (font != null)
            tmp.font = font;

        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = textAlignment;

        // Ensure it renders in world space
        tmp.enableAutoSizing = false;
        tmp.isOverlay = false;

        // Set sorting order
        MeshRenderer meshRenderer = tmp.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = sortingOrder;

        return tmp;
    }

    public static void LookAtCamera(this TextMeshPro tmp, Camera camera = null)
    {
        if(camera == null)
            camera = Camera.main;

        tmp.transform.rotation = Quaternion.LookRotation(tmp.transform.position - camera.transform.position);
    }
}
