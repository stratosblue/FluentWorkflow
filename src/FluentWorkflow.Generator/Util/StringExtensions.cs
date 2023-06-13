using System.Text;

namespace System;

internal static class StringExtensions
{
    #region Public 方法

    public static string ToVarName(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
        if (char.IsLower(value[0]))
        {
            return value;
        }
        var builder = new StringBuilder(value);
        builder[0] = char.ToLowerInvariant(value[0]);
        return builder.ToString();
    }

    #endregion Public 方法
}
