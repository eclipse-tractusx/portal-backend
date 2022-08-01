namespace CatenaX.NetworkServices.Framework.ErrorHandling;

/// <inheritdoc />
[Serializable]
public class UnexpectedConditionException : Exception
{
    public UnexpectedConditionException(string message) : base(message) { }

    protected UnexpectedConditionException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
