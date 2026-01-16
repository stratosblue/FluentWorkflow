using FluentWorkflow.Management.Web.Endpoints;

namespace FluentWorkflow.Management.Web.Endpoints
{
    /// <summary>
    /// 分页请求Dto
    /// </summary>
    public record PagingRequestDto
    {
        /// <summary>
        /// 页码
        /// </summary>
        public required int Page { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public required int PageSize { get; set; }
    }

    /// <summary>
    /// 分页响应Dto
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public record PagedResponseDto<TItem>
    {
        /// <summary>
        /// 总数量
        /// </summary>
        public required int? TotalCount { get; set; }

        /// <summary>
        /// 数据项列表
        /// </summary>
        public required IList<TItem> Items { get; set; }
    }
}

namespace System.Linq
{
    internal static class PagingRequestExtensions
    {
        #region Public 方法

        public static PagedResponseDto<TResponse> GetPagedResponse<T, TResponse>(this IEnumerable<T> items, PagingRequestDto input, Func<T, TResponse> valueMapDelegate)
        {
            return items.GetPagedResponse(input.Page, input.PageSize, valueMapDelegate);
        }

        public static PagedResponseDto<TResponse> GetPagedResponse<T, TResponse>(this IEnumerable<T> items, int page, int pageSize, Func<T, TResponse> valueMapDelegate)
        {
            return new()
            {
                TotalCount = items.Count(),
                Items = items.Paging(page, pageSize).Select(valueMapDelegate).ToList(),
            };
        }

        public static IEnumerable<T> Paging<T>(this IEnumerable<T> items, PagingRequestDto input) => items.Paging(input.Page, input.PageSize);

        public static IEnumerable<T> Paging<T>(this IEnumerable<T> items, int page, int pageSize)
        {
            return items.Skip((page - 1) * pageSize).Take(pageSize);
        }

        #endregion Public 方法
    }
}
