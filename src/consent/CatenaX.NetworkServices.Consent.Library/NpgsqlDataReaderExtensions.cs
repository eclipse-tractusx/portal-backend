using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Cosent.Library
{
    public static class NpgsqlDataReaderExtensions
    {
        public static IEnumerable<T> Query<T>(this NpgsqlDataReader reader) where T : new()
        {
            var proerteis = typeof(T).GetProperties();
            var pt = proerteis.Select(p => new { p, jpn = p.CustomAttributes.Where(ca => ca.AttributeType == typeof(JsonPropertyNameAttribute)).SelectMany(ca => ca.ConstructorArguments.Select(c => c.Value)).FirstOrDefault() as string, t = p.PropertyType });

            var l = new List<T>();
            while (reader.Read())
            {
                var cr = new T();
                for (int i = 0; i < proerteis.Count(); i++)
                {
                    var pti = pt.FirstOrDefault(p => p.jpn == reader.GetName(i));
                  
                    if(pti == null)
                    {
                        continue;
                    }

                    var o = default(object);
                    try
                    {
                        if (pti.t == typeof(Int32))
                        {
                            o = reader.GetInt32(i);

                        }
                        else if (pti.t == typeof(string))
                        {
                            o = reader.GetString(i);
                        }
                        else if (pti.t == typeof(DateTime))
                        {
                            o = reader.GetTimeStamp(i).ToDateTime();
                        }
                        pti.p.SetValue(cr, o);
                    }
                    catch (Exception e)
                    {

                        throw new Exception($"{pti.p.Name}{Environment.NewLine}{e.Message}");
                    }
                  

                }
                l.Add(cr);

            }
            return l;
        }
    }
}
