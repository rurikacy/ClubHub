using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     部门仓储实现
    /// </summary>
    public class DutyRepository : BaseRepository<Duty>, IDutyRepository
    {
        public DutyRepository(IConfiguration config) : base(config)
        {
        }
    }
}