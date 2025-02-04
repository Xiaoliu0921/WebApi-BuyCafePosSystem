using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BuyCafe.Models
{
    public class ProductCategory
    {
        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]
        [Display(Name = "分類名稱")]
        public string Name { get; set; }

        [Display(Name = "排序用值")]
        public int? SortValue { get; set; }   //初始值要跟ID一樣，在保存資料之前，在 Id 被生成後手動設定 SortValue。這可以透過在你的 DbContext 中覆寫 SaveChanges 方法來實現，或者在新增實體的時候手動設定。

        [Display(Name = "創建時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        public virtual ICollection<Product> Products { get; set; } //反向導航屬性 


    }
}