using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BuyCafe.Models
{
    public class Order
    {
        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(50)]
        [Display(Name = "顧客電話")]
        public string CustomerPhone { get; set; }   //1冰度 2甜度 3加價購

        [Display(Name = "訂單狀態")]
        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.點餐中;

        [Display(Name = "用餐類型")]
        public TypeEnum? Type { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "使用者識別碼")]
        [MaxLength(100)]
        public string Guid { get; set; }

        [Display(Name = "LinePay交易Id")]
        public long? TransactionId { get; set; }

        [Display(Name = "桌號")]
        public int? Table { get; set; }

        [Display(Name = "預約自取時間")]
        public DateTime? TakeTime { get; set; }

        [Display(Name = "總金額")]
        public int? TotalAmount { get; set; }

        [Display(Name ="顧客備註")]
        public string Note { get; set; }

        [Display(Name = "發票方式")]
        public InvoiceEnum? Invoice { get; set; } = InvoiceEnum.紙本;

        [Display(Name = "發票號碼")]
        public string InvoiceNumber {  get; set; }

        [Display(Name = "發票載具或統編")]
        public string invoiceCarrier { get; set; }

        [Display(Name = "用餐類型+編號")]
        public string TypeAndNumber {  get; set; }



        [Display(Name = "創建時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        public virtual ICollection<OrderItem> Items { get; set; } //反向導航屬性 



    }
}