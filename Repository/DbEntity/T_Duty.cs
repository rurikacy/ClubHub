using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     部门表
    /// </summary>
    [Table("T_Duty")]
    public class Duty : BaseModel
    {
        /// <summary>
        ///     所属社团（必须为社团表中包含的ID）
        /// </summary>
        [Required]
        public int ClubId { get; set; }

        /// <summary>
        ///     部门名称
        /// </summary>
        [MaxLength(30)]
        public string Name { get; set; }

        /// <summary>
        ///     部门简介
        /// </summary>
        [MaxLength(100)]
        public string Description { get; set; }

        /// <summary>
        ///     部门人数（根据部门成员表自动计算）
        /// </summary>
        public int MemberNum { get; set; }
    }
}