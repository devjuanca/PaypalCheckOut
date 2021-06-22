using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaypalCheckOut.Models
{

    //Clase auxiliar simulado un ambiente donde la transación poseerá otros costes asociados.
    public static class TransactionPayments
    {
        public static double Tax { get; set; } = 0.03;  //Un 3% de impuestos.
        public static double Shipping { get; set; } = 10.55;
        public static double Handling { get; set; } = 5;
        public static double Discount { get; set; } = 0.01;  //1% de descuento
    }
}
