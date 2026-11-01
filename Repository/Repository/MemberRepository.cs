using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     成员仓储实现
    /// </summary>
    public class MemberRepository : BaseRepository<Member>, IMemberRepository
    {
        public MemberRepository(IConfiguration config) : base(config)
        {
        }
    }
}