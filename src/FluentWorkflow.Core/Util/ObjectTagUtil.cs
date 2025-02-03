using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FluentWorkflow.Util;

/// <summary>
/// 对象Tag帮助类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ObjectTagUtil
{
    #region Public 方法

    /// <summary>
    /// <paramref name="hashCode"/> 转 hex 字符串
    /// </summary>
    /// <param name="hashCode"></param>
    /// <returns></returns>
    public static string ConvertHashCode(int hashCode)
    {
        var base64 = Convert.ToBase64String(BitConverter.GetBytes(hashCode));

        var builder = new StringBuilder(base64.Length);

        for (int i = 0; i < base64.Length; i++)
        {
            var current = base64[i];
            switch (current)
            {
                case '=':
                    break;

                case '+' or '/':
                    builder.Append('_');
                    break;

                default:
                    builder.Append(current);
                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// 获取对象 <paramref name="target"/> 的tag
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(target))]
    public static string? GetHashCodeTag(object? target)
    {
        if (target is null)
        {
            return null;
        }

        return ConvertHashCode(target.GetHashCode());
    }

    #endregion Public 方法
}
