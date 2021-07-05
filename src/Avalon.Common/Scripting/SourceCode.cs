/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Cysharp.Text;

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// Represents an instance of source code.
    /// </summary>
    public class SourceCode
    {
        public SourceCode(string code, string functionName, ScriptType scriptType)
        {
            this.Code = code;
            this.FunctionName = functionName;
            this.ScriptType = scriptType;
        }

        public SourceCode(string code)
        {
            this.Code = code;
        }

        public SourceCode()
        {
            
        }

        private string _code;

        /// <summary>
        /// The source code as provided by the user.
        /// </summary>
        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                this.Md5Hash = Argus.Cryptography.HashUtilities.MD5Hash(_code);
            }
        }

        /// <summary>
        /// Wraps the <see cref="Code"/> as a function accepting varargs.
        /// </summary>
        /// <remarks>
        /// This only supports Lua now but it can be expanded.
        /// </remarks>
        public string AsFunctionString
        {
            get
            {
                using var sb = ZString.CreateStringBuilder();
                sb.AppendFormat("function {0}(...)\n", this.FunctionName);
                sb.Append(this.Code);
                sb.Append("\nend");

                return sb.ToString();
            }
        }

        /// <summary>
        /// The MD5 hash for the <see cref="Code"/> property.
        /// </summary>
        public string Md5Hash { get; set; }

        /// <summary>
        /// The name of the function used for calling.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// The language for the script.
        /// </summary>
        public ScriptType ScriptType { get; set; }
    }
}
