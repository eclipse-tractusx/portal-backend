/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

public sealed class UserProfileConfig : IEquatable<UserProfileConfig>
{
    public IEnumerable<ProfileAttribute> Attributes { get; set; } = null!;

    public IEnumerable<ProfileGroup> Groups { get; set; } = null!;

    public bool Equals(UserProfileConfig? other) =>
        other is not null &&
        Attributes.OrderBy(x => x.Name).SequenceEqual(other.Attributes.OrderBy(x => x.Name)) &&
        Groups.OrderBy(x => x.Name).SequenceEqual(other.Groups.OrderBy(x => x.Name));

    public override bool Equals(object? obj) =>
        obj is not null && obj.GetType() == this.GetType() && Equals((UserProfileConfig)obj);

    public override int GetHashCode() =>
        HashCode.Combine(Attributes, Groups);
}

public sealed class ProfileAttribute : IEquatable<ProfileAttribute>
{
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public object? Validations { get; set; }
    public object? Annotations { get; set; }
    public ProfileAttributeRequired? Required { get; set; }
    public ProfileAttributePermission Permissions { get; set; } = null!;
    public ProfileAttributeSelector? Selector { get; set; }
    public string Group { get; set; } = null!;
    public bool Multivalued { get; set; }

    public bool Equals(ProfileAttribute? other) =>
        other is not null &&
        Name == other.Name &&
        DisplayName == other.DisplayName &&
        Equals(Validations, other.Validations) &&
        Equals(Annotations, other.Annotations) &&
        Equals(Required, other.Required) &&
        Permissions.Equals(other.Permissions) &&
        Equals(Selector, other.Selector) &&
        Group == other.Group &&
        Multivalued == other.Multivalued;

    public override bool Equals(object? obj) =>
        obj is not null && obj.GetType() == this.GetType() && Equals((ProfileAttribute)obj);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Name);
        hashCode.Add(DisplayName);
        hashCode.Add(Validations);
        hashCode.Add(Annotations);
        hashCode.Add(Required);
        hashCode.Add(Permissions);
        hashCode.Add(Selector);
        hashCode.Add(Group);
        hashCode.Add(Multivalued);
        return hashCode.ToHashCode();
    }
}

public sealed class ProfileAttributeRequired : IEquatable<ProfileAttributeRequired>
{
    public IEnumerable<string>? Roles { get; init; } = null!;
    public IEnumerable<string>? Scopes { get; init; } = null!;

    public bool Equals(ProfileAttributeRequired? other) =>
        other is not null &&
        ((Roles == null && other.Roles == null) ||
         Roles != null && other.Roles != null && Roles.Order().SequenceEqual(other.Roles.Order())) &&
        ((Scopes == null && other.Scopes == null) || Scopes != null && other.Scopes != null && Scopes.Order().SequenceEqual(other.Scopes.Order()));

    public override bool Equals(object? obj) =>
        obj is not null && obj.GetType() == this.GetType() && Equals((ProfileAttributeRequired)obj);

    public override int GetHashCode() => HashCode.Combine(Roles, Scopes);
}

public sealed class ProfileAttributePermission : IEquatable<ProfileAttributePermission>
{
    public IEnumerable<string> View { get; init; } = null!;
    public IEnumerable<string> Edit { get; init; } = null!;

    public bool Equals(ProfileAttributePermission? other) =>
        other is not null &&
        View.Order().SequenceEqual(other.View.Order()) &&
        Edit.Order().SequenceEqual(other.Edit.Order());

    public override bool Equals(object? obj) =>
        obj is not null && obj.GetType() == this.GetType() && Equals((ProfileAttributePermission)obj);

    public override int GetHashCode() =>
        HashCode.Combine(View, Edit);
}

public sealed class ProfileAttributeSelector : IEquatable<ProfileAttributeSelector>
{
    public IEnumerable<ProfileAttribute> Attributes { get; init; } = null!;
    public IEnumerable<ProfileGroup> Groups { get; init; } = null!;
    public object? UnmanagedAttributePolicy { get; init; }

    public bool Equals(ProfileAttributeSelector? other) =>
        other is not null &&
        Attributes.OrderBy(x => x.Name).SequenceEqual(other.Attributes.OrderBy(x => x.Name)) &&
        Groups.OrderBy(x => x.Name).SequenceEqual(other.Groups.OrderBy(x => x.Name)) &&
        Equals(UnmanagedAttributePolicy, other.UnmanagedAttributePolicy);

    public override bool Equals(object? obj) =>
        obj is not null && obj.GetType() == this.GetType() && Equals((ProfileAttributeSelector)obj);

    public override int GetHashCode() =>
        HashCode.Combine(Attributes, Groups, UnmanagedAttributePolicy);
}

public sealed class ProfileGroup : IEquatable<ProfileGroup>
{
    public string Name { get; init; } = null!;
    public string DisplayHeader { get; init; } = null!;
    public string DisplayDescription { get; init; } = null!;
    public IEnumerable<object> Annotations { get; init; } = null!;

    public bool Equals(ProfileGroup? other) =>
        other is not null &&
        Name == other.Name &&
        DisplayHeader == other.DisplayHeader &&
        DisplayDescription == other.DisplayDescription &&
        Annotations.Order().SequenceEqual(other.Annotations.Order());

    public override bool Equals(object? obj) =>
        obj is not null && obj.GetType() == this.GetType() && Equals((ProfileGroup)obj);

    public override int GetHashCode() =>
        HashCode.Combine(Name, DisplayHeader, DisplayDescription, Annotations);
}
