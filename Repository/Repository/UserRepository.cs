using System.Threading.Tasks;
using Dapper;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Repository.DbEntity;
using Repository.IRepository;

namespace Repository.Repository
{
    public class UserRepository : AppBaseService<User>, IUserRepository
    {
        public UserRepository(IConfiguration config)
        {
            Config = config;
        }

        /// <summary>
        ///     用户列表（分页）
        /// </summary>
        /// <param name="userName">用户名 or 真实姓名</param>
        /// <param name="phone">手机号</param>
        /// <param name="clubId">社团ID</param>
        /// <param name="pageIndex">开始页数</param>
        /// <param name="pageSize">每页条数</param>
        public async Task<PageData<DbEntity.UserViewModel>> GetPageDataAsync(string userName, string phone, int? clubId, int pageIndex, int pageSize)
        {
            string sql = @"SELECT u.Id, u.StudentId, u.Name, u.Password, u.Sex, u.PhoneNumber, u.UserType, u.EnterData,
                                  CASE 
                                      WHEN u.UserType = 0 THEN NULL
                                      WHEN u.UserType = 1 THEN (SELECT TOP 1 ClubId FROM T_Admin WHERE StudentId = u.StudentId)
                                      WHEN u.UserType = 2 THEN (SELECT TOP 1 ClubId FROM T_Member WHERE StudentId = u.StudentId)
                                  END AS ClubId,
                                  CASE 
                                      WHEN u.UserType = 0 THEN NULL
                                      WHEN u.UserType = 1 THEN (SELECT TOP 1 c.Name FROM T_Admin a JOIN T_Club c ON a.ClubId = c.Id WHERE a.StudentId = u.StudentId)
                                      WHEN u.UserType = 2 THEN (SELECT TOP 1 c.Name FROM T_Member m JOIN T_Club c ON m.ClubId = c.Id WHERE m.StudentId = u.StudentId)
                                  END AS ClubName
                           FROM dbo.T_User u 
                           WHERE 1=1";

            DynamicParameters parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(userName))
            {
                sql += " AND ( u.StudentId LIKE @userName OR u.Name LIKE @userName ) ";
                parameters.Add("userName", "%" + userName + "%");
            }

            if (!string.IsNullOrEmpty(phone))
            {
                sql += " AND u.PhoneNumber LIKE @phone ";
                parameters.Add("phone", "%" + phone + "%");
            }

            if (clubId.HasValue)
            {
                sql = @"SELECT * FROM (" + sql + @") AS UserData 
                        WHERE ClubId = @clubId";
                parameters.Add("clubId", clubId.Value);
            }

            return await PagedAsync<DbEntity.UserViewModel>(sql, parameters, "Id DESC", pageIndex, pageSize);
        }
    }
}