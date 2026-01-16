using FluentWorkflow.Management.Shared;

namespace FluentWorkflow.Management.Web.Endpoints;

/// <summary>
/// 标准Api响应
/// </summary>
public class StandardApiResponse
{
    #region Public 方法

    /// <summary>
    /// 错误
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static StandardApiResponse<object> Error(string? message)
    {
        return new StandardApiResponse<object>()
        {
            Code = "ERROR",
            Message = message ?? "Internal server error",
        };
    }

    /// <summary>
    /// 成功
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static StandardApiResponse<T> Success<T>(T? data)
    {
        return new StandardApiResponse<T>()
        {
            Code = "SUCCESS",
            Message = null,
            Data = data
        };
    }

    #endregion Public 方法
}

/// <summary>
/// 标准Api响应
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class StandardApiResponse<T> : IErrorMessage
{
    #region Public 属性

    /// <summary>
    /// Code
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }

    #endregion Public 属性
}
