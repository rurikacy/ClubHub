using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;
using UserViewModel = Repository.IRepository.UserViewModel;

namespace Repository.Repository
{
    /// <summary>
    /// 用户管理实现（管理员端）
    /// </summary>
    public class UserManagementRepository : AppBaseService<Admin>, IUserManagementRepository
    {
        public UserManagementRepository(IConfiguration config)
        {
            Config = config;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsersAsync()
        {
            const string sql = @"
                SELECT 
                    a.Id, a.StudentId, a.Name, a.Sex, a.PhoneNumber,
                    0 as UserType, '管理员' as UserTypeName,
                    1 as Status, '启用' as StatusName,
                    a.ClubId, c.Name as ClubName, a.EnterData
                FROM T_Admin a
                LEFT JOIN T_Club c ON a.ClubId = c.Id
                
                UNION ALL
                
                SELECT 
                    m.Id, m.StudentId, m.Name, m.Sex, m.PhoneNumber,
                    1 as UserType, '成员' as UserTypeName,
                    1 as Status, '启用' as StatusName,
                    m.ClubId, c.Name as ClubName, m.EnterData
                FROM T_Member m
                LEFT JOIN T_Club c ON m.ClubId = c.Id
                ORDER BY EnterData DESC";

            return await FindAllAsync<UserViewModel>(sql);
        }

        public async Task<PageData<UserViewModel>> GetPagedUsersAsync(int page, int limit, string keyword = null,
            byte? userType = null, byte? status = null)
        {
            string adminSql = @"
                SELECT 
                    a.Id, a.StudentId, a.Name, a.Sex, a.PhoneNumber,
                    0 as UserType, '管理员' as UserTypeName,
                    1 as Status, '启用' as StatusName,
                    a.ClubId, c.Name as ClubName, a.EnterData
                FROM T_Admin a
                LEFT JOIN T_Club c ON a.ClubId = c.Id
                WHERE 1=1";

            string memberSql = @"
                SELECT 
                    m.Id, m.StudentId, m.Name, m.Sex, m.PhoneNumber,
                    1 as UserType, '成员' as UserTypeName,
                    1 as Status, '启用' as StatusName,
                    m.ClubId, c.Name as ClubName, m.EnterData
                FROM T_Member m
                LEFT JOIN T_Club c ON m.ClubId = c.Id
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(keyword))
            {
                string keywordCondition = " AND (Name LIKE @keyword OR StudentId LIKE @keyword)";
                adminSql += keywordCondition;
                memberSql += keywordCondition;
                parameters.Add("keyword", $"%{keyword}%");
            }

            string finalSql;
            if (userType.HasValue)
            {
                finalSql = userType.Value == 0 ? adminSql : memberSql;
            }
            else
            {
                finalSql = $"{adminSql} UNION ALL {memberSql}";
            }

            finalSql += " ORDER BY EnterData DESC";

            return await PagedAsync<UserViewModel>(finalSql, parameters, "Id DESC", page, limit);
        }

        public async Task<UserDetailViewModel> GetUserDetailAsync(int userId, byte userType)
        {
            string sql;
            if (userType == 0)
            {
                sql = @"
                    SELECT 
                        a.Id, a.StudentId, a.Name, a.Sex, a.PhoneNumber,
                        0 as UserType, CASE WHEN a.AdminType = 0 THEN '超级管理员' ELSE '社团管理员' END as UserTypeName,
                        1 as Status, '启用' as StatusName,
                        a.ClubId, c.Name as ClubName, a.EnterData
                    FROM T_Admin a
                    LEFT JOIN T_Club c ON a.ClubId = c.Id
                    WHERE a.Id = @userId";
            }
            else
            {
                sql = @"
                    SELECT 
                        m.Id, m.StudentId, m.Name, m.Sex, m.PhoneNumber,
                        1 as UserType, '成员' as UserTypeName,
                        1 as Status, '启用' as StatusName,
                        m.ClubId, c.Name as ClubName, m.EnterData
                    FROM T_Member m
                    LEFT JOIN T_Club c ON m.ClubId = c.Id
                    WHERE m.Id = @userId";
            }

            var users = await FindAllAsync<UserDetailViewModel>(sql, new { userId });
            return users.FirstOrDefault();
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, byte userType, byte status)
        {
            // 由于现有表结构没有Status字段，这里预留接口
            // 如果需要启用/禁用功能，需要修改T_Admin和T_Member表添加Status字段
            // 暂时返回true
            await Task.CompletedTask;
            return true;
        }

        public async Task<bool> UpdateUserAsync(UserUpdateModel model)
        {
            if (model.UserType == 0)
            {
                const string sql = @"
                    UPDATE T_Admin 
                    SET Name = @Name, Sex = @Sex, PhoneNumber = @PhoneNumber, ClubId = @ClubId
                    WHERE Id = @Id";
                int result = await ExecuteAsync(sql, model);
                return result > 0;
            }
            else
            {
                const string sql = @"
                    UPDATE T_Member 
                    SET Name = @Name, Sex = @Sex, PhoneNumber = @PhoneNumber, ClubId = @ClubId
                    WHERE Id = @Id";
                int result = await ExecuteAsync(sql, model);
                return result > 0;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId, byte userType)
        {
            string sql;
            if (userType == 0)
            {
                sql = "DELETE FROM T_Admin WHERE Id = @userId";
            }
            else
            {
                sql = "DELETE FROM T_Member WHERE Id = @userId";
            }

            int result = await ExecuteAsync(sql, new { userId });
            return result > 0;
        }

        public async Task<bool> ResetPasswordAsync(int userId, byte userType, string newPassword)
        {
            string sql;
            if (userType == 0)
            {
                sql = "UPDATE T_Admin SET Password = @newPassword WHERE Id = @userId";
            }
            else
            {
                sql = "UPDATE T_Member SET Password = @newPassword WHERE Id = @userId";
            }

            int result = await ExecuteAsync(sql, new { userId, newPassword });
            return result > 0;
        }

        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            const string sql = @"
                SELECT 
                    (SELECT COUNT(*) FROM T_Admin) + (SELECT COUNT(*) FROM T_Member) as TotalUsers,
                    (SELECT COUNT(*) FROM T_Admin) as AdminCount,
                    (SELECT COUNT(*) FROM T_Member) as MemberCount,
                    (SELECT COUNT(*) FROM T_Admin) + (SELECT COUNT(*) FROM T_Member) as ActiveUsers,
                    0 as InactiveUsers,
                    (SELECT COUNT(*) FROM T_Admin WHERE CAST(EnterData as DATE) = CAST(GETDATE() as DATE)) +
                    (SELECT COUNT(*) FROM T_Member WHERE CAST(EnterData as DATE) = CAST(GETDATE() as DATE)) as TodayNewUsers";

            var result = await FindAllAsync<UserStatistics>(sql);
            return result.FirstOrDefault() ?? new UserStatistics();
        }
    }
}
