using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Framework.Models.Validation;

/// <summary>
/// Implementation of <see cref="IValidateOptions{TOptions}"/> that uses DataAnnotation's <see cref="Validator"/> for validation.
/// </summary>
/// <typeparam name="TOptions">The instance being validated.</typeparam>
public class EnumEnumerableValidation<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    private readonly IConfiguration _section;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">The name of the option.</param>
    /// <param name="section"></param>
    public EnumEnumerableValidation(string name, IConfiguration section)
    {
        Name = name;
        _section = section;
    }

    /// <summary>
    /// The options name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Validates a specific named options instance (or all when <paramref name="name"/> is null).
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>The <see cref="ValidateOptionsResult"/> result.</returns>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // Null name is used to configure all named options.
        if (name == null || name != Name)
        {
            return ValidateOptionsResult.Skip;
        }

        var validationResults = new List<ValidationResult>();
        var propertyInfos = options.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(EnumEnumerationAttribute)));
        foreach (var propertyInfo in propertyInfos)
        {
            var configuredValues = new List<string>();
            _section.GetSection(propertyInfo.Name).Bind(configuredValues);
            var propertyType = propertyInfo.PropertyType
                .GetGenericArguments()
                .FirstOrDefault();
            if (propertyType is not { IsEnum: true })
            {
                throw new ConfigurationException($"{propertyInfo.Name} must be of type IEnumerable<Enum>");
            }

            var listType = typeof(List<>).MakeGenericType(propertyType);
            var propertyValue = Activator.CreateInstance(listType);
            _section.GetSection(propertyInfo.Name).Bind(propertyValue);

            var notMatchingValues = configuredValues.Where(value => !Enum.TryParse(propertyType, value, out _));
            if (notMatchingValues.Any())
            {
                validationResults.Add(
                    new ValidationResult($"{string.Join(",", notMatchingValues)} is not a valid value for {propertyType}. Valid values are: {string.Join(", ", propertyType.GetEnumNames())}",
                        new[] { propertyInfo.Name }));
            }
        }

        return validationResults.Any() ?
            ValidateOptionsResult.Fail(validationResults.Select(r => $"DataAnnotation validation failed for members: '{string.Join(",", r.MemberNames)}' with the error: '{r.ErrorMessage}'.").ToList()) :
            ValidateOptionsResult.Success;

        // Ignored if not validating this instance.
    }
}
