using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Timers;

namespace Fanex.MailDelivery.Client.Logging;

public static class RabbitMQLogger
{
    private const int SendingMaxTime = 10;
    private static int sendingMessageCount = SendingMaxTime;
    private static readonly Timer timer = new(10 * 60 * 1000);

    public static void LogError(string message, Exception exception)
    {
        if (sendingMessageCount <= 0)
        {
            return;
        }

        sendingMessageCount -= 1;

        if (sendingMessageCount == 0)
        {
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        LogService.LogError(message, exception);
    }

    private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        timer.Stop();
        timer.Elapsed -= Timer_Elapsed;

        sendingMessageCount = SendingMaxTime;
    }
}

internal static class LogMessageFormatter
{
    private static readonly Regex Pattern = new("(?<!{){@?(?<arg>[^ :{}]+)(?<format>:[^}]+)?}",
        RegexOptions.Compiled);

    private static string ReplaceFirst(string text, string search, string replace)
    {
        var pos = text.IndexOf(search, StringComparison.Ordinal);
        if (pos < 0)
        {
            return text;
        }

        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    public static string FormatStructuredMessage(string targetMessage, object[] formatParameters)
    {
        if (formatParameters == null || formatParameters.Length == 0)
        {
            return targetMessage;
        }

        List<string> processedArguments = null;
        var formattedTargetMessage = targetMessage;

        foreach (Match match in Pattern.Matches(formattedTargetMessage))
        {
            var arg = match.Groups["arg"].Value;
            int outValue;

            if (!int.TryParse(arg, out outValue))
            {
                processedArguments ??= new List<string>(formatParameters.Length);
                var argumentIndex = processedArguments.IndexOf(arg);
                if (argumentIndex == -1)
                {
                    argumentIndex = processedArguments.Count;
                    processedArguments.Add(arg);
                }

                formattedTargetMessage = ReplaceFirst(formattedTargetMessage, match.Value,
                    string.Concat("{", argumentIndex.ToString(), match.Groups["format"].Value, "}"));
            }
        }

        try
        {
            return string.Format(CultureInfo.InvariantCulture, formattedTargetMessage, formatParameters);
        }
        catch (FormatException ex)
        {
            throw new FormatException(
                "The input string '" + formattedTargetMessage + "' could not be formatted using string.Format", ex);
        }
    }
}