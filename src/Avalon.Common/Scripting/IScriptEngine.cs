/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// The implementation structure for a hosted scripting engine.
    /// </summary>
    public interface IScriptEngine
    {
        /// <summary>
        /// List of shared objects registered with <see cref="RegisterObject{T}"/>.
        /// </summary>
        Dictionary<string, object> SharedObjects { get; set; }

        /// <summary>
        /// A reference to the parent <see cref="ScriptHost"/>.
        /// </summary>
        ScriptHost ScriptHost { get; set; }

        /// <summary>
        /// Registers an instantiated object with the script engine.  This object will be passed into
        /// the engine for use by scripts, including it's state if any.  An object if thread safe
        /// can be shared between many script environments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="prefix"></param>
        public void RegisterObject<T>(Type t, object item, string prefix);

        /// <summary>
        /// Loads a function into the script engine and/or all instances of the script engines in
        /// memory pools that exist.
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="code"></param>
        void LoadFunction(string functionName, string code);

        /// <summary>
        /// Executes code synchronously and returns <see cref="T"/> or null based.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        T Execute<T>(string code);

        /// <summary>
        /// Executes code asynchronously and return <see cref="T"/> or null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        Task<T> ExecuteAsync<T>(string code);

        /// <summary>
        /// Executes a function.  If the function isn't stored a copy will be loaded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName"></param>
        /// <param name="code"></param>
        /// <param name="args"></param>
        Task<T> ExecuteFunctionAsync<T>(string functionName, string code, params string[] args);

        /// <summary>
        /// Executes a function.  If the function isn't stored a copy will be loaded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName"></param>
        /// <param name="args"></param>
        Task<T> ExecuteFunctionAsync<T>(string functionName, params string[] args);

        /// <summary>
        /// Executes a function.  If the function isn't stored a copy will be loaded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName"></param>
        /// <param name="code"></param>
        /// <param name="args"></param>
        public T ExecuteFunction<T>(string functionName, string code, params string[] args);

        /// <summary>
        /// Executes a function.  If the function isn't stored a copy will be loaded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName"></param>
        /// <param name="args"></param>
        T ExecuteFunction<T>(string functionName, params string[] args);

        /// <summary>
        /// Executes code in a new static instance of the script environment that will be 
        /// discarded after use.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        T ExecuteStatic<T>(string code);

        /// <summary>
        /// Executes code in a new static instance of the script environment that will be 
        /// discarded after use.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        Task<T> ExecuteStaticAsync<T>(string code);

        /// <summary>
        /// Validates the code against against the implementing script engine.
        /// </summary>
        /// <param name="code"></param>
        ValidationResult Validate(string code);

        /// <summary>
        /// Validates the code against against the implementing script engine.
        /// </summary>
        /// <param name="code"></param>
        Task<ValidationResult> ValidateAsync(string code);

        /// <summary>
        /// Executes the scripting languages garbage collecting feature if it exists.  As an
        /// example, MoonSharp's Lua interpreter uses the default .NET GC while NLua defers
        /// to the native Lua garbage collection.
        /// </summary>
        void GarbageCollect();

        /// <summary>
        /// Resets the scripting environment back to the default state.  If any memory pools or
        /// shared objects are used those are also discarded.
        /// </summary>
        void Reset();

        /// <summary>
        /// An exception handler that should be run when an exception occurs anywhere in the
        /// script engine.  This allows for ease of use in exception handling when a fire and
        /// forget async call is made.
        /// </summary>
        Action<ScriptExceptionData> ExceptionHandler { get; set; }

    }
}