using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     通知表
    /// </summary>
    [Table("T_Message")]
    public class Message : BaseModel
    {
        /// <summary>
        ///     发布社团（必须为社团表中包含的ID）
        /// </summary>
        [Required]
        public int ClubId { get; set; }

        /// <summary>
        ///     发布人（必须为管理员表中包含的ID）
        /// </summary>
        [Required]
        public int AdminId { get; set; }

        /// <summary>
        ///     发布时间（自动为新增数据当天日期）
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }

        /// <summary>
        ///     通知标题
        /// </summary>
        [MaxLength(50)]
        public string Title { get; set; }

        /// <summary>
        ///     通知内容
        /// </summary>
        [MaxLength(500)]
        public string Content { get; set; }
    }
}