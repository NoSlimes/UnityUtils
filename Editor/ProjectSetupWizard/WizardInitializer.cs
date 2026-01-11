using UnityEngine;
using UnityEditor;

namespace NoSlimes.UnityUtils.Editor.EditorWindows.ProjectSetupWizard
{
    [InitializeOnLoad]
    public class WizardInitializer
    {
        static WizardInitializer()
        {
            EditorApplication.delayCall += () =>
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game"))
                {
                    ProjectSetupEditorWizard.ShowWindow();
                }
            };
        }
    }
}