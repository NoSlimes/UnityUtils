using UnityEditor;
using UnityEngine;
using NoSlimes.UnityUtils.Runtime.ActionStacks;
using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;

[CustomEditor(typeof(ActionStackBase<>), true)]
public class ActionStackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        IActionStack stateStack = (IActionStack)target;

        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField($"{stateStack.GetType().Name} Actions", EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        if (Application.isPlaying)
        {
            var actions = stateStack.Actions;
            for (int i = 0; i < actions.Count; i++)
            {
                IAction action = actions[i];
                GUI.backgroundColor = (i == 0) ? Color.green : Color.white;

                EditorGUILayout.BeginVertical("box");
                string activeTag = (i == 0) ? "[ACTIVE] " : "[PAUSED] ";
                EditorGUILayout.LabelField($"{activeTag}Action {i}: {action.GetType().Name}", EditorStyles.boldLabel);

                switch (action)
                {
                    case MonoBehaviour mb:
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField("GameObject", mb.gameObject, typeof(GameObject), true);
                        EditorGUI.EndDisabledGroup();
                        break;
                    case ScriptableObject so:
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField("ScriptableObject", so, typeof(ScriptableObject), false);
                        EditorGUI.EndDisabledGroup();
                        break;
                    default:
                        EditorGUILayout.LabelField("Action Type", action.GetType().FullName);
                        break;
                }

                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"{stateStack.GetType().Name} action information is only available during play mode.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
    }
}