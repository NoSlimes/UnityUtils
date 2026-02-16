using NoSlimes.UnityUtils.Runtime.ActionStacks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActionStack), true)]
public class ActionStackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ActionStack stateStack = (ActionStack)target;

        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField($"{nameof(ActionStack)} Actions", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (Application.isPlaying)
        {
            for (int i = 0; i < stateStack.Actions.Count; i++)
            {
                ActionStack.IAction state = stateStack.Actions[i];

                if (i == 0) GUI.backgroundColor = Color.green;
                else GUI.backgroundColor = Color.white;

                EditorGUILayout.BeginVertical("box");

                string activeTag = (i == 0) ? "[ACTIVE] " : "[PAUSED] ";
                EditorGUILayout.LabelField($"{activeTag}Action {i}: {state.GetType().Name}", EditorStyles.boldLabel);
                if (state is MonoBehaviour mb)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField("GameObject", mb.gameObject, typeof(GameObject), true);
                    EditorGUI.EndDisabledGroup();
                }
                else if (state is ScriptableObject so)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField("ScriptableObject", so, typeof(ScriptableObject), false);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.LabelField("Action Type", state.GetType().FullName);
                }
                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"{nameof(ActionStack)} information is only available during play mode.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }
}
