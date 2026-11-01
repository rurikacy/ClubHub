using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     部门成员仓储实现
    /// </summary>
    public class DutyMemberRepository : BaseRepository<DutyMember>, IDutyMemberRepository
    {
        public DutyMemberRepository(IConfiguration config) : base(config)
        {
        }
    }
}