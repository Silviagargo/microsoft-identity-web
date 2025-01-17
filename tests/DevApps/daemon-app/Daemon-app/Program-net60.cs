﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//#define UseMicrosoftGraphSdk

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using GraphServiceClient = Microsoft.Graph.GraphServiceClient;

namespace daemon_console
{
    /// <summary>
    /// This sample shows how to query the Microsoft Graph from a daemon application
    /// which uses application permissions.
    /// For more information see https://aka.ms/msal-net-client-credentials
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            
            services.Configure<MicrosoftIdentityOptions>(option => builder.Configuration.Bind(option));
            services.AddMicrosoftGraph(); // or services.AddTokenAcquisition() if you don't need graph

            // Add a cache
            services.AddDistributedTokenCaches();
            services.AddDistributedMemoryCache(); /* or SQL, Redis, ... */

            var app = builder.Build();

#if UseMicrosoftGraphSdk
            GraphServiceClient graphServiceClient = app.Services.GetRequiredService<GraphServiceClient>();
            var users = await graphServiceClient.Users
                .Request()
                .WithAppOnly()
                .GetAsync();
            Console.WriteLine($"{users.Count} users");
#else
            // Get the token acquisition service
            ITokenAcquirer tokenAcquirer = app.Services.GetRequiredService<ITokenAcquirer>();
            var result = await tokenAcquirer.GetAuthenticationResultForAppAsync("https://graph.microsoft.com/.default");
            Console.WriteLine($"Token expires on {result.ExpiresOn}");

#endif
        }
    }
}
