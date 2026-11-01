using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     社团仓储实现
    /// </summary>
    public class ClubRepository : BaseRepository<T_Club>, IClubRepository
    {
        public ClubRepository(IConfiguration config) : base(config)
        {
        }
    }
}