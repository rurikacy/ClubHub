using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    public class ModuleRepository : AppBaseService<Module>, IModuleRepository
    {
        public ModuleRepository(IConfiguration config)
        {
            Config = config;
        }

        /// <summary>
        ///     按用户类型(UserType)获取模块
        ///     0：超级管理员 1：社团管理员 2：社团成员
        ///     直接依据 T_Module.Type 字段进行过滤，用于运行时菜单渲染
        /// </summary>
        /// <param name="userType">用户类型</param>
        /// <returns>该用户类型可见的模块列表</returns>
        public async Task<List<Module>> GetModulesByUserType(int userType)
        {
            const string sql = @"SELECT * FROM dbo.T_Module WHERE Type = @userType";

            await using SqlConnection conn = Connection;
            await conn.OpenAsync();
            IEnumerable<Module> models = await conn.QueryAsync<Module>(sql, new { userType });
            return models.ToList();
        }

        /// <summary>
        ///     根据用户类型和ClubId状态获取模块
        /// </summary>
        /// <param name="userType">用户类型：0=超管，1=社管，2=成员</param>
        /// <param name="hasClub">是否有所属社团</param>
        /// <returns>模块列表</returns>
        public async Task<List<Module>> GetModulesByUserTypeAndClubStatus(int userType, bool hasClub)
        {
            // 管理员（UserType=0或1）不受ClubId影响，返回完整模块列表
            if (userType == 0 || userType == 1)
            {
                return await GetModulesByUserType(userType);
            }

            // 成员（UserType=2）根据ClubId状态过滤模块
            if (userType == 2)
            {
                string sql =
                    // ClubId为null的成员：只返回"社团报名"模块及其父模块
                    // 需要包含父模块，否则菜单树无法正确构建
                    !hasClub
                        ? @"SELECT * FROM dbo.T_Module 
                            WHERE Type = @userType 
                            AND (Name = N'社团报名' OR Id IN (
                                SELECT DISTINCT ParentId FROM dbo.T_Module 
                                WHERE Type = @userType AND Name = N'社团报名'
                            ))"
                        :
                        // ClubId不为null的成员：返回除"社团报名"外的所有成员模块
                        @"SELECT * FROM dbo.T_Module 
                            WHERE Type = @userType 
                            AND Name != N'社团报名'";

                await using SqlConnection conn = Connection;
                await conn.OpenAsync();
                IEnumerable<Module> models = await conn.QueryAsync<Module>(sql, new { userType });
                return models.ToList();
            }

            // 默认返回空列表（不应该到达这里）
            return new List<Module>();
        }

        /// <summary>
        ///     获取游客可见的模块（只包含社团浏览）
        /// </summary>
        /// <returns>模块列表</returns>
        public async Task<List<Module>> GetModulesForGuest()
        {
            // 游客只能看到"社团浏览"功能
            // 由于数据库中可能没有"社团浏览"模块，我们直接返回一个硬编码的模块列表
            // 包含一个父菜单和一个子菜单（社团浏览）
            var guestModules = new List<Module>
            {
                new Module
                {
                    Id = 9998,
                    Name = "社团广场",
                    ParentId = 0,
                    Url = "",
                    Type = 3
                },
                new Module
                {
                    Id = 9999,
                    Name = "社团浏览",
                    ParentId = 9998,
                    Url = "/ClubHub/Club/GuestView",
                    Type = 3
                }
            };

            return await Task.FromResult(guestModules);
        }
    }
}