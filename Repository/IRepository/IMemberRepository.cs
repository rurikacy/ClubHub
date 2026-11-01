using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     成员仓储接口
    /// </summary>
    public interface IMemberRepository : IBaseRepository<Member>
    {
        // 可以在这里添加成员特定的查询方法
    }
}