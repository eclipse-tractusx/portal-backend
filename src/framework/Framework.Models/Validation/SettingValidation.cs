using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Framework.Models.Validation;

/// <summary>
/// Extension methods for adding options related validation
/// </summary>
public static class SettingValidation
{
    /// <summary>
    /// Register this options instance for validation of the custom <see cref="EnumEnumerationAttribute"/>.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="optionsBuilder">The options builder to add the services to.</param>
    /// <param name="section">The current configuration section</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that additional calls can be chained.</returns>
    public static OptionsBuilder<TOptions> ValidateEnumEnumeration<TOptions>(this OptionsBuilder<TOptions> optionsBuilder, IConfigurationSection section) where TOptions : class
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(new EnumEnumerableValidation<TOptions>(optionsBuilder.Name, section));
        return optionsBuilder;
    }
}
