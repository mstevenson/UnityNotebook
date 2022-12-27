using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityNotebook
{
    public static class Renderers
    {
        private static readonly List<OutputRendererBase> OutputRenderers = new();
        private static readonly TextRenderer TextRenderer = new();

        private static void Init()
        {
            if (OutputRenderers.Count != 0)
            {
                return;
            }
            var rendererTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(OutputRendererBase)));
            foreach (var type in rendererTypes)
            {
                OutputRenderers.Add((OutputRendererBase)System.Activator.CreateInstance(type));
            }
        }

        public static OutputRendererBase GetRendererForType(Type type)
        {
            Init();
            foreach (var renderer in OutputRenderers)
            {
                foreach (var supportedType in renderer.SupportedTypes)
                {
                    if (type == supportedType)
                    {
                        return renderer;
                    }
                }
            }
            // fallback to support any type by displaying ToString()
            return TextRenderer;
        }
    }
}