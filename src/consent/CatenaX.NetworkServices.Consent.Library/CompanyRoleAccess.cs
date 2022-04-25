﻿using CatenaX.NetworkServices.Consent.Library.Data;
using Npgsql;
using System.Collections.Generic;

namespace CatenaX.NetworkServices.Consent.Library
{
    public class CompanyRoleAccess
    {
        private readonly string connectionString;

        public CompanyRoleAccess(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEnumerable<ConsentForCompanyRole> GetCompanyRoles()
        {
            var query = "select * from get_company_role()";
            using (var con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                var cmd = new NpgsqlCommand(query, con);
               return cmd.ExecuteReader().Query<ConsentForCompanyRole>();

            }
        }

        public IEnumerable<ConsentForCompanyRole> GetCompanyRoles(int roleId)
        {
            var query = $"select * from get_company_role({roleId})";
            using (var con = new NpgsqlConnection(connectionString))
            {
                con.Open();

                var cmd = new NpgsqlCommand(query, con);
                return cmd.ExecuteReader().Query<ConsentForCompanyRole>();

            }
        }
    }
}
