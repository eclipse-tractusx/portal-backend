using CatenaX.NetworkServices.Registration.Service.Model;

namespace CatenaX.NetworkServices.Registration.Service.RegistrationAccess;

public interface IRegistrationDBAccess
{
    Task SetIdp(SetIdp idpToSet);
}
