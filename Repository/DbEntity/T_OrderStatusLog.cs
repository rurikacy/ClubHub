using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    /// 订单状态变更日志表
    /// </summary>
    [Table("T_OrderStatusLog")]
    public class OrderStatusLog : BaseModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// 原状态
        /// </summary>
        [Required]
        public byte OldStatus { get; set; }

        /// <summary>
        /// 新状态
        /// </summary>
        [Required]
        public byte NewStatus { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        [Required]
        public int OperatorId { get; set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        [MaxLength(50)]
        public string OperatorName { get; set; }

        /// <summary>
        /// 操作备注
        /// </summary>
        [MaxLength(500)]
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }
    }
}
