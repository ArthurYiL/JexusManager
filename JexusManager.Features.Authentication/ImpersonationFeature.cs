// Copyright (c) Lex Li. All rights reserved.
// 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace JexusManager.Features.Authentication
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Forms;

    using JexusManager.Services;
    using Microsoft.Web.Administration;
    using Microsoft.Web.Management.Client;
    using Microsoft.Web.Management.Client.Extensions;
    using Microsoft.Web.Management.Client.Win32;
    using Microsoft.Web.Management.Server;
    using Module = Microsoft.Web.Management.Client.Module;

    internal class ImpersonationFeature : AuthenticationFeature
    {
        private sealed class FeatureTaskList : TaskList
        {
            private readonly ImpersonationFeature _owner;

            public FeatureTaskList(ImpersonationFeature owner)
            {
                _owner = owner;
            }

            public override ICollection GetTaskItems()
            {
                var result = new ArrayList();
                if (!_owner.IsEnabled)
                {
                    result.Add(new MethodTaskItem("Enable", "Enable", string.Empty).SetUsage());
                }

                if (_owner.IsEnabled)
                {
                    result.Add(new MethodTaskItem("Disable", "Disable", string.Empty).SetUsage());
                }

                result.Add(new MethodTaskItem("Edit", "Edit...", string.Empty).SetUsage());
                return result.ToArray(typeof(TaskItem)) as TaskItem[];
            }

            [Obfuscation(Exclude = true)]
            public void Enable()
            {
                _owner.Enable();
            }

            [Obfuscation(Exclude = true)]
            public void Disable()
            {
                _owner.Disable();
            }

            [Obfuscation(Exclude = true)]
            public void Edit()
            {
                _owner.Edit();
            }
        }

        private FeatureTaskList _taskList;

        public ImpersonationFeature(Module module) : base(module)
        {
        }

        public override TaskList GetTaskList()
        {
            return _taskList ?? (_taskList = new FeatureTaskList(this));
        }

        public override void Load()
        {
            var service = (IConfigurationService)GetService(typeof(IConfigurationService));
            var section = service.GetSection("system.web/identity");
            var enabled = (bool)section["impersonate"];
            SetEnabled(enabled);
        }

        private void Enable()
        {
            var service = (IConfigurationService)GetService(typeof(IConfigurationService));
            var section = service.GetSection("system.web/identity");
            section["impersonate"] = true;
            service.ServerManager.CommitChanges();
            SetEnabled(true);
        }

        private void Disable()
        {
            var service = (IConfigurationService)GetService(typeof(IConfigurationService));
            var section = service.GetSection("system.web/identity");
            section["impersonate"] = false;
            service.ServerManager.CommitChanges();
            SetEnabled(false);
        }

        private void Edit()
        {
            var service = (IConfigurationService)GetService(typeof(IConfigurationService));
            var section = service.GetSection("system.web/identity");
            var dialog = new ImpersonationEditDialog(Module, new ImpersonationItem(section), this);
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            service.ServerManager.CommitChanges();
            OnAuthenticationSettingsSaved();
        }

        public override Version MinimumFrameworkVersion => FxVersion20;

        public override bool ShowHelp()
        {
            DialogHelper.ProcessStart("http://go.microsoft.com/fwlink/?LinkId=210461#Impersonation");
            return true;
        }

        public override bool IsFeatureEnabled
        {
            get
            {
                var service = (IConfigurationService)GetService(typeof(IConfigurationService));
                return service.Scope == ManagementScope.Server && PublicNativeMethods.IsProcessElevated;
            }
        }

        public override AuthenticationType AuthenticationType => AuthenticationType.Other;

        public override string Name => "ASP.NET Impersonation";
    }
}
