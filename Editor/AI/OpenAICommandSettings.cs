// https://github.com/keijiro/AICommand

using UnityEditor;
using UnityEngine;

namespace AICommand
{
    [FilePath("UserSettings/NotebookOpenAISettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class OpenAICommandSettings : ScriptableSingleton<OpenAICommandSettings>
    {
        public string api = "gpt-3.5-turbo";
        public string apiKey;
        public int timeout = 30;
        public string systemPrompt = "Write a sequence of commands that can be executed by a C# REPL using the Unity Editor and Unity Engine APIs to accomplish the given task. Do not explain anything. Never add comments, and never use FindGameObjectsWithTag.";
        public void Save() => Save(true);
        void OnDisable() => Save();
    }

    sealed class OpenAICommandSettingsProvider : SettingsProvider
    {
        public OpenAICommandSettingsProvider() : base("Project/OpenAI Command", SettingsScope.Project) {}

        private GUIStyle _textAreaWrap;

        public override void OnGUI(string search)
        {
            if (_textAreaWrap == null)
            {
                _textAreaWrap = new GUIStyle(EditorStyles.textArea);
                _textAreaWrap.wordWrap = true;
            }
            
            var settings = OpenAICommandSettings.instance;

            var api = settings.api;
            var key = settings.apiKey;
            var timeout = settings.timeout;
            var prompt = settings.systemPrompt;

            EditorGUI.BeginChangeCheck();

            api = EditorGUILayout.TextField("API", api);
            key = EditorGUILayout.TextField("API Key", key);
            timeout = EditorGUILayout.IntField("Timeout", timeout);
            GUILayout.Label("System Prompt");
            prompt = EditorGUILayout.TextArea(prompt, _textAreaWrap);

            if (EditorGUI.EndChangeCheck())
            {
                settings.api = api;
                settings.apiKey = key;
                settings.timeout = timeout;
                settings.systemPrompt = prompt;
                settings.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider()
            => new OpenAICommandSettingsProvider();
    }
}