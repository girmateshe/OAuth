﻿using Common.Configuration;
using Ninject;
using Policy.Pets.App_Start;
using Policy.Pets.Authentication;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Policy.Pets
{
    public static class WebApiConfigeration
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            var kernel = ApiSetup.CreateKernel();

            var resolver = new NinjectDependencyResolver(kernel);

            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            config.Formatters.Add(jsonFormatter);

            var apiConfig = kernel.Get<IConfiguration>();

            if(apiConfig.IsAuthenticationEnabled  == true)
            {
                config.Filters.Add(new ApiAuthorizeAttribute());

                /*
                config.Filters.Add(new AuthorizeAttribute());

                config.MessageHandlers.Add(new JsonWebTokenValidationHandler
                {
                    Audience = "yL9jlFUmwEXx8J5RtJ1GmDZ8bC9iZHED",
                    SymmetricKey = "WWNzA82bwubJ5mDLpfOJI1mHpRE9HrFaZgCJ_YvG3xkPOCo2DD8iDGzs6XmJdMai"
                }); */

            }

            //setup dependency resolver for API           
            config.DependencyResolver = resolver;
            //setup dependency for UI controllers
            //DependencyResolver.SetResolver(resolver.GetService, resolver.GetServices);

            foreach (var item in GlobalConfiguration.Configuration.Formatters)
            {
                if (typeof (JsonMediaTypeFormatter) == item.GetType())
                {
                    item.AddQueryStringMapping("responseType", "json", "application/json");
                    item.AddQueryStringMapping("format", "json", "application/json");
                    item.AddRequestHeaderMapping("Accept", "Xml", System.StringComparison.CurrentCultureIgnoreCase, true, "application/json");
                }
            }
        }
    }
}
