using UnityEditor;
using UnityEngine;
using System.IO;

public class PrefabVariantConverter : EditorWindow
{
    private GameObject targetPrefab;
    private GameObject basePrefab;

    [MenuItem("Assets/Convert To Prefab Variant Window...", true)]
    private static bool ValidateConvertToVariantWindow()
    {
        if (!(Selection.activeObject is GameObject prefab))
            return false;

        var type = PrefabUtility.GetPrefabAssetType(prefab);
        // Allow both Regular and Variant prefabs
        return type == PrefabAssetType.Regular || type == PrefabAssetType.Variant;
    }

    [MenuItem("Assets/Convert To Prefab Variant Window...")]
    private static void OpenWindow()
    {
        var window = GetWindow<PrefabVariantConverter>("Prefab Variant Converter");
        window.targetPrefab = Selection.activeObject as GameObject;
        window.FindBasePrefab(); // try auto-detect
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Target Prefab", EditorStyles.boldLabel);
        targetPrefab = (GameObject)EditorGUILayout.ObjectField(targetPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Base Prefab", EditorStyles.boldLabel);
        basePrefab = (GameObject)EditorGUILayout.ObjectField(basePrefab, typeof(GameObject), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert to Variant"))
        {
            if (targetPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "No target prefab selected.", "OK");
                return;
            }

            if (basePrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "No base prefab selected.", "OK");
                return;
            }

            ConvertToVariant(targetPrefab, basePrefab);
        }
    }

    private void FindBasePrefab()
    {
        if (targetPrefab == null) return;

        string targetPath = AssetDatabase.GetAssetPath(targetPrefab);
        string folder = Path.GetDirectoryName(targetPath);
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName.EndsWith("Base"))
            {
                basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                break;
            }
        }
    }

    private static void ConvertToVariant(GameObject target, GameObject basePrefab)
    {
        string assetPath = AssetDatabase.GetAssetPath(target);

        // Step 1: make a working instance from base prefab
        var workingInstance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);

        // Step 2: instantiate the old prefab (could be regular OR variant)
        var oldInstance = (GameObject)PrefabUtility.InstantiatePrefab(target);

        // Step 3: merge extra components and children
        MergeGameObjects(oldInstance, workingInstance);

        // Step 4: save the merged instance as prefab variant (overwrites original)
        PrefabUtility.SaveAsPrefabAssetAndConnect(workingInstance, assetPath, InteractionMode.UserAction);

        // Cleanup
        GameObject.DestroyImmediate(oldInstance);
        GameObject.DestroyImmediate(workingInstance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"{Path.GetFileName(assetPath)} is now a variant of {basePrefab.name}.", "OK");
    }

    private static void MergeGameObjects(GameObject source, GameObject target)
    {
        CopyComponents(source, target);

        // Recurse children
        foreach (Transform child in source.transform)
        {
            Transform targetChild = target.transform.Find(child.name);
            if (targetChild == null)
            {
                // Entire child missing -> copy it
                GameObject newChild = Object.Instantiate(child.gameObject, target.transform);
                newChild.name = child.name;
            }
            else
            {
                MergeGameObjects(child.gameObject, targetChild.gameObject);
            }
        }
    }

    private static void CopyComponents(GameObject source, GameObject target)
    {
        var sourceComponents = source.GetComponents<Component>();
        var targetComponents = target.GetComponents<Component>();

        foreach (var srcComp in sourceComponents)
        {
            if (srcComp == null || srcComp is Transform) continue;

            // Check if target already has same type
            var existing = System.Array.Find(targetComponents, c => c != null && c.GetType() == srcComp.GetType());
            if (existing == null)
            {
                // Component missing -> copy it
                UnityEditorInternal.ComponentUtility.CopyComponent(srcComp);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
            }
            else
            {
                // Exists -> overwrite values
                UnityEditorInternal.ComponentUtility.CopyComponent(srcComp);
                UnityEditorInternal.ComponentUtility.PasteComponentValues(existing);
            }
        }
    }
}
