/*
 * 
 * Copyright (c) 2013, MasterCard International Incorporated
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

using Nop.Core;
using Nop.Plugin.Payments.Simplify.Models;
using Nop.Plugin.Payments.Simplify;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Security;
using Nop.Services.Logging;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework;


namespace Nop.Plugin.Payments.Simplify.Controllers
{
    public class SimplifyController : BaseNopPaymentController
    {
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger _logger;
        private readonly SimplifyPaymentSettings _simplifyPaymentSettings;

        public SimplifyController(
            SimplifyPaymentSettings simplifyPaymentSettings,
            IStoreService storeService,
            ISettingService settingService,
            IWorkContext workContext,
            IEncryptionService encryptionService,
            ILogger logger
        )
        {
            this._simplifyPaymentSettings = simplifyPaymentSettings;
            this._logger = logger;
            this._storeService = storeService;
            this._settingService = settingService;
            this._workContext = workContext;
            this._encryptionService = encryptionService;
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
                model.LiveMode_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.LiveMode, storeScope);
                model.SandboxPublicKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.SandboxPublicKey, storeScope);
                model.SandboxPrivateKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.SandboxPrivateKey, storeScope);
                model.LivePublicKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.LivePublicKey, storeScope);
                model.LivePrivateKey_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.LivePrivateKey, storeScope);
                model.DebugEnabled_OverrideForStore = _settingService.SettingExists(simplifyPaymentSettings, x => x.DebugEnabled, storeScope);
            }

            Log("Configure model " + model.ToString());

            return View("Nop.Plugin.Payments.Simplify.Views.PaymentSimplify.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            Log("Configure [post]");
            if (!ModelState.IsValid)
                return Configure();

            Log("Configure model " + model.ToString());

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var simplifyPaymentSettings = _settingService.LoadSetting<SimplifyPaymentSettings>(storeScope);

            Log("Configure storeScope " + storeScope);
            Log("Configure settings " + simplifyPaymentSettings.ToString());

            //save settings
            simplifyPaymentSettings.LiveMode = model.LiveMode;
            simplifyPaymentSettings.SandboxPublicKey = model.SandboxPublicKey;
            simplifyPaymentSettings.SandboxPrivateKey = _encryptionService.EncryptText(model.SandboxPrivateKey);
            simplifyPaymentSettings.LivePublicKey = model.LivePublicKey;
            simplifyPaymentSettings.LivePrivateKey = _encryptionService.EncryptText(model.LivePrivateKey);
            simplifyPaymentSettings.DebugEnabled = model.DebugEnabled;



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
            _settingService.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {

            var model = new PaymentInfoModel();

            if (_simplifyPaymentSettings.LiveMode)
            {
                model.PublicKey = _simplifyPaymentSettings.LivePublicKey.Trim();
            }
            else
            {
                model.PublicKey = _simplifyPaymentSettings.SandboxPublicKey.Trim();
            }

            model.DebugEnabled = _simplifyPaymentSettings.DebugEnabled;

            Log("PaymentInfo model " + model.ToString());

            return View("Nop.Plugin.Payments.Simplify.Views.PaymentSimplify.PaymentInfo", model);
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

        public void Log(string msg)
        {
            if (_simplifyPaymentSettings != null && _simplifyPaymentSettings.DebugEnabled)
            {
                _logger.Information("SimplifyController." + msg);
            }
        }
    }
}
