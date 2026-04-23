using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace SmartEstate.Infrastructure.Logging;

public sealed class SensitivePropertyDestructuringPolicy : IDestructuringPolicy
{
    private static readonly HashSet<string> _sensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordhash", "token", "accesstoken", "refreshtoken",
        "secret", "apikey", "api_key", "authorization", "credential", "credentials"
    };

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        var type = value.GetType();

        if (type.Namespace?.StartsWith("SmartEstate") != true)
        {
            result = null!;
            return false;
        }

        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Select(p => new LogEventProperty(
                p.Name,
                _sensitiveNames.Contains(p.Name)
                    ? new ScalarValue("***REDACTED***")
                    : propertyValueFactory.CreatePropertyValue(p.GetValue(value), true)))
            .ToList();

        result = new StructureValue(properties);
        return true;
    }
}
