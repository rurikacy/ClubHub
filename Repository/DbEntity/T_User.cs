using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     用户表
    /// </summary>
    [Table("T_User")]
    public class User : BaseModel
    {
        /// <summary>
        ///     学号/工号（登录账号，唯一）
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string StudentId { get; set; }

        /// <summary>
        ///     姓名
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        /// <summary>
        ///     性别 0：未知 1：男 2：女
        /// </summary>
        [Required]
        public byte Sex { get; set; }

        /// <summary>
        ///     手机号
        /// </summary>
        [MaxLength(11)]
        public string PhoneNumber { get; set; }

        /// <summary>
        ///     用户类型 0：超级管理员 1：社团管理员 2：社团成员
        /// </summary>
        [Required]
        public byte UserType { get; set; }

        /// <summary>
        ///     密码
        /// </summary>
        [MaxLength(100)]
        public string Password { get; set; }

        /// <summary>
        ///     注册日期
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }
    }
}