using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     基础数据
    /// </summary>
    public class BaseRepository : AppBaseService<User>, IBaseRepository
    {
        public BaseRepository(IConfiguration config)
        {
            Config = config;
        }
    }

    /// <summary>
    ///     泛型基础数据实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseRepository<T> : AppBaseService<T>, IBaseRepository<T> where T : class
    {
        protected BaseRepository(IConfiguration config)
        {
            Config = config;
        }
    }
}