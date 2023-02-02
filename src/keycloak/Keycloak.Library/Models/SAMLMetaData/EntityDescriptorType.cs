/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using System.Xml.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.SAMLMetaData;

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
