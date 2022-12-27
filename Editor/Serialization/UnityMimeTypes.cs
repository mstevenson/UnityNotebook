using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityNotebook
{
    public static class UnityMimeTypes
    {
        public const string AnimationCurve = "application/vnd.unity3d.animationcurve+json";
        public const string Color = "application/vnd.unity3d.color+json";
        public const string Gradient = "application/vnd.unity3d.gradient+json";
        public const string Matrix4x4 = "application/vnd.unity3d.matrix4x4+json";
        public const string Quaternion = "application/vnd.unity3d.quaternion+json";
        public const string Rect = "application/vnd.unity3d.rect+json";
        public const string Bounds = "application/vnd.unity3d.bounds+json";
        public const string Vector2 = "application/vnd.unity3d.vector2+json";
        public const string Vector3 = "application/vnd.unity3d.vector3+json";
        public const string Vector4 = "application/vnd.unity3d.vector4+json";
        public const string Transform = "application/vnd.unity3d.transform+json";
        public const string GameObject = "application/vnd.unity3d.gameobject+json";
        public const string Mesh = "application/vnd.unity3d.mesh+json";
        public const string Texture2D = "application/vnd.unity3d.texture2d+json";
        public const string Material = "application/vnd.unity3d.material+json";
        
        private static readonly Dictionary<Type, string> TypeToMimeType = new()
        {
            {typeof(AnimationCurve), AnimationCurve},
            {typeof(Color), Color},
            {typeof(Gradient), Gradient},
            {typeof(Matrix4x4), Matrix4x4},
            {typeof(Quaternion), Quaternion},
            {typeof(Rect), Rect},
            {typeof(Bounds), Bounds},
            {typeof(Vector2), Vector2},
            {typeof(Vector3), Vector3},
            {typeof(Vector4), Vector4},
            {typeof(Transform), Transform},
            {typeof(GameObject), GameObject},
            {typeof(Mesh), Mesh},
            {typeof(Texture2D), Texture2D},
            {typeof(Material), Material},
        };

        public static string GetMimeType(Type type)
        {
            return TypeToMimeType[type];
        }
        
        public static bool HasMimeType(string entryMimeType)
        {
            foreach (var (_, value) in TypeToMimeType)
            {
                if (value == entryMimeType)
                {
                    return true;
                }
            }
            return false;
        }
        
        public static bool HasType(Type type)
        {
            return TypeToMimeType.ContainsKey(type);
        }
        
        public static Type GetType(string mimeType)
        {
            foreach (var (key, value) in TypeToMimeType)
            {
                if (value == mimeType)
                {
                    return key;
                }
            }
            return null;
        }
    }
}