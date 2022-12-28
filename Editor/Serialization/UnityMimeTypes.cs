using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityNotebook
{
    public static class UnityMimeTypes
    {
        private const string UnityObjectPreview = "application/vnd.unity3d.objectpreview+json";
        private const string AnimationCurve = "application/vnd.unity3d.animationcurve+json";
        private const string Color = "application/vnd.unity3d.color+json";
        private const string Gradient = "application/vnd.unity3d.gradient+json";
        private const string Matrix4x4 = "application/vnd.unity3d.matrix4x4+json";
        private const string Quaternion = "application/vnd.unity3d.quaternion+json";
        private const string Rect = "application/vnd.unity3d.rect+json";
        private const string Bounds = "application/vnd.unity3d.bounds+json";
        private const string Vector2 = "application/vnd.unity3d.vector2+json";
        private const string Vector3 = "application/vnd.unity3d.vector3+json";
        private const string Vector4 = "application/vnd.unity3d.vector4+json";
        private const string Transform = "application/vnd.unity3d.transform+json";
        private const string GameObject = "application/vnd.unity3d.gameobject+json";
        private const string Mesh = "application/vnd.unity3d.mesh+json";
        private const string Texture2D = "application/vnd.unity3d.texture2d+json";
        private const string Material = "application/vnd.unity3d.material+json";
        
        private static readonly List<(Type type, string mimeType)> TypeToMimeType = new()
        {
            (typeof(UnityObjectPreview), UnityObjectPreview),
            (typeof(AnimationCurve), AnimationCurve),
            (typeof(Color), Color),
            (typeof(Gradient), Gradient),
            (typeof(Matrix4x4), Matrix4x4),
            (typeof(Quaternion), Quaternion),
            (typeof(Rect), Rect),
            (typeof(Bounds), Bounds),
            (typeof(Vector2), Vector2),
            (typeof(Vector3), Vector3),
            (typeof(Vector4), Vector4),
            (typeof(Transform), Transform),
            (typeof(GameObject), GameObject),
            (typeof(Mesh), Mesh),
            (typeof(Texture2D), Texture2D),
            (typeof(Material), Material),
        };

        public static string GetMimeType(Type type)
        {
            foreach (var (t, mimeType) in TypeToMimeType)
            {
                if (t == type)
                {
                    return mimeType;
                }
            }
            return null;
        }
        
        public static Type GetType(string mimeType)
        {
            foreach (var (type, m) in TypeToMimeType)
            {
                if (m == mimeType)
                {
                    return type;
                }
            }
            return null;
        }
    }
}