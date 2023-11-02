/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Hash Script Commands
    /// </summary>
    /// <remarks>
    /// TODO: Use a shared WebClient where the headers can be passed in.
    /// </remarks>
    [MoonSharpModule(Namespace = "http")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HttpScriptCommands
    {
        /// <summary>
        /// Downloads a string from a URL using the GET method.
        /// </summary>
        /// <param name="url"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the GET method.",
                               ParameterCount = 1)]
        public string Get(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the GET method.
        /// </summary>
        /// <param name="url"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the GET method.",
            ParameterCount = 3)]
        public string Get(string url, string headerKey, string headerValue)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(headerKey, headerValue);
                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the POST method.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the POST method.",
                               ParameterCount = 1)]
        public string Post(string url)
        {
            using (var client = new WebClient())
            {
                return client.UploadString(url, "");
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the POST method.  Data is a formatted string
        /// posted as a form in the format: "Time = 12:00am temperature = 50";
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the POST method.  Post form values in the format: LastName=Smith",
                               ParameterCount = 1)]
        [Description("Downloads a string from a URL using the POST method.  Post form values in the format: LastName=Smith")]
        public string Post(string url, string data)
        {
            using (var client = new WebClient())
            {
                return client.UploadString(url, data);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the POST method.  Data is a formatted string
        /// posted as a form in the format: "Time = 12:00am temperature = 50";
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="headerKey"></param>
        /// <param name="headerValue"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the POST method.  Post form values in the format: LastName=Smith",
            ParameterCount = 4)]
        [Description("Downloads a string from a URL using the POST method.  Post form values in the format: LastName=Smith")]
        public string Post(string url, string data, string headerKey, string headerValue)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(headerKey, headerValue);
                return client.UploadString(url, data);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the PUT method.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the PUT method.",
                               ParameterCount = 1)]
        public string Put(string url)
        {
            using (var client = new WebClient())
            {
                return client.UploadString(url, "PUT","");
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the PUT method.  Data is a formatted string
        /// posted as a form in the format: "Time = 12:00am temperature = 50";
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the PUT method.  Post form values in the format: LastName=Smith",
                               ParameterCount = 1)]
        [Description("Downloads a string from a URL using the PUT method.  Post form values in the format: LastName=Smith")]
        public string Put(string url, string data)
        {
            using (var client = new WebClient())
            {
                return client.UploadString(url, "PUT", data);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the PUT method.  Data is a formatted string
        /// posted as a form in the format: "Time = 12:00am temperature = 50";
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="headerKey"></param>
        /// <param name="headerValue"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the PUT method.  Post form values in the format: LastName=Smith",
            ParameterCount = 4)]
        [Description("Downloads a string from a URL using the PUT method.  Post form values in the format: LastName=Smith")]
        public string Put(string url, string data, string headerKey, string headerValue)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(headerKey, headerValue);
                return client.UploadString(url, "PUT", data);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the DELETE method.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the DELETE method.",
                               ParameterCount = 1)]
        public string Delete(string url)
        {
            using (var client = new WebClient())
            {
                return client.UploadString(url, "DELETE", "");
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the DELETE method.  Data is a formatted string
        /// posted as a form in the format: "Time = 12:00am temperature = 50";
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the DELETE method.  Post form values in the format: LastName=Smith",
                               ParameterCount = 1)]
        [Description("Downloads a string from a URL using the DELETE method.  Post form values in the format: LastName=Smith")]
        public string Delete(string url, string data)
        {
            using (var client = new WebClient())
            {
                return client.UploadString(url, "DELETE", data);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the DELETE method.  Data is a formatted string
        /// posted as a form in the format: "Time = 12:00am temperature = 50";
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="headerKey"></param>
        /// <param name="headerValue"></param>
        [MoonSharpModuleMethod(Description = "Downloads a string from a URL using the DELETE method.  Post form values in the format: LastName=Smith",
            ParameterCount = 4)]
        [Description("Downloads a string from a URL using the DELETE method.  Post form values in the format: LastName=Smith")]
        public string Delete(string url, string data, string headerKey, string headerValue)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add(headerKey, headerValue);
                return client.UploadString(url, "DELETE", data);
            }
        }
    }
}