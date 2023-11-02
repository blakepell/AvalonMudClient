/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Hash Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "hash")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HashScriptCommands
    {
        /// <summary>
        /// Returns the MD5 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Returns the MD5 hash for the provided string.",
                               ParameterCount = 1)]
        public string MD5(string value)
        {
            return Argus.Cryptography.HashUtilities.MD5Hash(value);
        }

        /// <summary>
        /// Returns the SHA1 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        /// 
        [MoonSharpModuleMethod(Description = "Provides the SHA1 hash for the given string.",
                               ParameterCount = 1)]
        public string SHA1(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha1Hash(value);
        }

        /// <summary>
        /// Returns the SHA256 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Provides the SHA256 hash for the given string.",
                               ParameterCount = 1)]
        public string SHA256(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha256Hash(value);
        }

        /// <summary>
        /// Returns the SHA384 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Provides the SHA384 hash for the given string.",
                               ParameterCount = 1)]
        public string SHA384(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha384Hash(value);
        }

        /// <summary>
        /// Returns the SHA512 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Provides the SHA512 hash for the given string.",
                               ParameterCount = 1)]
        public string SHA512(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha512Hash(value);
        }

        /// <summary>
        /// Returns the CRC32 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Provides the CRC32 hash for the given string.",
                               ParameterCount = 1)]
        public uint CRC32(string value)
        {
            return Argus.Cryptography.HashUtilities.CRC32(value, Encoding.UTF8);
        }

        /// <summary>
        /// Encodes a base64 string.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Encodes a base64 value.",
                               ParameterCount = 1)]
        public string EncodeBase64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Decodes a base64 string.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Decodes a base64 value.",
                               ParameterCount = 1)]
        public string DecodeBase64(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        /// <summary>
        /// Encodes a value for use in a URL.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Encodes a value for use in a URL.",
                               ParameterCount = 1)]
        public string UrlEncode(string value)
        {
            return WebUtility.UrlEncode(value);
        }

        /// <summary>
        /// Decodes a value for use in a URL.
        /// </summary>
        /// <param name="value"></param>
        [MoonSharpModuleMethod(Description = "Decodes a value for use in a URL.",
                               ParameterCount = 1)]
        public string UrlDecode(string value)
        {
            return WebUtility.UrlDecode(value);
        }
    }
}