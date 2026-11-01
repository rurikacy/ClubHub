using System.Threading.Tasks;
using Repository.DbEntity;

namespace Repository.IRepository
{
    public interface IAuthRepository : IDependencyRepository, IAppBaseService<User>
    {
        /// <summary>
    ///     登录接口（根据角色）
    /// </summary>
    /// <param name="userName">用户名（StudentId）</param>
    /// <param name="password">密码</param>
    /// <param name="role">角色 0：超级管理员，1：社团管理员，2：社团成员</param>
    /// <returns>返回userId</returns>
    Task<int> Login(string userName, string password, int role);

    /// <summary>
    ///     自动识别角色登录
    /// </summary>
    /// <param name="userName">用户名（StudentId）</param>
    /// <param name="password">密码</param>
    /// <returns>返回(userId, role)，role: 0超管, 1社管, 2成员</returns>
    Task<(int userId, int role)> LoginAutoRole(string userName, string password);

        /// <summary>
        ///     根据用户ID和角色获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="role">角色 0：超管，1：社管，2：成员</param>
        /// <returns>用户信息对象（Admin或Member）</returns>
        Task<object> GetUserByIdAsync(int userId, int role);

        /// <summary>
        ///     根据StudentId和角色获取用户信息
        /// </summary>
        /// <param name="studentId">学号/工号</param>
        /// <param name="role">角色 0：超管，1：社管，2：成员</param>
        /// <returns>用户信息对象</returns>
        Task<object> GetUserByStudentIdAsync(string studentId, int role);

        /// <summary>
        ///     根据StudentId从T_User表获取用户实体，用于获取UserType
        /// </summary>
        /// <param name="studentId">学号/工号</param>
        /// <returns>T_User实体</returns>
        Task<User> GetUserEntityByStudentIdAsync(string studentId);
    }
}