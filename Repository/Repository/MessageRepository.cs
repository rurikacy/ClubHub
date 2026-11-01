using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    ///     通知仓储实现
    /// </summary>
    public class MessageRepository : BaseRepository<Message>, IMessageRepository
    {
        public MessageRepository(IConfiguration config) : base(config)
        {
        }
    }
}