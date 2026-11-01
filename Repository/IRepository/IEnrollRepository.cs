using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     申请仓储接口
    /// </summary>
    public interface IEnrollRepository : IBaseRepository<Enroll>
    {
        // 可以在这里添加申请特定的查询方法
    }
}