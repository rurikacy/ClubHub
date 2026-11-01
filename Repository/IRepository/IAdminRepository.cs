using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     管理员仓储接口
    /// </summary>
    public interface IAdminRepository : IDependencyRepository, IAppBaseService<Admin>
    {
        // 可以在这里添加管理员特定的查询方法
    }
}