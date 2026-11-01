using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    ///     基础数据
    /// </summary>
    public interface IBaseRepository : IDependencyRepository, IAppBaseService<User>
    {
    }

    /// <summary>
    ///     泛型基础数据接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseRepository<T> : IDependencyRepository, IAppBaseService<T> where T : class
    {
    }
}