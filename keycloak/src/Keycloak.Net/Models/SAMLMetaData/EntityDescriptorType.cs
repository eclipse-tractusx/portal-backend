/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
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

using System.Xml.Serialization;

namespace Keycloak.Net.Models.SAMLMetaData
{
    [XmlRoot(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata",
        ElementName = "EntityDescriptor",
        IsNullable = false)]
    public class EntityDescriptorType
    {
        [XmlAttribute(AttributeName = "entityID")]
        public string EntityId { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata",
            ElementName = "IDPSSODescriptor",
            IsNullable = true)]
        public IdpSsoDescriptorType[] IdpSsoDescriptor { get; set; }
    }

    public class IdpSsoDescriptorType : SsoDescriptorType
    {
        [XmlAttribute(AttributeName = "WantAuthnRequestsSigned", DataType = "boolean")]
        public bool WantAuthnRequestsSigned { get; set; }

        [XmlAttribute(AttributeName = "protocolSupportEnumeration", DataType = "string")]
        public string ProtocolSupportEnumeration { get; set; }

        [XmlElement(IsNullable = false)]
        public EndpointType[] SingleSignOnService { get; set; }

        [XmlElement]
        public EndpointType[] NameIDMappingService { get; set; }

        [XmlElement]
        public EndpointType[] AssertionIDRequestService { get; set; }

        [XmlElement(DataType = "anyURI")]
        public string AttributeProfile { get; set; }

    }

    public class KeyDescriptorType
    {
        [XmlAttribute(AttributeName = "use")]
        public string Use { get; set; }

        [XmlElement]
        public KeyInfoType KeyInfo { get; set; }
    }

    public class KeyInfoType
    {
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public string KeyName { get; set; }

        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public X509DataType X509Data { get; set; }
    }

    public class X509DataType
    {
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public string X509Certificate { get; set; }
    }

    public class EndpointType
    {
        [XmlAttribute(DataType = "anyURI")]
        public string Binding { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string Location { get; set; }

        [XmlAttribute(DataType = "anyURI")]
        public string ResponseLocation { get; set; }
    }

    public class IndexedEndpointType : EndpointType
    {
        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "isDefault")]
        public bool IsDefault { get; set; }
    }

    public class SsoDescriptorType : RoleDescriptorType
    {
        [XmlElement(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata",
            IsNullable = true)]
        public IndexedEndpointType[] ArtifactResolutionService { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata",
            IsNullable = true)]
        public EndpointType[] SingleLogoutService { get; set; }

        [XmlElement(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata",
            IsNullable = true)]
        public EndpointType[] ManageNameIDService { get; set; }

        [XmlElement(DataType = "anyURI")]
        public string[] NameIDFormat { get; set; }
    }

    public class RoleDescriptorType
    {
        [XmlElement(Namespace = "urn:oasis:names:tc:SAML:2.0:metadata")]
        public KeyDescriptorType[] KeyDescriptor { get; set; }
    }
}
