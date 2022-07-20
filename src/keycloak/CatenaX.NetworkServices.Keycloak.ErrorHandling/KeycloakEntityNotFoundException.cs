namespace CatenaX.NetworkServices.Keycloak.ErrorHandling;

[Serializable]
public class KeycloakEntityNotFoundException : Exception
{
    public KeycloakEntityNotFoundException() { }
    public KeycloakEntityNotFoundException(string message) : base(message) { }
    public KeycloakEntityNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    protected KeycloakEntityNotFoundException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
