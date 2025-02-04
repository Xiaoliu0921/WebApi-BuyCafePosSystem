using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BuyCafe.Models
{
    public class Member
    {

        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(50)]
        [Display(Name ="顧客電話")]
        public string Phone { get; set; }

        [Display(Name = "點數")]
        public int Point { get; set; } = 0;

        [Display(Name ="性別")]
        public GenderEnum? Gender { get; set; }

        [Display(Name = "姓名")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Display(Name="生日")]
        public DateTime? Birthday { get; set; }


        [Display(Name = "創建時間")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;



    }
}