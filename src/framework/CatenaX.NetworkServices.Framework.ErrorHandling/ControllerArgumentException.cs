namespace CatenaX.NetworkServices.Framework.ErrorHandling;

/// <inheritdoc />
[Serializable]
public class ControllerArgumentException : Exception
{
    public ControllerArgumentException(string message) : base(message) { }

    public ControllerArgumentException(ArgumentException argumentException)
        : this(argumentException.Message)
    {
        ParamName = argumentException.ParamName;
    }

    public ControllerArgumentException(string message, string paramName)
        : base(String.Format("{0} (Parameter '{1}')", message, paramName))
    {
        ParamName = paramName;
    }

    public string? ParamName { get; }

    protected ControllerArgumentException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}