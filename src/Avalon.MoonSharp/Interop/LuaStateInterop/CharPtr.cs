#pragma warning disable 1591
//
// This part taken from KopiLua - https://github.com/NLua/KopiLua
//
// =========================================================================================================
//
// Kopi Lua License
// ----------------
// MIT License for KopiLua
// Copyright (c) 2012 LoDC
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial
// portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===============================================================================
// Lua License
// -----------
// Lua is licensed under the terms of the MIT license reproduced below.
// This means that Lua is free software and can be used for both academic
// and commercial purposes at absolutely no cost.
// For details and rationale, see http://www.lua.org/license.html .
// ===============================================================================
// Copyright (C) 1994-2008 Lua.org, PUC-Rio.
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;
using System.Text;
using Cysharp.Text;

namespace MoonSharp.Interpreter.Interop.LuaStateInterop
{
    public class CharPtr
    {
        public char[] chars;
        public int index;

        public CharPtr()
        {
            chars = null;
            index = 0;
        }

        public CharPtr(string str)
        {
            chars = (str + '\0').ToCharArray();
            index = 0;
        }

        public CharPtr(CharPtr ptr)
        {
            chars = ptr.chars;
            index = ptr.index;
        }

        public CharPtr(CharPtr ptr, int index)
        {
            chars = ptr.chars;
            this.index = index;
        }

        public CharPtr(char[] chars)
        {
            this.chars = chars;
            index = 0;
        }

        public CharPtr(char[] chars, int index)
        {
            this.chars = chars;
            this.index = index;
        }

        public CharPtr(byte[] bytes)
        {
            chars = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                chars[i] = (char) bytes[i];
            }

            index = 0;
        }

        public CharPtr(IntPtr ptr)
        {
            chars = new char[0];
            index = 0;
        }

        public char this[int offset]
        {
            get => chars[index + offset];
            set => chars[index + offset] = value;
        }

        public char this[uint offset]
        {
            get => chars[index + offset];
            set => chars[index + offset] = value;
        }

        public char this[long offset]
        {
            get => chars[index + (int) offset];
            set => chars[index + (int) offset] = value;
        }

        public static implicit operator CharPtr(string str)
        {
            return new CharPtr(str);
        }

        public static implicit operator CharPtr(char[] chars)
        {
            return new CharPtr(chars);
        }

        public static implicit operator CharPtr(byte[] bytes)
        {
            return new CharPtr(bytes);
        }

        public static CharPtr operator +(CharPtr ptr, int offset)
        {
            return new CharPtr(ptr.chars, ptr.index + offset);
        }

        public static CharPtr operator -(CharPtr ptr, int offset)
        {
            return new CharPtr(ptr.chars, ptr.index - offset);
        }

        public static CharPtr operator +(CharPtr ptr, uint offset)
        {
            return new CharPtr(ptr.chars, ptr.index + (int) offset);
        }

        public static CharPtr operator -(CharPtr ptr, uint offset)
        {
            return new CharPtr(ptr.chars, ptr.index - (int) offset);
        }

        public void inc()
        {
            index++;
        }

        public void dec()
        {
            index--;
        }

        public CharPtr next()
        {
            return new CharPtr(chars, index + 1);
        }

        public CharPtr prev()
        {
            return new CharPtr(chars, index - 1);
        }

        public CharPtr add(int ofs)
        {
            return new CharPtr(chars, index + ofs);
        }

        public CharPtr sub(int ofs)
        {
            return new CharPtr(chars, index - ofs);
        }

        public static bool operator ==(CharPtr ptr, char ch)
        {
            return ptr[0] == ch;
        }

        public static bool operator ==(char ch, CharPtr ptr)
        {
            return ptr[0] == ch;
        }

        public static bool operator !=(CharPtr ptr, char ch)
        {
            return ptr[0] != ch;
        }

        public static bool operator !=(char ch, CharPtr ptr)
        {
            return ptr[0] != ch;
        }

        public static CharPtr operator +(CharPtr ptr1, CharPtr ptr2)
        {
            string result = "";
            for (int i = 0; ptr1[i] != '\0'; i++)
            {
                result += ptr1[i];
            }

            for (int i = 0; ptr2[i] != '\0'; i++)
            {
                result += ptr2[i];
            }

            return new CharPtr(result);
        }

        public static int operator -(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars);
            return ptr1.index - ptr2.index;
        }

        public static bool operator <(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars);
            return ptr1.index < ptr2.index;
        }

        public static bool operator <=(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars);
            return ptr1.index <= ptr2.index;
        }

        public static bool operator >(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars);
            return ptr1.index > ptr2.index;
        }

        public static bool operator >=(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars);
            return ptr1.index >= ptr2.index;
        }

        public static bool operator ==(CharPtr ptr1, CharPtr ptr2)
        {
            object o1 = ptr1;
            object o2 = ptr2;
            if ((o1 == null) && (o2 == null))
            {
                return true;
            }

            if (o1 == null)
            {
                return false;
            }

            if (o2 == null)
            {
                return false;
            }

            return (ptr1.chars == ptr2.chars) && (ptr1.index == ptr2.index);
        }

        public static bool operator !=(CharPtr ptr1, CharPtr ptr2)
        {
            return !(ptr1 == ptr2);
        }

        public override bool Equals(object o)
        {
            return this == (o as CharPtr);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                for (int i = index; (i < chars.Length) && (chars[i] != '\0'); i++)
                {
                    sb.Append(chars[i]);
                }

                return sb.ToString();
            }
        }

        public string ToString(int length)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                for (int i = index; (i < chars.Length) && i < (length + index); i++)
                {
                    sb.Append(chars[i]);
                }

                return sb.ToString();
            }
        }
    }
}