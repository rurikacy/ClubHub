using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    /// <summary>
    /// 角色管理实现
    /// </summary>
    public class RoleRepository : AppBaseService<Role>, IRoleRepository
    {
        public RoleRepository(IConfiguration config)
        {
            Config = config;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            const string sql = "SELECT * FROM T_Role WHERE Status = 1 ORDER BY Level, Id";
            return await FindAllAsync<Role>(sql);
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM T_Role WHERE Id = @id";
            var roles = await FindAllAsync<Role>(sql, new { id });
            return roles.FirstOrDefault();
        }

        public async Task<Role> GetByCodeAsync(string code)
        {
            const string sql = "SELECT * FROM T_Role WHERE Code = @code";
            var roles = await FindAllAsync<Role>(sql, new { code });
            return roles.FirstOrDefault();
        }

        public async Task<int> InsertAsync(Role role)
        {
            role.CreateTime = DateTime.Now;
            role.Status = 1;
            return await AddAsync(role);
        }

        public async Task<bool> ModifyAsync(Role role)
        {
            role.UpdateTime = DateTime.Now;
            return await UpdateAsync(role);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "UPDATE T_Role SET Status = 0, UpdateTime = GETDATE() WHERE Id = @id";
            int result = await ExecuteAsync(sql, new { id });
            return result > 0;
        }

        public async Task<PageData<Role>> GetPagedAsync(int page, int limit, string name = null, byte? status = null)
        {
            string sql = "SELECT * FROM T_Role WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(name))
            {
                sql += " AND Name LIKE @name";
                parameters.Add("name", $"%{name}%");
            }

            if (status.HasValue)
            {
                sql += " AND Status = @status";
                parameters.Add("status", status.Value);
            }

            sql += " ORDER BY Level, Id";
            return await PagedAsync<Role>(sql, parameters, "Id DESC", page, limit);
        }

        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId, byte userType)
        {
            const string sql = @"
                SELECT r.* FROM T_Role r
                INNER JOIN T_UserRole ur ON r.Id = ur.RoleId
                WHERE ur.UserId = @userId AND ur.UserType = @userType AND r.Status = 1
                ORDER BY r.Level";
            return await FindAllAsync<Role>(sql, new { userId, userType });
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, byte userType, int roleId)
        {
            // 先检查是否已存在
            const string checkSql = @"
                SELECT COUNT(1) FROM T_UserRole 
                WHERE UserId = @userId AND UserType = @userType AND RoleId = @roleId";
            int count = await ExecuteScalarAsync<int>(checkSql, new { userId, userType, roleId });

            if (count > 0)
            {
                return true; // 已存在，无需重复添加
            }

            const string sql = @"
                INSERT INTO T_UserRole (UserId, RoleId, UserType, CreateTime)
                VALUES (@userId, @roleId, @userType, GETDATE())";
            int result = await ExecuteAsync(sql, new { userId, userType, roleId });
            return result > 0;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, byte userType, int roleId)
        {
            const string sql = @"
                DELETE FROM T_UserRole 
                WHERE UserId = @userId AND UserType = @userType AND RoleId = @roleId";
            int result = await ExecuteAsync(sql, new { userId, userType, roleId });
            return result > 0;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId)
        {
            const string sql = @"
                SELECT p.* FROM T_Permission p
                INNER JOIN T_RolePermission rp ON p.Id = rp.PermissionId
                WHERE rp.RoleId = @roleId AND p.Status = 1
                ORDER BY p.SortOrder";
            return await FindAllAsync<Permission>(sql, new { roleId });
        }

        public async Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            await using var connection = new SqlConnection(Config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 先删除原有权限
                const string deleteSql = "DELETE FROM T_RolePermission WHERE RoleId = @roleId";
                await connection.ExecuteAsync(deleteSql, new { roleId }, transaction);

                // 添加新权限
                if (permissionIds != null && permissionIds.Any())
                {
                    const string insertSql = @"
                        INSERT INTO T_RolePermission (RoleId, PermissionId, CreateTime)
                        VALUES (@roleId, @permissionId, GETDATE())";

                    foreach (var permissionId in permissionIds)
                    {
                        await connection.ExecuteAsync(insertSql, new { roleId, permissionId }, transaction);
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
