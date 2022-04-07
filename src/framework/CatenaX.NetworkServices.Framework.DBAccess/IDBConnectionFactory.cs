using System.Data;

namespace CatenaX.NetworkServices.Framework.DBAccess

{
    public interface IDBConnectionFactory
    {
        IDbConnection Connection();
        string Schema();
    }
}
