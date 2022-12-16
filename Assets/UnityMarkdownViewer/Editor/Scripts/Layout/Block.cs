using System;
using UnityEngine;

namespace MG.MDV
{
    public abstract class Block
    {
        public string ID;
        public Rect Rect;
        public Block Parent;
        public float Indent;

        public abstract void Arrange( Context context, Vector2 anchor, float maxWidth );
        public abstract void Draw( Context context );

        protected Block( float indent )
        {
            Indent = indent;
        }

    }
}
