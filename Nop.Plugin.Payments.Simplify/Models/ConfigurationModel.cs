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
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;


namespace Nop.Plugin.Payments.Simplify.Models
{
    public class ConfigurationModel
    {

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Simplify.Fields.HostedMode")]
        public bool HostedMode { get; set; }
        public bool HostedMode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Simplify.Fields.LiveMode")]
        public bool LiveMode {get; set;}
        public bool LiveMode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Simplify.Fields.SandboxPublicKey")]
        public string SandboxPublicKey { get; set; }
        public bool SandboxPublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Simplify.Fields.SandboxPrivateKey")]
        public string SandboxPrivateKey { get; set; }
        public bool SandboxPrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Simplify.Fields.LivePublicKey")]
        public string LivePublicKey { get; set; }
        public bool LivePublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Simplify.Fields.LivePrivateKey")]
        public string LivePrivateKey { get; set; }
        public bool LivePrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Simplify.Fields.DebugEnabled")]
        public bool DebugEnabled { get; set; }
        public bool DebugEnabled_OverrideForStore { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
              .Append("ConfigurationModel {")
              .Append("ActiveStoreScopeConfiguration=").Append(ActiveStoreScopeConfiguration)
              .Append(", HostedMode=").Append(HostedMode).Append(HostedMode_OverrideForStore ? " (*)" : "")
              .Append(", LiveMode=").Append(LiveMode).Append(LiveMode_OverrideForStore ? " (*)" : "")
              .Append(", SandboxPublicKey=").Append(SandboxPublicKey).Append(SandboxPublicKey_OverrideForStore ? " (*)" : "")
              .Append(", SandboxPrivateKey=").Append(SimplifyPaymentHelper.FirstAndLast4(SandboxPrivateKey)).Append(SandboxPrivateKey_OverrideForStore ? " (*)" : "")
              .Append(", LivePublicKey=").Append(LivePublicKey).Append(LivePublicKey_OverrideForStore ? " (*)" : "")
              .Append(", LivePrivateKey=").Append(SimplifyPaymentHelper.FirstAndLast4(LivePrivateKey)).Append(LivePrivateKey_OverrideForStore ? " (*)" : "")
              .Append("}").ToString();
        }
    }
}