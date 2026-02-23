using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoSlimes.UnityUtils.Editor.EditorWindows.ScriptCounter
{
    public class ScriptCounterWindow : EditorWindow
    {
        private struct ScriptInfo
        {
            public string Name;
            public int LineCount;
            public long SizeBytes;
            public string Path;
            public string ScriptType;
            public int CommentLineCount;
            public int NonEmptyLineCount;
            public bool HasUpdateMethod;
        }

        private enum SortOption { Name, Type, Lines, Size, CommentRatio }
        private SortOption currentSort = SortOption.Lines;
        private bool sortAscending = false;

        private List<ScriptInfo> scriptList = new();

        private Button analyzeButton;
        private Button selectFolderButton;
        private Label folderLabel;
        private ProgressBar progressBar;
        private VisualElement dashboardContainer;
        private ListView scriptListView;

        private Label lblTotalScripts, lblTotalLines, lblTotalSize;
        private Label lblAvgLines, lblCommentPct, lblMonoPct, lblUpdateCount;

        private VisualElement typeStatsContainer;

        private Button btnBiggest, btnSmallest;

        private Label headerName, headerType, headerLines, headerSize, headerComments;

        private bool isProcessing = false;
        private float currentProgress = 0f;
        private string currentProcessingFile = "";

        private string selectedFolder = "Assets";

        [MenuItem("Tools/UnityUtils/Script Analytics")]
        private static void OpenWindow()
        {
            ScriptCounterWindow window = GetWindow<ScriptCounterWindow>();
            window.titleContent = new GUIContent("Script Analytics");
            window.minSize = new Vector2(620, 600);
            window.Show();
        }

        public void CreateGUI()
        {
            TryFindScriptsFolder();

            VisualElement root = rootVisualElement;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;

            var title = new Label("Project Script Analytics");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.alignSelf = Align.Center;
            title.style.marginBottom = 10;
            root.Add(title);

            var folderRow = new VisualElement();
            folderRow.style.flexDirection = FlexDirection.Row;
            folderRow.style.marginBottom = 5;

            selectFolderButton = new Button(SelectFolder) { text = "Select Folder" };
            selectFolderButton.style.height = 25;

            folderLabel = new Label(selectedFolder);
            folderLabel.style.flexGrow = 1;
            folderLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            folderLabel.style.marginLeft = 6;

            folderRow.Add(selectFolderButton);
            folderRow.Add(folderLabel);
            root.Add(folderRow);

            analyzeButton = new Button(CountScriptsAsync) { text = "Analyze Scripts" };
            analyzeButton.style.height = 30;
            root.Add(analyzeButton);

            progressBar = new ProgressBar();
            progressBar.style.marginTop = 5;
            progressBar.style.display = DisplayStyle.None;
            root.Add(progressBar);

            dashboardContainer = new VisualElement();
            dashboardContainer.style.marginTop = 15;
            dashboardContainer.style.display = DisplayStyle.None;
            dashboardContainer.style.backgroundColor = new Color(0, 0, 0, 0.2f);
            dashboardContainer.style.paddingTop = 10;
            dashboardContainer.style.paddingBottom = 10;
            dashboardContainer.style.paddingLeft = 10;
            dashboardContainer.style.paddingRight = 10;
            dashboardContainer.style.borderTopLeftRadius = 5;
            dashboardContainer.style.borderTopRightRadius = 5;
            dashboardContainer.style.borderBottomLeftRadius = 5;
            dashboardContainer.style.borderBottomRightRadius = 5;

            VisualElement row1 = CreateRow();
            lblTotalScripts = CreateStatBox(row1, "Total Scripts");
            lblTotalLines = CreateStatBox(row1, "Total Lines");
            lblTotalSize = CreateStatBox(row1, "Total Size");
            dashboardContainer.Add(row1);

            VisualElement row2 = CreateRow();
            lblAvgLines = CreateStatBox(row2, "Avg Lines");
            lblCommentPct = CreateStatBox(row2, "Comments %");
            lblMonoPct = CreateStatBox(row2, "MonoBehaviour %");
            lblUpdateCount = CreateStatBox(row2, "Scripts w/ Update");
            dashboardContainer.Add(row2);

            typeStatsContainer = new VisualElement();
            typeStatsContainer.style.marginTop = 10;
            typeStatsContainer.style.flexDirection = FlexDirection.Row;
            typeStatsContainer.style.flexWrap = Wrap.Wrap;
            dashboardContainer.Add(typeStatsContainer);

            VisualElement row3 = CreateRow();
            row3.style.marginTop = 10;

            btnBiggest = new Button(() => PingScript(btnBiggest.userData as string));
            btnBiggest.style.flexGrow = 1;
            btnBiggest.style.height = 25;

            btnSmallest = new Button(() => PingScript(btnSmallest.userData as string));
            btnSmallest.style.flexGrow = 1;
            btnSmallest.style.height = 25;

            row3.Add(btnBiggest);
            row3.Add(btnSmallest);
            dashboardContainer.Add(row3);

            root.Add(dashboardContainer);

            var listHeader = new VisualElement();
            listHeader.style.flexDirection = FlexDirection.Row;
            listHeader.style.marginTop = 15;
            listHeader.style.paddingLeft = 5;
            listHeader.style.paddingRight = 5;
            listHeader.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            listHeader.style.height = 20;
            listHeader.style.alignItems = Align.Center;

            headerName = CreateHeaderLabel("Name", 210, SortOption.Name);
            headerType = CreateHeaderLabel("Type", 160, SortOption.Type);
            headerLines = CreateHeaderLabel("Lines", 55, SortOption.Lines);
            headerSize = CreateHeaderLabel("Size", 55, SortOption.Size);
            headerComments = CreateHeaderLabel("Comm. %", 70, SortOption.CommentRatio);

            headerComments.style.marginLeft = 10;

            listHeader.Add(headerName);
            listHeader.Add(headerType);
            listHeader.Add(headerLines);
            listHeader.Add(headerSize);
            listHeader.Add(headerComments);
            root.Add(listHeader);

            scriptListView = new ListView();
            scriptListView.style.flexGrow = 1;
            scriptListView.style.marginTop = 2;
            scriptListView.fixedItemHeight = 22;
            scriptListView.makeItem = MakeListItem;
            scriptListView.bindItem = BindListItem;
            scriptListView.selectionType = SelectionType.Single;
            scriptListView.selectionChanged += OnSelectionChanged;
            root.Add(scriptListView);

            UpdateHeaderVisuals();

            root.schedule.Execute(() =>
            {
                if (isProcessing)
                {
                    progressBar.value = currentProgress * 100f;
                    progressBar.title = $"Scanning: {currentProcessingFile}";
                }
            }).Every(50);
        }

        private void TryFindScriptsFolder()
        {
            var dirs = Directory.GetDirectories(Application.dataPath, "Scripts", SearchOption.AllDirectories);
            if (dirs.Length > 0)
            {
                selectedFolder = "Assets" + dirs[0].Replace(Application.dataPath, "").Replace("\\", "/");
            }
        }

        private void SelectFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Script Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(path)) return;
            if (!path.StartsWith(Application.dataPath)) return;
            selectedFolder = "Assets" + path.Replace(Application.dataPath, "").Replace("\\", "/");
            folderLabel.text = selectedFolder;
        }

        private VisualElement MakeListItem()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.paddingLeft = 5;

            var lblName = new Label();
            lblName.style.width = 210;
            lblName.style.overflow = Overflow.Hidden;

            var lblType = new Label();
            lblType.style.width = 160;
            lblType.style.overflow = Overflow.Hidden;
            lblType.style.unityFontStyleAndWeight = FontStyle.Bold;

            var lblLines = new Label();
            lblLines.style.width = 55;

            var lblSize = new Label();
            lblSize.style.width = 55;

            var lblComm = new Label();
            lblComm.style.width = 70;
            lblComm.style.marginLeft = 10;

            container.Add(lblName);
            container.Add(lblType);
            container.Add(lblLines);
            container.Add(lblSize);
            container.Add(lblComm);

            return container;
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (index >= scriptList.Count) return;

            ScriptInfo info = scriptList[index];

            var lblName = element.ElementAt(0) as Label;
            var lblType = element.ElementAt(1) as Label;
            var lblLines = element.ElementAt(2) as Label;
            var lblSize = element.ElementAt(3) as Label;
            var lblComm = element.ElementAt(4) as Label;

            lblName.text = info.Name;
            lblType.text = info.ScriptType;
            lblLines.text = info.LineCount.ToString("N0");
            lblSize.text = FormatBytes(info.SizeBytes);

            float ratio = info.NonEmptyLineCount > 0 ? (float)info.CommentLineCount / info.NonEmptyLineCount : 0f;
            lblComm.text = $"{ratio:P0}";

            Color typeColor = Color.gray;
            string t = info.ScriptType;

            if (t.Contains("MonoBehaviour"))
            {
                typeColor = new Color(0.4f, 1f, 0.4f); // Green
            }
            else if (t.Contains("ScriptableObject"))
            {
                typeColor = new Color(0.4f, 0.8f, 1f); // Cyan
            }
            else if (t.Contains("Interface"))
            {
                typeColor = new Color(1f, 0.9f, 0.4f); // Yellowish
            }
            else if (t.Contains("Enum"))
            {
                typeColor = new Color(1f, 0.6f, 0.2f); // Orange
            }
            else if (t.Contains("Editor") || t.Contains("EditorWindow"))
            {
                typeColor = new Color(1f, 0.5f, 0.6f); // Red/Pink
            }
            else if (t.Contains("Struct"))
            {
                typeColor = new Color(0.8f, 0.5f, 1f); // Purple
            }
            else if (t.Contains("Class"))
            {
                typeColor = new Color(0.7f, 0.7f, 1f); // Blueish
            }

            lblType.style.color = typeColor;

            Color c = info.LineCount > 1000 ? new Color(1f, 0.4f, 0.4f) :
                      info.LineCount > 500 ? new Color(1f, 0.7f, 0.2f) :
                      Color.white;

            lblName.style.color = c;
            lblLines.style.color = c;
            lblSize.style.color = Color.gray;
            lblComm.style.color = Color.gray;
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (selectedItems.FirstOrDefault() is ScriptInfo info)
                PingScript(info.Path);
        }

        private VisualElement CreateRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            return row;
        }

        private Label CreateStatBox(VisualElement parent, string title)
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.alignItems = Align.Center;

            var t = new Label(title);
            t.style.fontSize = 10;
            t.style.color = Color.gray;

            var v = new Label("-");
            v.style.fontSize = 18;
            v.style.unityFontStyleAndWeight = FontStyle.Bold;

            container.Add(t);
            container.Add(v);
            parent.Add(container);

            return v;
        }

        private Label CreateHeaderLabel(string text, float width, SortOption option)
        {
            var l = new Label(text);
            l.style.width = width;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            l.RegisterCallback<MouseDownEvent>(evt => OnHeaderClicked(option));
            return l;
        }

        private void OnHeaderClicked(SortOption option)
        {
            if (currentSort == option)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSort = option;
                // Default defaults: Text = Ascending, Numbers = Descending
                if (option == SortOption.Name || option == SortOption.Type) sortAscending = true;
                else sortAscending = false;
            }

            ApplySort();
            UpdateHeaderVisuals();
        }

        private void ApplySort()
        {
            if (scriptList == null || scriptList.Count == 0) return;

            IEnumerable<ScriptInfo> query = scriptList;

            switch (currentSort)
            {
                case SortOption.Name:
                    query = sortAscending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case SortOption.Type:
                    query = sortAscending ? query.OrderBy(x => x.ScriptType) : query.OrderByDescending(x => x.ScriptType);
                    break;
                case SortOption.Lines:
                    query = sortAscending ? query.OrderBy(x => x.LineCount) : query.OrderByDescending(x => x.LineCount);
                    break;
                case SortOption.Size:
                    query = sortAscending ? query.OrderBy(x => x.SizeBytes) : query.OrderByDescending(x => x.SizeBytes);
                    break;
                case SortOption.CommentRatio:
                    query = sortAscending
                        ? query.OrderBy(x => x.NonEmptyLineCount > 0 ? (double)x.CommentLineCount / x.NonEmptyLineCount : 0)
                        : query.OrderByDescending(x => x.NonEmptyLineCount > 0 ? (double)x.CommentLineCount / x.NonEmptyLineCount : 0);
                    break;
            }

            scriptList = query.ToList();
            scriptListView.itemsSource = scriptList;
            scriptListView.Rebuild();
        }

        private void UpdateHeaderVisuals()
        {
            if (headerName != null) headerName.text = "Name";
            if (headerType != null) headerType.text = "Type";
            if (headerLines != null) headerLines.text = "Lines";
            if (headerSize != null) headerSize.text = "Size";
            if (headerComments != null) headerComments.text = "Comm. %";

            string arrow = sortAscending ? " ↑" : " ↓";

            switch (currentSort)
            {
                case SortOption.Name: headerName.text += arrow; break;
                case SortOption.Type: headerType.text += arrow; break;
                case SortOption.Lines: headerLines.text += arrow; break;
                case SortOption.Size: headerSize.text += arrow; break;
                case SortOption.CommentRatio: headerComments.text += arrow; break;
            }
        }

        private async void CountScriptsAsync()
        {
            isProcessing = true;
            analyzeButton.SetEnabled(false);
            progressBar.style.display = DisplayStyle.Flex;
            dashboardContainer.style.display = DisplayStyle.None;

            scriptList.Clear();
            scriptListView.itemsSource = scriptList;
            scriptListView.Rebuild();

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript", new[] { selectedFolder });
            List<string> filePaths = scriptGUIDs
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".cs"))
                .ToList();

            Dictionary<string, string> fileTypeMap = new Dictionary<string, string>();

            for (int i = 0; i < filePaths.Count; i++)
            {
                string path = filePaths[i];
                string typeName = "Unknown";

                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null)
                {
                    System.Type cls = monoScript.GetClass();
                    if (cls != null)
                    {
                        if (cls.IsEnum) typeName = "Enum";
                        else if (cls.IsInterface) typeName = "Interface";
                        else if (cls.IsValueType && !cls.IsPrimitive) typeName = "Struct";
                        else
                        {
                            string baseType = "C# Class";

                            if (typeof(MonoBehaviour).IsAssignableFrom(cls)) baseType = "MonoBehaviour";
                            else if (typeof(ScriptableObject).IsAssignableFrom(cls)) baseType = "ScriptableObject";
                            else if (typeof(UnityEditor.Editor).IsAssignableFrom(cls)) baseType = "Editor";
                            else if (typeof(EditorWindow).IsAssignableFrom(cls)) baseType = "EditorWindow";

                            if (cls.IsAbstract)
                            {
                                typeName = "Abstract " + baseType;
                            }
                            else
                            {
                                typeName = baseType;
                            }
                        }
                    }
                    else
                    {
                        typeName = "Unknown/Generic";
                    }
                }
                fileTypeMap[path] = typeName;
            }

            List<ScriptInfo> resultData = await Task.Run(() =>
            {
                var results = new List<ScriptInfo>();
                int count = filePaths.Count;

                for (int i = 0; i < count; i++)
                {
                    string path = filePaths[i];
                    currentProgress = (float)i / count;
                    currentProcessingFile = Path.GetFileName(path);

                    if (!File.Exists(path)) continue;

                    int lines = 0;
                    int commentLines = 0;
                    int nonEmptyLines = 0;
                    bool hasUpdate = false;

                    foreach (string line in File.ReadLines(path))
                    {
                        lines++;

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            nonEmptyLines++;
                            string trimmed = line.TrimStart();
                            if (trimmed.StartsWith("//") || trimmed.StartsWith("/*"))
                                commentLines++;
                        }

                        if (line.Contains("void Update()") || line.Contains("void FixedUpdate()") || line.Contains("void LateUpdate()"))
                            hasUpdate = true;
                    }

                    long size = new FileInfo(path).Length;

                    string typeStr = fileTypeMap.ContainsKey(path) ? fileTypeMap[path] : "Unknown";

                    results.Add(new ScriptInfo
                    {
                        Name = Path.GetFileName(path),
                        LineCount = lines,
                        SizeBytes = size,
                        Path = path,
                        ScriptType = typeStr,
                        CommentLineCount = commentLines,
                        NonEmptyLineCount = nonEmptyLines, 
                        HasUpdateMethod = hasUpdate
                    });
                }
                return results;
            });

            scriptList = resultData;
            ApplySort();
            UpdateHeaderVisuals();

            long totalLines = scriptList.Sum(x => x.LineCount);
            long totalSize = scriptList.Sum(x => x.SizeBytes);
            int totalScripts = scriptList.Count;

            long totalComments = scriptList.Sum(x => x.CommentLineCount);
            long totalNonEmptyLines = scriptList.Sum(x => x.NonEmptyLineCount);
            int totalUpdateScripts = scriptList.Count(x => x.HasUpdateMethod);
            
            int totalMonos = scriptList.Count(x => x.ScriptType.Contains("MonoBehaviour"));
            float monoPct = totalScripts > 0 ? (float)totalMonos / totalScripts : 0f;
            float updateRatio = totalMonos > 0 ? (float)totalUpdateScripts / totalMonos : 0f;

            lblTotalScripts.text = totalScripts.ToString("N0");
            lblTotalLines.text = totalLines.ToString("N0");
            lblTotalSize.text = FormatBytes(totalSize);

            lblAvgLines.text = (totalScripts > 0 ? (float)totalLines / totalScripts : 0).ToString("F1");

            double commentPct = totalNonEmptyLines > 0 ? (double)totalComments / totalNonEmptyLines * 100.0 : 0;
            lblCommentPct.text = $"{commentPct:F1}%";

            lblMonoPct.text = $"{monoPct * 100f:F0}%";
            lblUpdateCount.text = $"{totalUpdateScripts:N0} ({updateRatio * 100f:F0}%)";

            if (updateRatio > 0.5f) lblUpdateCount.style.color = new Color(1f, 0.4f, 0.4f);
            else if (updateRatio > 0.2f) lblUpdateCount.style.color = new Color(1f, 0.8f, 0.2f);
            else lblUpdateCount.style.color = new Color(0.4f, 1f, 0.4f);

            typeStatsContainer.Clear();
            if (scriptList.Count > 0)
            {
                var groups = scriptList
                    .GroupBy(x => x.ScriptType.Replace("Abstract ", ""))
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count);

                foreach (var g in groups)
                {
                    var lbl = CreateStatBox(typeStatsContainer, g.Type);
                    float typePct = totalScripts > 0 ? (float)g.Count / totalScripts * 100f : 0f;
                    lbl.text = $"{g.Count:N0} ({typePct:F0}%)";
                    lbl.parent.style.marginBottom = 5;
                    lbl.parent.style.marginRight = 10;
                    lbl.parent.style.minWidth = 80;
                }
            }

            if (scriptList.Count > 0)
            {
                var sortedByLines = scriptList.OrderByDescending(x => x.LineCount).ToList();
                var biggest = sortedByLines[0];
                var smallest = sortedByLines[^1];

                btnBiggest.text = $"↑ Most lines: {biggest.Name} ({biggest.LineCount:N0})";
                btnBiggest.userData = biggest.Path;

                btnSmallest.text = $"↓ Fewest lines: {smallest.Name} ({smallest.LineCount:N0})";
                btnSmallest.userData = smallest.Path;
            }

            scriptListView.itemsSource = scriptList;
            scriptListView.Rebuild();

            isProcessing = false;
            analyzeButton.SetEnabled(true);
            progressBar.style.display = DisplayStyle.None;
            dashboardContainer.style.display = DisplayStyle.Flex;
        }

        private void PingScript(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (obj != null) EditorGUIUtility.PingObject(obj);
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
