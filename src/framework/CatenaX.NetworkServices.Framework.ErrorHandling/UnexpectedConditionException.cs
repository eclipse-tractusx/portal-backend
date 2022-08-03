namespace CatenaX.NetworkServices.Framework.ErrorHandling;

[Serializable]
public class UnexpectedConditionException : Exception
{
    public UnexpectedConditionException() { }
    public UnexpectedConditionException(string message) : base(message) { }
    public UnexpectedConditionException(string message, Exception inner) : base(message, inner) { }
    protected UnexpectedConditionException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
