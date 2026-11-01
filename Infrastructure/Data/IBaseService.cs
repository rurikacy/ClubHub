using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public interface IBaseService<T> where T : class
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<TU> ExecuteScalarAsync<TU>(string sql, object param = null);

        /// <summary>
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object param = null);

        /// <summary>
        ///     新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<int> AddAsync(T t);

        /// <summary>
        ///     批量新增
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<int> AddListAsync(List<T> t);

        /// <summary>
        ///     更新
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(T t);

        /// <summary>
        ///     批量更新
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<bool> UpdateListAsync(List<T> t);

        /// <summary>
        ///     删除
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(T t);

        /// <summary>
        ///     批量删除
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<bool> DeleteListAsync(List<T> t);

        /// <summary>
        ///     通过id获取单个数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> SingleAsync(int id);

        /// <summary>
        ///     根据SQL语句获取符合条件的所有对象
        /// </summary>
        Task<IEnumerable<TU>> FindAllAsync<TU>(string sql, object param = null);

        /// <summary>
        ///     获取一个表的所有数据（慎用）性能问题
        /// </summary>
        /// <typeparam name="T">数据的类型</typeparam>
        Task<IEnumerable<U>> FindAllAsync<U>() where U : class;

        /// <summary>
        ///     异步分页查询
        /// </summary>
        /// <param name="sql">完整的查询语句</param>
        /// <param name="param">过滤条件</param>
        /// <param name="sortBy">排序</param>
        /// <param name="pageIndex">当前是第几页</param>
        /// <param name="pageSize">每页的个数</param>
        Task<PageData<U>> PagedAsync<U>(string sql, object param, string sortBy, long pageIndex, long pageSize);
    }
}