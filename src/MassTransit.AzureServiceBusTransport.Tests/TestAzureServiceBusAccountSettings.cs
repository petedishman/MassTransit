﻿// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.AzureServiceBusTransport.Tests
{
    using System;
    using Microsoft.ServiceBus;


    public class TestAzureServiceBusAccountSettings :
        AzureServiceBusTokenProviderSettings
    {
        const string KeyName = "MassTransitBuild";
        const string SharedAccessKey = "PjgbZPZT+S8UPZi7W3t9oSJZzx4HmKjv4o0Xxl6/JaM=";
        readonly TokenScope _tokenScope;
        readonly TimeSpan _tokenTimeToLive;

        public TestAzureServiceBusAccountSettings()
        {
            _tokenTimeToLive = TimeSpan.FromDays(1);
            _tokenScope = TokenScope.Namespace;
        }

        string AzureServiceBusTokenProviderSettings.KeyName
        {
            get { return KeyName; }
        }

        string AzureServiceBusTokenProviderSettings.SharedAccessKey
        {
            get { return SharedAccessKey; }
        }

        TimeSpan AzureServiceBusTokenProviderSettings.TokenTimeToLive
        {
            get { return _tokenTimeToLive; }
        }

        TokenScope AzureServiceBusTokenProviderSettings.TokenScope
        {
            get { return _tokenScope; }
        }
    }
}