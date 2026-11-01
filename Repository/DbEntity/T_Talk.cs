using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     讨论中心消息表
    /// </summary>
    [Table("T_Talk")]
    public class Talk : BaseModel
    {
        /// <summary>
        ///     所属社团ID
        /// </summary>
        [Required]
        public int ClubId { get; set; }

        /// <summary>
        ///     发送人ID
        /// </summary>
        [Required]
        public int SenderId { get; set; }

        /// <summary>
        ///     发送人类型: 0=成员, 1=管理员, 2=超管
        /// </summary>
        [Required]
        public byte SenderType { get; set; }

        /// <summary>
        ///     消息内容
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        /// <summary>
        ///     发送时间
        /// </summary>
        [Required]
        public DateTime SendTime { get; set; }

        /// <summary>
        ///     状态: 0=正常, 1=已删除
        /// </summary>
        [Required]
        public byte Status { get; set; }
    }
}