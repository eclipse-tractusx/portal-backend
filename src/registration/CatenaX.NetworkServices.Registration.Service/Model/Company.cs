namespace CatenaX.NetworkServices.Registration.Service.Model
{

	public class Company
	{
		public string bpn { get; set; }
		public string parent { get; set; }
		public string accountGroup { get; set; }
		public string name1 { get; set; }
		public string name2 { get; set; }
		public string name3 { get; set; }
		public string name4 { get; set; }
		public string addressVersion { get; set; }
		public string country { get; set; }
		public string city { get; set; }
		public int postalCode { get; set; }
		public string street1 { get; set; }
		public string street2 { get; set; }
		public string street3 { get; set; }
		public int houseNumber { get; set; }
		public string taxNumber1 { get; set; }
		public string taxNumber1Type { get; set; }
		public string taxNumber2 { get; set; }
		public string taxNumber2Type { get; set; }
		public string taxNumber3 { get; set; }
		public string taxNumber3Type { get; set; }
		public string taxNumber4 { get; set; }
		public string taxNumber4Type { get; set; }
		public string taxNumber5 { get; set; }
		public string taxNumber5Type { get; set; }
		public string vatNumber { get; set; }
		public string vatNumberType { get; set; }
	}
}