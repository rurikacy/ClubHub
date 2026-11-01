using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     部门仓储接口
    /// </summary>
    public interface IDutyRepository : IBaseRepository<Duty>
    {
        // 可以在这里添加部门特定的查询方法
    }
}