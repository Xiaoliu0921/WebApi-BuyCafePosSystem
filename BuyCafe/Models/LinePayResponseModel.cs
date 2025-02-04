using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuyCafe.Models
{
    // Responcse inf.transactionId、PaymentUrl.Web、PaymentUrl.App
    public class LinePayResponseDto
    {
        public string returnCode { get; set; }
        public string returnMessage { get; set; }
        public LinePayResponseInfoDto info { get; set; }
    }

    public class LinePayResponseInfoDto
    {
        public long transactionId { get; set; }
        public PaymentUrlDto paymentUrl { get; set; }
    }

    public class PaymentUrlDto
    {
        public string web { get; set; }
        public string app { get; set; }
    }

    // Confirm Response
    public class PaymentConfirmResponseDto
    {
        public string returnCode { get; set; }
        public string returnMessage { get; set; }
    }
}