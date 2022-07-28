namespace CatenaX.NetworkServices.Keycloak.ErrorHandling;

[Serializable]
public class KeycloakNoSuccessException : Exception
{
    public KeycloakNoSuccessException() { }
    public KeycloakNoSuccessException(string message) : base(message) { }
    public KeycloakNoSuccessException(string message, System.Exception inner) : base(message, inner) { }
    protected KeycloakNoSuccessException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
