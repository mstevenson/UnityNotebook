using UnityEngine;

namespace MG.MDV
{
    public class Context
    {
        public Context( GUISkin skin, HandlerImages images )
        {
            mStyleConverter = new StyleConverter( skin );
            mImages         = images;

            Apply( Style.Default );
        }

        StyleConverter      mStyleConverter;
        GUIStyle            mStyleGUI;
        HandlerImages       mImages;

        public Texture  FetchImage( string url )        { return mImages.FetchImage( url ); }

        public float    LineHeight => mStyleGUI.lineHeight;
        public float    MinWidth => LineHeight * 2.0f;
        public float    IndentSize => LineHeight * 1.5f;

        public void     Reset() => Apply( Style.Default );
        public GUIStyle Apply( Style style ) { mStyleGUI = mStyleConverter.Apply( style ); return mStyleGUI; }
        public Vector2  CalcSize( GUIContent content ) => mStyleGUI.CalcSize( content );
    }
}
