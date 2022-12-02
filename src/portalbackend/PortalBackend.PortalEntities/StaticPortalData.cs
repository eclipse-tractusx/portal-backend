/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;

public static class StaticPortalData
{
    public static ICollection<Language> Languages => new[]
    {
        new Language("de", "deutsch", "german"),
        new Language("en", "englisch", "english")
    };

    public static ICollection<CompanyRoleAssignedRoleCollection> CompanyRoleAssignedRoleCollections => new []
    {
        new CompanyRoleAssignedRoleCollection(CompanyRoleId.ACTIVE_PARTICIPANT, new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2")),
        new CompanyRoleAssignedRoleCollection(CompanyRoleId.APP_PROVIDER, new Guid("ec428950-8b64-4646-b336-28af869b5d73")),
        new CompanyRoleAssignedRoleCollection(CompanyRoleId.SERVICE_PROVIDER, new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc")),
        new CompanyRoleAssignedRoleCollection(CompanyRoleId.OPERATOR, new Guid("1a24eca5-901f-4191-84a7-4ef09a894575")),
    };

    public static ICollection<CompanyRoleRegistrationData> CompanyRoleRegistrationDatas => new []
    {
        new CompanyRoleRegistrationData(CompanyRoleId.ACTIVE_PARTICIPANT, true),
        new CompanyRoleRegistrationData(CompanyRoleId.APP_PROVIDER, true),
        new CompanyRoleRegistrationData(CompanyRoleId.SERVICE_PROVIDER, true),
        new CompanyRoleRegistrationData(CompanyRoleId.OPERATOR, false),
    };

    public static ICollection<CompanyRoleDescription> CompanyRoleDescriptions => new[]
    {
        new CompanyRoleDescription(CompanyRoleId.ACTIVE_PARTICIPANT, "de", "Netzwerkteilnehmer"),
        new CompanyRoleDescription(CompanyRoleId.ACTIVE_PARTICIPANT, "en", "Participant"),
        new CompanyRoleDescription(CompanyRoleId.APP_PROVIDER, "de", "Softwareanbieter"),
        new CompanyRoleDescription(CompanyRoleId.APP_PROVIDER, "en", "Application Provider"),
        new CompanyRoleDescription(CompanyRoleId.SERVICE_PROVIDER, "de", "Dienstanbieter"),
        new CompanyRoleDescription(CompanyRoleId.SERVICE_PROVIDER, "en", "Service Provider"),
        new CompanyRoleDescription(CompanyRoleId.OPERATOR, "de", "Betreiber"),
        new CompanyRoleDescription(CompanyRoleId.OPERATOR, "en", "Operator"),
    };

    public static ICollection<UserRoleCollection> UserRoleCollections => new [] {
        new UserRoleCollection(new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2"), "CX Participant"),
        new UserRoleCollection(new Guid("ec428950-8b64-4646-b336-28af869b5d73"), "App Provider"),
        new UserRoleCollection(new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc"), "Service Provider"),
        new UserRoleCollection(new Guid("1a24eca5-901f-4191-84a7-4ef09a894575"), "Operator"),
    };

    public static ICollection<UserRoleCollectionDescription> UserRoleCollectionDescriptions => new [] {
        new UserRoleCollectionDescription(new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2"),"de","CX Netzwerkteilnehmer"),
        new UserRoleCollectionDescription(new Guid("8cb12ea2-aed4-4d75-b041-ba297df3d2f2"),"en","CX Participant"),
        new UserRoleCollectionDescription(new Guid("ec428950-8b64-4646-b336-28af869b5d73"),"de","Softwareanbieter"),
        new UserRoleCollectionDescription(new Guid("ec428950-8b64-4646-b336-28af869b5d73"),"en","App Provider"),
        new UserRoleCollectionDescription(new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc"),"de","Dienstanbieter"),
        new UserRoleCollectionDescription(new Guid("a5b8b1de-7759-4620-9c87-6b6d74fb4fbc"),"en","Service Provider"),
        new UserRoleCollectionDescription(new Guid("1a24eca5-901f-4191-84a7-4ef09a894575"),"de","Betreiber"),
        new UserRoleCollectionDescription(new Guid("1a24eca5-901f-4191-84a7-4ef09a894575"),"en","Operator"),
    };

    public static ICollection<UseCase> UseCases => new[]
    {
        new UseCase(new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd9"), "None", "None"),
        new UseCase(new Guid("1aacde78-35ec-4df3-ba1e-f988cddcbbd8"), "Circular Economy", "CE"),
        new UseCase(new Guid("41e4a4c0-aae4-41c0-97c9-ebafde410de4"), "Demand and Capacity Management", "DCM"),
        new UseCase(new Guid("c065a349-f649-47f8-94d5-1a504a855419"), "Quality Management", "QM"),
        new UseCase(new Guid("6909ccc7-37c8-4088-99ab-790f20702460"), "Business Partner Management", "BPDM"),
        new UseCase(new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b86"), "Traceability", "T"),
        new UseCase(new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b87"), "Sustainability & CO2-Footprint", "CO2"),
        new UseCase(new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b88"), "Manufacturing as a Service", "MaaS"),
        new UseCase(new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b89"), "Real-Time Control", "RTC"),
        new UseCase(new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90"), "Modular Production", "MP")
    };

    public static ICollection<Country> Countries => new[]
    {
        new Country("DE", "Deutschland", "Germany")
        {
            Alpha3Code = "DEU"
        },
        new Country("GB", "United Kingdom of Great Britain and Northern Ireland (the)", "United Kingdom of Great Britain and Northern Ireland (the)")
        {
            Alpha3Code = "GBR"
        },
        new Country("AF", "Afghanistan", "Afghanistan")
        {
            Alpha3Code = "AFG"
        },
        new Country("AL", "Albania", "Albania")
        {
            Alpha3Code = "ALB"
        },
        new Country("DZ", "Algeria", "Algeria")
        {
            Alpha3Code = "DZA"
        },
        new Country("AS", "American Samoa", "American Samoa")
        {
            Alpha3Code = "ASM"
        },
        new Country("AD", "Andorra", "Andorra")
        {
            Alpha3Code = "AND"
        },
        new Country("AO", "Angola", "Angola")
        {
            Alpha3Code = "AGO"
        },
        new Country("AI", "Anguilla", "Anguilla")
        {
            Alpha3Code = "AIA"
        },
        new Country("AQ", "Antarctica", "Antarctica")
        {
            Alpha3Code = "ATA"
        },
        new Country("AG", "Antigua and Barbuda", "Antigua and Barbuda")
        {
            Alpha3Code = "ATG"
        },
        new Country("AR", "Argentina", "Argentina")
        {
            Alpha3Code = "ARG"
        },
        new Country("AM", "Armenia", "Armenia")
        {
            Alpha3Code = "ARM"
        },
        new Country("AW", "Aruba", "Aruba")
        {
            Alpha3Code = "ABW"
        },
        new Country("AU", "Australia", "Australia")
        {
            Alpha3Code = "AUS"
        },
        new Country("AT", "Austria", "Austria")
        {
            Alpha3Code = "AUT"
        },
        new Country("AZ", "Azerbaijan", "Azerbaijan")
        {
            Alpha3Code = "AZE"
        },
        new Country("BS", "Bahamas", "Bahamas (the)")
        {
            Alpha3Code = "BHS"
        },
        new Country("BH", "Bahrain", "Bahrain")
        {
            Alpha3Code = "BHR"
        },
        new Country("BD", "Bangladesh", "Bangladesh")
        {
            Alpha3Code = "BGD"
        },
        new Country("BB", "Barbados", "Barbados")
        {
            Alpha3Code = "BRB"
        },
        new Country("BY", "Belarus", "Belarus")
        {
            Alpha3Code = "BLR"
        },
        new Country("BE", "Belgium", "Belgium")
        {
            Alpha3Code = "BEL"
        },
        new Country("BZ", "Belize", "Belize")
        {
            Alpha3Code = "BLZ"
        },
        new Country("BJ", "Benin", "Benin")
        {
            Alpha3Code = "BEN"
        },
        new Country("BM", "Bermuda", "Bermuda")
        {
            Alpha3Code = "BMU"
        },
        new Country("AX", "Åland Islands", "Åland Islands")
        {
            Alpha3Code = "ALA"
        },
        new Country("BT", "Bhutan", "Bhutan")
        {
            Alpha3Code = "BTN"
        },
        new Country("BO", "Bolivien", "Bolivia (Plurinational State of)")
        {
            Alpha3Code = "BOL"
        },
        new Country("BQ", "Bonaire, Sint Eustatius and Saba", "Bonaire, Sint Eustatius and Saba")
        {
            Alpha3Code = "BES"
        },
        new Country("BA", "Bosnien and Herzegovenien", "Bosnia and Herzegovina")
        {
            Alpha3Code = "BIH"
        },
        new Country("BW", "Botswana", "Botswana")
        {
            Alpha3Code = "BWA"
        },
        new Country("BV", "Bouvet Island", "Bouvet Island")
        {
            Alpha3Code = "BVT"
        },
        new Country("BR", "Brasilien", "Brazil")
        {
            Alpha3Code = "BRA"
        },
        new Country("IO", "British Indian Ocean Territory", "British Indian Ocean Territory (the)")
        {
            Alpha3Code = "IOT"
        },
        new Country("BN", "Brunei Darussalam", "Brunei Darussalam")
        {
            Alpha3Code = "BRN"
        },
        new Country("BG", "Bulgarien", "Bulgaria")
        {
            Alpha3Code = "BGR"
        },
        new Country("BF", "Burkina Faso", "Burkina Faso")
        {
            Alpha3Code = "BFA"
        },
        new Country("BI", "Burundi", "Burundi")
        {
            Alpha3Code = "BDI"
        },
        new Country("CV", "Cabo Verde", "Cabo Verde")
        {
            Alpha3Code = "CPV"
        },
        new Country("KH", "Cambodia", "Cambodia")
        {
            Alpha3Code = "KHM"
        },
        new Country("CM", "Cameroon", "Cameroon")
        {
            Alpha3Code = "CMR"
        },
        new Country("CA", "Kanada", "Canada")
        {
            Alpha3Code = "CAN"
        },
        new Country("KY", "Cayman Islands (the)", "Cayman Islands (the)")
        {
            Alpha3Code = "CYM"
        },
        new Country("CF", "Central African Republic (the)", "Central African Republic (the)")
        {
            Alpha3Code = "CAF"
        },
        new Country("TD", "Chad", "Chad")
        {
            Alpha3Code = "TCD"
        },
        new Country("CL", "Chile", "Chile")
        {
            Alpha3Code = "CHL"
        },
        new Country("CN", "China", "China")
        {
            Alpha3Code = "CHN"
        },
        new Country("CX", "Weihnachtsinseln", "Christmas Island")
        {
            Alpha3Code = "CXR"
        },
        new Country("CC", "Cocos (Keeling) Islands (the)", "Cocos (Keeling) Islands (the)")
        {
            Alpha3Code = "CCK"
        },
        new Country("CO", "Kolumbien", "Colombia")
        {
            Alpha3Code = "COL"
        },
        new Country("KM", "Comoros", "Comoros (the)")
        {
            Alpha3Code = "COM"
        },
        new Country("CD", "Kongo", "Congo (the Democratic Republic of the)")
        {
            Alpha3Code = "COD"
        },
        new Country("CK", "Cook Islands", "Cook Islands (the)")
        {
            Alpha3Code = "COK"
        },
        new Country("CR", "Costa Rica", "Costa Rica")
        {
            Alpha3Code = "CRI"
        },
        new Country("HR", "Kroatien", "Croatia")
        {
            Alpha3Code = "HRV"
        },
        new Country("CU", "Kuba", "Cuba")
        {
            Alpha3Code = "CUB"
        },
        new Country("CW", "Curaçao", "Curaçao")
        {
            Alpha3Code = "CUW"
        },
        new Country("CY", "Zypern", "Cyprus")
        {
            Alpha3Code = "CYP"
        },
        new Country("CZ", "Tschechien", "Czechia")
        {
            Alpha3Code = "CZE"
        },
        new Country("CI", "Côte d'Ivoire", "Côte d'Ivoire")
        {
            Alpha3Code = "CIV"
        },
        new Country("DK", "Dänemark", "Denmark")
        {
            Alpha3Code = "DNK"
        },
        new Country("DJ", "Djibouti", "Djibouti")
        {
            Alpha3Code = "DJI"
        },
        new Country("DM", "Dominica", "Dominica")
        {
            Alpha3Code = "DMA"
        },
        new Country("DO", "Dominikanische Republik", "Dominican Republic (the)")
        {
            Alpha3Code = "DOM"
        },
        new Country("EC", "Ecuador", "Ecuador")
        {
            Alpha3Code = "ECU"
        },
        new Country("EG", "Ägypten", "Egypt")
        {
            Alpha3Code = "EGY"
        },
        new Country("SV", "El Salvador", "El Salvador")
        {
            Alpha3Code = "SLV"
        },
        new Country("GQ", "Equatorial Guinea", "Equatorial Guinea")
        {
            Alpha3Code = "GNQ"
        },
        new Country("ER", "Eritrea", "Eritrea")
        {
            Alpha3Code = "ERI"
        },
        new Country("EE", "Estonia", "Estonia")
        {
            Alpha3Code = "EST"
        },
        new Country("SZ", "Eswatini", "Eswatini")
        {
            Alpha3Code = "SWZ"
        },
        new Country("ET", "Ethiopia", "Ethiopia")
        {
            Alpha3Code = "ETH"
        },
        new Country("FK", "Falkland Islands (the) [Malvinas]", "Falkland Islands (the) [Malvinas]")
        {
            Alpha3Code = "FLK"
        },
        new Country("FO", "Faroe Islands (the)", "Faroe Islands (the)")
        {
            Alpha3Code = "FRO"
        },
        new Country("FJ", "Fiji", "Fiji")
        {
            Alpha3Code = "FJI"
        },
        new Country("FI", "Finland", "Finland")
        {
            Alpha3Code = "FIN"
        },
        new Country("FR", "Frankreich", "France")
        {
            Alpha3Code = "FRA"
        },
        new Country("GF", "French Guiana", "French Guiana")
        {
            Alpha3Code = "GUF"
        },
        new Country("PF", "French Polynesia", "French Polynesia")
        {
            Alpha3Code = "PYF"
        },
        new Country("TF", "French Southern Territories (the)", "French Southern Territories (the)")
        {
            Alpha3Code = "ATF"
        },
        new Country("GA", "Gabon", "Gabon")
        {
            Alpha3Code = "GAB"
        },
        new Country("GM", "Gambia (the)", "Gambia (the)")
        {
            Alpha3Code = "GMB"
        },
        new Country("GE", "Georgia", "Georgia")
        {
            Alpha3Code = "GEO"
        },
        new Country("GH", "Ghana", "Ghana")
        {
            Alpha3Code = "GHA"
        },
        new Country("GI", "Gibraltar", "Gibraltar")
        {
            Alpha3Code = "GIB"
        },
        new Country("GR", "Greece", "Greece")
        {
            Alpha3Code = "GRC"
        },
        new Country("GL", "Greenland", "Greenland")
        {
            Alpha3Code = "GRL"
        },
        new Country("GD", "Grenada", "Grenada")
        {
            Alpha3Code = "GRD"
        },
        new Country("GP", "Guadeloupe", "Guadeloupe")
        {
            Alpha3Code = "GLP"
        },
        new Country("GU", "Guam", "Guam")
        {
            Alpha3Code = "GUM"
        },
        new Country("GT", "Guatemala", "Guatemala")
        {
            Alpha3Code = "GTM"
        },
        new Country("GG", "Guernsey", "Guernsey")
        {
            Alpha3Code = "GGY"
        },
        new Country("GN", "Guinea", "Guinea")
        {
            Alpha3Code = "GIN"
        },
        new Country("GW", "Guinea-Bissau", "Guinea-Bissau")
        {
            Alpha3Code = "GNB"
        },
        new Country("GY", "Guyana", "Guyana")
        {
            Alpha3Code = "GUY"
        },
        new Country("HT", "Haiti", "Haiti")
        {
            Alpha3Code = "HTI"
        },
        new Country("HM", "Heard Island and McDonald Islands", "Heard Island and McDonald Islands")
        {
            Alpha3Code = "HMD"
        },
        new Country("VA", "Holy See (the)", "Holy See (the)")
        {
            Alpha3Code = "VAT"
        },
        new Country("HN", "Honduras", "Honduras")
        {
            Alpha3Code = "HND"
        },
        new Country("HK", "Hong Kong", "Hong Kong")
        {
            Alpha3Code = "HKG"
        },
        new Country("HU", "Hungary", "Hungary")
        {
            Alpha3Code = "HUN"
        },
        new Country("IS", "Iceland", "Iceland")
        {
            Alpha3Code = "ISL"
        },
        new Country("IN", "India", "India")
        {
            Alpha3Code = "IND"
        },
        new Country("ID", "Indonesia", "Indonesia")
        {
            Alpha3Code = "IDN"
        },
        new Country("IR", "Iran (Islamic Republic of)", "Iran (Islamic Republic of)")
        {
            Alpha3Code = "IRN"
        },
        new Country("IQ", "Iraq", "Iraq")
        {
            Alpha3Code = "IRQ"
        },
        new Country("IE", "Ireland", "Ireland")
        {
            Alpha3Code = "IRL"
        },
        new Country("IM", "Isle of Man", "Isle of Man")
        {
            Alpha3Code = "IMN"
        },
        new Country("IL", "Israel", "Israel")
        {
            Alpha3Code = "ISR"
        },
        new Country("IT", "Italy", "Italy")
        {
            Alpha3Code = "ITA"
        },
        new Country("JM", "Jamaica", "Jamaica")
        {
            Alpha3Code = "JAM"
        },
        new Country("JP", "Japan", "Japan")
        {
            Alpha3Code = "JPN"
        },
        new Country("JE", "Jersey", "Jersey")
        {
            Alpha3Code = "JEY"
        },
        new Country("JO", "Jordan", "Jordan")
        {
            Alpha3Code = "JOR"
        },
        new Country("KZ", "Kazakhstan", "Kazakhstan")
        {
            Alpha3Code = "KAZ"
        },
        new Country("KE", "Kenya", "Kenya")
        {
            Alpha3Code = "KEN"
        },
        new Country("KI", "Kiribati", "Kiribati")
        {
            Alpha3Code = "KIR"
        },
        new Country("KP", "Korea (the Democratic People's Republic of)", "Korea (the Democratic People's Republic of)")
        {
            Alpha3Code = "PRK"
        },
        new Country("KR", "Korea (the Republic of)", "Korea (the Republic of)")
        {
            Alpha3Code = "KOR"
        },
        new Country("KW", "Kuwait", "Kuwait")
        {
            Alpha3Code = "KWT"
        },
        new Country("KG", "Kyrgyzstan", "Kyrgyzstan")
        {
            Alpha3Code = "KGZ"
        },
        new Country("LA", "Lao People's Democratic Republic (the)", "Lao People's Democratic Republic (the)")
        {
            Alpha3Code = "LAO"
        },
        new Country("LV", "Latvia", "Latvia")
        {
            Alpha3Code = "LVA"
        },
        new Country("LB", "Lebanon", "Lebanon")
        {
            Alpha3Code = "LBN"
        },
        new Country("LS", "Lesotho", "Lesotho")
        {
            Alpha3Code = "LSO"
        },
        new Country("LR", "Liberia", "Liberia")
        {
            Alpha3Code = "LBR"
        },
        new Country("LY", "Libya", "Libya")
        {
            Alpha3Code = "LBY"
        },
        new Country("LI", "Liechtenstein", "Liechtenstein")
        {
            Alpha3Code = "LIE"
        },
        new Country("LT", "Lithuania", "Lithuania")
        {
            Alpha3Code = "LTU"
        },
        new Country("LU", "Luxembourg", "Luxembourg")
        {
            Alpha3Code = "LUX"
        },
        new Country("MO", "Macao", "Macao")
        {
            Alpha3Code = "MAC"
        },
        new Country("MG", "Madagascar", "Madagascar")
        {
            Alpha3Code = "MDG"
        },
        new Country("MW", "Malawi", "Malawi")
        {
            Alpha3Code = "MWI"
        },
        new Country("MY", "Malaysia", "Malaysia")
        {
            Alpha3Code = "MYS"
        },
        new Country("MV", "Maldives", "Maldives")
        {
            Alpha3Code = "MDV"
        },
        new Country("ML", "Mali", "Mali")
        {
            Alpha3Code = "MLI"
        },
        new Country("MT", "Malta", "Malta")
        {
            Alpha3Code = "MLT"
        },
        new Country("MH", "Marshall Islands (the)", "Marshall Islands (the)")
        {
            Alpha3Code = "MHL"
        },
        new Country("MQ", "Martinique", "Martinique")
        {
            Alpha3Code = "MTQ"
        },
        new Country("MR", "Mauritania", "Mauritania")
        {
            Alpha3Code = "MRT"
        },
        new Country("MU", "Mauritius", "Mauritius")
        {
            Alpha3Code = "MUS"
        },
        new Country("YT", "Mayotte", "Mayotte")
        {
            Alpha3Code = "MYT"
        },
        new Country("MX", "Mexico", "Mexico")
        {
            Alpha3Code = "MEX"
        },
        new Country("FM", "Micronesia (Federated States of)", "Micronesia (Federated States of)")
        {
            Alpha3Code = "FSM"
        },
        new Country("MD", "Moldova (the Republic of)", "Moldova (the Republic of)")
        {
            Alpha3Code = "MDA"
        },
        new Country("MC", "Monaco", "Monaco")
        {
            Alpha3Code = "MCO"
        },
        new Country("MN", "Mongolia", "Mongolia")
        {
            Alpha3Code = "MNG"
        },
        new Country("ME", "Montenegro", "Montenegro")
        {
            Alpha3Code = "MNE"
        },
        new Country("MS", "Montserrat", "Montserrat")
        {
            Alpha3Code = "MSR"
        },
        new Country("MA", "Morocco", "Morocco")
        {
            Alpha3Code = "MAR"
        },
        new Country("MZ", "Mozambique", "Mozambique")
        {
            Alpha3Code = "MOZ"
        },
        new Country("MM", "Myanmar", "Myanmar")
        {
            Alpha3Code = "MMR"
        },
        new Country("NA", "Namibia", "Namibia")
        {
            Alpha3Code = "NAM"
        },
        new Country("NR", "Nauru", "Nauru")
        {
            Alpha3Code = "NRU"
        },
        new Country("NP", "Nepal", "Nepal")
        {
            Alpha3Code = "NPL"
        },
        new Country("NL", "Netherlands (the)", "Netherlands (the)")
        {
            Alpha3Code = "NLD"
        },
        new Country("NC", "New Caledonia", "New Caledonia")
        {
            Alpha3Code = "NCL"
        },
        new Country("NZ", "New Zealand", "New Zealand")
        {
            Alpha3Code = "NZL"
        },
        new Country("NI", "Nicaragua", "Nicaragua")
        {
            Alpha3Code = "NIC"
        },
        new Country("NE", "Niger (the)", "Niger (the)")
        {
            Alpha3Code = "NER"
        },
        new Country("NG", "Nigeria", "Nigeria")
        {
            Alpha3Code = "NGA"
        },
        new Country("NU", "Niue", "Niue")
        {
            Alpha3Code = "NIU"
        },
        new Country("NF", "Norfolk Island", "Norfolk Island")
        {
            Alpha3Code = "NFK"
        },
        new Country("MK", "North Macedonia", "North Macedonia")
        {
            Alpha3Code = "MKD"
        },
        new Country("MP", "Northern Mariana Islands (the)", "Northern Mariana Islands (the)")
        {
            Alpha3Code = "MNP"
        },
        new Country("NO", "Norway", "Norway")
        {
            Alpha3Code = "NOR"
        },
        new Country("OM", "Oman", "Oman")
        {
            Alpha3Code = "OMN"
        },
        new Country("PK", "Pakistan", "Pakistan")
        {
            Alpha3Code = "PAK"
        },
        new Country("PW", "Palau", "Palau")
        {
            Alpha3Code = "PLW"
        },
        new Country("PS", "Palestine, State of", "Palestine, State of")
        {
            Alpha3Code = "PSE"
        },
        new Country("PA", "Panama", "Panama")
        {
            Alpha3Code = "PAN"
        },
        new Country("PG", "Papua New Guinea", "Papua New Guinea")
        {
            Alpha3Code = "PNG"
        },
        new Country("PY", "Paraguay", "Paraguay")
        {
            Alpha3Code = "PRY"
        },
        new Country("PE", "Peru", "Peru")
        {
            Alpha3Code = "PER"
        },
        new Country("PH", "Philippines (the)", "Philippines (the)")
        {
            Alpha3Code = "PHL"
        },
        new Country("PN", "Pitcairn", "Pitcairn")
        {
            Alpha3Code = "PCN"
        },
        new Country("PL", "Poland", "Poland")
        {
            Alpha3Code = "POL"
        },
        new Country("PT", "Portugal", "Portugal")
        {
            Alpha3Code = "PRT"
        },
        new Country("PR", "Puerto Rico", "Puerto Rico")
        {
            Alpha3Code = "PRI"
        },
        new Country("QA", "Qatar", "Qatar")
        {
            Alpha3Code = "QAT"
        },
        new Country("RO", "Romania", "Romania")
        {
            Alpha3Code = "ROU"
        },
        new Country("RU", "Russian Federation (the)", "Russian Federation (the)")
        {
            Alpha3Code = "RUS"
        },
        new Country("RW", "Rwanda", "Rwanda")
        {
            Alpha3Code = "RWA"
        },
        new Country("RE", "Réunion", "Réunion")
        {
            Alpha3Code = "REU"
        },
        new Country("BL", "Saint Barthélemy", "Saint Barthélemy")
        {
            Alpha3Code = "BLM"
        },
        new Country("SH", "Saint Helena, Ascension and Tristan da Cunha", "Saint Helena, Ascension and Tristan da Cunha")
        {
            Alpha3Code = "SHN"
        },
        new Country("KN", "Saint Kitts and Nevis", "Saint Kitts and Nevis")
        {
            Alpha3Code = "KNA"
        },
        new Country("LC", "Saint Lucia", "Saint Lucia")
        {
            Alpha3Code = "LCA"
        },
        new Country("MF", "Saint Martin (French part)", "Saint Martin (French part)")
        {
            Alpha3Code = "MAF"
        },
        new Country("PM", "Saint Pierre and Miquelon", "Saint Pierre and Miquelon")
        {
            Alpha3Code = "SPM"
        },
        new Country("VC", "Saint Vincent and the Grenadines", "Saint Vincent and the Grenadines")
        {
            Alpha3Code = "VCT"
        },
        new Country("WS", "Samoa", "Samoa")
        {
            Alpha3Code = "WSM"
        },
        new Country("SM", "San Marino", "San Marino")
        {
            Alpha3Code = "SMR"
        },
        new Country("ST", "Sao Tome and Principe", "Sao Tome and Principe")
        {
            Alpha3Code = "STP"
        },
        new Country("SA", "Saudi Arabia", "Saudi Arabia")
        {
            Alpha3Code = "SAU"
        },
        new Country("SN", "Senegal", "Senegal")
        {
            Alpha3Code = "SEN"
        },
        new Country("RS", "Serbia", "Serbia")
        {
            Alpha3Code = "SRB"
        },
        new Country("SC", "Seychelles", "Seychelles")
        {
            Alpha3Code = "SYC"
        },
        new Country("SL", "Sierra Leone", "Sierra Leone")
        {
            Alpha3Code = "SLE"
        },
        new Country("SG", "Singapore", "Singapore")
        {
            Alpha3Code = "SGP"
        },
        new Country("SX", "Sint Maarten (Dutch part)", "Sint Maarten (Dutch part)")
        {
            Alpha3Code = "SXM"
        },
        new Country("SK", "Slovakia", "Slovakia")
        {
            Alpha3Code = "SVK"
        },
        new Country("SI", "Slovenia", "Slovenia")
        {
            Alpha3Code = "SVN"
        },
        new Country("SB", "Solomon Islands", "Solomon Islands")
        {
            Alpha3Code = "SLB"
        },
        new Country("SO", "Somalia", "Somalia")
        {
            Alpha3Code = "SOM"
        },
        new Country("ZA", "South Africa", "South Africa")
        {
            Alpha3Code = "ZAF"
        },
        new Country("GS", "South Georgia and the South Sandwich Islands", "South Georgia and the South Sandwich Islands")
        {
            Alpha3Code = "SGS"
        },
        new Country("SS", "South Sudan", "South Sudan")
        {
            Alpha3Code = "SSD"
        },
        new Country("ES", "Spain", "Spain")
        {
            Alpha3Code = "ESP"
        },
        new Country("LK", "Sri Lanka", "Sri Lanka")
        {
            Alpha3Code = "LKA"
        },
        new Country("SD", "Sudan (the)", "Sudan (the)")
        {
            Alpha3Code = "SDN"
        },
        new Country("SR", "Suriname", "Suriname")
        {
            Alpha3Code = "SUR"
        },
        new Country("SJ", "Svalbard and Jan Mayen", "Svalbard and Jan Mayen")
        {
            Alpha3Code = "SJM"
        },
        new Country("SE", "Sweden", "Sweden")
        {
            Alpha3Code = "SWE"
        },
        new Country("CH", "Switzerland", "Switzerland")
        {
            Alpha3Code = "CHE"
        },
        new Country("SY", "Syrian Arab Republic (the)", "Syrian Arab Republic (the)")
        {
            Alpha3Code = "SYR"
        },
        new Country("TW", "Taiwan (Province of China)", "Taiwan (Province of China)")
        {
            Alpha3Code = "TWN"
        },
        new Country("TJ", "Tajikistan", "Tajikistan")
        {
            Alpha3Code = "TJK"
        },
        new Country("TZ", "Tanzania, the United Republic of", "Tanzania, the United Republic of")
        {
            Alpha3Code = "TZA"
        },
        new Country("TH", "Thailand", "Thailand")
        {
            Alpha3Code = "THA"
        },
        new Country("TL", "Timor-Leste", "Timor-Leste")
        {
            Alpha3Code = "TLS"
        },
        new Country("TG", "Togo", "Togo")
        {
            Alpha3Code = "TGO"
        },
        new Country("TK", "Tokelau", "Tokelau")
        {
            Alpha3Code = "TKL"
        },
        new Country("TO", "Tonga", "Tonga")
        {
            Alpha3Code = "TON"
        },
        new Country("TT", "Trinidad and Tobago", "Trinidad and Tobago")
        {
            Alpha3Code = "TTO"
        },
        new Country("TN", "Tunisia", "Tunisia")
        {
            Alpha3Code = "TUN"
        },
        new Country("TR", "Turkey", "Turkey")
        {
            Alpha3Code = "TUR"
        },
        new Country("TM", "Turkmenistan", "Turkmenistan")
        {
            Alpha3Code = "TKM"
        },
        new Country("TC", "Turks and Caicos Islands (the)", "Turks and Caicos Islands (the)")
        {
            Alpha3Code = "TCA"
        },
        new Country("TV", "Tuvalu", "Tuvalu")
        {
            Alpha3Code = "TUV"
        },
        new Country("UG", "Uganda", "Uganda")
        {
            Alpha3Code = "UGA"
        },
        new Country("UA", "Ukraine", "Ukraine")
        {
            Alpha3Code = "UKR"
        },
        new Country("AE", "United Arab Emirates (the)", "United Arab Emirates (the)")
        {
            Alpha3Code = "ARE"
        },
        new Country("UM", "United States Minor Outlying Islands (the)", "United States Minor Outlying Islands (the)")
        {
            Alpha3Code = "UMI"
        },
        new Country("US", "United States of America (the)", "United States of America (the)")
        {
            Alpha3Code = "USA"
        },
        new Country("UY", "Uruguay", "Uruguay")
        {
            Alpha3Code = "URY"
        },
        new Country("UZ", "Uzbekistan", "Uzbekistan")
        {
            Alpha3Code = "UZB"
        },
        new Country("VU", "Vanuatu", "Vanuatu")
        {
            Alpha3Code = "VUT"
        },
        new Country("VE", "Venezuela (Bolivarian Republic of)", "Venezuela (Bolivarian Republic of)")
        {
            Alpha3Code = "VEN"
        },
        new Country("VN", "Viet Nam", "Viet Nam")
        {
            Alpha3Code = "VNM"
        },
        new Country("VG", "Virgin Islands (British)", "Virgin Islands (British)")
        {
            Alpha3Code = "VGB"
        },
        new Country("VI", "Virgin Islands (U.S.)", "Virgin Islands (U.S.)")
        {
            Alpha3Code = "VIR"
        },
        new Country("WF", "Wallis and Futuna", "Wallis and Futuna")
        {
            Alpha3Code = "WLF"
        },
        new Country("EH", "Western Sahara*", "Western Sahara*")
        {
            Alpha3Code = "ESH"
        },
        new Country("YE", "Yemen", "Yemen")
        {
            Alpha3Code = "YEM"
        },
        new Country("ZM", "Zambia", "Zambia")
        {
            Alpha3Code = "ZMB"
        }
    };

    public static IEnumerable<NotificationTypeAssignedTopic> NotificationTypeAssignedTopics => new[]
    {
        new NotificationTypeAssignedTopic(NotificationTypeId.INFO, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.TECHNICAL_USER_CREATION, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.CONNECTOR_REGISTERED, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.WELCOME_SERVICE_PROVIDER, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.WELCOME, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.WELCOME_USE_CASES, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.WELCOME_APP_MARKETPLACE, NotificationTopicId.INFO),
        new NotificationTypeAssignedTopic(NotificationTypeId.ACTION, NotificationTopicId.ACTION),
        new NotificationTypeAssignedTopic(NotificationTypeId.APP_SUBSCRIPTION_REQUEST, NotificationTopicId.ACTION),
        new NotificationTypeAssignedTopic(NotificationTypeId.SERVICE_REQUEST, NotificationTopicId.ACTION),
        new NotificationTypeAssignedTopic(NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, NotificationTopicId.OFFER),
        new NotificationTypeAssignedTopic(NotificationTypeId.APP_RELEASE_REQUEST, NotificationTopicId.OFFER),
        new NotificationTypeAssignedTopic(NotificationTypeId.SERVICE_ACTIVATION, NotificationTopicId.OFFER),
        new NotificationTypeAssignedTopic(NotificationTypeId.APP_RELEASE_APPROVAL, NotificationTopicId.OFFER)
    };
}
