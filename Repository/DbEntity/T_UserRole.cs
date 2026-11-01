using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    /// 用户角色关联表 - RBAC权限系统
    /// </summary>
    [Table("T_UserRole")]
    public class UserRole : BaseModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// 用户类型 0：管理员(Admin) 1：成员(Member)
        /// </summary>
        [Required]
        public byte UserType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }
    }
}
