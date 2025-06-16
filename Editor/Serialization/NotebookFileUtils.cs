using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace UnityNotebook
{
    public static class NotebookFileUtils
    {
        public static NotebookFormat GetFormatFromExtension(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return string.Equals(extension, ".dib") ? NotebookFormat.Dib : NotebookFormat.Ipynb;
        }
        
        public static string GetExtensionFromFormat(NotebookFormat format)
        {
            return format == NotebookFormat.Dib ? ".dib" : ".ipynb";
        }
        
        public static bool IsDibFile(string filePath)
        {
            return Path.GetExtension(filePath) == ".dib";
        }
        
        public static bool IsIpynbFile(string filePath)
        {
            return Path.GetExtension(filePath) == ".ipynb";
        }
        
        public static void WriteNotebookToFile(Notebook notebook, string filePath)
        {
            var format = GetFormatFromExtension(filePath);
            WriteNotebookToFile(notebook, filePath, format);
        }
        
        public static void WriteNotebookToFile(Notebook notebook, string filePath, NotebookFormat format)
        {
            if (format == NotebookFormat.Dib)
            {
                var dibContent = DibFormat.NotebookToDib(notebook);
                File.WriteAllText(filePath, dibContent);
            }
            else
            {
                var json = JsonConvert.SerializeObject(notebook, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
        }
        
        public static string GenerateUniqueNotebookPath(string directory, string baseName, NotebookFormat format)
        {
            var extension = GetExtensionFromFormat(format);
            var path = Path.Combine(directory, baseName + extension);
            return UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
        }
    }
}