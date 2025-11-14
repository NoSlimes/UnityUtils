using UnityEngine;
using TMPro;

public static class TextMeshUtils
{
    private const int sortingOrderDefault = 5000;

    public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null, TextAlignmentOptions textAlignment = TextAlignmentOptions.TopLeft, int sortingOrder = sortingOrderDefault, TMP_FontAsset font = null)
    {
        if (color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAlignment, sortingOrder, font);
    }

    public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAlignmentOptions textAlignment, int sortingOrder, TMP_FontAsset font = null)
    {
        GameObject gameObject = new GameObject("World_Text_TMP", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
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
}
