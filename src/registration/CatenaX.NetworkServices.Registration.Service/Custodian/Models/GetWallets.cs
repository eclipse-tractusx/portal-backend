using System;

namespace CatenaX.NetworkServices.Registration.Service.Custodian.Models
{
    public class GetWallets
    {
        public string bpn { get; set; }
        public string name { get; set; }
        public Wallet wallet { get; set; }
    }

    public class Wallet
    {
        public string did { get; set; }
        public DateTime createdAt { get; set; }
        public string publicKey { get; set; }
        public object[] vcs { get; set; }
    }
}



