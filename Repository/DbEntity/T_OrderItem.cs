using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.DbEntity
{
    /// <summary>
    /// 订单商品明细表
    /// </summary>
    [Table("T_OrderItem")]
    public class OrderItem : BaseModel
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; }

        /// <summary>
        /// 商品图片
        /// </summary>
        [MaxLength(500)]
        public string ProductImage { get; set; }

        /// <summary>
        /// 商品单价
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// 小计金额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreateTime { get; set; }
    }
}
