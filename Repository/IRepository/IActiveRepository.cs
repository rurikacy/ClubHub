using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     活动仓储接口
    /// </summary>
    public interface IActiveRepository : IBaseRepository<Active>
    {
        // 可以在这里添加活动特定的查询方法
    }
}