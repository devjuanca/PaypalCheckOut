using Microsoft.AspNetCore.Mvc;
using PaypalCheckOut.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service;
using Microsoft.AspNetCore.Http;
using PayPalHttp;
using PayPalCheckoutSdk.Orders;
using System.Globalization;

namespace PaypalCheckOut.Controllers
{
    public class CartController : Controller
    {
        readonly ICartServices _cartServices;
        readonly IPaypalService _paypalService;
       

        public CartController(ICartServices cartServices, IPaypalService paypalService)
        {
            _cartServices = cartServices;
            _paypalService = paypalService;
        }

        public IActionResult ShoppingCart()
        {
          


            var culture = CultureInfo.CreateSpecificCulture("es-ES");
            ViewBag.SubTotal = _cartServices.GetTotalToPay().ToString("C", culture);
            ViewBag.Tax = (_cartServices.GetTotalToPay() * TransactionPayments.Tax).ToString("c", culture);
            ViewBag.Shipping = ( TransactionPayments.Shipping).ToString("c", culture);
            ViewBag.Handling = ( TransactionPayments.Handling).ToString("c", culture);
            ViewBag.Discount = (_cartServices.GetTotalToPay() * TransactionPayments.Discount).ToString("c", culture);
            ViewBag.Total = (_cartServices.GetTotalToPay() 
                + (_cartServices.GetTotalToPay() * TransactionPayments.Tax)
                + TransactionPayments.Shipping 
                + TransactionPayments.Handling 
                - (_cartServices.GetTotalToPay() * TransactionPayments.Discount))
                .ToString("c",culture);
           
            return View(_cartServices.GetCartItems());
        }

        [HttpPost]
        public async Task<IActionResult> PayWithPaypal()
        {
            PayPalHttp.HttpResponse response = await _paypalService.CreateOrder(_cartServices.GetCartItems());

            var result = response.Result<Order>();

            HttpContext.Session.SetString("order_id", result.Id);

            return View("ViewOrderDetails", result);
        }


        public async Task<IActionResult> SuccededPayment(string token, string payerID)
        {
            var order_id = HttpContext.Session.GetString("order_id");

            PayPalHttp.HttpResponse response = await _paypalService.AuthorizeOrder(order_id);

            var authorized_order_result = response.Result<Order>();

         //Este proceso debe dividirse un 2 partes. Hacer la autorización y mostra una vista con la data de los resultados, desglose de importes etc
         //Y de ahi poner boton para continuar, capturar el pago y terminar la transaccion.
         
            var authorizationId = authorized_order_result.PurchaseUnits[0].Payments.Authorizations[0].Id;

            var result = await _paypalService.CaptureOrder(authorizationId);

            PayPalCheckoutSdk.Payments.Capture capture_data = result.Result<PayPalCheckoutSdk.Payments.Capture>();

            return View("ViewOrderConfirmation", capture_data);
        }

        public IActionResult CanceledPayment()
        {
            return View();
        }

        public IActionResult ErrorPayment()
        {
            return View();
        }

    }
}
