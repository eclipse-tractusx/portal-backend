namespace CatenaX.NetworkServices.Keycloak.ErrorHandling;

[Serializable]
public class KeycloakEntityConflictException : Exception
{
    public KeycloakEntityConflictException() { }
    public KeycloakEntityConflictException(string message) : base(message) { }
    public KeycloakEntityConflictException(string message, System.Exception inner) : base(message, inner) { }
    protected KeycloakEntityConflictException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
