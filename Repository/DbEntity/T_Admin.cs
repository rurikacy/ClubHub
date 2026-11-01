using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     管理员表
    /// </summary>
    [Table("T_Admin")]
    public class Admin : BaseModel
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
        ///     管理员级别 0：超管 1：社管（如果为1则ClubId必须为空，反之则必须非空）
        /// </summary>
        [Required]
        public byte AdminType { get; set; }

        /// <summary>
        ///     所属社团 ID（限定为T_Club含有的id，超管时可为空，社管时必须非空）
        /// </summary>
        public int? ClubId { get; set; }

        /// <summary>
        ///     注册日期
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }
    }
}