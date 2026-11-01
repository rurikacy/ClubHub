using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     问卷表
    /// </summary>
    [Table("T_Question")]
    public class Question : BaseModel
    {
        /// <summary>
        ///     所属社团ID
        /// </summary>
        [Required]
        public int ClubId { get; set; }

        /// <summary>
        ///     发布管理员ID
        /// </summary>
        [Required]
        public int AdminId { get; set; }

        /// <summary>
        ///     问卷标题
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        /// <summary>
        ///     问卷描述
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        ///     创建时间
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }

        /// <summary>
        ///     问卷状态：0=草稿，1=发布中，2=已结束
        /// </summary>
        [Required]
        public byte Status { get; set; }
    }
}