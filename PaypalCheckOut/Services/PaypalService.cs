using Microsoft.Extensions.Options;
using PaypalCheckOut.Models;
using PaypalCheckOut.PaypalHelper;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayPalHttp;
using PayPalCheckoutSdk.Payments;
using Money = PayPalCheckoutSdk.Orders.Money;

/// <summary>
/// Clase de servicios para el flujo de pago en Paypal. utilizando Intent = "Authorize" (El pago requiere autorización y ofrece reembolso.
/// (ver diferencia con Intent = "Capture")
/// </summary>
namespace Service
{
    public interface IPaypalService
    {
        Task<HttpResponse> CreateOrder(IEnumerable<ProductModel> cart_products);
        Task<HttpResponse> AuthorizeOrder(string order_id);
        Task<HttpResponse> CaptureOrder(string AuthorizationId);
    }

    public class PaypalService : IPaypalService
    {
        PayPalSettings _credentials;
        SandboxEnvironment _sandboxEnvironment;

        public PaypalService(IOptions<PayPalSettings> credentials)
        {
            _credentials = credentials.Value;
            
            _sandboxEnvironment = new SandboxEnvironment(credentials.Value.ClientId, credentials.Value.Secret);

        }

        //Primer paso - Crear la Orden.
        public async Task<HttpResponse> CreateOrder(IEnumerable<ProductModel> cart_products )
        {
            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(BuildRequestBody(cart_products));

            //3. Call PayPal to set up a transaction
            var response = await PayPalClient.client(_sandboxEnvironment).Execute(request);

            

            return response;
        }
        //Segundo Paso -  Autorizar la Orden
        public async Task<HttpResponse> AuthorizeOrder(string order_id)
        {
            var request = new OrdersAuthorizeRequest(order_id);
            request.Prefer("return=representation");
            request.RequestBody(new AuthorizeRequest());
            var response = await PayPalClient.client(_sandboxEnvironment).Execute(request);

            return response;
        }
        //Tercer Paso - Capturar la orden y autorizar el pago.
        public async Task<HttpResponse> CaptureOrder(string AuthorizationId)
        {
            var request = new AuthorizationsCaptureRequest(AuthorizationId);
            request.Prefer("return=representation");
            request.RequestBody(new CaptureRequest());
            var response = await PayPalClient.client(_sandboxEnvironment).Execute(request);

         
            return response;
        }

        private OrderRequest BuildRequestBody(IEnumerable<ProductModel> cart_products)
        {
            List<PurchaseUnitRequest> purchaseUnitRequests = new List<PurchaseUnitRequest>();
            purchaseUnitRequests.Add(GetPurchaseUnitRequest(cart_products));
                

            OrderRequest orderRequest = new OrderRequest
            {
                CheckoutPaymentIntent = "AUTHORIZE",
                ApplicationContext = GetApplicationContext(),
                PurchaseUnits = purchaseUnitRequests
            };
            
            return orderRequest;

        }
        private PurchaseUnitRequest GetPurchaseUnitRequest(IEnumerable<ProductModel> cart_items)
        {
            var total_amount = cart_items.Sum(a => a.Price * a.Quantity); //Total a pagar por los items.

            List<Item> items = new List<Item>();
            foreach (var item in cart_items)
            {
                items.Add(new Item
                {
                    Name = item.Name,
                    Category = item.Category,
                    Description = item.Description,
                    Quantity = item.Quantity.ToString(),
                    UnitAmount = new PayPalCheckoutSdk.Orders.Money()
                    {
                        CurrencyCode = "EUR",
                        Value = item.Price.MyToString()
                    }
                });
            }

            AmountWithBreakdown amount = new AmountWithBreakdown
            {
                CurrencyCode = "EUR",
                Value = TotalToPay(total_amount).MyToString(),

                AmountBreakdown = new AmountBreakdown
                {
                    ItemTotal = new Money { CurrencyCode = "EUR", Value = total_amount.ToString() },
                    TaxTotal = new Money
                    {
                        CurrencyCode = "EUR",
                        Value = (total_amount * TransactionPayments.Tax).MyToString()
                        
                    },
                    Shipping = new Money { CurrencyCode = "EUR", Value = TransactionPayments.Shipping.MyToString() },
                    Handling = new Money { CurrencyCode = "EUR", Value = TransactionPayments.Handling.MyToString() },
                    Discount = new Money { 
                        CurrencyCode = "EUR", 
                        Value = (total_amount * TransactionPayments.Discount).MyToString() },


                }

            };

            ShippingDetail shippingDetail = new ShippingDetail
            {
                Name = new Name
                {
                    FullName = "John Doe"
                },
                AddressPortable = new AddressPortable
                {
                    AddressLine1 = "123 Townsend St",
                    AddressLine2 = "Floor 6",
                    AdminArea2 = "San Francisco",
                    AdminArea1 = "CA",
                    PostalCode = "94107",
                    CountryCode = "US"
                }
            };


            return new PurchaseUnitRequest
            {
                ReferenceId = "PUHF",
                Description = "Electronics",
                CustomId = "CUST-Electronics",
                SoftDescriptor = "HighElectronics",
                AmountWithBreakdown = amount,
                Items = items,
                ShippingDetail = shippingDetail
                

            };
        }

        private ApplicationContext GetApplicationContext()
        {
            return new ApplicationContext
            {
                BrandName = "Electronics INC",
                LandingPage = "BILLING",
                UserAction = "CONTINUE",
                ShippingPreference = "SET_PROVIDED_ADDRESS",
                CancelUrl = _credentials.CancelURL,
                ReturnUrl = _credentials.ReturnURL
            };
        }

        private double TotalToPay(double total_amount_items)
        {
            return total_amount_items +

                   (total_amount_items * TransactionPayments.Tax) + //Sumando el % de taxes
                   TransactionPayments.Shipping + //Sumando el shipping
                   TransactionPayments.Handling - //Sumando el handling
                   (total_amount_items * TransactionPayments.Discount); //Restando el % de discount.
        }

    }
}


