using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Networking.UnityWebRequest.Result;

namespace MG.MDV
{
    public class HandlerImages
    {
        public string  CurrentPath;

        Texture                     mPlaceholder;
        List<ImageRequest>          mActiveRequests   = new();
        Dictionary<string,Texture>  mTextureCache     = new();

        private class ImageRequest
        {
            public string           URL; // original url
            public UnityWebRequest  Request;

            public ImageRequest( string url )
            {
                URL = url;
                Request = UnityWebRequestTexture.GetTexture( url );
                Request.SendWebRequest();
            }

            public Texture GetTexture()
            {
                return Request.downloadHandler is DownloadHandlerTexture handler ? handler.texture : null;
            }
        }

        private string RemapURL( string url )
        {
            if( Regex.IsMatch( url, @"^\w+:", RegexOptions.Singleline ) )
            {
                return url;
            }

            var projectDir = Path.GetDirectoryName( Application.dataPath );

            if( url.StartsWith( "/" ) )
            {
                return $"file:///{projectDir}{url}";
            }

            var assetDir = Path.GetDirectoryName( CurrentPath );
            return $"file:///{PathNormalise($"{projectDir}/{assetDir}/{url}")}";
        }
        
        private static readonly char[] Separators = { '/', '\\' };

        /// <summary>
        /// path combine with basic normalization (reduces '.' and '..' relative paths)
        /// </summary>
        public static string PathNormalise( string _a, string separator = "/" )
        {
            var a = (_a ?? "").Split( Separators, StringSplitOptions.RemoveEmptyEntries );

            var path = new List<string>();

            foreach( var el in a )
            {
                if( el == "." )
                {
                    continue;
                }
                if( el != ".." )
                {
                    path.Add( el );
                }
                else if( path.Count > 0 )
                {
                    path.RemoveAt( path.Count - 1 );
                }
            }

            return string.Join( separator, path.ToArray() );
        }

        public Texture FetchImage( string url )
        {
            url = RemapURL( url );

            Texture tex;

            if( mTextureCache.TryGetValue( url, out tex ) )
            {
                return tex;
            }

            if( mPlaceholder == null )
            {
                var style = GUI.skin.GetStyle( "btnPlaceholder" );
                mPlaceholder = style != null ? style.normal.background : null;
            }

            mActiveRequests.Add( new ImageRequest( url ) );
            mTextureCache[ url ] = mPlaceholder;

            return mPlaceholder;
        }

        public bool Update()
        {
            // Update requests
            
            var req = mActiveRequests.Find( r => r.Request.isDone );

            if( req == null )
            {
                return false;
            }

            if( req.Request.result == ProtocolError )
            {
                Debug.LogError($"HTTP Error: {req.URL} - {req.Request.responseCode} {req.Request.error}");
                mTextureCache[ req.URL ] = null;
            }
            else if( req.Request.result == ConnectionError )
            {
                Debug.LogError($"Network Error: {req.URL} - {req.Request.error}");
                mTextureCache[ req.URL ] = null;
            }
            else
            {
                mTextureCache[ req.URL ] = req.GetTexture();
            }

            mActiveRequests.Remove( req );
            return true;
        }
    }
}
