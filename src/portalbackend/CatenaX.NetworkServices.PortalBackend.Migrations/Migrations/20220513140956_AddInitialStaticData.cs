using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class AddInitialStaticData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "portal",
                table: "countries",
                columns: new[] { "alpha2code", "alpha3code", "country_name_de", "country_name_en" },
                values: new object[,]
                {
                    { "AD", "AND", "Andorra", "Andorra" },
                    { "AE", "ARE", "United Arab Emirates (the)", "United Arab Emirates (the)" },
                    { "AF", "AFG", "Afghanistan", "Afghanistan" },
                    { "AG", "ATG", "Antigua and Barbuda", "Antigua and Barbuda" },
                    { "AI", "AIA", "Anguilla", "Anguilla" },
                    { "AL", "ALB", "Albania", "Albania" },
                    { "AM", "ARM", "Armenia", "Armenia" },
                    { "AO", "AGO", "Angola", "Angola" },
                    { "AQ", "ATA", "Antarctica", "Antarctica" },
                    { "AR", "ARG", "Argentina", "Argentina" },
                    { "AS", "ASM", "American Samoa", "American Samoa" },
                    { "AT", "AUT", "Austria", "Austria" },
                    { "AU", "AUS", "Australia", "Australia" },
                    { "AW", "ABW", "Aruba", "Aruba" },
                    { "AX", "ALA", "Åland Islands", "Åland Islands" },
                    { "AZ", "AZE", "Azerbaijan", "Azerbaijan" },
                    { "BA", "BIH", "Bosnien and Herzegovenien", "Bosnia and Herzegovina" },
                    { "BB", "BRB", "Barbados", "Barbados" },
                    { "BD", "BGD", "Bangladesh", "Bangladesh" },
                    { "BE", "BEL", "Belgium", "Belgium" },
                    { "BF", "BFA", "Burkina Faso", "Burkina Faso" },
                    { "BG", "BGR", "Bulgarien", "Bulgaria" },
                    { "BH", "BHR", "Bahrain", "Bahrain" },
                    { "BI", "BDI", "Burundi", "Burundi" },
                    { "BJ", "BEN", "Benin", "Benin" },
                    { "BL", "BLM", "Saint Barthélemy", "Saint Barthélemy" },
                    { "BM", "BMU", "Bermuda", "Bermuda" },
                    { "BN", "BRN", "Brunei Darussalam", "Brunei Darussalam" },
                    { "BO", "BOL", "Bolivien", "Bolivia (Plurinational State of)" },
                    { "BQ", "BES", "Bonaire, Sint Eustatius and Saba", "Bonaire, Sint Eustatius and Saba" },
                    { "BR", "BRA", "Brasilien", "Brazil" },
                    { "BS", "BHS", "Bahamas", "Bahamas (the)" },
                    { "BT", "BTN", "Bhutan", "Bhutan" },
                    { "BV", "BVT", "Bouvet Island", "Bouvet Island" },
                    { "BW", "BWA", "Botswana", "Botswana" },
                    { "BY", "BLR", "Belarus", "Belarus" },
                    { "BZ", "BLZ", "Belize", "Belize" },
                    { "CA", "CAN", "Kanada", "Canada" },
                    { "CC", "CCK", "Cocos (Keeling) Islands (the)", "Cocos (Keeling) Islands (the)" },
                    { "CD", "COD", "Kongo", "Congo (the Democratic Republic of the)" },
                    { "CF", "CAF", "Central African Republic (the)", "Central African Republic (the)" },
                    { "CH", "CHE", "Switzerland", "Switzerland" },
                    { "CI", "CIV", "Côte d'Ivoire", "Côte d'Ivoire" },
                    { "CK", "COK", "Cook Islands", "Cook Islands (the)" },
                    { "CL", "CHL", "Chile", "Chile" },
                    { "CM", "CMR", "Cameroon", "Cameroon" },
                    { "CN", "CHN", "China", "China" },
                    { "CO", "COL", "Kolumbien", "Colombia" },
                    { "CR", "CRI", "Costa Rica", "Costa Rica" },
                    { "CU", "CUB", "Kuba", "Cuba" },
                    { "CV", "CPV", "Cabo Verde", "Cabo Verde" },
                    { "CW", "CUW", "Curaçao", "Curaçao" },
                    { "CX", "CXR", "Weihnachtsinseln", "Christmas Island" },
                    { "CY", "CYP", "Zypern", "Cyprus" },
                    { "CZ", "CZE", "Tschechien", "Czechia" },
                    { "DE", "DEU", "Deutschland", "Germany" },
                    { "DJ", "DJI", "Djibouti", "Djibouti" },
                    { "DK", "DNK", "Dänemark", "Denmark" },
                    { "DM", "DMA", "Dominica", "Dominica" },
                    { "DO", "DOM", "Dominikanische Republik", "Dominican Republic (the)" },
                    { "DZ", "DZA", "Algeria", "Algeria" },
                    { "EC", "ECU", "Ecuador", "Ecuador" },
                    { "EE", "EST", "Estonia", "Estonia" },
                    { "EG", "EGY", "Ägypten", "Egypt" },
                    { "EH", "ESH", "Western Sahara*", "Western Sahara*" },
                    { "ER", "ERI", "Eritrea", "Eritrea" },
                    { "ES", "ESP", "Spain", "Spain" },
                    { "ET", "ETH", "Ethiopia", "Ethiopia" },
                    { "FI", "FIN", "Finland", "Finland" },
                    { "FJ", "FJI", "Fiji", "Fiji" },
                    { "FK", "FLK", "Falkland Islands (the) [Malvinas]", "Falkland Islands (the) [Malvinas]" },
                    { "FM", "FSM", "Micronesia (Federated States of)", "Micronesia (Federated States of)" },
                    { "FO", "FRO", "Faroe Islands (the)", "Faroe Islands (the)" },
                    { "FR", "FRA", "Frankreich", "France" },
                    { "GA", "GAB", "Gabon", "Gabon" },
                    { "GB", "GBR", "United Kingdom of Great Britain and Northern Ireland (the)", "United Kingdom of Great Britain and Northern Ireland (the)" },
                    { "GD", "GRD", "Grenada", "Grenada" },
                    { "GE", "GEO", "Georgia", "Georgia" },
                    { "GF", "GUF", "French Guiana", "French Guiana" },
                    { "GG", "GGY", "Guernsey", "Guernsey" },
                    { "GH", "GHA", "Ghana", "Ghana" },
                    { "GI", "GIB", "Gibraltar", "Gibraltar" },
                    { "GL", "GRL", "Greenland", "Greenland" },
                    { "GM", "GMB", "Gambia (the)", "Gambia (the)" },
                    { "GN", "GIN", "Guinea", "Guinea" },
                    { "GP", "GLP", "Guadeloupe", "Guadeloupe" },
                    { "GQ", "GNQ", "Equatorial Guinea", "Equatorial Guinea" },
                    { "GR", "GRC", "Greece", "Greece" },
                    { "GS", "SGS", "South Georgia and the South Sandwich Islands", "South Georgia and the South Sandwich Islands" },
                    { "GT", "GTM", "Guatemala", "Guatemala" },
                    { "GU", "GUM", "Guam", "Guam" },
                    { "GW", "GNB", "Guinea-Bissau", "Guinea-Bissau" },
                    { "GY", "GUY", "Guyana", "Guyana" },
                    { "HK", "HKG", "Hong Kong", "Hong Kong" },
                    { "HM", "HMD", "Heard Island and McDonald Islands", "Heard Island and McDonald Islands" },
                    { "HN", "HND", "Honduras", "Honduras" },
                    { "HR", "HRV", "Kroatien", "Croatia" },
                    { "HT", "HTI", "Haiti", "Haiti" },
                    { "HU", "HUN", "Hungary", "Hungary" },
                    { "ID", "IDN", "Indonesia", "Indonesia" },
                    { "IE", "IRL", "Ireland", "Ireland" },
                    { "IL", "ISR", "Israel", "Israel" },
                    { "IM", "IMN", "Isle of Man", "Isle of Man" },
                    { "IN", "IND", "India", "India" },
                    { "IO", "IOT", "British Indian Ocean Territory", "British Indian Ocean Territory (the)" },
                    { "IQ", "IRQ", "Iraq", "Iraq" },
                    { "IR", "IRN", "Iran (Islamic Republic of)", "Iran (Islamic Republic of)" },
                    { "IS", "ISL", "Iceland", "Iceland" },
                    { "IT", "ITA", "Italy", "Italy" },
                    { "JE", "JEY", "Jersey", "Jersey" },
                    { "JM", "JAM", "Jamaica", "Jamaica" },
                    { "JO", "JOR", "Jordan", "Jordan" },
                    { "JP", "JPN", "Japan", "Japan" },
                    { "KE", "KEN", "Kenya", "Kenya" },
                    { "KG", "KGZ", "Kyrgyzstan", "Kyrgyzstan" },
                    { "KH", "KHM", "Cambodia", "Cambodia" },
                    { "KI", "KIR", "Kiribati", "Kiribati" },
                    { "KM", "COM", "Comoros", "Comoros (the)" },
                    { "KN", "KNA", "Saint Kitts and Nevis", "Saint Kitts and Nevis" },
                    { "KP", "PRK", "Korea (the Democratic People's Republic of)", "Korea (the Democratic People's Republic of)" },
                    { "KR", "KOR", "Korea (the Republic of)", "Korea (the Republic of)" },
                    { "KW", "KWT", "Kuwait", "Kuwait" },
                    { "KY", "CYM", "Cayman Islands (the)", "Cayman Islands (the)" },
                    { "KZ", "KAZ", "Kazakhstan", "Kazakhstan" },
                    { "LA", "LAO", "Lao People's Democratic Republic (the)", "Lao People's Democratic Republic (the)" },
                    { "LB", "LBN", "Lebanon", "Lebanon" },
                    { "LC", "LCA", "Saint Lucia", "Saint Lucia" },
                    { "LI", "LIE", "Liechtenstein", "Liechtenstein" },
                    { "LK", "LKA", "Sri Lanka", "Sri Lanka" },
                    { "LR", "LBR", "Liberia", "Liberia" },
                    { "LS", "LSO", "Lesotho", "Lesotho" },
                    { "LT", "LTU", "Lithuania", "Lithuania" },
                    { "LU", "LUX", "Luxembourg", "Luxembourg" },
                    { "LV", "LVA", "Latvia", "Latvia" },
                    { "LY", "LBY", "Libya", "Libya" },
                    { "MA", "MAR", "Morocco", "Morocco" },
                    { "MC", "MCO", "Monaco", "Monaco" },
                    { "MD", "MDA", "Moldova (the Republic of)", "Moldova (the Republic of)" },
                    { "ME", "MNE", "Montenegro", "Montenegro" },
                    { "MF", "MAF", "Saint Martin (French part)", "Saint Martin (French part)" },
                    { "MG", "MDG", "Madagascar", "Madagascar" },
                    { "MH", "MHL", "Marshall Islands (the)", "Marshall Islands (the)" },
                    { "MK", "MKD", "North Macedonia", "North Macedonia" },
                    { "ML", "MLI", "Mali", "Mali" },
                    { "MM", "MMR", "Myanmar", "Myanmar" },
                    { "MN", "MNG", "Mongolia", "Mongolia" },
                    { "MO", "MAC", "Macao", "Macao" },
                    { "MP", "MNP", "Northern Mariana Islands (the)", "Northern Mariana Islands (the)" },
                    { "MQ", "MTQ", "Martinique", "Martinique" },
                    { "MR", "MRT", "Mauritania", "Mauritania" },
                    { "MS", "MSR", "Montserrat", "Montserrat" },
                    { "MT", "MLT", "Malta", "Malta" },
                    { "MU", "MUS", "Mauritius", "Mauritius" },
                    { "MV", "MDV", "Maldives", "Maldives" },
                    { "MW", "MWI", "Malawi", "Malawi" },
                    { "MX", "MEX", "Mexico", "Mexico" },
                    { "MY", "MYS", "Malaysia", "Malaysia" },
                    { "MZ", "MOZ", "Mozambique", "Mozambique" },
                    { "NA", "NAM", "Namibia", "Namibia" },
                    { "NC", "NCL", "New Caledonia", "New Caledonia" },
                    { "NE", "NER", "Niger (the)", "Niger (the)" },
                    { "NF", "NFK", "Norfolk Island", "Norfolk Island" },
                    { "NG", "NGA", "Nigeria", "Nigeria" },
                    { "NI", "NIC", "Nicaragua", "Nicaragua" },
                    { "NL", "NLD", "Netherlands (the)", "Netherlands (the)" },
                    { "NO", "NOR", "Norway", "Norway" },
                    { "NP", "NPL", "Nepal", "Nepal" },
                    { "NR", "NRU", "Nauru", "Nauru" },
                    { "NU", "NIU", "Niue", "Niue" },
                    { "NZ", "NZL", "New Zealand", "New Zealand" },
                    { "OM", "OMN", "Oman", "Oman" },
                    { "PA", "PAN", "Panama", "Panama" },
                    { "PE", "PER", "Peru", "Peru" },
                    { "PF", "PYF", "French Polynesia", "French Polynesia" },
                    { "PG", "PNG", "Papua New Guinea", "Papua New Guinea" },
                    { "PH", "PHL", "Philippines (the)", "Philippines (the)" },
                    { "PK", "PAK", "Pakistan", "Pakistan" },
                    { "PL", "POL", "Poland", "Poland" },
                    { "PM", "SPM", "Saint Pierre and Miquelon", "Saint Pierre and Miquelon" },
                    { "PN", "PCN", "Pitcairn", "Pitcairn" },
                    { "PR", "PRI", "Puerto Rico", "Puerto Rico" },
                    { "PS", "PSE", "Palestine, State of", "Palestine, State of" },
                    { "PT", "PRT", "Portugal", "Portugal" },
                    { "PW", "PLW", "Palau", "Palau" },
                    { "PY", "PRY", "Paraguay", "Paraguay" },
                    { "QA", "QAT", "Qatar", "Qatar" },
                    { "RE", "REU", "Réunion", "Réunion" },
                    { "RO", "ROU", "Romania", "Romania" },
                    { "RS", "SRB", "Serbia", "Serbia" },
                    { "RU", "RUS", "Russian Federation (the)", "Russian Federation (the)" },
                    { "RW", "RWA", "Rwanda", "Rwanda" },
                    { "SA", "SAU", "Saudi Arabia", "Saudi Arabia" },
                    { "SB", "SLB", "Solomon Islands", "Solomon Islands" },
                    { "SC", "SYC", "Seychelles", "Seychelles" },
                    { "SD", "SDN", "Sudan (the)", "Sudan (the)" },
                    { "SE", "SWE", "Sweden", "Sweden" },
                    { "SG", "SGP", "Singapore", "Singapore" },
                    { "SH", "SHN", "Saint Helena, Ascension and Tristan da Cunha", "Saint Helena, Ascension and Tristan da Cunha" },
                    { "SI", "SVN", "Slovenia", "Slovenia" },
                    { "SJ", "SJM", "Svalbard and Jan Mayen", "Svalbard and Jan Mayen" },
                    { "SK", "SVK", "Slovakia", "Slovakia" },
                    { "SL", "SLE", "Sierra Leone", "Sierra Leone" },
                    { "SM", "SMR", "San Marino", "San Marino" },
                    { "SN", "SEN", "Senegal", "Senegal" },
                    { "SO", "SOM", "Somalia", "Somalia" },
                    { "SR", "SUR", "Suriname", "Suriname" },
                    { "SS", "SSD", "South Sudan", "South Sudan" },
                    { "ST", "STP", "Sao Tome and Principe", "Sao Tome and Principe" },
                    { "SV", "SLV", "El Salvador", "El Salvador" },
                    { "SX", "SXM", "Sint Maarten (Dutch part)", "Sint Maarten (Dutch part)" },
                    { "SY", "SYR", "Syrian Arab Republic (the)", "Syrian Arab Republic (the)" },
                    { "SZ", "SWZ", "Eswatini", "Eswatini" },
                    { "TC", "TCA", "Turks and Caicos Islands (the)", "Turks and Caicos Islands (the)" },
                    { "TD", "TCD", "Chad", "Chad" },
                    { "TF", "ATF", "French Southern Territories (the)", "French Southern Territories (the)" },
                    { "TG", "TGO", "Togo", "Togo" },
                    { "TH", "THA", "Thailand", "Thailand" },
                    { "TJ", "TJK", "Tajikistan", "Tajikistan" },
                    { "TK", "TKL", "Tokelau", "Tokelau" },
                    { "TL", "TLS", "Timor-Leste", "Timor-Leste" },
                    { "TM", "TKM", "Turkmenistan", "Turkmenistan" },
                    { "TN", "TUN", "Tunisia", "Tunisia" },
                    { "TO", "TON", "Tonga", "Tonga" },
                    { "TR", "TUR", "Turkey", "Turkey" },
                    { "TT", "TTO", "Trinidad and Tobago", "Trinidad and Tobago" },
                    { "TV", "TUV", "Tuvalu", "Tuvalu" },
                    { "TW", "TWN", "Taiwan (Province of China)", "Taiwan (Province of China)" },
                    { "TZ", "TZA", "Tanzania, the United Republic of", "Tanzania, the United Republic of" },
                    { "UA", "UKR", "Ukraine", "Ukraine" },
                    { "UG", "UGA", "Uganda", "Uganda" },
                    { "UM", "UMI", "United States Minor Outlying Islands (the)", "United States Minor Outlying Islands (the)" },
                    { "US", "USA", "United States of America (the)", "United States of America (the)" },
                    { "UY", "URY", "Uruguay", "Uruguay" },
                    { "UZ", "UZB", "Uzbekistan", "Uzbekistan" },
                    { "VA", "VAT", "Holy See (the)", "Holy See (the)" },
                    { "VC", "VCT", "Saint Vincent and the Grenadines", "Saint Vincent and the Grenadines" },
                    { "VE", "VEN", "Venezuela (Bolivarian Republic of)", "Venezuela (Bolivarian Republic of)" },
                    { "VG", "VGB", "Virgin Islands (British)", "Virgin Islands (British)" },
                    { "VI", "VIR", "Virgin Islands (U.S.)", "Virgin Islands (U.S.)" },
                    { "VN", "VNM", "Viet Nam", "Viet Nam" },
                    { "VU", "VUT", "Vanuatu", "Vanuatu" },
                    { "WF", "WLF", "Wallis and Futuna", "Wallis and Futuna" },
                    { "WS", "WSM", "Samoa", "Samoa" },
                    { "YE", "YEM", "Yemen", "Yemen" },
                    { "YT", "MYT", "Mayotte", "Mayotte" },
                    { "ZA", "ZAF", "South Africa", "South Africa" },
                    { "ZM", "ZMB", "Zambia", "Zambia" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "languages",
                columns: new[] { "short_name", "long_name_de", "long_name_en" },
                values: new object[,]
                {
                    { "de", "deutsch", "german" },
                    { "en", "englisch", "english" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "use_cases",
                columns: new[] { "id", "name", "shortname" },
                values: new object[,]
                {
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b86"), "Traceability", "T" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b87"), "Sustainability & CO2-Footprint", "CO2" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b88"), "Manufacturing as a Service", "MaaS" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b89"), "Real-Time Control", "RTC" },
                    { new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90"), "Modular Production", "MP" },
                    { new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd8"), "Circular Economy", "CE" },
                    { new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd9"), "None", "None" },
                    { new Guid("41e4a4c0-aae4-41c0-97c9-ebafde410de4"), "Demand and Capacity Management", "DCM" },
                    { new Guid("6909ccc7-37c8-4088-99ab-790f20702460"), "Business Partner Management", "BPDM" },
                    { new Guid("c065a349-f649-47f8-94d5-1a504a855419"), "Quality Management", "QM" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_role_descriptions",
                columns: new[] { "company_role_id", "language_short_name", "description" },
                values: new object[,]
                {
                    { 1, "de", "Netzwerkteilnehmer" },
                    { 1, "en", "Participant" },
                    { 2, "de", "Softwareanbieter" },
                    { 2, "en", "Application Provider" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 1, "de" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 1, "en" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 2, "de" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "company_role_descriptions",
                keyColumns: new[] { "company_role_id", "language_short_name" },
                keyValues: new object[] { 2, "en" });

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AD");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AQ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AX");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "AZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BB");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BD");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BJ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BQ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BV");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "BZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CD");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CV");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CX");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "CZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "DE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "DJ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "DK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "DM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "DO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "DZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "EC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "EE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "EG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "EH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ER");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ES");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ET");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "FI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "FJ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "FK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "FM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "FO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "FR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GB");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GD");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GP");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GQ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "GY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "HK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "HM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "HN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "HR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "HT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "HU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ID");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IQ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "IT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "JE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "JM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "JO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "JP");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KP");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "KZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LB");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LV");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "LY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MD");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ME");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ML");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MP");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MQ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MV");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MX");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "MZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NP");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "NZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "OM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "PY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "QA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "RE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "RO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "RS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "RU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "RW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SB");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SD");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SJ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ST");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SV");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SX");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "SZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TD");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TH");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TJ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TK");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TL");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TO");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TR");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TV");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TW");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "TZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "UA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "UG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "UM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "US");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "UY");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "UZ");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "VA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "VC");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "VE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "VG");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "VI");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "VN");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "VU");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "WF");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "WS");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "YE");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "YT");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ZA");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "countries",
                keyColumn: "alpha2code",
                keyValue: "ZM");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b86"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b87"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b88"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b89"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd8"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd9"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("41e4a4c0-aae4-41c0-97c9-ebafde410de4"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("6909ccc7-37c8-4088-99ab-790f20702460"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "use_cases",
                keyColumn: "id",
                keyValue: new Guid("c065a349-f649-47f8-94d5-1a504a855419"));

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "languages",
                keyColumn: "short_name",
                keyValue: "de");

            migrationBuilder.DeleteData(
                schema: "portal",
                table: "languages",
                keyColumn: "short_name",
                keyValue: "en");
        }
    }
}
