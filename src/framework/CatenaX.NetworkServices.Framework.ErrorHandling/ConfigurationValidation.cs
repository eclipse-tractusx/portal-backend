namespace CatenaX.NetworkServices.Framework.ErrorHandling;

public class ConfigurationValidation<TSettings>
{
    public ConfigurationValidation<TSettings> NotNull(object item, Func<string> getItemName)
    {
        if(item == null)
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null");
        }
        return this;
    }

    public ConfigurationValidation<TSettings> NotDefault(object item, Func<string> getItemName)
    {
        if(item == default)
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null");
        }
        return this;
    }

    public ConfigurationValidation<TSettings> NotNullOrEmpty(string? item, Func<string> getItemName)
    {
        if(string.IsNullOrEmpty(item))
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null or empty");
        }
        return this;
    }

    public ConfigurationValidation<TSettings> NotNullOrWhiteSpace(string? item, Func<string> getItemName)
    {
        if(string.IsNullOrWhiteSpace(item))
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null or whitespace");
        }
        return this;
    }
}
