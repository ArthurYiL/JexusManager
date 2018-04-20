﻿// Copyright (c) Lex Li. All rights reserved.
// 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace JexusManager.Features.Authentication
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Web.Administration;

    public class WindowsItem
    {
        public ConfigurationElement Element { get; set; }

        public WindowsItem(ConfigurationElement element)
        {
            Element = element;
            var extended = element.ChildElements["extendedProtection"];
            TokenChecking = Convert.ToInt32((long)extended["tokenChecking"]);
            UseKernelMode = (bool)element["useKernelMode"];

            var providers = element.GetCollection("providers");
            Providers = new List<ProviderItem>(providers.Count);
            foreach (ConfigurationElement provider in providers)
            {
                Providers.Add(new ProviderItem(provider));
            }
        }

        public List<ProviderItem> Providers { get; set; }

        public int TokenChecking { get; set; }
        public bool UseKernelMode { get; set; }

        public void Apply()
        {
            Element["useKernelMode"] = UseKernelMode;
            var extended = Element.ChildElements["extendedProtection"];
            extended["tokenChecking"] = (long)TokenChecking;

            var providers = Element.GetCollection("providers");
            providers.Clear();
            foreach (var item in Providers)
            {
                if (item.Element == null)
                {
                    item.Element = providers.CreateElement();
                    item.Apply();
                }

                providers.Add(item.Element);
            }
        }
    }
}
