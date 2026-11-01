using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     管理员仓储实现
    /// </summary>
    public class AdminRepository : AppBaseService<Admin>, IAdminRepository
    {
        public AdminRepository(IConfiguration config)
        {
            Config = config;
        }
    }
}