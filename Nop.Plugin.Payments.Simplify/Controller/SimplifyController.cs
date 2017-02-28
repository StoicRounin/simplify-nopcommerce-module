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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;

using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Simplify.Models;
using Nop.Plugin.Payments.Simplify;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Security;
using Nop.Services.Logging;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework;


namespace Nop.Plugin.Payments.Simplify.Controllers
{
    public class SimplifyController : BasePaymentController
    {
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger _logger;
        private readonly SimplifyPaymentSettings _simplifyPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IOrderProcessingService _orderProcessingService;

        public SimplifyController(
            SimplifyPaymentSettings simplifyPaymentSettings,
            IStoreService storeService,
            ISettingService settingService,
            IWorkContext workContext,
            IEncryptionService encryptionService,
            ILogger logger,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IOrderService orderService,
            IPaymentService paymentService,
            PaymentSettings paymentSettings,
            IOrderProcessingService orderProcessingService
        )
        {
            this._simplifyPaymentSettings = simplifyPaymentSettings;
            this._logger = logger;
            this._storeService = storeService;
            this._settingService = settingService;
            this._workContext = workContext;
            this._encryptionService = encryptionService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._webHelper = webHelper;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._paymentSettings = paymentSettings;
            this._orderProcessingService = orderProcessingService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            Log("Configure storeScope " + storeScope);

            var simplifyPaymentSettings = _settingService.LoadSetting<SimplifyPaymentSettings>(storeScope);
            Log("Configure settings " + simplifyPaymentSettings.ToString());

            var model = new ConfigurationModel();
            model.HostedMode = simplifyPaymentSettings.HostedMode;
            model.LiveMode = simplifyPaymentSettings.LiveMode;
            model.SandboxPublicKey = simplifyPaymentSettings.SandboxPublicKey;
            model.SandboxPrivateKey = _encryptionService.DecryptText(simplifyPaymentSettings.SandboxPrivateKey);
            model.LivePublicKey = simplifyPaymentSettings.LivePublicKey;
            model.LivePrivateKey = _encryptionService.DecryptText(simplifyPaymentSettings.LivePrivateKey);
            model.DebugEnabled = simplifyPaymentSettings.DebugEnabled;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                _logger.Information("Configure checking store scope overrides");
                model.HostedMode_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.HostedMode, storeScope);
                model.LiveMode_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.LiveMode, storeScope);
                model.SandboxPublicKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.SandboxPublicKey, storeScope);
                model.SandboxPrivateKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.SandboxPrivateKey, storeScope);
                model.LivePublicKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.LivePublicKey, storeScope);
                model.LivePrivateKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.LivePrivateKey, storeScope);
                model.DebugEnabled_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.DebugEnabled, storeScope);
            }

            Log("Configure model " + model.ToString());

            return View("~/Plugins/Payments.Simplify/Views/PaymentSimplify/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            Log("Configure [post]");
            if (!ModelState.IsValid)
            {
                Log("Configure [post] model state invalid");
                return Configure();
            }

            Log("Configure model " + model.ToString());

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var simplifyPaymentSettings = _settingService.LoadSetting<SimplifyPaymentSettings>(storeScope);

            Log("Configure storeScope " + storeScope);
            Log("Configure settings " + simplifyPaymentSettings.ToString());

            //save settings
            simplifyPaymentSettings.HostedMode = model.HostedMode;
            simplifyPaymentSettings.LiveMode = model.LiveMode;
            simplifyPaymentSettings.SandboxPublicKey = model.SandboxPublicKey;
            simplifyPaymentSettings.SandboxPrivateKey = _encryptionService.EncryptText(model.SandboxPrivateKey);
            simplifyPaymentSettings.LivePublicKey = model.LivePublicKey;
            simplifyPaymentSettings.LivePrivateKey = _encryptionService.EncryptText(model.LivePrivateKey);
            simplifyPaymentSettings.DebugEnabled = model.DebugEnabled;

            if (model.HostedMode_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(simplifyPaymentSettings, x => x.HostedMode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(simplifyPaymentSettings, x => x.HostedMode, storeScope);

            if (model.LiveMode_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(simplifyPaymentSettings, x => x.LiveMode, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(simplifyPaymentSettings, x => x.LiveMode, storeScope);

            if (model.SandboxPublicKey_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(simplifyPaymentSettings, x => x.SandboxPublicKey, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(simplifyPaymentSettings, x => x.SandboxPublicKey, storeScope);

            if (model.SandboxPrivateKey_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(simplifyPaymentSettings, x => x.SandboxPrivateKey, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(simplifyPaymentSettings, x => x.SandboxPrivateKey, storeScope);

            if (model.LivePublicKey_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(simplifyPaymentSettings, x => x.LivePublicKey, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(simplifyPaymentSettings, x => x.LivePublicKey, storeScope);

            if (model.LivePrivateKey_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(simplifyPaymentSettings, x => x.LivePrivateKey, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(simplifyPaymentSettings, x => x.LivePrivateKey, storeScope);

            if (model.DebugEnabled_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(simplifyPaymentSettings, x => x.DebugEnabled, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(simplifyPaymentSettings, x => x.DebugEnabled, storeScope);


            //now clear settings cache
            Log("Configure clearing cache");
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = createPaymentInfoModel();
            if (_simplifyPaymentSettings.HostedMode)
            {
                return View("~/Plugins/Payments.Simplify/Views/PaymentSimplify/HostedPaymentInfo.cshtml", model);
            }
            else
            {
                return View("~/Plugins/Payments.Simplify/Views/PaymentSimplify/PaymentInfo.cshtml", model);
            }
        }

        private PaymentInfoModel createPaymentInfoModel()
        {

            var model = new PaymentInfoModel();
            Log("PaymentInfo _simplifyPaymentSettings " + _simplifyPaymentSettings.ToString());

            model.PublicKey = getPublicKey();

            model.DebugEnabled = _simplifyPaymentSettings.DebugEnabled;

            Log("PaymentInfo model " + model.ToString());

            return model;
        }

        private String getPublicKey()
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

        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            Log("ValidatePaymentForm");

            return new List<string>();
        }


        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            Log("GetPaymentInfo");

            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.CustomValues.Add("SIMPLIFY_TOKEN", form["SimplifyToken"]);
            return paymentInfo;
        }

        public ActionResult HostedPaymentRedirect(string amount, string reference, string customerName)
        {
            var model = new PaymentInfoModel();
            model.Amount = amount;
            model.PublicKey = getPublicKey();
            model.RedirectUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentSimplify/PostHostedPaymentHandler";
            model.Reference = reference;
            model.StoreName = _storeContext.CurrentStore.Name;
            model.CustomerName = customerName;

            return View("~/Plugins/Payments.Simplify/Views/PaymentSimplify/HostedPaymentRedirect.cshtml", model);
        }
        
        
        [ValidateInput(false)]
        public ActionResult PostHostedPaymentHandler(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Simplify") as SimplifyPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Simplify Commerce module cannot be loaded");

            var cardToken = _webHelper.QueryString<string>("cardToken");
            var reference = _webHelper.QueryString<string>("reference");
            var amount = _webHelper.QueryString<string>("amount");

            if (cardToken == null && reference == null && amount == null)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            Guid orderNumberGuid = Guid.Empty;
            try
            {
                orderNumberGuid = new Guid(reference);
            }
            catch { }

            Order order = _orderService.GetOrderByGuid(orderNumberGuid);
            if (order == null)
            {
                Log("Could not find order " + orderNumberGuid);
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            long orderTotal = Convert.ToInt64(amount);
            decimal total = (decimal)orderTotal / (decimal)100.00;
            if (!order.OrderTotal.Equals(total))
            {
                string errorStr = string.Format("Returned order total {0} doesn't equal order total {1}. Order# {2}.", total, order.OrderTotal, order.Id);
                AddOrderNote(order, errorStr);

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ProcessPaymentResult result = processor.ProcessPayment(cardToken, orderTotal, orderNumberGuid.ToString(), order.StoreId);
 
            if (result.NewPaymentStatus != PaymentStatus.Paid)
            {
                string errorStr = string.Format("Submitting payment to order {0} failed with status {1}", order.Id, result.NewPaymentStatus);
                AddOrderNote(order, errorStr);

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            //mark order as paid
            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                try
                {
                    ProcessPaymentRequest request = new ProcessPaymentRequest();
                    request.CustomValues.Add("SIMPLIFY_TOKEN", cardToken);
                    order.CustomValuesXml = request.SerializeCustomValues();
                }
                catch (Exception e)
                {
                    _logger.Warning("Error setting token " + cardToken + " to custom values", e);
                }

                order.AuthorizationTransactionId = result.AuthorizationTransactionId;
                order.AuthorizationTransactionCode = result.AuthorizationTransactionCode;
                _orderService.UpdateOrder(order);

                _orderProcessingService.MarkOrderAsPaid(order);
            }
            else
            {
                string errorStr = string.Format("Payment was successful with AuthorizationTransactionId {0}, AuthorizationTransactionCode {1}, but not able to mark order {2} as PAID. Check if currently paid, refunded or voided. Current order status {3}", result.AuthorizationTransactionId, result.AuthorizationTransactionCode, order.Id, order.PaymentStatus);
                AddOrderNote(order, errorStr);
            }

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        private void AddOrderNote(Order order, string note)
        {
            _logger.Error(note);
            //order note
            order.OrderNotes.Add(new OrderNote
            {
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
        }

        public void Log(string msg)
        {
            if (_simplifyPaymentSettings != null && _simplifyPaymentSettings.DebugEnabled)
            {
                _logger.Information("SimplifyController." + msg);
            }
        }
    }
}
