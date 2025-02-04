using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace BuyCafe.Models
{
    public class ProductCustomization
    {
        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "分類Id")]
        public int ProductId { get; set; } //表示外鍵

        [JsonIgnore]
        [ForeignKey("ProductId")] //標示 MyMyProduct 是一個外鍵導航屬性
        public virtual Product MyProduct { get; set; } //導航屬性

        [MaxLength(50)]
        [Display(Name = "客製化名稱")]
        public string Title { get; set; }   //1冰度 2甜度 3加價購


        [Display(Name = "客製化Enum")]
        public int CustomizationEnum {  get; set; }  //1冰度 2甜度 3加價購

        [Display(Name = "創建時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;



    }
}