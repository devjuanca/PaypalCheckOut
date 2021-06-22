using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaypalCheckOut.PaypalHelper
{

    //Clase donde se mapea la data en appsettings Seccion PayPalSettings
    public class PayPalSettings
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string UrlAPI { get; set; }
        public string ReturnURL { get; set; }
        public string CancelURL { get; set; }
        public string Mode { get; set; }
        public string ConnectionTimeout { get; set; }
        public string RequestRetries { get; set; }



    }
}
