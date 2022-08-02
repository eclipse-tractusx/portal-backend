namespace CatenaX.NetworkServices.Framework.ErrorHandling;

/// <inheritdoc />
[Serializable]
public class ControllerArgumentException : ArgumentException
{
    public ControllerArgumentException(string message) : base(message) { }

    public ControllerArgumentException(string? message, string? paramName)
        : base(message, paramName: paramName)
    {
    }

    protected ControllerArgumentException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}