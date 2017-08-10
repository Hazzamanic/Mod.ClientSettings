using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Core.Settings;
using Orchard.Mvc.Routes;

namespace Mod.ClientSettings {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Admin/SiteSettings/{groupInfoId}",
                        new RouteValueDictionary {
                            {"area", "Mod.ClientSettings"},
                            {"controller", "Admin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary {
                            
                        },
                        new RouteValueDictionary {
                            {"area", "Mod.ClientSettings"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}