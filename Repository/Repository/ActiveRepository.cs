using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     活动仓储实现
    /// </summary>
    public class ActiveRepository : BaseRepository<Active>, IActiveRepository
    {
        public ActiveRepository(IConfiguration config) : base(config)
        {
        }
    }
}