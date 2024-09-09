/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using RestSharp;
using Avalon.Common;

namespace Avalon.Network
{
    /// <summary>
    /// API calls.
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// Logs an <see cref="Exception" /> to the exception repository.
        /// </summary>
        /// <param name="appId">The GUID app ID.</param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static async Task LogException(string appId, Exception? ex)
        {
            if (string.IsNullOrWhiteSpace(App.ApiKey))
            {
                return;
            }

            if (ex == null)
            {
                return;
            }

            // Validate the guid for the app, try to prevent bad requests from being made.
            if (string.IsNullOrEmpty(appId) || !Guid.TryParse(appId, out _))
            {
                return;
            }

            var http = AppServices.GetService<RestClient>();

            if (http != null)
            {
                // Create the wrapper of exception that can be serialized.
                var appException = new AppException()
                {
                    Message = ex.Message,
                    StackTrace = ex.ToFormattedString()
                };

                var request = new RestRequest("/exception/log/{appId}").AddUrlSegment("appId", appId);
                request.AddJsonBody(appException, "application/json");

                var result = await http.ExecutePostAsync(request);
            }
        }

        /// <summary>
        /// Logs an <see cref="Exception" /> to the exception repository.
        /// </summary>
        /// <param name="appId">The GUID app ID.</param>
        /// <param name="ex"></param>
        public static void LogExceptionSync(string appId, Exception? ex)
        {
            if (string.IsNullOrWhiteSpace(App.ApiKey))
            {
                return;
            }

            if (ex == null)
            {
                return;
            }

            // Validate the guid for the app, try to prevent bad requests from being made.
            if (string.IsNullOrEmpty(appId) || !Guid.TryParse(appId, out _))
            {
                return;
            }

            var http = AppServices.GetService<RestClient>();

            if (http != null)
            {
                // Create the wrapper of exception that can be serialized.
                var appException = new AppException()
                {
                    Message = ex.Message,
                    StackTrace = ex.ToFormattedString()
                };

                var request = new RestRequest("/exception/log/{appId}").AddUrlSegment("appId", appId);
                request.AddJsonBody(appException, "application/json");

                http.ExecutePost(request);
            }
        }

        /// <summary>
        /// Reports usage for the specified app.
        /// </summary>
        /// <param name="appId"></param>
        public static async Task ReportUsage(string appId)
        {
            if (string.IsNullOrWhiteSpace(App.ApiKey))
            {
                return;
            }

            // Fire and forget section of code
            await Task.Run(async () =>
            {
                try
                {
                    // Perform your long-running task or operation here
                    // This code will run on a background thread
                    var http = AppServices.GetService<RestClient>();

                    if (http != null)
                    {
                        var request = new RestRequest("/app/usage");
                        request.AddParameter("guid", appId, ParameterType.QueryString);
                        await http.ExecuteGetAsync(request);
                    }
                }
                catch
                {
                    // Eat exception in this block
                }
            });
        }
    }
}
