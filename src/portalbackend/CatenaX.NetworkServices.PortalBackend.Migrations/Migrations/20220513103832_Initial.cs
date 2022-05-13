using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatenaX.NetworkServices.PortalBackend.Migrations.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "portal");

            migrationBuilder.CreateTable(
                name: "agreement_categories",
                schema: "portal",
                columns: table => new
                {
                    agreement_category_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_categories", x => x.agreement_category_id);
                });

            migrationBuilder.CreateTable(
                name: "app_licenses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    licensetext = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_status",
                schema: "portal",
                columns: table => new
                {
                    app_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_status", x => x.app_status_id);
                });

            migrationBuilder.CreateTable(
                name: "company_application_status",
                schema: "portal",
                columns: table => new
                {
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("company_application_status_pkey", x => x.application_status_id);
                });

            migrationBuilder.CreateTable(
                name: "company_roles",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_roles", x => x.company_role_id);
                });

            migrationBuilder.CreateTable(
                name: "company_status",
                schema: "portal",
                columns: table => new
                {
                    company_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_status", x => x.company_status_id);
                });

            migrationBuilder.CreateTable(
                name: "company_user_status",
                schema: "portal",
                columns: table => new
                {
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_status", x => x.company_user_status_id);
                });

            migrationBuilder.CreateTable(
                name: "consent_status",
                schema: "portal",
                columns: table => new
                {
                    consent_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_status", x => x.consent_status_id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                schema: "portal",
                columns: table => new
                {
                    alpha2code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    alpha3code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    country_name_de = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    country_name_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.alpha2code);
                });

            migrationBuilder.CreateTable(
                name: "document_templates",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    documenttemplatename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    documenttemplateversion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                schema: "portal",
                columns: table => new
                {
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_types", x => x.document_type_id);
                });

            migrationBuilder.CreateTable(
                name: "iam_clients",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_provider_categories",
                schema: "portal",
                columns: table => new
                {
                    identity_provider_category_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_provider_categories", x => x.identity_provider_category_id);
                });

            migrationBuilder.CreateTable(
                name: "invitation_status",
                schema: "portal",
                columns: table => new
                {
                    invitation_status_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitation_status", x => x.invitation_status_id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                schema: "portal",
                columns: table => new
                {
                    short_name = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    long_name_de = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    long_name_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_languages", x => x.short_name);
                });

            migrationBuilder.CreateTable(
                name: "use_cases",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    shortname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_use_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    region = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    streetadditional = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    streetname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    streetnumber = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    zipcode = table.Column<decimal>(type: "numeric(19,2)", precision: 19, scale: 2, nullable: false),
                    country_alpha2code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_6jg6itw07d2qww62deuyk0kh",
                        column: x => x.country_alpha2code,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    iam_client_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_iam_clients_iam_client_id",
                        column: x => x.iam_client_id,
                        principalSchema: "portal",
                        principalTable: "iam_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_providers",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    identity_provider_category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_providers", x => x.id);
                    table.ForeignKey(
                        name: "fk_iwohgwi9342adf9asdnfuie28",
                        column: x => x.identity_provider_category_id,
                        principalSchema: "portal",
                        principalTable: "identity_provider_categories",
                        principalColumn: "identity_provider_category_id");
                });

            migrationBuilder.CreateTable(
                name: "company_role_descriptions",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_role_descriptions", x => new { x.company_role_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_company_role_descriptions_company_roles_company_role_id",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "company_role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_role_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    bpn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tax_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    parent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    shortname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    company_status_id = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_owihadhfweilwefhaf682khj",
                        column: x => x.company_status_id,
                        principalSchema: "portal",
                        principalTable: "company_status",
                        principalColumn: "company_status_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_w70yf6urddd0ky7ev90okenf",
                        column: x => x.address_id,
                        principalSchema: "portal",
                        principalTable: "addresses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_role_descriptions",
                schema: "portal",
                columns: table => new
                {
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_descriptions", x => new { x.user_role_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_user_role_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_descriptions_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "iam_identity_providers",
                schema: "portal",
                columns: table => new
                {
                    iam_idp_alias = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_identity_providers", x => x.iam_idp_alias);
                    table.ForeignKey(
                        name: "fk_9balkda89j2498dkj2lkjd9s3",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "apps",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    app_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    marketing_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_apps", x => x.id);
                    table.ForeignKey(
                        name: "fk_68a9joedhyf43smfx2xc4rgm",
                        column: x => x.provider_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_owihadhfweilwefhaf111aaa",
                        column: x => x.app_status_id,
                        principalSchema: "portal",
                        principalTable: "app_status",
                        principalColumn: "app_status_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_applications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_3prv5i3o84vwvh7v0hh3sav7",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_akuwiehfiadf8928fhefhuda",
                        column: x => x.application_status_id,
                        principalSchema: "portal",
                        principalTable: "company_application_status",
                        principalColumn: "application_status_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_roles", x => new { x.company_id, x.company_role_id });
                    table.ForeignKey(
                        name: "fk_4db4hgj3yvqlkn9h6q8m4e0j",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_my2p7jlqrjf0tq1f8rhk0i0a",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "company_role_id");
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_use_cases",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_use_cas", x => new { x.company_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_m5eyaohrl0g9ju52byxsouqk",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_u65fkdrxnbkp8n0s7mate01v",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_identity_provider",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_identity_provider", x => new { x.company_id, x.identity_provider_id });
                    table.ForeignKey(
                        name: "fk_haad983jkald89wlkejidk234",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_iwzehadf8whjd8asjdfuwefhs",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_users",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    firstname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_users_company_user_status_id",
                        column: x => x.company_user_status_id,
                        principalSchema: "portal",
                        principalTable: "company_user_status",
                        principalColumn: "company_user_status_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ku01366dbcqk8h32lh8k5sx1",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "agreements",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agreement_category_id = table.Column<int>(type: "integer", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    agreement_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issuer_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreements", x => x.id);
                    table.ForeignKey(
                        name: "fk_n4nnf5bn8i3i9ijrf7kchfvc",
                        column: x => x.issuer_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_ooy9ydkah696jxh4lq7pn0xe",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_owqie84qkle78dasifljiwer",
                        column: x => x.agreement_category_id,
                        principalSchema: "portal",
                        principalTable: "agreement_categories",
                        principalColumn: "agreement_category_id");
                    table.ForeignKey(
                        name: "fk_whby66dika71srejhja6g75a",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_assigned_clients",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    iam_client_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_clients", x => new { x.app_id, x.iam_client_id });
                    table.ForeignKey(
                        name: "fk_4m022ek8gffepnqlnuxwyxp8",
                        column: x => x.iam_client_id,
                        principalSchema: "portal",
                        principalTable: "iam_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_oayyvy590ngh5705yspep0up",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_assigned_licenses",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_license_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_licenses", x => new { x.app_id, x.app_license_id });
                    table.ForeignKey(
                        name: "fk_3of613iyw1jx8gcj5i46jc1h",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_mes2xm3i1wotryfc88be4dkf",
                        column: x => x.app_license_id,
                        principalSchema: "portal",
                        principalTable: "app_licenses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_assigned_use_cases",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_assigned_use_cases", x => new { x.app_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_qi320sp8lxy7drw6kt4vheka",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_sjyfs49ma0kxaqfknjbaye0i",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_descriptions",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    description_long = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    description_short = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("app_descriptions_pkey", x => new { x.app_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_qamy6j7s3klebrf2s69v9k0i",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_vrom2pjij9x6stgovhaqkfxf",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateTable(
                name: "app_detail_images",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_detail_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_oayyvy590ngh5705yspep12a",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_languages",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_language", x => new { x.app_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_oayyvy590ngh5705yspep101",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_oayyvy590ngh5705yspep102",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateTable(
                name: "app_tags",
                schema: "portal",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_tags", x => new { x.app_id, x.tag_name });
                    table.ForeignKey(
                        name: "fk_qi320sp8lxy7drw6kt4vheka",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_assigned_apps",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_assigned_apps", x => new { x.company_id, x.app_id });
                    table.ForeignKey(
                        name: "fk_k1dqlv81463yes0k8f2giyaf",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_t365qpfvehuq40om25dyrnn5",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_app_favourites",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comp_user_ass_app_favour", x => new { x.company_user_id, x.app_id });
                    table.ForeignKey(
                        name: "fk_eip97mygnbglivrtmkagesjh",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "apps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_wva553r3xiew3ngbdkvafk85",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comp_user_assigned_roles", x => new { x.company_user_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_0c9rjjf9gm3l0n6reb4o0f1s",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bw1yhel67uhrxfk7mevovq5p",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    document = table.Column<uint>(type: "oid", nullable: false),
                    documenthash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    documentname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: true),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_xcgobngn7vk56k8nfkualsvn",
                        column: x => x.document_type_id,
                        principalSchema: "portal",
                        principalTable: "document_types",
                        principalColumn: "document_type_id");
                    table.ForeignKey(
                        name: "fk_xcgobngn7vk56k8nfkuaysvn",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "iam_users",
                schema: "portal",
                columns: table => new
                {
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_users", x => x.user_entity_id);
                    table.ForeignKey(
                        name: "fk_iweorqwaeilskjeijekkalwo",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    invitation_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_9tgenb7p09hr5c24haxjw259",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_dlrst4ju9d0wcgkh4w1nnoj3",
                        column: x => x.company_application_id,
                        principalSchema: "portal",
                        principalTable: "company_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_woihaodhawoeir72alfidosd",
                        column: x => x.invitation_status_id,
                        principalSchema: "portal",
                        principalTable: "invitation_status",
                        principalColumn: "invitation_status_id");
                });

            migrationBuilder.CreateTable(
                name: "agreement_assigned_company_roles",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_ass_comp_roles", x => new { x.agreement_id, x.company_role_id });
                    table.ForeignKey(
                        name: "fk_ljol11mdo76f4kv7fwqn1qc6",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_qh1hby9qcrr3gmy1cvi7nd3h",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "company_role_id");
                });

            migrationBuilder.CreateTable(
                name: "agreement_assigned_document_templates",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_template_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_ass_doc_templa", x => new { x.agreement_id, x.document_template_id });
                    table.ForeignKey(
                        name: "fk_bvrvs5aktrcn4t6565pnj3ur",
                        column: x => x.document_template_id,
                        principalSchema: "portal",
                        principalTable: "document_templates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_fvcwoptsuer9p23m055osose",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "consents",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    comment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    consent_status_id = table.Column<int>(type: "integer", nullable: false),
                    target = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consents", x => x.id);
                    table.ForeignKey(
                        name: "fk_36j22f34lgi2444n4tynxamh",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_39a5cbiv35v59ysgfon5oole",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_aiodhuwehw8wee20adskdfo2",
                        column: x => x.consent_status_id,
                        principalSchema: "portal",
                        principalTable: "consent_status",
                        principalColumn: "consent_status_id");
                    table.ForeignKey(
                        name: "fk_asqxie2r7yr06cdrw9ifaex8",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_cnrtafckouq96m0fw2qtpwbs",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "app_status",
                columns: new[] { "app_status_id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "IN_REVIEW" },
                    { 3, "ACTIVE" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_application_status",
                columns: new[] { "application_status_id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "ADD_COMPANY_DATA" },
                    { 3, "INVITE_USER" },
                    { 4, "SELECT_COMPANY_ROLE" },
                    { 5, "UPLOAD_DOCUMENTS" },
                    { 6, "VERIFY" },
                    { 7, "SUBMITTED" },
                    { 8, "CONFIRMED" },
                    { 9, "DECLINED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_roles",
                columns: new[] { "company_role_id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE_PARTICIPANT" },
                    { 2, "APP_PROVIDER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_status",
                columns: new[] { "company_status_id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "REJECTED" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_user_status",
                columns: new[] { "company_user_status_id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "consent_status",
                columns: new[] { "consent_status_id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_types",
                columns: new[] { "document_type_id", "label" },
                values: new object[,]
                {
                    { 1, "CXFrameContract" },
                    { 2, "CommercialRegisterExtract" },
                    { 3, "AppContract" },
                    { 4, "DataContract" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_provider_categories",
                columns: new[] { "identity_provider_category_id", "label" },
                values: new object[,]
                {
                    { 1, "KEYCLOAK_SHARED" },
                    { 2, "KEYCLOAK_OIDC" },
                    { 3, "KEYCLOAK_SAML" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "invitation_status",
                columns: new[] { "invitation_status_id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "PENDING" },
                    { 3, "ACCEPTED" },
                    { 4, "DECLINED" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_addresses_country_alpha2code",
                schema: "portal",
                table: "addresses",
                column: "country_alpha2code");

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_company_roles_company_role_id",
                schema: "portal",
                table: "agreement_assigned_company_roles",
                column: "company_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_document_templates_document_template_id",
                schema: "portal",
                table: "agreement_assigned_document_templates",
                column: "document_template_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agreements_agreement_category_id",
                schema: "portal",
                table: "agreements",
                column: "agreement_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_app_id",
                schema: "portal",
                table: "agreements",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_issuer_company_id",
                schema: "portal",
                table: "agreements",
                column: "issuer_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_use_case_id",
                schema: "portal",
                table: "agreements",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_clients_iam_client_id",
                schema: "portal",
                table: "app_assigned_clients",
                column: "iam_client_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_licenses_app_license_id",
                schema: "portal",
                table: "app_assigned_licenses",
                column: "app_license_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_assigned_use_cases_use_case_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_descriptions_language_short_name",
                schema: "portal",
                table: "app_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_app_detail_images_app_id",
                schema: "portal",
                table: "app_detail_images",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_languages_language_short_name",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_apps_app_status_id",
                schema: "portal",
                table: "apps",
                column: "app_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_provider_company_id",
                schema: "portal",
                table: "apps",
                column: "provider_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_address_id",
                schema: "portal",
                table: "companies",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_company_status_id",
                schema: "portal",
                table: "companies",
                column: "company_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_application_status_id",
                schema: "portal",
                table: "company_applications",
                column: "application_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_applications_company_id",
                schema: "portal",
                table: "company_applications",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_apps_app_id",
                schema: "portal",
                table: "company_assigned_apps",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_roles_company_role_id",
                schema: "portal",
                table: "company_assigned_roles",
                column: "company_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_assigned_use_cases_use_case_id",
                schema: "portal",
                table: "company_assigned_use_cases",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_identity_provider_identity_provider_id",
                schema: "portal",
                table: "company_identity_provider",
                column: "identity_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_role_descriptions_language_short_name",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_app_favourites_app_id",
                schema: "portal",
                table: "company_user_assigned_app_favourites",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_user_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_user_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_company_id",
                schema: "portal",
                table: "company_users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_users_company_user_status_id",
                schema: "portal",
                table: "company_users",
                column: "company_user_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_agreement_id",
                schema: "portal",
                table: "consents",
                column: "agreement_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_company_id",
                schema: "portal",
                table: "consents",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_company_user_id",
                schema: "portal",
                table: "consents",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_consent_status_id",
                schema: "portal",
                table: "consents",
                column: "consent_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_consents_document_id",
                schema: "portal",
                table: "consents",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_company_user_id",
                schema: "portal",
                table: "documents",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_document_type_id",
                schema: "portal",
                table: "documents",
                column: "document_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_iam_clients_client_client_id",
                schema: "portal",
                table: "iam_clients",
                column: "client_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_identity_providers_identity_provider_id",
                schema: "portal",
                table: "iam_identity_providers",
                column: "identity_provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_users_company_user_id",
                schema: "portal",
                table: "iam_users",
                column: "company_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_providers_identity_provider_category_id",
                schema: "portal",
                table: "identity_providers",
                column: "identity_provider_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_company_application_id",
                schema: "portal",
                table: "invitations",
                column: "company_application_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_company_user_id",
                schema: "portal",
                table: "invitations",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_invitation_status_id",
                schema: "portal",
                table: "invitations",
                column: "invitation_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_descriptions_language_short_name",
                schema: "portal",
                table: "user_role_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_iam_client_id",
                schema: "portal",
                table: "user_roles",
                column: "iam_client_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agreement_assigned_company_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_assigned_document_templates",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_assigned_clients",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_assigned_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_detail_images",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_languages",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_tags",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_apps",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_identity_provider",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_role_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_app_favourites",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_users",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "invitations",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "document_templates",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreements",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consent_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_applications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "invitation_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "languages",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "document_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_users",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "apps",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_categories",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_provider_categories",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_application_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_clients",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "addresses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "countries",
                schema: "portal");
        }
    }
}
