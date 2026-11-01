using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     社团仓储接口
    /// </summary>
    public interface IClubRepository : IBaseRepository<T_Club>
    {
        // 可以在这里添加社团特定的查询方法
    }
}