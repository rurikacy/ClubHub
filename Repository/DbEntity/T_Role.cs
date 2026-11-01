using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    /// 角色表 - RBAC权限系统
    /// </summary>
    [Table("T_Role")]
    public class Role : BaseModel
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 角色编码
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// 角色级别 0：系统超级管理员 1：系统管理员 2：普通用户
        /// </summary>
        [Required]
        public byte Level { get; set; }

        /// <summary>
        /// 状态 0：禁用 1：启用
        /// </summary>
        [Required]
        public byte Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }
}
