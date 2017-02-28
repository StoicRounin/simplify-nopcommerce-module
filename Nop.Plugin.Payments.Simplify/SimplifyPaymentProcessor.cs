/*
 * 
 * Copyright (c) 2013 - 2017, MasterCard International Incorporated
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification, are 
 * permitted provided that the following conditions are met:
 *
 * Redistributions of source code must retain the above copyright notice, this list of 
 * conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of 
 * conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * Neither the name of the MasterCard International Incorporated nor the names of its 
 * contributors may be used to endorse or promote products derived from this software 
 * without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
 * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; 
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER 
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
 * IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Web;

using Nop.Core;
using Nop.Core.Plugins;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Payments.Simplify.Controllers;
using Nop.Services.Payments;
using Nop.Services.Logging;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Security;

using SimplifyCommerce.Payments;

namespace Nop.Plugin.Payments.Simplify
{
    public class SimplifyPaymentProcessor : BasePlugin, IPaymentMethod
    {
        private readonly SimplifyPaymentSettings _simplifyPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly PaymentsApi _paymentsApi;
        private readonly ILocalizationService _localizationService;
        private readonly IEncryptionService _encryptionService;
        private readonly HttpContextBase _httpContext;
        private readonly IWebHelper _webHelper;
        private readonly IOrderProcessingService _orderProcessingService;

        public SimplifyPaymentProcessor(
            SimplifyPaymentSettings simplifyPaymentSettings,
            ISettingService settingService,
            IPaymentService paymentService,
            IStoreContext storeContext,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IOrderService orderService,
            ILocalizationService localizationService,
            IEncryptionService encryptionService,
            ILogger logger,
            HttpContextBase httpContext,
            IWebHelper webHelper,
            IOrderProcessingService orderProcessingService
        )
        {
            this._simplifyPaymentSettings = simplifyPaymentSettings;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._storeContext = storeContext;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._orderService = orderService;
            this._localizationService = localizationService;
            this._encryptionService = encryptionService;
            this._logger = logger;
            this._httpContext = httpContext;
            this._webHelper = webHelper;
            this._orderProcessingService = orderProcessingService;

            this._paymentsApi = new PaymentsApi();
        }


        #region Methods

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _logger.Information("Installing Simplify Commerce payment plugin");

            // Default settings
            var settings = new SimplifyPaymentSettings()
            {
                HostedMode = true,
                LiveMode = true,
                SandboxPublicKey = "",
                SandboxPrivateKey = "",
                LivePublicKey = "",
                LivePrivateKey = "",
                DebugEnabled = false
            };
            _settingService.SaveSetting(settings);

            // Locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Notes", "Simplify Commerce payment processing. Sign up for an account at <a href=\"http://www.simplify.com\" target=\"_blank\">www.simplify.com</a>");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.RedirectionTip", "You will be redirected to Simplify Commerce site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.HostedMode", "Enable Hosted Mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.HostedMode.Hint", "In hosted mode, securely accept payment on a form hosted by Simplify Commerce. Customer card details are not sent to your site. You must use a hosted payment sandbox or live API key which can be obtained by logging into Simplify Commerce (http://www.simplify.com).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.LiveMode", "Enable Live Mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.LiveMode.Hint", "In live mode your live API keys are used to make real payments.  Otherwise your sandbox API keys are used to make test payments.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPublicKey", "Sandbox Public Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPublicKey.Hint", "Your Simplify Commerce public sandbox API key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPrivateKey", "Sandbox Private Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPrivateKey.Hint", "Your Simplify Commerce private sandbox private API key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePublicKey", "Live Public Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePublicKey.Hint", "Your Simplify Commerce public live API key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePrivateKey", "Live Private Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePrivateKey.Hint", "Your Simplify Commerce private live private API key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.DebugEnabled", "Enable Debug Mode");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Fields.DebugEnabled.Hint", "Enables debug logging for this plugin");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardNumber", "Card Number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardNumber.Hint", "The card number on the front or back of the card.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardExpiry", "Expiry Date");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardExpiry.Hint", "The expiration date on the front of the card.  Month and Year.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardSecurityCode", "Security Code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardSecurityCode.Hint", "The card security code. Usually 3-4 digits printed on the front of the card on on the signature strip on the back.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.Description", "{0} order {1}");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.Exception", "Unable to process payment - an error has occured");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Refund.Reason", "Refund from {0}");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Refund.Exception", "Unable to process refund - an error has occured");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.Error.Currency", "Currency {0} is not supported.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.Error.Token", "Unable to process payment - no card token generated.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.Error.Store", "Unable to process payment - store is not the current context.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Simplify.Payment.Declined", "Payment declined");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            _logger.Information("Uninstalling Simplify Commerce payment plugin");

            // Settings
            _settingService.DeleteSetting<SimplifyPaymentSettings>();

            // Locales
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Notes");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.HostedMode");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.HostedMode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.LiveMode");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.LiveMode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPublicKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPublicKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPrivateKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.SandboxPrivateKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePublicKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePublicKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePrivateKey");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.LivePrivateKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.DebugEnabled");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Fields.DebugEnabled.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardNumber");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardNumber.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardExpiry");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardExpiry.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardSecurityCode");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.CardSecurityCode.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.Description");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.Error.Currency");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.Error.Token");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.Error.Store");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.Declined");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Payment.Exception");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Refund.Reason");
            this.DeletePluginLocaleResource("Plugins.Payments.Simplify.Refund.Exception");


            base.Uninstall();
        }


        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            Log("ProcessPayment settings " + _simplifyPaymentSettings.ToString());

            if (_simplifyPaymentSettings.HostedMode)
            {
                var r = new ProcessPaymentResult();
                r.NewPaymentStatus = PaymentStatus.Pending;
                return r;
            }

            var token = (string)processPaymentRequest.CustomValues["SIMPLIFY_TOKEN"];
            Log(string.Format("ProcessPayment token: {0}, order total: {1}, Order GUID {2}, Store ID {3}",
                    token, processPaymentRequest.OrderTotal, processPaymentRequest.OrderGuid, processPaymentRequest.StoreId));

            long amount = (long)(processPaymentRequest.OrderTotal * 100L);

            return ProcessPayment(token, amount, processPaymentRequest.OrderGuid.ToString(), processPaymentRequest.StoreId);
        }

        public ProcessPaymentResult ProcessPayment(string token, long amount, string orderId, int storeId)
        {
            Log(string.Format("ProcessPayment to Simplify token: {0}, amount: {1}, Order GUID {2}, Store ID {3}",
                    token, amount, orderId, storeId));

            var currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode.ToString();
            Log("ProcessPayment Currency " + currency);
            if (!ValidCurrency(currency))
            {
                var r = new ProcessPaymentResult();
                r.AddError(GetMsg("Plugins.Payments.Simplify.Payment.Error.Currency", currency));
                return r;
            }

            if (String.IsNullOrEmpty(token))
            {
                var r = new ProcessPaymentResult();
                r.AddError(GetMsg("Plugins.Payments.Simplify.Payment.Error.Token"));
                return r;
            }

            if (_storeContext.CurrentStore.Id != storeId)
            {
                var r = new ProcessPaymentResult();
                r.AddError(GetMsg("Plugins.Payments.Simplify.Payment.Error.Store"));
                return r;
            }

            var storeName = _storeContext.CurrentStore.Name;

            Payment payment = new Payment();
            payment.Amount = amount;
            payment.Currency = currency;
            payment.Token = token;
            payment.Description = string.Format(_localizationService.GetResource("Plugins.Payments.Simplify.Payment.Description"), storeName, orderId); // TODO l10n
            payment.Reference = orderId;

            Log("ProcessPayment description: " + payment.Description);
            Log("ProcessPayment reference: " + payment.Reference);

            var result = new ProcessPaymentResult();
            try
            {
                payment = (Payment)_paymentsApi.Create(payment, GetAuth());

                Log(string.Format("ProcessPayment - payment {0} {1}", payment.Id, payment.PaymentStatus));

                if (payment.PaymentStatus.Equals("APPROVED"))
                {
                    Log("ProcessPayment - auth code: " + payment.AuthCode);
                    result.AuthorizationTransactionCode = payment.AuthCode;
                    result.AuthorizationTransactionId = payment.Id;
                    result.NewPaymentStatus = PaymentStatus.Paid;
                }
                else
                {
                    result.AddError(GetMsg("Plugins.Payments.Simplify.Payment.Declined"));
                }

            }
            catch (Exception e)
            {
                LogException("ProcessPayment", e);
                result.AddError(GetMsg("Plugins.Payments.Simplify.Payment.Exception"));
            }

            Log("ProcessPayment paymentStatus: " + result.NewPaymentStatus);
            return result;
        }


        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            Log("PostProcessPayment");

            if (_simplifyPaymentSettings.HostedMode)
            {
                var builder = new StringBuilder();
                builder.AppendFormat("{0}{1}?", _webHelper.GetStoreLocation(false), "Plugins/PaymentSimplify/HostedPaymentRedirect");
                
                var orderTotal = (long)Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2) * 100L;
                builder.AppendFormat("&amount={0}", orderTotal.ToString());
                builder.AppendFormat("&reference={0}", postProcessPaymentRequest.Order.OrderGuid);
                builder.AppendFormat("&customerName={0}",
                    HttpUtility.UrlEncode(string.Format("{0} {1}",
                        postProcessPaymentRequest.Order.Customer.BillingAddress.FirstName,
                        postProcessPaymentRequest.Order.Customer.BillingAddress.LastName)));

                _httpContext.Response.Redirect(builder.ToString());
            }
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return 0;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            Log("Capture");
            return null;
        }


        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            Log(string.Format("Refund Amount: {0}, IsPartial {1}, TransactionCode {2}, TransactionId {3}",
                    refundPaymentRequest.AmountToRefund, refundPaymentRequest.IsPartialRefund,
                    refundPaymentRequest.Order.AuthorizationTransactionCode, refundPaymentRequest.Order.AuthorizationTransactionId));

            var result = new RefundPaymentResult();
            try
            {
                var payment = new Payment(refundPaymentRequest.Order.AuthorizationTransactionId);

                var refund = new Refund();
                refund.Amount = (long)(refundPaymentRequest.AmountToRefund * 100);
                refund.Payment = payment;
                var storeName = _storeContext.CurrentStore.Name;
                refund.Reason = GetMsg("Plugins.Payments.Simplify.Refund.Reason", storeName);
                refund = (Refund)_paymentsApi.Create(refund, GetAuth());

                Log("Refund id " + refund.Id);

                if (refundPaymentRequest.IsPartialRefund)
                {
                    result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
                }
                else
                {
                    result.NewPaymentStatus = PaymentStatus.Refunded;
                }
            }
            catch (Exception e)
            {
                LogException("Refund", e);
                result.AddError(GetMsg("Plugins.Payments.Simplify.Refund.Exception"));
            }

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            Log("Void");

            return null;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            Log("ProcessRecurringPayment");

            return null;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            Log("CancelRecurringPayment");

            return null;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            Log("CanRepostProcessPayment");

            return false;

        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            Log("GetConfigurationRoute");

            actionName = "Configure";
            controllerName = "Simplify";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Simplify.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            Log("GetPaymentInfoRoute");


            actionName = "PaymentInfo";
            controllerName = "Simplify";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Simplify.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(SimplifyController);
        }

        public Boolean ValidCurrency(string currency)
        {
            // support any currency?
            return true;
        }

        private string GetMsg(string key, params Object[] args)
        {
            return string.Format(_localizationService.GetResource(key), args);
        }


        private Authentication GetAuth()
        {
            Authentication auth = new Authentication();
            auth.PublicApiKey = GetPublicKey();
            auth.PrivateApiKey = GetPrivateKey();
            Log("GetAuth public key:  " + auth.PublicApiKey);
            Log("GetAuth private key: " + SimplifyPaymentHelper.FirstAndLast4(auth.PrivateApiKey));

            return auth;
        }


        private string GetPublicKey()
        {
            if (_simplifyPaymentSettings.LiveMode)
            {
                return _simplifyPaymentSettings.LivePublicKey.Trim();
            }
            else
            {
                return _simplifyPaymentSettings.SandboxPublicKey.Trim();
            }
        }

        private string GetPrivateKey()
        {
            if (_simplifyPaymentSettings.LiveMode)
            {
                return _encryptionService.DecryptText(_simplifyPaymentSettings.LivePrivateKey).Trim();
            }
            else
            {
                return _encryptionService.DecryptText(_simplifyPaymentSettings.SandboxPrivateKey).Trim();
            }
        }

        public void LogException(string msg, Exception e)
        {
            String errorData = "";
            if (e != null && e is ApiException)
            {
                errorData = ((ApiException)e).ErrorData.ToString();
            }
            _logger.Error("Simplify ApiException " + errorData, e);
        }

        public void Log(string msg)
        {
            if (_simplifyPaymentSettings != null && _simplifyPaymentSettings.DebugEnabled)
            {
                _logger.Information("SimplifyPaymentProcessor." + msg);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                if (_simplifyPaymentSettings.HostedMode)
                {
                    return PaymentMethodType.Redirection;
                }
                else
                {
                    return PaymentMethodType.Standard;
                }
            }
        }

        #endregion


        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }
    }
}
