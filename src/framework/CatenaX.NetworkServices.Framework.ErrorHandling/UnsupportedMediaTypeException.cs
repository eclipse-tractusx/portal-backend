namespace CatenaX.NetworkServices.Framework.ErrorHandling;

[Serializable]
public class UnsupportedMediaTypeException : Exception
{
    public UnsupportedMediaTypeException() { }
    public UnsupportedMediaTypeException(string message) : base(message) { }
    public UnsupportedMediaTypeException(string message, System.Exception inner) : base(message, inner) { }
    protected UnsupportedMediaTypeException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
