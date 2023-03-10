using System;
using UnityEngine;
using Object = UnityEngine.Object;
 
namespace UnityNotebook
{
    // https://forum.unity.com/threads/serializereference-primitive-types.968293/
    [Serializable]
    public struct ValueWrapper
    {
        private interface IValue
        {
            object Object { get; }
        }
        
        private class Ref<T> : IValue
        {
            public T Value;
            public object Object => Value;
        }
        
        private class Int8Value : Ref<sbyte> { }
        private class Int16Value : Ref<short> { }
        private class Int32Value : Ref<int> { }
        private class Int64Value : Ref<long> { }
        private class UInt8Value : Ref<byte> { }
        private class UInt16Value : Ref<ushort> { }
        private class UInt32Value : Ref<uint> { }
        private class UInt64Value : Ref<ulong> { }
        private class FloatValue : Ref<float> { }
        private class DoubleValue : Ref<double> { }
        private class BoolValue : Ref<bool> { }
        private class CharValue : Ref<char> { }
        private class StringValue : Ref<string> { }
        private class AnimationCurveValue : Ref<AnimationCurve> { }
        private class UnityObjectValue : Ref<Object> { }

        [SerializeReference]
        private object _serialized;
        
        public object Object => _serialized is IValue v ? v.Object : _serialized;

        public ValueWrapper(object obj)
        {
            _serialized = obj switch
            {
                Object unityObj => new UnityObjectValue {Value = unityObj},
                string str => new StringValue {Value = str},
                _ when obj != null && obj.GetType().IsPrimitive => obj switch
                {
                    sbyte v => new Int8Value {Value = v},
                    short v => new Int16Value {Value = v},
                    int v => new Int32Value {Value = v},
                    long v => new Int64Value {Value = v},
                    byte v => new UInt8Value {Value = v},
                    ushort v => new UInt16Value {Value = v},
                    uint v => new UInt32Value {Value = v},
                    ulong v => new UInt64Value {Value = v},
                    float v => new FloatValue {Value = v},
                    double v => new DoubleValue {Value = v},
                    bool v => new BoolValue {Value = v},
                    char v => new CharValue {Value = v},
                    AnimationCurve v => new AnimationCurveValue {Value = v},
                    _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
                },
                _ => obj
            };
        }
    }
}

