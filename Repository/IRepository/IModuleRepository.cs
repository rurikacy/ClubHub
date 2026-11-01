using System.Collections.Generic;
using System.Threading.Tasks;
using Repository.DbEntity;

namespace Repository.IRepository
{
    public interface IModuleRepository : IDependencyRepository, IAppBaseService<Module>
    {
        /// <summary>
        ///     根据用户类型(UserType)获取模块，用于运行时菜单渲染
        ///     0：超级管理员 1：社团管理员 2：社团成员
        /// </summary>
        /// <param name="userType">用户类型</param>
        /// <returns></returns>
        Task<List<Module>> GetModulesByUserType(int userType);

        /// <summary>
    ///     根据用户类型和ClubId状态获取模块
    /// </summary>
    /// <param name="userType">用户类型：0=超管，1=社管，2=成员</param>
    /// <param name="hasClub">是否有所属社团</param>
    /// <returns>模块列表</returns>
    Task<List<Module>> GetModulesByUserTypeAndClubStatus(int userType, bool hasClub);

    /// <summary>
    ///     获取游客可见的模块（只包含社团浏览）
    /// </summary>
    /// <returns>模块列表</returns>
    Task<List<Module>> GetModulesForGuest();
}
}