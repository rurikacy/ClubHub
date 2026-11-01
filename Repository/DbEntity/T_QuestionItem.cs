using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     问卷题目表
    /// </summary>
    [Table("T_QuestionItem")]
    public class QuestionItem : BaseModel
    {
        /// <summary>
        ///     所属问卷ID
        /// </summary>
        [Required]
        public int QuestionId { get; set; }

        /// <summary>
        ///     题目内容
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        /// <summary>
        ///     题目类型：1=单选，2=多选
        /// </summary>
        [Required]
        public byte ItemType { get; set; }

        /// <summary>
        ///     选项内容（JSON格式存储，如：["选项A","选项B","选项C"]）
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Options { get; set; }

        /// <summary>
        ///     题目排序
        /// </summary>
        [Required]
        public int SortOrder { get; set; }
    }
}