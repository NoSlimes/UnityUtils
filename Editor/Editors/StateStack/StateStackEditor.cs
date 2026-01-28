using NoSlimes.UnityUtils.Runtime.ActionStacks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(StateStack), true)]
public class StateStackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        StateStack stateStack = (StateStack)target;

        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField($"{nameof(StateStack)} States", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (Application.isPlaying)
        {
            for (int i = 0; i < stateStack.States.Count; i++)
            {
                StateStack.IState state = stateStack.States[i];

                if (i == 0) GUI.backgroundColor = Color.green;
                else GUI.backgroundColor = Color.white;

                EditorGUILayout.BeginVertical("box");

                string activeTag = (i == 0) ? "[ACTIVE] " : "[PAUSED] ";
                EditorGUILayout.LabelField($"{activeTag}State {i}: {state.GetType().Name}", EditorStyles.boldLabel);
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
                    EditorGUILayout.LabelField("State Type", state.GetType().FullName);
                }
                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"{nameof(StateStack)} information is only available during play mode.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }
}
