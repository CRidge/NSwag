﻿//-----------------------------------------------------------------------
// <copyright file="SwaggerExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using NSwag.AspNet.Owin.Middlewares;
using NSwag.SwaggerGeneration;
using NSwag.SwaggerGeneration.WebApi;
using Owin;

namespace NSwag.AspNet.Owin
{
    /// <summary>Provides OWIN extensions to enable Swagger UI.</summary>
    public static class SwaggerExtensions
    {
        #region Swagger

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssembly">The Web API assembly to search for controller types.</param>
        /// <param name="settings">The Swagger generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwagger(
            this IAppBuilder app,
            Assembly webApiAssembly,
            SwaggerSettings settings)
        {
            return app.UseSwagger(new[] { webApiAssembly }, settings);
        }

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssemblies">The Web API assemblies to search for controller types.</param>
        /// <param name="settings">The Swagger generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwagger(
            this IAppBuilder app,
            IEnumerable<Assembly> webApiAssemblies,
            SwaggerSettings settings)
        {
            var controllerTypes = webApiAssemblies.SelectMany(WebApiToSwaggerGenerator.GetControllerClasses);
            return app.UseSwagger(controllerTypes, settings);
        }

        /// <summary>Addes the Swagger generator to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="settings">The Swagger generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwagger(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            SwaggerSettings settings)
        {
            return app.UseSwagger(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        /// <summary>Addes the Swagger generator to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="settings">The Swagger generator settings.</param>
        /// <param name="schemaGenerator">The schema generator.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwagger(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            SwaggerSettings settings,
            SwaggerJsonSchemaGenerator schemaGenerator)
        {
            app.Use<SwaggerMiddleware>(settings.ActualSwaggerRoute, controllerTypes, settings, schemaGenerator);
            app.UseStageMarker(PipelineStage.MapHandler);
            return app;
        }

        /// <summary>Adds the Swagger generator to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="configure">Configure the Swagger generator settings.</param>
        public static IAppBuilder UseSwagger(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            Action<SwaggerSettings> configure = null)
        {
            var settings = new SwaggerSettings();
            configure?.Invoke(settings);
            return app.UseSwagger(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        #endregion

        #region SwaggerUi

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssembly">The Web API assembly to search for controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi(
            this IAppBuilder app,
            Assembly webApiAssembly,
            SwaggerUiSettings settings)
        {
            return app.UseSwaggerUi(new[] { webApiAssembly }, settings);
        }

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssemblies">The Web API assemblies to search for controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi(
            this IAppBuilder app,
            IEnumerable<Assembly> webApiAssemblies,
            SwaggerUiSettings settings)
        {
            var controllerTypes = webApiAssemblies.SelectMany(WebApiToSwaggerGenerator.GetControllerClasses);
            return app.UseSwaggerUi(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        /// <summary>Addes the Swagger UI (only) to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="settings">The Swagger UI settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi(
            this IAppBuilder app,
            SwaggerUiSettings settings)
        {
            return app.UseSwaggerUi(null, settings, null);
        }

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <param name="schemaGenerator">The schema generator.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            SwaggerUiSettings settings,
            SwaggerJsonSchemaGenerator schemaGenerator)
        {
            if (controllerTypes != null)
                app.Use<SwaggerMiddleware>(settings.ActualSwaggerRoute, controllerTypes, settings, schemaGenerator);

            app.Use<RedirectMiddleware>(settings.ActualSwaggerUiRoute, settings.ActualSwaggerRoute);
            app.Use<SwaggerUiIndexMiddleware>(settings.ActualSwaggerUiRoute + "/index.html", settings, "NSwag.AspNet.Owin.SwaggerUi.index.html");
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString(settings.ActualSwaggerUiRoute),
                FileSystem = new EmbeddedResourceFileSystem(typeof(SwaggerExtensions).Assembly, "NSwag.AspNet.Owin.SwaggerUi")
            });
            app.UseStageMarker(PipelineStage.MapHandler);
            return app;
        }

        /// <summary>Adds the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="configure">Configure the Swagger generator and UI settings.</param>
        public static IAppBuilder UseSwaggerUi(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            Action<SwaggerUiSettings> configure = null)
        {
            var settings = new SwaggerUiSettings();
            configure?.Invoke(settings);
            return app.UseSwaggerUi(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        #endregion

        #region SwaggerUi3

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssembly">The Web API assembly to search for controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi3(
            this IAppBuilder app,
            Assembly webApiAssembly,
            SwaggerUi3Settings settings)
        {
            return app.UseSwaggerUi3(new[] { webApiAssembly }, settings);
        }

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssemblies">The Web API assemblies to search for controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi3(
            this IAppBuilder app,
            IEnumerable<Assembly> webApiAssemblies,
            SwaggerUi3Settings settings)
        {
            var controllerTypes = webApiAssemblies.SelectMany(WebApiToSwaggerGenerator.GetControllerClasses);
            return app.UseSwaggerUi3(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        /// <summary>Addes the Swagger UI (only) to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="settings">The Swagger UI settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi3(
            this IAppBuilder app,
            SwaggerUi3Settings settings)
        {
            return app.UseSwaggerUi3(null, settings, null);
        }

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <param name="schemaGenerator">The schema generator.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerUi3(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            SwaggerUi3Settings settings,
            SwaggerJsonSchemaGenerator schemaGenerator)
        {
            if (controllerTypes != null)
                app.Use<SwaggerMiddleware>(settings.ActualSwaggerRoute, controllerTypes, settings, schemaGenerator);

            app.Use<RedirectMiddleware>(settings.ActualSwaggerUiRoute, settings.ActualSwaggerRoute);
            app.Use<SwaggerUiIndexMiddleware>(settings.ActualSwaggerUiRoute + "/index.html", settings, "NSwag.AspNet.Owin.SwaggerUi3.index.html");
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString(settings.ActualSwaggerUiRoute),
                FileSystem = new EmbeddedResourceFileSystem(typeof(SwaggerExtensions).Assembly, "NSwag.AspNet.Owin.SwaggerUi3")
            });
            app.UseStageMarker(PipelineStage.MapHandler);
            return app;
        }

        /// <summary>Adds the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="configure">Configure the Swagger generator and UI settings.</param>
        public static IAppBuilder UseSwaggerUi3(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            Action<SwaggerUi3Settings> configure = null)
        {
            var settings = new SwaggerUi3Settings();
            configure?.Invoke(settings);
            return app.UseSwaggerUi3(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        #endregion

        #region ReDoc

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssembly">The Web API assembly to search for controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerReDoc(
            this IAppBuilder app,
            Assembly webApiAssembly,
            SwaggerReDocSettings settings)
        {
            return app.UseSwaggerReDoc(new[] { webApiAssembly }, settings);
        }

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="webApiAssemblies">The Web API assemblies to search for controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerReDoc(
            this IAppBuilder app,
            IEnumerable<Assembly> webApiAssemblies,
            SwaggerReDocSettings settings)
        {
            var controllerTypes = webApiAssemblies.SelectMany(WebApiToSwaggerGenerator.GetControllerClasses);
            return app.UseSwaggerReDoc(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        /// <summary>Addes the Swagger UI (only) to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="settings">The Swagger UI settings.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerReDoc(
            this IAppBuilder app,
            SwaggerReDocSettings settings)
        {
            return app.UseSwaggerReDoc(null, settings, null);
        }

        /// <summary>Addes the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="settings">The Swagger UI and generator settings.</param>
        /// <param name="schemaGenerator">The schema generator.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseSwaggerReDoc(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            SwaggerReDocSettings settings,
            SwaggerJsonSchemaGenerator schemaGenerator)
        {
            if (controllerTypes != null)
                app.Use<SwaggerMiddleware>(settings.ActualSwaggerRoute, controllerTypes, settings, schemaGenerator);

            app.Use<RedirectMiddleware>(settings.ActualSwaggerUiRoute, settings.ActualSwaggerRoute);
            app.Use<SwaggerUiIndexMiddleware>(settings.ActualSwaggerUiRoute + "/index.html", settings, "NSwag.AspNet.Owin.ReDoc.index.html");
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString(settings.ActualSwaggerUiRoute),
                FileSystem = new EmbeddedResourceFileSystem(typeof(SwaggerExtensions).Assembly, "NSwag.AspNet.Owin.ReDoc")
            });
            app.UseStageMarker(PipelineStage.MapHandler);
            return app;
        }

        /// <summary>Adds the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="controllerTypes">The Web API controller types.</param>
        /// <param name="configure">Configure the Swagger generator and UI settings.</param>
        public static IAppBuilder UseSwaggerReDoc(
            this IAppBuilder app,
            IEnumerable<Type> controllerTypes,
            Action<SwaggerReDocSettings> configure = null)
        {
            var settings = new SwaggerReDocSettings();
            configure?.Invoke(settings);
            return app.UseSwaggerReDoc(controllerTypes, settings, new SwaggerJsonSchemaGenerator(settings));
        }

        #endregion
    }
}
