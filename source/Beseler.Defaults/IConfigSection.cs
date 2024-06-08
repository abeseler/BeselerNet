using Beseler.Defaults;
using Microsoft.Extensions.DependencyInjection;

namespace Beseler.Defaults;
public interface IConfigSection
{
    static abstract string SectionName { get; }
}

public static class ConfigSectionExtensions
{
    public static IServiceCollection BindConfiguration<T>(this IServiceCollection services) where T : class, IConfigSection
    {
        services.AddOptions<T>().BindConfiguration(T.SectionName);
        return services;
    }
}
