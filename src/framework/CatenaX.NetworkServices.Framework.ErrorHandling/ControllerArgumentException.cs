namespace CatenaX.NetworkServices.Framework.ErrorHandling;

/// <inheritdoc />
[Serializable]
public class ControllerArgumentException : Exception
{
    public ControllerArgumentException(string message) : base(message) { }

    public ControllerArgumentException(ArgumentException argumentException)
        : this(argumentException.Message, argumentException.ParamName) { }

    public ControllerArgumentException(string? message, string? paramName)
        : base(message)
    {
        ParamName = paramName;
    }

    public string? ParamName { get; }

    protected ControllerArgumentException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}