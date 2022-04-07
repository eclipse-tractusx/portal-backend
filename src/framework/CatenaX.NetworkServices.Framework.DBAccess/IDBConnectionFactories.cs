namespace CatenaX.NetworkServices.Framework.DBAccess

{
    public interface IDBConnectionFactories
    {
        IDBConnectionFactory Get(string identifier);
    }
}
