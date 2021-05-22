/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Scripting
{
    public class SourceCode
    {
        public SourceCode(string code)
        {
            this.Code = code;
        }

        public SourceCode()
        {
            
        }

        private string _code;

        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                this.Md5Hash = Argus.Cryptography.HashUtilities.MD5Hash(_code);
            }
        }

        public string Md5Hash { get; set; }
    }
}
