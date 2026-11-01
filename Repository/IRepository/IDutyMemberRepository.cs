using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     部门成员仓储接口
    /// </summary>
    public interface IDutyMemberRepository : IBaseRepository<DutyMember>
    {
        // 可以在这里添加部门成员特定的查询方法
    }
}