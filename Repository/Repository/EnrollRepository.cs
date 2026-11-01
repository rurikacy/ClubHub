using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     申请仓储实现
    /// </summary>
    public class EnrollRepository : BaseRepository<Enroll>, IEnrollRepository
    {
        public EnrollRepository(IConfiguration config) : base(config)
        {
        }
    }
}