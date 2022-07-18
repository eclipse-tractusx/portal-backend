namespace CatenaX.NetworkServices.Framework.ErrorHandling;

[Serializable]
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException() { }
    public EntityNotFoundException(string message) : base(message) { }
    public EntityNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    protected EntityNotFoundException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
