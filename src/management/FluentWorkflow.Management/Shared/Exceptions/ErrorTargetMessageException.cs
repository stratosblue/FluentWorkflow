namespace FluentWorkflow.Management.Shared;

/// <summary>
/// 错误的目标消息异常
/// </summary>
public class ErrorTargetMessageException : Exception
{
    #region Public 属性

    /// <summary>
    /// 请求对象
    /// </summary>
    public object Request { get; }

    /// <summary>
    /// 响应对象
    /// </summary>
    public object Response { get; }

    /// <summary>
    /// 目标消息类型
    /// </summary>
    public Type Type { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="ErrorTargetMessageException"/>
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="type">目标消息类型</param>
    /// <param name="response">响应对象</param>
    /// <param name="message">错误信息</param>
    public ErrorTargetMessageException(object request, Type type, object response, string? message = null)
        : base(message ?? BuildMessage(response, type, response))
    {
        Request = request;
        Type = type;
        Response = response;
    }

    #endregion Public 构造函数

    #region Private 方法

    private static string BuildMessage(object request, Type type, object response)
    {
        return $"Request: {request} need a \"{type}\" response. But the response is the {response} of type \"{response.GetType()}\"";
    }

    #endregion Private 方法
}
