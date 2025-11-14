using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NoSlimes.Utils.Common.VisualElements;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoSlimes.Utils.Editor.ProjectSetupWizard
{
    public partial class ProjectSetupEditorWizard : EditorWindow
    {
        private const string SetNamespaceToggleKey = "ProjectSetupWizard_SetNamespace";
        private const string DirectoryStructureKey = "ProjectSetupWizard_DirectoryStructure";
        private const string ImportTMPEssentialsKey = "ProjectSetupWizard_ImportTMPEssentials";

        private const string TMPEssentialsMenuItemPath = "Window/TextMeshPro/Import TMP Essential Resources";

        private readonly Dictionary<string, string> packages = new()
        {
            { "DevCon", "https://github.com/NoSlimes/DevCon.git" },
            { "DLog", "https://github.com/NoSlimes/DLog.git"},

            { "Newtonsoft Json", "com.unity.nuget.newtonsoft-json" },
            { "Cinemachine", "com.unity.cinemachine"},
            { "ProBuilder", "com.unity.probuilder" },
        };

        private readonly Dictionary<string, bool> packageToggles = new();
        private readonly Dictionary<string, string> editorPrefsKeys = new();
        private Label statusLabel;

        private bool setNamespaceToggle = true;
        private TextField namespaceField;

        private bool directoryStructureToggle = true;
        private bool importTMPEssentialsToggle = true;

        [MenuItem("Tools/Project Setup Wizard")]
        public static void ShowWindow()
        {
            ProjectSetupEditorWizard window = GetWindow<ProjectSetupEditorWizard>("Project Setup Wizard");
            window.minSize = new Vector2(350, 400);
            window.maxSize = new Vector2(350, 600);
        }

        private void OnEnable()
        {
            // Load package toggles
            foreach (var kvp in packages)
            {
                string key = $"ProjectSetupWizard_Import_{kvp.Key}";
                editorPrefsKeys[kvp.Key] = key;
                packageToggles[kvp.Key] = EditorPrefs.GetBool(key, true);
            }

            setNamespaceToggle = EditorPrefs.GetBool(SetNamespaceToggleKey, true);
            directoryStructureToggle = EditorPrefs.GetBool(DirectoryStructureKey, true);
            importTMPEssentialsToggle = EditorPrefs.GetBool(ImportTMPEssentialsKey, true);

            editorPrefsKeys["RootNamespace"] = "ProjectSetupWizard_RootNamespace";
        }

        private void CreateGUI()
        {
            VisualElement root = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch,
                    justifyContent = Justify.FlexStart,
                    paddingTop = 15,
                    paddingBottom = 15,
                    paddingLeft = 15,
                    paddingRight = 15,
                }
            };


            Label title = new Label("Project Setup Wizard")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 28,
                    color = Color.white,
                    marginBottom = 10
                }
            };
            root.Add(title);

            // Project Options
            var optionsSection = CreateSection("Project Options");
            optionsSection.Add(CreateOptionToggle("Create Default Directory Structure", directoryStructureToggle, val =>
            {
                directoryStructureToggle = val;
                EditorPrefs.SetBool(DirectoryStructureKey, val);
            }));
            optionsSection.Add(CreateOptionToggle("Import TMP Essential Resources", importTMPEssentialsToggle, val =>
            {
                importTMPEssentialsToggle = val;
                EditorPrefs.SetBool(ImportTMPEssentialsKey, val);
            }));
            root.Add(optionsSection);

            // Packages
            var packagesSection = CreateSection("Packages");
            foreach (var kvp in packages)
                packagesSection.Add(CreatePackageToggle(kvp.Key));
            root.Add(packagesSection);

            // Namespace Section
            var namespaceSection = CreateSection("Project Root Namespace");

            namespaceSection.Add(CreateOptionToggle("Set Root Namespace", setNamespaceToggle, val =>
            {
                setNamespaceToggle = val;
                EditorPrefs.SetBool(SetNamespaceToggleKey, val);
                namespaceField.SetEnabled(setNamespaceToggle);
            }));

            string initialNamespace = EditorPrefs.GetString(editorPrefsKeys["RootNamespace"], "");
            namespaceField = new TextField("Namespace")
            {
                value = initialNamespace,
                style =
                {
                    fontSize = 14,
                    paddingLeft = 4,
                    paddingRight = 4,
                    height = 24,
                    marginBottom = 8
                },
                enabledSelf = setNamespaceToggle
            };
            namespaceField.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetString(editorPrefsKeys["RootNamespace"], evt.newValue);
            });
            namespaceSection.Add(namespaceField);
            root.Add(namespaceSection);

            // Run Setup Button
            Button setupButton = new(() => _ = RunSetup())
            {
                text = "Run Setup",
                style =
                {
                    marginTop = 10,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    backgroundColor = new Color(0.2f, 0.6f, 0.9f),
                    color = Color.white,
                    paddingLeft = 6,
                    paddingRight = 6,
                    paddingTop = 4,
                    paddingBottom = 4
                }
            };
            root.Add(setupButton);

            // Status Label
            statusLabel = new Label("")
            {
                style =
                {
                    marginTop = 10,
                    fontSize = 12,
                    unityFontStyleAndWeight = FontStyle.Italic,
                    color = Color.lightSlateGray,
                    alignSelf = Align.Stretch,
                    whiteSpace = WhiteSpace.Normal,  // wrap text
                    overflow = Overflow.Visible
                }
            };
            root.Add(statusLabel);

            rootVisualElement.Add(root);
        }

        private VisualElement CreateSection(string titleText)
        {
            VisualElement container = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch,
                    marginTop = 12
                }
            };

            Label titleLabel = new(titleText)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 6
                }
            };

            VisualElement separator = new()
            {
                style =
                {
                    height = 1,
                    backgroundColor = new Color(0.5f, 0.5f, 0.5f),
                    marginBottom = 6
                }
            };

            container.Add(titleLabel);
            container.Add(separator);
            return container;
        }

        private ToggleButton CreatePackageToggle(string packageName)
        {
            bool currentValue = packageToggles[packageName];
            ToggleButton toggle = new($"Import {packageName} Package", currentValue);
            toggle.OnValueChanged += val =>
            {
                packageToggles[packageName] = val;
                EditorPrefs.SetBool(editorPrefsKeys[packageName], val);
            };
            return toggle;
        }

        private ToggleButton CreateOptionToggle(string label, bool initialValue, System.Action<bool> onValueChanged)
        {
            ToggleButton toggle = new(label, initialValue);
            toggle.OnValueChanged += val => onValueChanged.Invoke(val);
            return toggle;
        }

        private async Task RunSetup()
        {
            if (directoryStructureToggle)
            {
                statusLabel.text = "Creating project directories...";
                await CreateDirectoriesAsync(rootNode, Application.dataPath);
            }

            foreach (var kvp in packages)
            {
                if (packageToggles.TryGetValue(kvp.Key, out bool shouldImport) && shouldImport)
                {
                    statusLabel.text = $"Importing {kvp.Key} package...";
                    await AddPackageAsync(kvp.Value);
                    AssetDatabase.Refresh();
                }
            }

            if (setNamespaceToggle)
            {
                string rootNamespace = namespaceField.value;
                EditorSettings.projectGenerationRootNamespace = rootNamespace;
                statusLabel.text = $"Namespace set to: {rootNamespace}";
            }

            if (importTMPEssentialsToggle)
            {
                statusLabel.text = "Importing TMP Essential Resources...";
                try
                {
                    EditorApplication.ExecuteMenuItem(TMPEssentialsMenuItemPath);

                    // Disable the toggle after import to prevent repeated imports
                    importTMPEssentialsToggle = false;
                    EditorPrefs.SetBool(ImportTMPEssentialsKey, false);
                    Repaint();
                }
                catch
                {
                    statusLabel.text = "Failed to import TMP Essential Resources. Please ensure TextMeshPro is installed.";
                }
            }

            statusLabel.text = "Setup complete!";
            AssetDatabase.Refresh();
        }

        private async Task AddPackageAsync(string packageURL)
        {
            AddRequest request = UnityEditor.PackageManager.Client.Add(packageURL);
            bool finished = false;

            EditorApplication.CallbackFunction callback = null;
            callback = () =>
            {
                if (request.IsCompleted)
                {
                    if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                    {
                        string msg = $"Successfully added package: {packageURL}";
                        statusLabel.text = msg;
                        Debug.Log(msg);
                    }
                    else if (request.Status >= UnityEditor.PackageManager.StatusCode.Failure)
                    {
                        string msg = $"Failed to add package: {packageURL}\nError: {request.Error.message}";
                        statusLabel.text = msg;
                        Debug.LogError(msg);
                    }

                    EditorApplication.update -= callback;
                    finished = true;
                }
            };

            EditorApplication.update += callback;

            while (!finished)
                await Task.Yield();
        }

        private async Task<string> CreateDirectoriesAsync(DirectoryNode node, string parentPath, bool skipRoot = true)
        {
            string currentPath = skipRoot ? parentPath : Path.Combine(parentPath, node.Name);

            if (!skipRoot)
            {
                string relativePath = "Assets" + currentPath.Replace(Application.dataPath, "").Replace("\\", "/");

                if (!Directory.Exists(currentPath))
                {
                    Directory.CreateDirectory(currentPath);
                    statusLabel.text = $"Created directory: {relativePath}";
                }
                else
                {
                    statusLabel.text = $"Directory already exists: {relativePath}";
                }
                await Task.Yield();
            }

            foreach (DirectoryNode child in node.Children)
                await CreateDirectoriesAsync(child, currentPath, false);

            return currentPath;
        }
    }
}
