using System.Collections.Generic;

namespace Infrastructure.Data
{
    public class PageData<T>
    {
        /// <summary>
        ///     总条数
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        ///     返回
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        ///     当前页
        /// </summary>
        public long PageIndex { get; set; }

        /// <summary>
        ///     每页条数
        /// </summary>
        public long PageSize { get; set; }

        /// <summary>
        ///     总页数
        /// </summary>
        public long TotalPages { get; set; }

        /// <summary>
        ///     返回筛选集合
        /// </summary>
        public Dictionary<string, List<string>> ResultFilter = new Dictionary<string, List<string>>();
    }
}