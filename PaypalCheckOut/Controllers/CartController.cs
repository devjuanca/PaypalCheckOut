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
            
            ViewBag.Total = _cartServices.GetTotalToPay();
            
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
