using JetBrains.Annotations;
using UnityEngine;

namespace UnityNotebook
{
    // Static methods that are available to a running notebook cell
    [UsedImplicitly]
    public static class RuntimeMethods
    {
        public static void Show(object data)
        {
            if (data == null)
            {
                return;
            }
            var notebook = NBState.OpenedNotebook;
            var cell = NBState.RunningCell;
            var output = new CellOutputDisplayData();
            if (data is Object obj)
            {
                var preview = UnityObjectPreview.Create(obj);
                output.values.Add(new ValueWrapper(preview));
            }
            else
            {
                output.values.Add(new ValueWrapper(data));
            }
            notebook.cells[cell].outputs.Add(output);
        }
    }
}