using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     管理员表
    /// </summary>
    [Table("T_Member")]
    public class Member : BaseModel
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
        ///     密码
        /// </summary>
        [MaxLength(100)]
        public string Password { get; set; }

        /// <summary>
        ///     所属社团 ID 号，如果未加入社团则为空
        /// </summary>
        public int? ClubId { get; set; }

        /// <summary>
        ///     注册日期
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }
    }
}