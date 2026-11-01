using Infrastructure.Data;

namespace Repository
{
    public abstract class AppBaseService<T> : BaseService<T> where T : class
    {
    }
}