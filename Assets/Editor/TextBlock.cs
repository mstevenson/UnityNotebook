public class TextBlock
{
    private int _lineCount;
    private static readonly int[] LineCharCounts = new int[1000];
    private static readonly char[] Buffer = new char[100000];
    
    public void SetText(params string[] strings)
    {
        _lineCount = 0;
        uint bufferIndex = 0;
        uint lineIndex = 0;
        ushort currentLineCharCount = 0;
        foreach (var str in strings)
        {
            foreach (var c in str)
            {
                if (c == '\r') // normalize line endings to LF
                {
                    continue;
                }
                bufferIndex++;
                Buffer[bufferIndex] = c;
                currentLineCharCount++;
                if (c == '\n')
                {
                    _lineCount++;
                    LineCharCounts[lineIndex] = currentLineCharCount;
                    currentLineCharCount = 0;
                    lineIndex++;
                }
            }
        }
    }

    public string[] GetLines()
    {
        var lines = new string[_lineCount];
        var bufferIndex = 0;
        for (var lineIndex = 0; lineIndex < _lineCount; lineIndex++)
        {
            var lineCharCount = LineCharCounts[lineIndex];
            var line = new string(Buffer, bufferIndex, lineCharCount);
            lines[lineIndex] = line;
            bufferIndex += lineCharCount;
        }
        return lines;
    }
}
