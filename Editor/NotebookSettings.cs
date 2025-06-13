using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    public sealed class NotebookSettingsProvider : SettingsProvider
    {
        public NotebookSettingsProvider() : base("Project/Unity Notebook", SettingsScope.Project) {}

        public override void OnGUI(string search)
        {
            EditorGUILayout.HelpBox(
                "Choose the default file format for new notebooks:\n" +
                ".ipynb - Standard Jupyter notebook with execution outputs (better for presentation)\n" +
                ".dib - .NET Interactive notebook with only code (better for version control)",
                MessageType.Info);
            
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();

            var currentFormat = NBState.PreferredFormat;
            var newFormat = (NotebookFormat)EditorGUILayout.EnumPopup("Default File Format", currentFormat);

            if (EditorGUI.EndChangeCheck())
            {
                NBState.PreferredFormat = newFormat;
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateNotebookSettingsProvider() => new NotebookSettingsProvider();
    }
}