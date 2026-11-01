using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     问卷答案表
    /// </summary>
    [Table("T_QuestionAnswer")]
    public class QuestionAnswer : BaseModel
    {
        /// <summary>
        ///     所属问卷ID
        /// </summary>
        [Required]
        public int QuestionId { get; set; }

        /// <summary>
        ///     所属题目ID
        /// </summary>
        [Required]
        public int ItemId { get; set; }

        /// <summary>
        ///     回答成员ID
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        /// <summary>
        ///     答案内容（选项索引，多选用逗号分隔，如："0" 或 "0,2,3"）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Answer { get; set; }

        /// <summary>
        ///     提交时间
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }
    }
}