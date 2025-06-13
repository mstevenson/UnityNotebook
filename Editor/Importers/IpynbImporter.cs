using Newtonsoft.Json;
using UnityEditor.AssetImporters;

namespace UnityNotebook
{
    // Parses .ipynb (IPython Notebook) asset files and imports them into Unity's AssetDatabase
    [ScriptedImporter(1, "ipynb")]
    public class IpynbImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var json = System.IO.File.ReadAllText(ctx.assetPath);
            var notebook = JsonConvert.DeserializeObject<Notebook>(json);
            ctx.AddObjectToAsset("main", notebook);
            ctx.SetMainObject(notebook);
        }
    }
}