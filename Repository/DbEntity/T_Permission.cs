using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    /// 权限表 - RBAC权限系统
    /// </summary>
    [Table("T_Permission")]
    public class Permission : BaseModel
    {
        /// <summary>
        /// 权限名称
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 权限编码
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        /// <summary>
        /// 权限类型 0：菜单 1：按钮 2：接口
        /// </summary>
        [Required]
        public byte Type { get; set; }

        /// <summary>
        /// 父级权限ID
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// 权限路径
        /// </summary>
        [MaxLength(200)]
        public string Path { get; set; }

        /// <summary>
        /// 权限图标
        /// </summary>
        [MaxLength(50)]
        public string Icon { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [Required]
        public int SortOrder { get; set; }

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
    }
}
