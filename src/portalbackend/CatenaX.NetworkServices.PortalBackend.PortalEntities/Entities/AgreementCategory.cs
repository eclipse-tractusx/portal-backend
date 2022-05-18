﻿using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class AgreementCategory
{
    private AgreementCategory()
    {
        Label = null!;
        Agreements = new HashSet<Agreement>();
    }

    public AgreementCategory(AgreementCategoryId agreementCategoryId) : this()
    {
        AgreementCategoryId = agreementCategoryId;
        Label = agreementCategoryId.ToString();
    }

    [Key]
    public AgreementCategoryId AgreementCategoryId { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Agreement> Agreements { get; private set; }
}
