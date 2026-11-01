using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     活动表
    /// </summary>
    [Table("T_Active")]
    public class Active : BaseModel
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
        ///     活动标题
        /// </summary>
        [MaxLength(50)]
        public string Title { get; set; }

        /// <summary>
        ///     活动内容
        /// </summary>
        [MaxLength(200)]
        public string Content { get; set; }

        /// <summary>
        ///     发布时间（自动为新增数据当天日期）
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }

        /// <summary>
        ///     开始时间
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }

        /// <summary>
        ///     结束时间
        /// </summary>
        [Required]
        public DateTime EndTime { get; set; }

        /// <summary>
        ///     参与人数量（为活动报名表中ActiveId等于此表id的人的总和）
        /// </summary>
        public int JoinNum { get; set; }

        /// <summary>
        ///     活动地点
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Place { get; set; }

        /// <summary>
        ///     最低开设人数（活动开始时的最低报名人数要求）
        /// </summary>
        public int MinParticipants { get; set; }

        /// <summary>
        ///     活动状态 0:正常 1:已取消
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        ///     取消原因
        /// </summary>
        [MaxLength(200)]
        public string CancelReason { get; set; }
    }
}