using Infrastructure.Data;

namespace Repository
{
    public interface IAppBaseService<T> : IBaseService<T> where T : class
    {
    }
}