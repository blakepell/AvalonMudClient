// Disable warnings about XML documentation

#pragma warning disable 1591

using lua_Integer = System.Int32;

namespace MoonSharp.Interpreter.Interop.LuaStateInterop
{
    public partial class LuaBase
    {
        protected static int memcmp(CharPtr ptr1, CharPtr ptr2, uint size)
        {
            return memcmp(ptr1, ptr2, (int) size);
        }

        protected static int memcmp(CharPtr ptr1, CharPtr ptr2, int size)
        {
            for (int i = 0; i < size; i++)
            {
                if (ptr1[i] != ptr2[i])
                {
                    if (ptr1[i] < ptr2[i])
                    {
                        return -1;
                    }

                    return 1;
                }
            }

            return 0;
        }

        protected static CharPtr memchr(CharPtr ptr, char c, uint count)
        {
            for (uint i = 0; i < count; i++)
            {
                if (ptr[i] == c)
                {
                    return new CharPtr(ptr.chars, (int) (ptr.index + i));
                }
            }

            return null;
        }

        protected static CharPtr strpbrk(CharPtr str, CharPtr charset)
        {
            for (int i = 0; str[i] != '\0'; i++)
            for (int j = 0; charset[j] != '\0'; j++)
            {
                if (str[i] == charset[j])
                {
                    return new CharPtr(str.chars, str.index + i);
                }
            }

            return null;
        }

        protected static bool isalpha(char c)
        {
            return char.IsLetter(c);
        }

        protected static bool iscntrl(char c)
        {
            return char.IsControl(c);
        }

        protected static bool isdigit(char c)
        {
            return char.IsDigit(c);
        }

        protected static bool islower(char c)
        {
            return char.IsLower(c);
        }

        protected static bool ispunct(char c)
        {
            return char.IsPunctuation(c);
        }

        protected static bool isspace(char c)
        {
            return (c == ' ') || (c >= (char) 0x09 && c <= (char) 0x0D);
        }

        protected static bool isupper(char c)
        {
            return char.IsUpper(c);
        }

        protected static bool isalnum(char c)
        {
            return char.IsLetterOrDigit(c);
        }

        protected static bool isxdigit(char c)
        {
            return "0123456789ABCDEFabcdef".IndexOf(c) >= 0;
        }

        protected static bool isgraph(char c)
        {
            return !char.IsControl(c) && !char.IsWhiteSpace(c);
        }

        protected static bool isalpha(int c)
        {
            return char.IsLetter((char) c);
        }

        protected static bool iscntrl(int c)
        {
            return char.IsControl((char) c);
        }

        protected static bool isdigit(int c)
        {
            return char.IsDigit((char) c);
        }

        protected static bool islower(int c)
        {
            return char.IsLower((char) c);
        }

        protected static bool ispunct(int c)
        {
            return ((char) c != ' ') && !isalnum((char) c);
        } // *not* the same as Char.IsPunctuation

        protected static bool isspace(int c)
        {
            return ((char) c == ' ') || ((char) c >= (char) 0x09 && (char) c <= (char) 0x0D);
        }

        protected static bool isupper(int c)
        {
            return char.IsUpper((char) c);
        }

        protected static bool isalnum(int c)
        {
            return char.IsLetterOrDigit((char) c);
        }

        protected static bool isgraph(int c)
        {
            return !char.IsControl((char) c) && !char.IsWhiteSpace((char) c);
        }

        protected static char tolower(char c)
        {
            return char.ToLower(c);
        }

        protected static char toupper(char c)
        {
            return char.ToUpper(c);
        }

        protected static char tolower(int c)
        {
            return char.ToLower((char) c);
        }

        protected static char toupper(int c)
        {
            return char.ToUpper((char) c);
        }


        // find c in str
        protected static CharPtr strchr(CharPtr str, char c)
        {
            for (int index = str.index; str.chars[index] != 0; index++)
            {
                if (str.chars[index] == c)
                {
                    return new CharPtr(str.chars, index);
                }
            }

            return null;
        }

        protected static CharPtr strcpy(CharPtr dst, CharPtr src)
        {
            int i;
            for (i = 0; src[i] != '\0'; i++)
            {
                dst[i] = src[i];
            }

            dst[i] = '\0';
            return dst;
        }

        protected static CharPtr strncpy(CharPtr dst, CharPtr src, int length)
        {
            int index = 0;
            while ((src[index] != '\0') && (index < length))
            {
                dst[index] = src[index];
                index++;
            }

            while (index < length)
            {
                dst[index++] = '\0';
            }

            return dst;
        }

        protected static int strlen(CharPtr str)
        {
            int index = 0;
            while (str[index] != '\0')
            {
                index++;
            }

            return index;
        }

        public static void sprintf(CharPtr buffer, CharPtr str, params object[] argv)
        {
            string temp = Tools.sprintf(str.ToString(), argv);
            strcpy(buffer, temp);
        }
    }
}