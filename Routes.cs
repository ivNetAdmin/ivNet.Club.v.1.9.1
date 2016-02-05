using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace ivNet.Club
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            var rdl = new List<RouteDescriptor>();
            rdl.AddRange(AdminRoutes());
            rdl.AddRange(SiteRoutes());
            return rdl;
        }

        private IEnumerable<RouteDescriptor> AdminRoutes()
        {
            return new[]
            {
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/admin/team-selection",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubAdmin"},
                            {"action", "TeamSelection"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                }, 
                new RouteDescriptor
                {
                     Route = new Route(
                        "club/admin/fixture",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubAdmin"},
                            {"action", "Fixture"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/admin/membership",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubAdmin"},
                            {"action", "Membership"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },  new RouteDescriptor
                {
                     Route = new Route(
                        "club/admin/registration",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubAdmin"},
                            {"action", "Registration"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/admin/configure",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubAdmin"},
                            {"action", "Configure"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/admin/user-stories",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubAdmin"},
                            {"action", "UserStories"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        private IEnumerable<RouteDescriptor> SiteRoutes()
        {
            return new[]
            {
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/stats",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubSite"},
                            {"action", "Stats"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/fixtures",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubSite"},
                            {"action", "Fixtures"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/registration",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubSite"},
                            {"action", "Registration"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/members/my-details",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubSite"},
                            {"action", "MyDetails"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor
                {
                     Route = new Route(
                        "club/members/my-availability",
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"},
                            {"controller", "ClubSite"},
                            {"action", "MyAvailability"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "ivNet.Club"}
                        },
                        new MvcRouteHandler())
                }


                
            };
        }
    }
}