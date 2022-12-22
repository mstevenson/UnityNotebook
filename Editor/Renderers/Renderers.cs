using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityNotebook
{
    public static class Renderers
    {
        private static List<OutputRendererBase> _renderers = new();
        private static readonly Dictionary<string, List<OutputRendererBase>> _renderersByMimeType = new();

        private static void Init()
        {
            if (_renderers.Count != 0)
            {
                return;
            }
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(OutputRendererBase)));
            _renderers = types.Select(t => (OutputRendererBase)System.Activator.CreateInstance(t)).ToList();
            
            // Iterate over the string array MimeTypes in each OutputRenderer type and add the types to the dictionary
            foreach (var renderer in _renderers)
            {
                foreach (var mimeType in renderer.MimeTypes)
                {
                    if (!_renderersByMimeType.ContainsKey(mimeType))
                    {
                        _renderersByMimeType.Add(mimeType, new List<OutputRendererBase>());
                    }
                    _renderersByMimeType[mimeType].Add(renderer);
                }
            }
        }

        public static OutputRendererBase GetRendererForMimeType(string mimeType)
        {
            Init();
            if (_renderersByMimeType.TryGetValue(mimeType, out var renderers))
            {
                return renderers[0];
            }
            return _renderersByMimeType["text/plain"][0];
        }

        public static Notebook.CellOutput GetCellOutputForObject(object obj)
        {
            Init();
            foreach (var renderer in _renderers)
            {
                if (renderer.SupportedTypes.Any(t => t.IsInstanceOfType(obj)))
                {
                    return renderer.ObjectToCellOutput(obj);
                }
            }
            // Fallback, supports any type via ToString()
            return _renderersByMimeType["text/plain"][0].ObjectToCellOutput(obj);
        }
    }
}