#pragma warning disable CS9124

namespace FluentWorkflow.Management.Shared.Communication;

/// <summary>
/// 递增的 int32 Id生成器
/// </summary>
/// <param name="initValue">初始值</param>
/// <param name="step">每次增加的值</param>
/// <param name="threshold">最大值(超过后会重新从 <paramref name="initValue"/> 开始，但并不是完全不能超出此值)</param>
public sealed class IncreasingIdGenerator(int initValue, int step, int threshold = int.MaxValue - 0x0FFF_FFFF)
{
    #region Private 字段

    private readonly object _syncRoot = new();

    private int _id = initValue;

    #endregion Private 字段

    #region Public 方法

    /// <summary>
    /// 获取下一个值
    /// </summary>
    /// <returns></returns>
    public int Next()
    {
        var id = Interlocked.Add(ref _id, step);
        if (id > threshold)
        {
            lock (_syncRoot)
            {
                if (Volatile.Read(ref _id) > threshold)
                {
                    Volatile.Write(ref _id, initValue);
                }
            }
        }
        return id;
    }

    #endregion Public 方法
}
