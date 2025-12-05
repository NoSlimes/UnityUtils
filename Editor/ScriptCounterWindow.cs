using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoSlimes.Utils.Editor.EditorWindows.ScriptCounter
{
    public class ScriptCounterWindow : EditorWindow
    {
        private struct ScriptInfo
        {
            public string Name;
            public int LineCount;
            public long SizeBytes;
            public string Path;
        }

        private List<ScriptInfo> scriptList = new();

        private Button analyzeButton;
        private ProgressBar progressBar;
        private VisualElement dashboardContainer;
        private ListView scriptListView;

        private Label lblTotalScripts, lblTotalLines, lblTotalSize;
        private Label lblAvgLines, lblAvgSize;
        private Button btnBiggest, btnSmallest;

        private bool isProcessing = false;
        private float currentProgress = 0f;
        private string currentProcessingFile = "";

        [MenuItem("Tools/UnityUtils/Script Analytics")]
        private static void OpenWindow()
        {
            ScriptCounterWindow window = GetWindow<ScriptCounterWindow>();
            window.titleContent = new GUIContent("Script Analytics");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        public void CreateGUI()
        {
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
            lblAvgSize = CreateStatBox(row2, "Avg Size");
            dashboardContainer.Add(row2);

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

            listHeader.Add(CreateHeaderLabel("Name", 250));
            listHeader.Add(CreateHeaderLabel("Lines", 80));
            listHeader.Add(CreateHeaderLabel("Size", 80));
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

            root.schedule.Execute(() =>
            {
                if (isProcessing)
                {
                    progressBar.value = currentProgress * 100f;
                    progressBar.title = $"Scanning: {currentProcessingFile}";
                }
            }).Every(50);
        }

        private VisualElement MakeListItem()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.paddingLeft = 5;

            var lblName = new Label();
            lblName.name = "NameLabel";
            lblName.style.width = 250;
            lblName.style.overflow = Overflow.Hidden;

            var lblLines = new Label();
            lblName.name = "LinesLabel";
            lblLines.style.width = 80;

            var lblSize = new Label();
            lblName.name = "SizeLabel";
            lblSize.style.width = 80;

            container.Add(lblName);
            container.Add(lblLines);
            container.Add(lblSize);

            return container;
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (index >= scriptList.Count) return;

            ScriptInfo info = scriptList[index];
            Label lblName = element.Q<Label>(null, "NameLabel");
            Label lblLines = element.Q<Label>(null, "LinesLabel");
            Label lblSize = element.Q<Label>(null, "SizeLabel");

            lblName = element.ElementAt(0) as Label;
            lblLines = element.ElementAt(1) as Label;
            lblSize = element.ElementAt(2) as Label;

            lblName.text = info.Name;
            lblLines.text = info.LineCount.ToString("N0");
            lblSize.text = FormatBytes(info.SizeBytes);

            if (info.LineCount > 1000)
            {
                lblName.style.color = new Color(1f, 0.4f, 0.4f);
                lblLines.style.color = new Color(1f, 0.4f, 0.4f);
            }
            else if (info.LineCount > 500)
            {
                lblName.style.color = new Color(1f, 0.7f, 0.2f);
                lblLines.style.color = new Color(1f, 0.7f, 0.2f);
            }
            else
            {
                lblName.style.color = Color.white;
                lblLines.style.color = Color.white;
            }

            lblSize.style.color = Color.gray;
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            var item = selectedItems.FirstOrDefault();
            if (item is ScriptInfo info)
            {
                PingScript(info.Path);
            }
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

        private Label CreateHeaderLabel(string text, float width)
        {
            var l = new Label(text);
            l.style.width = width;
            l.style.unityFontStyleAndWeight = FontStyle.Bold;
            return l;
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

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" });
            List<string> filePaths = new();

            foreach (var guid in scriptGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".cs")) filePaths.Add(path);
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

                    if (File.Exists(path))
                    {
                        int lines = 0;
                        long size = new FileInfo(path).Length;

                        using (StreamReader sr = new(path))
                        {
                            while (sr.ReadLine() != null) lines++;
                        }

                        results.Add(new ScriptInfo
                        {
                            Name = Path.GetFileName(path),
                            LineCount = lines,
                            SizeBytes = size,
                            Path = path
                        });
                    }
                }
                return results;
            });

            scriptList = resultData.OrderByDescending(x => x.LineCount).ToList();

            long totalLines = scriptList.Sum(x => x.LineCount);
            long totalSize = scriptList.Sum(x => x.SizeBytes);
            int totalScripts = scriptList.Count;

            lblTotalScripts.text = totalScripts.ToString("N0");
            lblTotalLines.text = totalLines.ToString("N0");
            lblTotalSize.text = FormatBytes(totalSize);

            float avgL = totalScripts > 0 ? (float)totalLines / totalScripts : 0;
            lblAvgLines.text = avgL.ToString("F1");
            lblAvgSize.text = FormatBytes(totalSize / (totalScripts > 0 ? totalScripts : 1));

            if (scriptList.Count > 0)
            {
                ScriptInfo biggest = scriptList[0];
                ScriptInfo smallest = scriptList[^1];

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
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
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