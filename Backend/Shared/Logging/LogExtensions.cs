using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Shared.Logging
{
    public static class LogExtensions
    {
        public static void LogLayerInformation(this ILogger logger,
            ILogPool pool,
            string module,
            string layer,
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "")
        {
            logger.LogInformation("{Layer}::{Member} - {Message}", layer, member, message);
            pool.Add(module, layer, file, member, "Information", message);
        }

        public static void LogLayerError(this ILogger logger,
            ILogPool pool,
            string module,
            string layer,
            Exception exception,
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "")
        {
            logger.LogError(
                "{Layer}::{Member} - {Message} ({ExceptionType})",
                layer,
                member,
                message,
                exception.GetType().Name);
            pool.Add(module, layer, file, member, "Error", message, exception);
        }

        public static void LogLayerWarning(this ILogger logger,
            ILogPool pool,
            string module,
            string layer,
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "")
        {
            logger.LogWarning("{Layer}::{Member} - {Message}", layer, member, message);
            pool.Add(module, layer, file, member, "Warning", message);
        }

        public static void LogLayerDebug(this ILogger logger,
            ILogPool pool,
            string module,
            string layer,
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "")
        {
            logger.LogDebug("{Layer}::{Member} - {Message}", layer, member, message);
            pool.Add(module, layer, file, member, "Debug", message);
        }
    }
}
