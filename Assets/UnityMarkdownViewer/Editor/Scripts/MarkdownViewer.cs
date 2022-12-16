using Markdig;
using UnityEditor;
using UnityEngine;
using Markdig.Extensions.Tables;

namespace MG.MDV
{
    public class MarkdownViewer
    {
        public static readonly Vector2 Margin = new( 6.0f, 4.0f );

        private GUISkin         mSkin;
        private string          mText;
        private string          mCurrentPath;
        private HandlerImages   mHandlerImages   = new();
        private Layout          mLayout;

        public MarkdownViewer( GUISkin skin, string path, string content )
        {
            mSkin        = skin;
            mCurrentPath = path;
            mText        = content;
            mLayout = ParseDocument();
            mHandlerImages.CurrentPath   = mCurrentPath;
        }

        public bool Update()
        {
            return mHandlerImages.Update();
        }

        private Layout ParseDocument()
        {
            var context  = new Context( mSkin, mHandlerImages );
            var builder  = new LayoutBuilder( context );
            var renderer = new RendererMarkdown( builder );

            var pipelineBuilder = new MarkdownPipelineBuilder().UseAutoLinks();
            pipelineBuilder.UsePipeTables(new PipeTableOptions { RequireHeaderSeparator = true });

            var pipeline = pipelineBuilder.Build();
            pipeline.Setup( renderer );

            var doc = Markdown.Parse( mText, pipeline );
            renderer.Render( doc );

            return builder.GetLayout();
        }

        private void ClearBackground( float height )
        {
            var rectFullScreen = new Rect( 0.0f, 0.0f, Screen.width, Mathf.Max( height, Screen.height ) );
            GUI.DrawTexture( rectFullScreen, mSkin.window.normal.background, ScaleMode.StretchToFill, false );
        }

        public void Draw()
        {
            GUI.skin    = mSkin;
            GUI.enabled = true;

            // useable width of inspector windows
            var contentWidth = EditorGUIUtility.currentViewWidth - mSkin.verticalScrollbar.fixedWidth - 2.0f * Margin.x;

            // draw content
            ClearBackground( mLayout.Height );
            DrawMarkdown( contentWidth );
        }

        private void DrawMarkdown( float width )
        {
            switch( Event.current.type )
            {
                case EventType.Ignore:
                    break;
                case EventType.ContextClick:
                    var menu = new GenericMenu();
                    menu.ShowAsContext();
                    break;
                case EventType.Layout:
                    GUILayout.Space( mLayout.Height );
                    mLayout.Arrange( width );
                    break;
                default:
                    mLayout.Draw();
                    break;
            }
        }
    }
}
