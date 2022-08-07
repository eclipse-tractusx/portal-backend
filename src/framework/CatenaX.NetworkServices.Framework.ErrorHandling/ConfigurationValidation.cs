namespace CatenaX.NetworkServices.Framework.ErrorHandling;

public class ConfigurationValidation
{
    public static void ValidateNotNull<TSettings>(object item, Func<string> getItemName)
    {
        if(item == null)
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null");
        }
    }

    public static void ValidateNotDefault<TSettings>(object item, Func<string> getItemName)
    {
        if(item == default)
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null");
        }
    }

    public static void ValidateNotNullOrEmpty<TSettings>(string? item, Func<string> getItemName)
    {
        if(string.IsNullOrEmpty(item))
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null or empty");
        }
    }

    public static void ValidateNotNullOrWhiteSpace<TSettings>(string? item, Func<string> getItemName)
    {
        if(string.IsNullOrWhiteSpace(item))
        {
            throw new ConfigurationException($"{typeof(TSettings).Name}: {getItemName()} must not be null or whitespace");
        }
    }
}
