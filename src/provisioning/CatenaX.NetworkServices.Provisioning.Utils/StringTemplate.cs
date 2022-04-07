using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CatenaX.NetworkServices.Provisioning.Utils
{
    public class StringTemplate
    {
        private readonly string _Template;
        public StringTemplate(string template)
        {
            _Template = template;
        }

        public string Apply(IDictionary<string,string> parameters)
        {
            return Regex.Replace(
                _Template,
                @"\{(\w+)\}", //replaces any text surrounded by { and }
                m =>
                {
                    string value;
                    return parameters.TryGetValue(m.Groups[1].Value, out value) ? value : "null";
                }
            );
        }
    }
}
