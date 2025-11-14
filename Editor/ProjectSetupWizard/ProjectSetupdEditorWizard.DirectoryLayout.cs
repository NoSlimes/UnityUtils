using System.Collections.Generic;

namespace NoSlimes.Utils.Editor.ProjectSetupWizard
{
    public partial class ProjectSetupEditorWizard
    {
        public class DirectoryNode
        {
            public string Name { get; set; }
            public List<DirectoryNode> Children { get; set; } = new();

            public DirectoryNode(string name)
            {
                Name = name;
            }
        }

        private readonly DirectoryNode rootNode = new("Assets")
        {
            Children =
            {
                new("_Game")
                {
                    Children =
                    {
                        new("Scripts")
                        {
                            Children =
                            {
                                new("Runtime")
                                {
                                    Children =
                                    {
                                        new("Managers"),
                                        new("Controllers"),
                                        new("Systems"),
                                        new("Utilities")
                                    }

                                },
                                new("Editor"),
                                new("Compatability"),
                                new("Utils")
                            }
                        },
                        new("Art")
                        {
                            Children =
                            {
                                new("UI"),
                                new("2D"),
                                new("3D"),
                                new("Animations")
                            }
                        },
                        new("Audio")
                        {
                            Children =
                            {
                                new("Music"),
                                new("SFX")
                            }
                        },
                        new("Scenes"),
                        new("Materials"),
                        new("Prefabs")
                        {
                            Children =
                            {
                                new("UI"),
                                new("Player")
                            }
                        }
                    }
                },
                new("Settings"),
                new("Tools"),
                new("Plugins")
            }
        };
    }
}