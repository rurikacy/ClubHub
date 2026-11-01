using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     通知仓储接口
    /// </summary>
    public interface IMessageRepository : IBaseRepository<Message>
    {
        // 可以在这里添加通知特定的查询方法
    }
}