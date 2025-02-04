using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace BuyCafe.Models
{
    public class OrderItem
    {
        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "訂單Id")]
        public int OrderId { get; set; } //表示外鍵

        [JsonIgnore]
        [ForeignKey("OrderId")] //標示 MyOrder 是一個外鍵導航屬性
        public virtual Order MyOrder { get; set; } //導航屬性

        [Display(Name = "訂單Id")]
        public int ProductId { get; set; } //純記錄用但不設置外鍵 (之後要做編輯再來設置)

        [Display(Name = "商品圖片路徑")]
        public string ImagePath { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "商品名稱")]
        [MaxLength(100)]
        public string Name { get; set; }  //抓商品名稱

        [Display(Name = "客製化選項")]
        public string Customization { get; set; }  //只抓選項存成Json(ex:{"去冰","半糖"}

        [Display(Name ="數量")]
        public int Quantity { get; set; }

        [Display(Name = "單價")]
        public int Price { get; set; }  //要記得加入客製化加價跟加價購

        [Display(Name="點數")]
        public int? Point { get; set; }

        [Display(Name = "創建時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;


    }
}