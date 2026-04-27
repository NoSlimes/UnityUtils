using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NoSlimes.UnityUtils.Editor.EditorWindows.ScriptCounter
{
    public class ScriptCounterWindow : EditorWindow
    {
        private struct TypeInfo
        {
            public string TypeName;
            public string Category;
            public string FileName;
            public string FullPath;
            public int FileLineCount;
            public long FileSizeBytes;
            public int FileCommentLines;
            public int FileNonEmptyLines;
            public bool HasUpdateMethod;
        }

        private enum SortOption { Name, Type, File, Lines, Size, CommentRatio }
        private SortOption currentSort = SortOption.Lines;
        private bool sortAscending = false;

        private List<TypeInfo> typeList = new List<TypeInfo>();

        private static readonly Regex UpdateRegex = new Regex(@"\b(void|override)\s+(Update|FixedUpdate|LateUpdate)\s*\(", RegexOptions.Compiled);
        private static readonly Regex TypeDiscoveryRegex = new Regex(@"\b(class|struct|interface|enum)\s+([A-Za-z0-9_<>]+)", RegexOptions.Compiled);

        private Button analyzeButton;
        private Label folderLabel;
        private ProgressBar progressBar;
        private VisualElement dashboardContainer;
        private ListView scriptListView;
        private Label lblTotalScriptsTypes, lblTotalLines, lblTotalSize, lblAvgLines, lblCommentPct, lblMonoPct, lblUpdateCount;
        private VisualElement typeStatsContainer;
        private Button btnBiggest, btnSmallest;
        private Label headerName, headerType, headerFile, headerLines, headerSize, headerComments;

        private bool isProcessing = false;
        private float currentProgress = 0f;
        private string currentProcessingFile = "";
        private string selectedFolder;

        [MenuItem("Tools/UnityUtils/Script Analytics")]
        private static void OpenWindow()
        {
            ScriptCounterWindow window = GetWindow<ScriptCounterWindow>("Script Analytics");
            window.minSize = new Vector2(720, 600);
            window.Show();
        }

        public void CreateGUI()
        {
            if (string.IsNullOrEmpty(selectedFolder))
            {
                TryFindScriptsFolder();
            }

            VisualElement root = rootVisualElement;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;

            Label title = new Label("Project Script Analytics");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.alignSelf = Align.Center;
            title.style.marginBottom = 10;
            root.Add(title);

            VisualElement folderRow = new VisualElement();
            folderRow.style.flexDirection = FlexDirection.Row;
            folderRow.style.marginBottom = 5;

            Button selectBtn = new Button(SelectFolder) { text = "Select Folder" };
            selectBtn.style.height = 25;
            folderRow.Add(selectBtn);

            folderLabel = new Label(selectedFolder);
            folderLabel.style.flexGrow = 1;
            folderLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            folderLabel.style.marginLeft = 6;
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

            VisualElement row1 = CreateRow();
            lblTotalScriptsTypes = CreateStatBox(row1, "Scripts / Types");
            lblTotalLines = CreateStatBox(row1, "Total Lines");
            lblTotalSize = CreateStatBox(row1, "Total Size");
            dashboardContainer.Add(row1);

            VisualElement row2 = CreateRow();
            lblAvgLines = CreateStatBox(row2, "Avg File Lines");
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

            VisualElement listHeader = new VisualElement();
            listHeader.style.flexDirection = FlexDirection.Row;
            listHeader.style.marginTop = 15;
            listHeader.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            listHeader.style.height = 20;
            listHeader.style.alignItems = Align.Center;

            headerName = CreateHeaderLabel("Type Name", 180, SortOption.Name);
            headerType = CreateHeaderLabel("Category", 130, SortOption.Type);
            headerFile = CreateHeaderLabel("Defined In", 180, SortOption.File);
            headerFile.style.marginLeft = 20;

            headerLines = CreateHeaderLabel("Lines", 55, SortOption.Lines);
            headerSize = CreateHeaderLabel("Size", 55, SortOption.Size);
            headerComments = CreateHeaderLabel("Comm. %", 70, SortOption.CommentRatio);

            listHeader.Add(headerName);
            listHeader.Add(headerType);
            listHeader.Add(headerFile);
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

            // Events
            scriptListView.selectionChanged += OnSelectionChanged; // Ping on single click
            scriptListView.itemsChosen += OnItemsChosen;           // Open on double click/enter

            root.Add(scriptListView);

            root.schedule.Execute(() =>
            {
                if (isProcessing)
                {
                    progressBar.value = currentProgress * 100f;
                    progressBar.title = $"Scanning: {currentProcessingFile}";
                }
            }).Every(50);
        }

        private async void CountScriptsAsync()
        {
            isProcessing = true;
            analyzeButton.SetEnabled(false);
            progressBar.style.display = DisplayStyle.None; // Display later during processing
            dashboardContainer.style.display = DisplayStyle.None;

            typeList.Clear();

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript", new[] { selectedFolder });
            List<string> filePaths = scriptGUIDs.Select(AssetDatabase.GUIDToAssetPath).Where(p => p.EndsWith(".cs")).Distinct().ToList();

            progressBar.style.display = DisplayStyle.Flex;

            var allAssemblyTypes = new Dictionary<string, Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && (a.GetName().Name.Contains("Assembly-CSharp") || a.GetName().Name.Contains("NoSlimes")));

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try { types = assembly.GetTypes(); } catch { continue; }
                foreach (Type t in types)
                {
                    if (t.Name.StartsWith("<") || t.Name.Contains("$") || t.Name.Contains("__")) continue;
                    if (!allAssemblyTypes.ContainsKey(t.Name)) allAssemblyTypes.Add(t.Name, t);
                }
            }

            typeList = await Task.Run(() =>
            {
                var results = new List<TypeInfo>();
                for (int i = 0; i < filePaths.Count; i++)
                {
                    string path = filePaths[i];
                    currentProgress = (float)i / filePaths.Count;
                    currentProcessingFile = Path.GetFileName(path);

                    int lines = 0, commentLines = 0, nonEmptyLines = 0;
                    bool hasUpdate = false;
                    string content;
                    try { content = File.ReadAllText(path); } catch { continue; }

                    using (StringReader reader = new StringReader(content))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines++;
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                nonEmptyLines++;
                                string trimmed = line.TrimStart();
                                if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*")) commentLines++;
                            }
                            if (UpdateRegex.IsMatch(line)) hasUpdate = true;
                        }
                    }

                    MatchCollection matches = TypeDiscoveryRegex.Matches(content);
                    if (matches.Count == 0)
                    {
                        results.Add(CreateEntry("Unknown", "Generic/Other", path, lines, commentLines, nonEmptyLines, false));
                    }
                    else
                    {
                        foreach (Match m in matches)
                        {
                            string keyword = m.Groups[1].Value;
                            string name = m.Groups[2].Value;
                            if (name.Contains("<")) name = name.Split('<')[0];

                            string category = char.ToUpper(keyword[0]) + keyword.Substring(1);

                            if (allAssemblyTypes.TryGetValue(name, out Type type))
                            {
                                if (type.IsInterface) category = "Interface";
                                else if (type.IsEnum) category = "Enum";
                                else if (type.IsValueType && !type.IsPrimitive) category = "Struct";
                                else if (typeof(MonoBehaviour).IsAssignableFrom(type)) category = "MonoBehaviour";
                                else if (typeof(ScriptableObject).IsAssignableFrom(type)) category = "ScriptableObject";
                                else if (typeof(UnityEditor.Editor).IsAssignableFrom(type)) category = "Editor";

                                if (!type.IsInterface && !type.IsEnum)
                                {
                                    if (type.IsAbstract && type.IsSealed) category = "Static " + category;
                                    else if (type.IsAbstract) category = "Abstract " + category;
                                }
                            }
                            results.Add(CreateEntry(name, category, path, lines, commentLines, nonEmptyLines, hasUpdate));
                        }
                    }
                }
                return results;
            });

            ApplySort();
            UpdateDashboard();

            isProcessing = false;
            analyzeButton.SetEnabled(true);
            progressBar.style.display = DisplayStyle.None;
            dashboardContainer.style.display = DisplayStyle.Flex;
        }

        private TypeInfo CreateEntry(string name, string cat, string path, int lines, int comm, int nonEmp, bool update)
        {
            return new TypeInfo
            {
                TypeName = name,
                Category = cat,
                FileName = Path.GetFileName(path),
                FullPath = path,
                FileLineCount = lines,
                FileSizeBytes = new FileInfo(path).Length,
                FileCommentLines = comm,
                FileNonEmptyLines = nonEmp,
                HasUpdateMethod = update
            };
        }

        private void UpdateDashboard()
        {
            List<TypeInfo> uniqueFiles = typeList.GroupBy(t => t.FullPath).Select(g => g.First()).ToList();
            long totalLines = uniqueFiles.Sum(x => x.FileLineCount);
            int totalFiles = uniqueFiles.Count;

            lblTotalScriptsTypes.text = $"{totalFiles} / {typeList.Count}";
            lblTotalLines.text = totalLines.ToString("N0");
            lblTotalSize.text = FormatBytes(uniqueFiles.Sum(x => x.FileSizeBytes));
            lblAvgLines.text = (totalFiles > 0 ? (float)totalLines / totalFiles : 0).ToString("F1");

            long totalComments = uniqueFiles.Sum(x => x.FileCommentLines);
            long totalNonEmpty = uniqueFiles.Sum(x => x.FileNonEmptyLines);
            lblCommentPct.text = $"{(totalNonEmpty > 0 ? (double)totalComments / totalNonEmpty * 100 : 0):F1}%";

            int totalMonos = typeList.Count(x => x.Category.Contains("MonoBehaviour"));
            int totalUpdates = typeList.Count(x => x.HasUpdateMethod);
            lblMonoPct.text = $"{(typeList.Count > 0 ? (float)totalMonos / typeList.Count * 100 : 0):F0}%";
            lblUpdateCount.text = $"{totalUpdates} ({(totalMonos > 0 ? (float)totalUpdates / totalMonos * 100 : 0):F0}%)";

            typeStatsContainer.Clear();
            var typeCounts = typeList.GroupBy(t => t.Category.Replace("Abstract ", "").Replace("Static ", ""))
                .Select(g => new { Type = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count);

            foreach (var g in typeCounts)
            {
                Label lbl = CreateStatBox(typeStatsContainer, g.Type);
                lbl.text = $"{g.Count}";
                lbl.parent.style.minWidth = 80;
            }

            if (uniqueFiles.Count > 0)
            {
                List<TypeInfo> sorted = uniqueFiles.OrderByDescending(x => x.FileLineCount).ToList();
                btnBiggest.text = $"↑ Most lines: {sorted[0].FileName} ({sorted[0].FileLineCount:N0})";
                btnBiggest.userData = sorted[0].FullPath;
                btnSmallest.text = $"↓ Fewest lines: {sorted[^1].FileName} ({sorted[^1].FileLineCount:N0})";
                btnSmallest.userData = sorted[^1].FullPath;
            }
        }

        private void BindListItem(VisualElement element, int index)
        {
            TypeInfo info = typeList[index];
            List<Label> labels = element.Children().Cast<Label>().ToList();

            labels[0].text = info.TypeName;
            labels[1].text = info.Category;
            labels[2].text = info.FileName;
            labels[3].text = info.FileLineCount.ToString("N0");
            labels[4].text = FormatBytes(info.FileSizeBytes);
            labels[5].text = $"{(info.FileNonEmptyLines > 0 ? (float)info.FileCommentLines / info.FileNonEmptyLines : 0):P0}";

            labels[1].style.color = GetTypeColor(info.Category);
            labels[2].style.color = Color.gray;

            Color lineCol = info.FileLineCount > 1000 ? new Color(1f, 0.4f, 0.4f) : info.FileLineCount > 500 ? new Color(1f, 0.7f, 0.2f) : Color.white;
            labels[0].style.color = labels[3].style.color = lineCol;
        }

        private Color GetTypeColor(string t)
        {
            if (t.Contains("MonoBehaviour"))
            {
                return new Color(0.4f, 1f, 0.4f);
            }
            if (t.Contains("ScriptableObject"))
            {
                return new Color(0.4f, 0.8f, 1f);
            }
            if (t.Contains("Interface"))
            {
                return new Color(1f, 0.9f, 0.4f);
            }
            if (t.Contains("Static"))
            {
                return new Color(0.7f, 1f, 1f);
            }
            if (t.Contains("Enum"))
            {
                return new Color(1f, 0.6f, 0.2f);
            }
            if (t.Contains("Struct"))
            {
                return new Color(0.8f, 0.5f, 1f);
            }

            return new Color(0.7f, 0.7f, 1f);
        }

        private VisualElement CreateRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            return row;
        }

        private Label CreateStatBox(VisualElement parent, string title)
        {
            VisualElement c = new VisualElement();
            c.style.flexGrow = 1;
            c.style.alignItems = Align.Center;

            Label t = new Label(title);
            t.style.fontSize = 10;
            t.style.color = Color.gray;
            c.Add(t);

            Label v = new Label("-");
            v.style.fontSize = 18;
            v.style.unityFontStyleAndWeight = FontStyle.Bold;
            c.Add(v);

            parent.Add(c);
            return v;
        }

        private Label CreateHeaderLabel(string text, float width, SortOption option)
        {
            Label l = new Label(text);
            l.style.width = width;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            l.RegisterCallback<MouseDownEvent>(e => OnHeaderClicked(option));
            return l;
        }

        private VisualElement MakeListItem()
        {
            VisualElement c = new VisualElement();
            c.style.flexDirection = FlexDirection.Row;
            c.style.alignItems = Align.Center;
            c.style.paddingLeft = 5;

            c.Add(new Label { style = { width = 180, overflow = Overflow.Hidden } }); // Type Name
            c.Add(new Label { style = { width = 130, overflow = Overflow.Hidden, unityFontStyleAndWeight = FontStyle.Bold } }); // Category
            c.Add(new Label { style = { width = 180, overflow = Overflow.Hidden, marginLeft = 20 } }); // File 
            c.Add(new Label { style = { width = 55 } }); // Lines
            c.Add(new Label { style = { width = 55 } }); // Size
            c.Add(new Label { style = { width = 70, marginLeft = 10 } }); // Comm %

            return c;
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
                sortAscending = (option == SortOption.Name || option == SortOption.Type || option == SortOption.File);
            }

            ApplySort();
            UpdateHeaderVisuals();
        }

        private void ApplySort()
        {
            if (typeList == null || typeList.Count == 0) return;
            IEnumerable<TypeInfo> q = typeList;
            switch (currentSort)
            {
                case SortOption.Name: q = sortAscending ? q.OrderBy(x => x.TypeName) : q.OrderByDescending(x => x.TypeName); break;
                case SortOption.Type: q = sortAscending ? q.OrderBy(x => x.Category) : q.OrderByDescending(x => x.Category); break;
                case SortOption.File: q = sortAscending ? q.OrderBy(x => x.FileName) : q.OrderByDescending(x => x.FileName); break;
                case SortOption.Lines: q = sortAscending ? q.OrderBy(x => x.FileLineCount) : q.OrderByDescending(x => x.FileLineCount); break;
                case SortOption.Size: q = sortAscending ? q.OrderBy(x => x.FileSizeBytes) : q.OrderByDescending(x => x.FileSizeBytes); break;
                case SortOption.CommentRatio: q = sortAscending ? q.OrderBy(x => (float)x.FileCommentLines / Math.Max(1, x.FileNonEmptyLines)) : q.OrderByDescending(x => (float)x.FileCommentLines / Math.Max(1, x.FileNonEmptyLines)); break;
            }
            typeList = q.ToList();
            scriptListView.itemsSource = typeList;
            scriptListView.Rebuild();
        }

        private void UpdateHeaderVisuals()
        {
            headerName.text = "Type Name";
            headerType.text = "Category";
            headerFile.text = "Defined In";
            headerLines.text = "Lines";
            headerSize.text = "Size";
            headerComments.text = "Comm. %";

            string arrow = sortAscending ? " ↑" : " ↓";
            switch (currentSort)
            {
                case SortOption.Name: headerName.text += arrow; break;
                case SortOption.Type: headerType.text += arrow; break;
                case SortOption.File: headerFile.text += arrow; break;
                case SortOption.Lines: headerLines.text += arrow; break;
                case SortOption.Size: headerSize.text += arrow; break;
                case SortOption.CommentRatio: headerComments.text += arrow; break;
            }
        }

        private void TryFindScriptsFolder()
        {
            var dirs = Directory.GetDirectories(Application.dataPath, "_Game", SearchOption.AllDirectories);
            if (dirs.Length > 0)
            {
                selectedFolder = "Assets" + dirs[0].Replace(Application.dataPath, "").Replace("\\", "/");
            }
            else
            {
                selectedFolder = "Assets";
            }
        }

        private void SelectFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Script Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                selectedFolder = "Assets" + path.Replace(Application.dataPath, "").Replace("\\", "/");
                folderLabel.text = selectedFolder;
            }
        }

        private void OnSelectionChanged(IEnumerable<object> items)
        {
            if (items.FirstOrDefault() is TypeInfo info)
            {
                PingScript(info.FullPath);
            }
        }

        private void OnItemsChosen(IEnumerable<object> items)
        {
            if (items.FirstOrDefault() is TypeInfo info)
            {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(info.FullPath);
                if (obj != null)
                {
                    AssetDatabase.OpenAsset(obj);
                }
            }
        }

        private void PingScript(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj != null)
                {
                    EditorGUIUtility.PingObject(obj);
                }
            }
        }

        private string FormatBytes(long b)
        {
            string[] s = { "B", "KB", "MB" };
            double l = b;
            int o = 0;
            while (l >= 1024 && o < s.Length - 1)
            {
                o++;
                l /= 1024;
            }
            return $"{l:0.##} {s[o]}";
        }
    }
}