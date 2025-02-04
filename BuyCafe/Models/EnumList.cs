using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuyCafe.Models
{
    public class EnumList
    {
    }

    public enum OrderStatusEnum
    {
        點餐中=1,
        待結帳=2,
        準備中=3,
        待取餐=4,
        已完成=5
    }

    public enum TypeEnum
    {
        內用= 1,
        外帶 = 2,
        預約自取 = 3        
    }

    public enum IdentityEnum
    {
        外場=1,
        內場=2,
        店長=3
    }

    public enum ProductCustomizationEnum
    {
        冰度=1,
        冰度僅冰=2,
        甜度=3,
        更換燕麥奶=4,
        要不要鮮奶油=5,
        加價購=6
    }

    public enum GenderEnum
    {
        男=1,
        女=2,
        其他=3
    }

    public enum InvoiceEnum
    {
        載具=1,
        統編=2,
        捐贈發票=3,
        紙本=4
    }

    // 驗證後結果枚舉
    public enum VerificationResult
    {
        Success=1, // 成功
        InvalidCode=2, // 無效
        CodeExpired=3 // 過期
    }
}