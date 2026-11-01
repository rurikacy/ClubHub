using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    public class AuthRepository : AppBaseService<User>, IAuthRepository
    {
        public AuthRepository(IConfiguration config)
        {
            Config = config;
        }

        /// <summary>
        ///     登录接口（根据角色）
        /// </summary>
        /// <param name="userName">用户名（StudentId）</param>
        /// <param name="password">密码</param>
        /// <param name="role">角色 0：超级管理员，1：社团管理员，2：社团成员</param>
        /// <returns>登录成功返回用户Id，否则返回0</returns>
        public async Task<int> Login(string userName, string password, int role)
        {
            // 根据新的数据库结构：
            // role = 0: 从T_Admin表查询，AdminType = 0 (超管)
            // role = 1: 从T_Admin表查询，AdminType = 1 (社管)
            // role = 2: 从T_Member表查询 (成员)

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return 0;
            }

            string sql;
            if (role == 0 || role == 1)
            {
                // 管理员登录（超管或社管）
                sql = @"SELECT Id FROM dbo.T_Admin 
                       WHERE StudentId = @userName 
                       AND AdminType = @role 
                       AND Password = @password";
            }
            else
            {
                // 成员登录
                sql = @"SELECT Id FROM dbo.T_Member 
                       WHERE StudentId = @userName 
                       AND Password = @password";
            }

            return await ExecuteScalarAsync<int>(sql, new { userName, role, password });
        }

        /// <summary>
        ///     自动识别角色登录
        /// </summary>
        /// <param name="userName">用户名（StudentId）</param>
        /// <param name="password">密码</param>
        /// <returns>返回(userId, role)，role: 0超管, 1社管, 2成员</returns>
        public async Task<(int userId, int role)> LoginAutoRole(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return (0, -1);
            }

            // 1. 先检查是否是超级管理员 (AdminType = 0)
            const string superAdminSql = @"SELECT Id FROM dbo.T_Admin 
                                           WHERE StudentId = @userName 
                                           AND AdminType = 0 
                                           AND Password = @password";
            int superAdminId = await ExecuteScalarAsync<int>(superAdminSql, new { userName, password });
            if (superAdminId > 0)
            {
                return (superAdminId, 0);
            }

            // 2. 再检查是否是社团管理员 (AdminType = 1)
            const string clubAdminSql = @"SELECT Id FROM dbo.T_Admin 
                                          WHERE StudentId = @userName 
                                          AND AdminType = 1 
                                          AND Password = @password";
            int clubAdminId = await ExecuteScalarAsync<int>(clubAdminSql, new { userName, password });
            if (clubAdminId > 0)
            {
                return (clubAdminId, 1);
            }

            // 3. 最后检查是否是成员
            const string memberSql = @"SELECT Id FROM dbo.T_Member 
                                       WHERE StudentId = @userName 
                                       AND Password = @password";
            int memberId = await ExecuteScalarAsync<int>(memberSql, new { userName, password });
            if (memberId > 0)
            {
                return (memberId, 2);
            }

            return (0, -1);
        }

        /// <summary>
        ///     根据用户ID和角色获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="role">角色 0：超管，1：社管，2：成员</param>
        /// <returns>用户信息对象（Admin或Member）</returns>
        public async Task<object> GetUserByIdAsync(int userId, int role)
        {
            if (role == 0 || role == 1)
            {
                // 管理员
                const string sql = @"SELECT * FROM dbo.T_Admin WHERE Id = @userId";
                IEnumerable<Admin> admins = await FindAllAsync<Admin>(sql, new { userId });
                return admins.FirstOrDefault();
            }
            else
            {
                // 成员
                const string sql = @"SELECT * FROM dbo.T_Member WHERE Id = @userId";
                IEnumerable<Member> members = await FindAllAsync<Member>(sql, new { userId });
                return members.FirstOrDefault();
            }
        }

        /// <summary>
        ///     根据StudentId和角色获取用户信息
        /// </summary>
        /// <param name="studentId">学号/工号</param>
        /// <param name="role">角色 0：超管，1：社管，2：成员</param>
        /// <returns>用户信息对象</returns>
        public async Task<object> GetUserByStudentIdAsync(string studentId, int role)
        {
            if (role == 0 || role == 1)
            {
                // 管理员
                const string sql = @"SELECT * FROM dbo.T_Admin WHERE StudentId = @studentId";
                IEnumerable<Admin> admins = await FindAllAsync<Admin>(sql, new { studentId });
                return admins.FirstOrDefault();
            }
            else
            {
                // 成员
                const string sql = @"SELECT * FROM dbo.T_Member WHERE StudentId = @studentId";
                IEnumerable<Member> members = await FindAllAsync<Member>(sql, new { studentId });
                return members.FirstOrDefault();
            }
        }

        /// <summary>
        ///     根据StudentId从T_User表获取用户实体，用于获取UserType
        /// </summary>
        /// <param name="studentId">学号/工号</param>
        /// <returns>T_User实体</returns>
        public async Task<User> GetUserEntityByStudentIdAsync(string studentId)
        {
            const string sql = @"SELECT * FROM dbo.T_User WHERE StudentId = @studentId";
            IEnumerable<User> users = await FindAllAsync<User>(sql, new { studentId });
            return users.FirstOrDefault();
        }
    }
}