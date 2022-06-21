using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Open_MediaServer.Utils;

public static class StringUtils
{
    private static readonly Random Random = new();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }

    public static string FormatBytesWithIdentifier(long bytes)
    {
        string[] suffix = {"B", "KB", "MB", "GB", "TB"};
        int i;
        double dblSByte = bytes;
        for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
        {
            dblSByte = bytes / 1024.0;
        }

        return $"{dblSByte:0.##} {suffix[i]}";
    }

    public static string FirstCharToUpper(this string input) => input switch
    {
        null => throw new ArgumentNullException(nameof(input)),
        "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
        _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
    };

    public static string GetDisplayUrl(this HttpRequest request, string host = null)
    {
        var scheme = request.Scheme ?? string.Empty;
        host ??= request.Host.Value ?? string.Empty;
        var pathBase = request.PathBase.Value ?? string.Empty;
        var path = request.Path.Value ?? string.Empty;
        var queryString = request.QueryString.Value ?? string.Empty;

        var length = scheme.Length + Uri.SchemeDelimiter.Length + host.Length
                     + pathBase.Length + path.Length + queryString.Length;

        return new StringBuilder(length)
            .Append(scheme)
            .Append(Uri.SchemeDelimiter)
            .Append(host)
            .Append(pathBase)
            .Append(path)
            .Append(queryString)
            .ToString();
    }
}