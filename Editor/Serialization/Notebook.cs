using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UnityNotebook
{
    // Implementation of the Jupyter Notebook file format
    // https://nbformat.readthedocs.io/en/latest/format_description.html
    // https://github.com/jupyter/nbformat/blob/main/nbformat/v4/nbformat.v4.schema.json
    [JsonConverter(typeof(NotebookJsonConverter))]
    public class Notebook : ScriptableObject
    {
        public int format = 4;
        public int formatMinor = 2;
        public List<Cell> cells = new();
    }
}
