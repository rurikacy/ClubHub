using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     活动报名仓储实现
    /// </summary>
    public class ActiveSignRepository : BaseRepository<ActiveSign>, IActiveSignRepository
    {
        public ActiveSignRepository(IConfiguration config) : base(config)
        {
        }
    }
}