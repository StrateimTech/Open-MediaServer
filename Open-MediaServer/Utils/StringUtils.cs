﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Open_MediaServer.Database.Schema;
using SQLite;

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

    public static string ToFormattedString(this DateTime uploadDate)
    {
        var sinceDate = DateTime.UtcNow.Subtract(uploadDate);

        if (sinceDate.TotalDays < 365)
        {
            var avgMonths = sinceDate.Days / 30;
            if (avgMonths <= 0)
            {
                if (sinceDate.TotalHours < 24)
                {
                    if (sinceDate.TotalMinutes < 60)
                    {
                        if (sinceDate.Minutes <= 0)
                        {
                            return "moments ago";
                        }

                        return $"{sinceDate.Minutes} minute{(sinceDate.Minutes > 1 ? "s" : String.Empty)} ago";
                    }

                    return $"{sinceDate.Hours} hour{(sinceDate.Hours > 1 ? "s" : String.Empty)} ago";
                }

                return $"{sinceDate.Days} day{(sinceDate.Days > 1 ? "s" : String.Empty)} ago";
            }

            return $"{avgMonths} month{(avgMonths > 1 ? "s" : String.Empty)} ago";
        }

        var years = sinceDate.Days / 365;
        return $"{years} year{(years > 1 ? "s" : String.Empty)} ago";
    }

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

    public static async Task<string> GenerateUniqueMediaId(this SQLiteAsyncConnection db, int length = 8)
    {
        var id = RandomString(length);
        if (await db.FindAsync<DatabaseSchema.Media>(media => media.Id == id) != null)
        {
            return await GenerateUniqueMediaId(db, length);
        }

        return id;
    }
}