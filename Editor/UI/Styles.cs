using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityNotebook
{
    public static class Styles
    {
        public static GUIStyle TextStyle;
        public static GUIStyle TextNoBackgroundStyle;
        public static GUIStyle CodeStyle;
        public static GUIStyle CodeNoBackgroundStyle;
        public static GUIStyle CellBoxStyle;
        public static GUIStyle CellBoxSelectedStyle;
        public static GUIStyle CellBoxSelectedEditStyle;
        public static GUIStyle CodeCellBoxStyle;
        public static GUIStyle CodeCellBoxSelectedStyle;
        public static GUIStyle CodeCellBoxSelectedEditStyle;
        
        private static string _packagePath;
        private static bool _initialized;
        
        public static void Init()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;
            
            if (string.IsNullOrEmpty(_packagePath))
            {
                _packagePath = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly()).assetPath;
            }
            
            if (TextStyle == null)
            {
                TextStyle = new GUIStyle(EditorStyles.textField)
                {
                    richText = true,
                    wordWrap = true,
                    stretchHeight = false,
                    stretchWidth = true,
                    padding = new RectOffset(4, 4, 4, 4),
                };
            }

            if (TextNoBackgroundStyle == null)
            {
                TextNoBackgroundStyle = new GUIStyle()
                {
                    fontStyle = TextStyle.fontStyle,
                    fontSize = TextStyle.fontSize,
                    normal = TextStyle.normal,
                    active = TextStyle.active,
                    focused = TextStyle.focused,
                    hover = TextStyle.hover,
                    padding = TextStyle.padding,
                    margin = TextStyle.margin,
                    wordWrap = TextStyle.wordWrap,
                    clipping = TextStyle.clipping,
                    stretchHeight = TextStyle.stretchHeight,
                    stretchWidth = TextStyle.stretchWidth,
                    font = TextStyle.font,
                    richText = true
                };
            }

            if (CodeStyle == null)
            {
                var fontAsset = AssetDatabase.LoadAssetAtPath<Font>($"{_packagePath}/Assets/Menlo-Regular.ttf");
                if (fontAsset == null)
                {
                    Debug.LogError("Failed to load code editor font");
                }

                CodeStyle = new GUIStyle(GUI.skin.textArea)
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    wordWrap = false,
                    clipping = TextClipping.Clip,
                    stretchHeight = false,
                    stretchWidth = true,
                    font = fontAsset,
                    richText = true
                };
            }

            if (CodeNoBackgroundStyle == null)
            {
                CodeNoBackgroundStyle = new GUIStyle()
                {
                    fontStyle = CodeStyle.fontStyle,
                    fontSize = CodeStyle.fontSize,
                    normal = CodeStyle.normal,
                    active = CodeStyle.active,
                    focused = CodeStyle.focused,
                    hover = CodeStyle.hover,
                    padding = CodeStyle.padding,
                    margin = CodeStyle.margin,
                    wordWrap = CodeStyle.wordWrap,
                    clipping = CodeStyle.clipping,
                    stretchHeight = CodeStyle.stretchHeight,
                    stretchWidth = CodeStyle.stretchWidth,
                    font = CodeStyle.font,
                    richText = true
                };
            }
            
            if (CellBoxStyle == null)
            {
                CellBoxStyle = new GUIStyle("box")
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    margin = new RectOffset()
                };
            }
            
            Texture2D BuildTexture(Color color)
            {
                const int width = 64;
                const int height = 64;
                var pixels = new Color[width * height];
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = color;
                }
                var result = new Texture2D(width, height);
                result.SetPixels(pixels);
                result.Apply();
                return result;
            }

            if (CellBoxSelectedStyle == null)
            {
                CellBoxSelectedStyle = new GUIStyle(CellBoxStyle)
                {
                    normal = new GUIStyleState()
                    {
                        background = BuildTexture(new Color(0.23f, 0.29f, 0.37f)),
                    }
                };
            }

            if (CellBoxSelectedEditStyle == null)
            {
                CellBoxSelectedEditStyle = new GUIStyle(CellBoxStyle)
                {
                    normal = new GUIStyleState()
                    {
                        background = BuildTexture(new Color(0.21f, 0.33f, 0.26f)),
                    }
                };
            }
            
            if (CodeCellBoxStyle == null)
            {
                CodeCellBoxStyle = new GUIStyle(CellBoxStyle)
                {
                    padding = new RectOffset(8, 8, 8, 1)
                };
            }
            if (CodeCellBoxSelectedStyle == null)
            {
                CodeCellBoxSelectedStyle = new GUIStyle(CellBoxSelectedStyle)
                {
                    padding = new RectOffset(8, 8, 8, 1)
                };
            }

            if (CodeCellBoxSelectedEditStyle == null)
            {
                CodeCellBoxSelectedEditStyle = new GUIStyle(CellBoxSelectedEditStyle)
                {
                    padding = new RectOffset(8, 8, 8, 1)
                };
            }
        }
    }
}