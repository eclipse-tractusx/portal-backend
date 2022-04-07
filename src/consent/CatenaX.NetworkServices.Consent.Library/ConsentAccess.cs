using CatenaX.NetworkServices.Cosent.Library.Data;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatenaX.NetworkServices.Cosent.Library
{
    public class ConsentAccess
    {
        private readonly string connectionString;

        public ConsentAccess(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEnumerable<Consent> GetConsents(IEnumerable<int> ids)
        {
            var query = $"select * from get_consents(ARRAY [{string.Join(",", ids)}]);";
            using (var con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                var cmd = new NpgsqlCommand(query, con);
                return cmd.ExecuteReader().Query<Consent>();
            }
        }

        public IEnumerable<SignedConsent> GetSignedConsentsForCompanyId(string companyId)
        {
            var query = $"select * from get_signed_consents_for_company_id('{companyId}')";
            using (var con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                var cmd = new NpgsqlCommand(query, con);
                return cmd.ExecuteReader().Query<SignedConsent>();
            }
        }

        public void SignConsent(string companyId, int consentId, int companyRoleId, string signatory)
        {
            var query = $"SELECT sign_consent('{companyId}',{consentId},{companyRoleId}, '{signatory}')";
            using (var con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                var cmd = new NpgsqlCommand(query, con);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
