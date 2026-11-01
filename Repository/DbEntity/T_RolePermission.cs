using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    /// 角色权限关联表 - RBAC权限系统
    /// </summary>
    [Table("T_RolePermission")]
    public class RolePermission : BaseModel
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [Required]
        public int PermissionId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }
    }
}
