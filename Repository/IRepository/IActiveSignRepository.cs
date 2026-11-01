using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     活动报名仓储接口
    /// </summary>
    public interface IActiveSignRepository : IBaseRepository<ActiveSign>
    {
        // 可以在这里添加活动报名特定的查询方法
    }
}