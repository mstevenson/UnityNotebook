using UnityEngine;

public class TextBlock
{
    public int LineCount { get; private set; }
    public int CharacterCount { get; private set; }
    private int[] _lineCharCounts = new int[1000];
    private char[] _buffer = new char[100000];
    
    public void SetText(params string[] strings)
    {
        _memoizedLines = null;
        _memoizedString = null;
        
        CharacterCount = 0;
        LineCount = 0;
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
                _buffer[bufferIndex] = c;
                CharacterCount++;
                currentLineCharCount++;
                if (c == '\n')
                {
                    LineCount++;
                    _lineCharCounts[lineIndex] = currentLineCharCount;
                    currentLineCharCount = 0;
                    lineIndex++;
                }
            }
        }
    }

    private string[] _memoizedLines;
    public string[] GetLines()
    {
        if (_memoizedLines != null)
        {
            return _memoizedLines;
        }
        _memoizedLines = new string[LineCount];
        var bufferIndex = 0;
        for (var lineIndex = 0; lineIndex < LineCount; lineIndex++)
        {
            var lineCharCount = _lineCharCounts[lineIndex];
            var line = new string(_buffer, bufferIndex, lineCharCount);
            _memoizedLines[lineIndex] = line;
            bufferIndex += lineCharCount;
        }
        return _memoizedLines;
    }

    private string _memoizedString;
    public override string ToString()
    {
        if (_memoizedString != null)
        {
            return _memoizedString;
        }
        _memoizedString = new string(_buffer, 0, CharacterCount);
        return _memoizedString;
    }
}
