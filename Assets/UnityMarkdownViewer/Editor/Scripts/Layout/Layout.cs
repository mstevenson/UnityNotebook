using UnityEngine;

namespace MG.MDV
{
    public class Layout
    {
        Context mContext;
        BlockContainer mDocument;

        public Layout( Context context, BlockContainer doc )
        {
            mContext  = context;
            mDocument = doc;
        }

        public float Height => mDocument.Rect.height;
        
        public void Arrange( float maxWidth, float posY )
        {
            mContext.Reset();
            mDocument.Arrange( mContext, MarkdownViewer.Margin + new Vector2(0, posY), maxWidth );
        }

        public void Draw()
        {
            mContext.Reset();
            mDocument.Draw( mContext );
        }
    }
}

