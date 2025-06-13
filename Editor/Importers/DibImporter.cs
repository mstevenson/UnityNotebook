using UnityEditor.AssetImporters;

namespace UnityNotebook
{
    // Parses .ipynb (IPython Notebook) asset files and imports them into Unity's AssetDatabase
    [ScriptedImporter(1, "dib")]
    public class DibImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var dibContent = System.IO.File.ReadAllText(ctx.assetPath);
            var notebook = DibFormat.ParseDibToNotebook(dibContent);
            ctx.AddObjectToAsset("main", notebook);
            ctx.SetMainObject(notebook);
        }
    }
}