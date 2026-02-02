using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace AuthGateway.ReverseProxy;

public class ReverseProxyConfigProvider : IProxyConfigProvider
{
    private readonly IProxyConfig _config;

    public ReverseProxyConfigProvider(string ordersApiUrl, string adminApiUrl)
    {
        var routes = new[]
        {
            new RouteConfig
            {
                RouteId = "orders-get-route",
                ClusterId = "orders-cluster",
                Match = new RouteMatch
                {
                    Path = "/api/orders/{**catch-all}"
                },
                AuthorizationPolicy = "UserChile",
                Order = 1
            },
            new RouteConfig
            {
                RouteId = "orders-modify-route",
                ClusterId = "orders-cluster",
                Match = new RouteMatch
                {
                    Path = "/api/orders",
                    Methods = new[] { "POST", "PUT", "PATCH", "DELETE" }
                },
                AuthorizationPolicy = "RequireAdmin",
                Order = 0
            },
            new RouteConfig
            {
                RouteId = "admin-reports-route",
                ClusterId = "admin-cluster",
                Match = new RouteMatch
                {
                    Path = "/api/admin/reports/{**catch-all}"
                },
                AuthorizationPolicy = "AdminITWorkingHours",
                Order = 0
            },
            new RouteConfig
            {
                RouteId = "admin-route",
                ClusterId = "admin-cluster",
                Match = new RouteMatch
                {
                    Path = "/api/admin/{**catch-all}"
                },
                AuthorizationPolicy = "AdminChileIT",
                Order = 1
            }
        };

        var clusters = new[]
        {
            new ClusterConfig
            {
                ClusterId = "orders-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["orders-destination"] = new DestinationConfig
                    {
                        Address = ordersApiUrl
                    }
                }
            },
            new ClusterConfig
            {
                ClusterId = "admin-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["admin-destination"] = new DestinationConfig
                    {
                        Address = adminApiUrl
                    }
                }
            }
        };

        _config = new CustomProxyConfig(routes, clusters);
    }

    public IProxyConfig GetConfig() => _config;
}

internal class CustomProxyConfig : IProxyConfig
{
    private static readonly CancellationTokenSource _cancellationTokenSource = new();
    private static readonly IChangeToken _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

    public IReadOnlyList<RouteConfig> Routes { get; }
    public IReadOnlyList<ClusterConfig> Clusters { get; }

    public CustomProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        Routes = routes;
        Clusters = clusters;
    }

    public IChangeToken ChangeToken => _changeToken;
}
