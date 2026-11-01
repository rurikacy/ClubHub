using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     部门成员表
    /// </summary>
    [Table("T_DutyMember")]
    public class DutyMember : BaseModel
    {
        /// <summary>
        ///     部门ID（必须为部门表中包含的ID）
        /// </summary>
        [Required]
        public int DutyId { get; set; }

        /// <summary>
        ///     成员ID（必须为成员表中包含的ID）
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        /// <summary>
        ///     成员级别 0：成员 1：部长
        /// </summary>
        [Required]
        public byte Role { get; set; }
    }
}