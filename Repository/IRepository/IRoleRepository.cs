using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Data;
using Repository.DbEntity;

namespace Repository.IRepository
{
    /// <summary>
    /// 角色管理接口
    /// </summary>
    public interface IRoleRepository : IDependencyRepository
    {
        /// <summary>
        /// 获取所有角色
        /// </summary>
        Task<IEnumerable<Role>> GetAllAsync();

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        Task<Role> GetByIdAsync(int id);

        /// <summary>
        /// 根据编码获取角色
        /// </summary>
        Task<Role> GetByCodeAsync(string code);

        /// <summary>
        /// 添加角色
        /// </summary>
        Task<int> InsertAsync(Role role);

        /// <summary>
        /// 更新角色
        /// </summary>
        Task<bool> ModifyAsync(Role role);

        /// <summary>
        /// 删除角色
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 分页获取角色列表
        /// </summary>
        Task<PageData<Role>> GetPagedAsync(int page, int limit, string name = null, byte? status = null);

        /// <summary>
        /// 获取用户的角色列表
        /// </summary>
        Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId, byte userType);

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        Task<bool> AssignRoleToUserAsync(int userId, byte userType, int roleId);

        /// <summary>
        /// 移除用户的角色
        /// </summary>
        Task<bool> RemoveRoleFromUserAsync(int userId, byte userType, int roleId);

        /// <summary>
        /// 获取角色的权限列表
        /// </summary>
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId);

        /// <summary>
        /// 为角色分配权限
        /// </summary>
        Task<bool> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds);
    }
}
