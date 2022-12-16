using UnityEngine;

namespace MG.MDV
{
    public struct Style
    {
        public static readonly Style Default = default;

        private const int FlagBold      = 0x0100;
        private const int FlagItalic    = 0x0200;
        private const int FlagFixed     = 0x0400;
        private const int FlagLink      = 0x0800;
        private const int FlagBlock     = 0x1000;

        private const int MaskSize      = 0x000F;
        private const int MaskWeight    = 0x0300;

        private int mStyle;

        public static bool operator==( Style a, Style b ) => a.mStyle == b.mStyle;

        public static bool operator!=( Style a, Style b ) => a.mStyle != b.mStyle;

        public override bool Equals( object a ) => a is Style style && style.mStyle == mStyle;

        public override int GetHashCode() => mStyle.GetHashCode();

        public void Clear()
        {
            mStyle = 0x0000;
        }

        public bool Bold
        {
            get => ( mStyle & FlagBold ) != 0x0000;
            set { if( value ) mStyle |= FlagBold; else mStyle &= ~FlagBold; }
        }

        public bool Italic
        {
            get => ( mStyle & FlagItalic ) != 0x0000;
            set { if( value ) mStyle |= FlagItalic; else mStyle &= ~FlagItalic; }
        }

        public bool Fixed
        {
            get => ( mStyle & FlagFixed ) != 0x0000;
            set { if( value ) mStyle |= FlagFixed; else mStyle &= ~FlagFixed; }
        }

        public bool Link
        {
            get => ( mStyle & FlagLink ) != 0x0000;
            set { if( value ) mStyle |= FlagLink; else mStyle &= ~FlagLink; }
        }

        public bool Block
        {
            get => ( mStyle & FlagBlock ) != 0x0000;
            set { if( value ) mStyle |= FlagBlock; else mStyle &= ~FlagBlock; }
        }

        public int Size
        {
            get => mStyle & MaskSize;
            set => mStyle = ( mStyle & ~MaskSize ) | Mathf.Clamp( value, 0, 6 );
        }

        public FontStyle GetFontStyle()
        {
            return (mStyle & MaskWeight) switch
            {
                FlagBold => FontStyle.Bold,
                FlagItalic => FontStyle.Italic,
                FlagBold | FlagItalic => FontStyle.BoldAndItalic,
                _ => FontStyle.Normal
            };
        }
    }
}

