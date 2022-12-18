using Editor;
using UnityEngine;

public class TextBlock
{
    public int LineCount { get; private set; }
    public int CharacterCount { get; private set; }
    private int[] _lineCharCounts = new int[1000];
    private char[] _buffer = new char[100000];
    
    private string[] _memoizedLines;
    private string _memoizedRawString;
    private string _memoizedHighlighting;
    
    public void SetText(params string[] strings)
    {
        if (strings == null)
        {
            return;
        }
        _memoizedLines = null;
        _memoizedRawString = null;
        _memoizedHighlighting = null;
        
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
    
    public string RawString()
    {
        if (_memoizedRawString != null)
        {
            return _memoizedRawString;
        }
        _memoizedRawString = new string(_buffer, 0, CharacterCount);
        return _memoizedRawString;
    }

    public string HighlightedString()
    {
        if (_memoizedHighlighting != null)
        {
            return _memoizedHighlighting;
        }
        _memoizedHighlighting = SyntaxHighlighting.SyntaxToHtml(RawString());
        return _memoizedHighlighting;
    }
}
