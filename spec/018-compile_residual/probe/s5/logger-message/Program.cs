// Logging.md #5, verbatim from its fence, then one call so the method is used.
using System;
using Microsoft.Extensions.Logging;

internal static partial class Log
{
    [LoggerMessage(LogLevel.Information, "Building send pipeline for command: {CommandType} {Id}")]
    public static partial void BuildingSendPipelineForCommand(ILogger logger, Type commandType, string id);
}

internal static class Program
{
    private static void Main() =>
        Log.BuildingSendPipelineForCommand(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, typeof(Program), "1");
}
