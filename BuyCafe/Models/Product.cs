using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace BuyCafe.Models
{
    public class Product  //商品
    {
        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "分類Id")]
        public int CategoryId { get; set; } //表示外鍵

        [JsonIgnore]
        [ForeignKey("CategoryId")] //標示 MyCategory 是一個外鍵導航屬性
        public virtual ProductCategory MyCategory { get; set; } //導航屬性

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]
        [Display(Name = "商品名稱")]
        public string Name { get; set; }

        [MaxLength(500)]
        [Display(Name = "商品描述")]
        public string Description { get; set; }

        [Display(Name = "價格")]
        public int Price { get; set; } = 0;

        [Display(Name = "點數")]
        public int? Point { get; set; }

        [Display(Name = "商品圖片路徑")]
        public string ImagePath { get; set; }

        [Display(Name = "排序用值")]
        public int? SortValue { get; set; }   //初始值要跟ID一樣，在保存資料之前，在 Id 被生成後手動設定 SortValue。這可以透過在你的 DbContext 中覆寫 SaveChanges 方法來實現，或者在新增實體的時候手動設定。

        [Display(Name = "剩餘份數")]
        public int? LeftCount { get; set; }

        [Display(Name = "是否可供應")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "可否集點")]
        public bool isPoint { get; set; } = true;

        [Display(Name = "創建時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        public virtual ICollection<ProductCustomization> Customizations { get; set; } //反向導航屬性 


    }
}