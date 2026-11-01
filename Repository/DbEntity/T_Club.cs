using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    ///     社团表
    /// </summary>
    [Table("T_Club")]
    public class T_Club : BaseModel
    {
        /// <summary>
        ///     社团名称
        /// </summary>
        [MaxLength(20)]
        public string Name { get; set; }

        /// <summary>
        ///     社团简介
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; }

        /// <summary>
        ///     创办时间（自动为新增数据当天日期）
        /// </summary>
        [Required]
        public DateTime EnterData { get; set; }
    }
}