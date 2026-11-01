using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     活动报名表
    /// </summary>
    [Table("T_ActiveSign")]
    public class ActiveSign : BaseModel
    {
        /// <summary>
        ///     申请人（必须为成员表中包含的ID）
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        /// <summary>
        ///     报名活动（必须为活动表中包含的ID）
        /// </summary>
        [Required]
        public int ActiveId { get; set; }

        /// <summary>
        ///     报名时间（自动为新增数据当天日期）
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }

        /// <summary>
        ///     参与情况 0：未开始 1：已报名 2：已参加 3：未参加
        /// </summary>
        [Required]
        public byte SignType { get; set; }
    }
}