using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Data;
using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    /// 用户管理接口（管理员端）
    /// </summary>
    public interface IUserManagementRepository : IDependencyRepository
    {
        /// <summary>
        /// 获取所有用户（包括管理员和成员）
        /// </summary>
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync();

        /// <summary>
        /// 分页获取用户列表
        /// </summary>
        Task<PageData<UserViewModel>> GetPagedUsersAsync(int page, int limit, string keyword = null, 
            byte? userType = null, byte? status = null);

        /// <summary>
        /// 根据ID获取用户详情
        /// </summary>
        Task<UserDetailViewModel> GetUserDetailAsync(int userId, byte userType);

        /// <summary>
        /// 更新用户状态（启用/禁用）
        /// </summary>
        Task<bool> UpdateUserStatusAsync(int userId, byte userType, byte status);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        Task<bool> UpdateUserAsync(UserUpdateModel model);

        /// <summary>
        /// 删除用户
        /// </summary>
        Task<bool> DeleteUserAsync(int userId, byte userType);

        /// <summary>
        /// 重置用户密码
        /// </summary>
        Task<bool> ResetPasswordAsync(int userId, byte userType, string newPassword);

        /// <summary>
        /// 获取用户统计信息
        /// </summary>
        Task<UserStatistics> GetUserStatisticsAsync();
    }

    /// <summary>
    /// 用户视图模型
    /// </summary>
    public class UserViewModel
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public string Name { get; set; }
        public byte Sex { get; set; }
        public string PhoneNumber { get; set; }
        public byte UserType { get; set; }
        public string UserTypeName { get; set; }
        public byte Status { get; set; }
        public string StatusName { get; set; }
        public int? ClubId { get; set; }
        public string ClubName { get; set; }
        public DateTime EnterData { get; set; }
        public List<string> Roles { get; set; }
    }

    /// <summary>
    /// 用户详情视图模型
    /// </summary>
    public class UserDetailViewModel
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public string Name { get; set; }
        public byte Sex { get; set; }
        public string PhoneNumber { get; set; }
        public byte UserType { get; set; }
        public string UserTypeName { get; set; }
        public byte Status { get; set; }
        public string StatusName { get; set; }
        public int? ClubId { get; set; }
        public string ClubName { get; set; }
        public DateTime EnterData { get; set; }
        public List<RoleInfo> Roles { get; set; }
    }

    /// <summary>
    /// 角色信息
    /// </summary>
    public class RoleInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    /// <summary>
    /// 用户更新模型
    /// </summary>
    public class UserUpdateModel
    {
        public int Id { get; set; }
        public byte UserType { get; set; }
        public string Name { get; set; }
        public byte Sex { get; set; }
        public string PhoneNumber { get; set; }
        public int? ClubId { get; set; }
    }

    /// <summary>
    /// 用户统计信息
    /// </summary>
    public class UserStatistics
    {
        /// <summary>
        /// 总用户数
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// 管理员数
        /// </summary>
        public int AdminCount { get; set; }

        /// <summary>
        /// 成员数
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// 启用用户数
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// 禁用用户数
        /// </summary>
        public int InactiveUsers { get; set; }

        /// <summary>
        /// 今日新增用户
        /// </summary>
        public int TodayNewUsers { get; set; }
    }
}
