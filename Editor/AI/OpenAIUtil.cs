// https://github.com/keijiro/AICommand

using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace AICommand {
    internal static class OpenAIUtil
    {
        public const string ApiKeyErrorText = "API Key hasn't been set. Please check the project settings (Edit > Project Settings > AI Command > API Key).";
        
        public static bool ValidateApiKey => !string.IsNullOrEmpty(OpenAICommandSettings.instance.apiKey);
        
        public static string SanitizeCode(string result)
        {
            result = Regex.Replace(result, @"^```.*$", "", RegexOptions.Multiline);
            result = Regex.Replace(result, @"^\s*$\n|\r", "", RegexOptions.Multiline);
            return result;
        }
        
        private static string CreateChatRequestBody(string systemPrompt, string userPrompt)
        {
            var system = new OpenAI.RequestMessage
            {
                role = "system",
                content = systemPrompt
            };
            
            var msg = new OpenAI.RequestMessage
            {
                role = "user",
                content = userPrompt
            };

            var req = new OpenAI.Request
            {
                model = OpenAICommandSettings.instance.api,
                messages = new [] { system, msg }
            };

            return JsonUtility.ToJson(req);
        }

        public static string InvokeChat(string systemPrompt, string userPrompt)
        {
            var settings = OpenAICommandSettings.instance;

            // POST
            const string url = "https://api.openai.com/v1/chat/completions";
            var body = CreateChatRequestBody(systemPrompt, userPrompt);
            using var post = UnityWebRequest.Put(url, body);
            post.method = "POST"; // HACK in Unity 2021, UnityWebRequest.Post doesn't include the body. 
            post.SetRequestHeader("Content-Type", "application/json");
            post.SetRequestHeader("Authorization", "Bearer " + settings.apiKey);
            
            post.timeout = settings.timeout;
            
            // Debug.Log(body);

            // Request start
            var req = post.SendWebRequest();
            
            // Progress bar (Totally fake! Don't try this at home.)
            for (var progress = 0.0f; !req.isDone; progress += 0.01f)
            {
                EditorUtility.DisplayProgressBar("AI Command", "Generating...", progress);
                System.Threading.Thread.Sleep(100);
                progress += 0.01f;
            }
            EditorUtility.ClearProgressBar();

            // Response extraction
            var json = post.downloadHandler.text;
            var data = JsonUtility.FromJson<OpenAI.Response>(json);
            // Debug.Log(json);
            return data.choices[0].message.content;
        }
    }

} // namespace AICommand