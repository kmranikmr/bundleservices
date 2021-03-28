/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Microsoft.AspNetCore.Routing;

namespace Common.WebApiCore
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            /* commented temporarily
            routeBuilder.Routes.Clear();

            routeBuilder.MapRoute(
                name: "Default",
                template: "",
                defaults: new { controller = "NgxApp", action = "Index" }
            );
            */
        }
    }
}
