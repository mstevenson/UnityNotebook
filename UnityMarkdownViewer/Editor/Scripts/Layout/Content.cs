using System.Text.RegularExpressions;
using UnityEngine;

namespace MG.MDV
{
    public abstract class Content
    {
        public Rect         Location;
        public Style        Style;
        public GUIContent   Payload;
        public string       Link;

        public float Width => Location.width;
        public float Height => Location.height;

        protected Content( GUIContent payload, Style style, string link )
        {
            Payload = payload;
            Style = style;
            Link = link;
        }

        public void CalcSize( Context context )
        {
            Location.size = context.CalcSize( Payload );
        }

        public void Draw( Context context )
        {
            if( string.IsNullOrEmpty( Link ) )
            {
                GUI.Label( Location, Payload, context.Apply( Style ) );
            }
            else if( GUI.Button( Location, Payload, context.Apply( Style ) ) )
            {
                if( Regex.IsMatch( Link, @"^\w+:", RegexOptions.Singleline ) )
                {
                    Application.OpenURL( Link );
                }
            }
        }

        public virtual void Update( Context context )
        {
        }
    }
}

