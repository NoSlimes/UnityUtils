using UnityEngine;
using UnityEditor;

namespace NoSlimes.Utils.Editor.EditorWindows.ProjectSetupWizard
{
    [InitializeOnLoad]
    public class WizardInitializer
    {
        private const string FirstTimeKey = "ProjectSetupWizard_FirstTime_Shown";

        static WizardInitializer()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool(FirstTimeKey, false))
                {
                    ProjectSetupEditorWizard.ShowWindow();
                    EditorPrefs.SetBool(FirstTimeKey, true);
                }
            };
        }
    }
}