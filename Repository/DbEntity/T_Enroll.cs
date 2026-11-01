using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     申请表（学生申请加入社团）
    /// </summary>
    [Table("T_Enroll")]
    public class Enroll : BaseModel
    {
        /// <summary>
        ///     申请社团（必须为社团表中包含的ID）
        /// </summary>
        [Required]
        public int ClubId { get; set; }

        /// <summary>
        ///     申请人（必须为成员表中包含的ID）
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        /// <summary>
        ///     申请时间（自动为新增数据当天日期）
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }

        /// <summary>
        ///     处理状态 0：批准 1：退回
        /// </summary>
        public byte? HandType { get; set; }

        /// <summary>
        ///     处理说明
        /// </summary>
        [MaxLength(100)]
        public string HandInfo { get; set; }

        /// <summary>
        ///     处理时间
        /// </summary>
        public DateTime? HandDate { get; set; }
    }
}