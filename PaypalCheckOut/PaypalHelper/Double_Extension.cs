using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaypalCheckOut.PaypalHelper
{
    public static class Double_Extension
    {
        public static string MyToString(this double num)
        {
            return num.ToString().Replace(',', '.');
        }
    }
}
