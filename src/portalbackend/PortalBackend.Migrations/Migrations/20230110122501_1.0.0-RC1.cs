/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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
// See https://aka.ms/new-console-template for more information

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Migrations
{
    public partial class _100RC1 : Migration
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
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_app_subscription_detail20221118",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_app_subscription_detail20221118", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_application20221005",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_application20221005", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_user_assigned_role20221018",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user_assigned_role20221018", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_company_user20221005",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    firstname = table.Column<string>(type: "text", nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "text", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_user20221005", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_offer_subscription20221005",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer_subscription20221005", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_offer20221013",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    marketing_url = table.Column<string>(type: "text", nullable: true),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    contact_number = table.Column<string>(type: "text", nullable: true),
                    provider = table.Column<string>(type: "text", nullable: false),
                    offer_type_id = table.Column<int>(type: "integer", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    offer_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_offer20221013", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_operation",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_user_role20221017",
                schema: "portal",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role = table.Column<string>(type: "text", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_user_role20221017", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "company_application_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_application_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_roles",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_service_account_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_service_account_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_user_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "connector_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connector_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "connector_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connector_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consent_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_statuses", x => x.id);
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
                name: "document_status",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_types", x => x.id);
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
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_provider_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invitation_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitation_statuses", x => x.id);
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
                name: "notification_topic",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_topic", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_type",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_licenses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    licensetext = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_licenses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_subscription_statuses",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_subscription_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_types",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unique_identifiers",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unique_identifiers", x => x.id);
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
                name: "user_role_collections",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_collections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_role_registration_data",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    is_registration_role = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_role_registration_data", x => x.company_role_id);
                    table.ForeignKey(
                        name: "fk_company_role_registration_data_company_roles_company_role_id",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    zipcode = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    country_alpha2code = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_addresses_countries_country_temp_id",
                        column: x => x.country_alpha2code,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code",
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
                        name: "fk_identity_providers_identity_provider_categories_identity_pr",
                        column: x => x.identity_provider_category_id,
                        principalSchema: "portal",
                        principalTable: "identity_provider_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_role_descriptions",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_role_descriptions", x => new { x.company_role_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_company_role_descriptions_company_roles_company_role_id",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_role_descriptions_languages_language_temp_id1",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_type_assigned_topics",
                schema: "portal",
                columns: table => new
                {
                    notification_type_id = table.Column<int>(type: "integer", nullable: false),
                    notification_topic_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_type_assigned_topics", x => new { x.notification_type_id, x.notification_topic_id });
                    table.ForeignKey(
                        name: "fk_notification_type_assigned_topics_notification_topic_notifi",
                        column: x => x.notification_topic_id,
                        principalSchema: "portal",
                        principalTable: "notification_topic",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notification_type_assigned_topics_notification_type_notific",
                        column: x => x.notification_type_id,
                        principalSchema: "portal",
                        principalTable: "notification_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "country_assigned_identifier",
                schema: "portal",
                columns: table => new
                {
                    country_alpha2code = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    unique_identifier_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_country_assigned_identifier", x => new { x.country_alpha2code, x.unique_identifier_id });
                    table.ForeignKey(
                        name: "fk_country_assigned_identifier_countries_country_alpha2code",
                        column: x => x.country_alpha2code,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code");
                    table.ForeignKey(
                        name: "fk_country_assigned_identifier_unique_identifiers_unique_ident",
                        column: x => x.unique_identifier_id,
                        principalSchema: "portal",
                        principalTable: "unique_identifiers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_role_assigned_role_collections",
                schema: "portal",
                columns: table => new
                {
                    company_role_id = table.Column<int>(type: "integer", nullable: false),
                    user_role_collection_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_role_assigned_role_collections", x => x.company_role_id);
                    table.ForeignKey(
                        name: "fk_company_role_assigned_role_collections_company_roles_compan",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_role_assigned_role_collections_user_role_collection",
                        column: x => x.user_role_collection_id,
                        principalSchema: "portal",
                        principalTable: "user_role_collections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role_collection_descriptions",
                schema: "portal",
                columns: table => new
                {
                    user_role_collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_collection_descriptions", x => new { x.user_role_collection_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_user_role_collection_descriptions_languages_language_short_",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_role_collection_descriptions_user_role_collections_use",
                        column: x => x.user_role_collection_id,
                        principalSchema: "portal",
                        principalTable: "user_role_collections",
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
                        name: "fk_iam_identity_providers_identity_providers_identity_provider",
                        column: x => x.identity_provider_id,
                        principalSchema: "portal",
                        principalTable: "identity_providers",
                        principalColumn: "id");
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
                    table.PrimaryKey("pk_agreement_assigned_company_roles", x => new { x.agreement_id, x.company_role_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_company_roles_company_roles_company_role",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "agreement_assigned_documents",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_documents", x => new { x.agreement_id, x.document_id });
                });

            migrationBuilder.CreateTable(
                name: "agreement_assigned_offer_types",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_offer_types", x => new { x.agreement_id, x.offer_type_id });
                    table.ForeignKey(
                        name: "fk_agreement_assigned_offer_types_offer_types_offer_type_id",
                        column: x => x.offer_type_id,
                        principalSchema: "portal",
                        principalTable: "offer_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "agreement_assigned_offers",
                schema: "portal",
                columns: table => new
                {
                    agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreement_assigned_offers", x => new { x.agreement_id, x.offer_id });
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
                    issuer_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agreements", x => x.id);
                    table.ForeignKey(
                        name: "fk_agreements_agreement_categories_agreement_category_id",
                        column: x => x.agreement_category_id,
                        principalSchema: "portal",
                        principalTable: "agreement_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_agreements_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
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
                        name: "fk_app_assigned_use_cases_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "app_instances",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    iam_client_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_instances", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_instances_iam_clients_iam_client_id",
                        column: x => x.iam_client_id,
                        principalSchema: "portal",
                        principalTable: "iam_clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                    table.PrimaryKey("pk_app_languages", x => new { x.app_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_app_languages_languages_language_temp_id",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                });

            migrationBuilder.CreateTable(
                name: "app_subscription_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    app_subscription_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_subscription_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_app_subscription_details_app_instances_app_instance_id",
                        column: x => x.app_instance_id,
                        principalSchema: "portal",
                        principalTable: "app_instances",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    business_partner_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    shortname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    company_status_id = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<Guid>(type: "uuid", nullable: true),
                    self_description_document_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_companies_addresses_address_id",
                        column: x => x.address_id,
                        principalSchema: "portal",
                        principalTable: "addresses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_companies_company_statuses_company_status_id",
                        column: x => x.company_status_id,
                        principalSchema: "portal",
                        principalTable: "company_statuses",
                        principalColumn: "id",
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
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_applications_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_applications_company_application_statuses_applicati",
                        column: x => x.application_status_id,
                        principalSchema: "portal",
                        principalTable: "company_application_statuses",
                        principalColumn: "id",
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
                        name: "fk_company_assigned_roles_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_roles_company_roles_company_role_id",
                        column: x => x.company_role_id,
                        principalSchema: "portal",
                        principalTable: "company_roles",
                        principalColumn: "id");
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
                    table.PrimaryKey("pk_company_assigned_use_cases", x => new { x.company_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_company_assigned_use_cases_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_assigned_use_cases_use_cases_use_case_id",
                        column: x => x.use_case_id,
                        principalSchema: "portal",
                        principalTable: "use_cases",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_identifiers",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unique_identifier_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_identifiers", x => new { x.company_id, x.unique_identifier_id });
                    table.ForeignKey(
                        name: "fk_company_identifiers_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_identifiers_unique_identifiers_unique_identifier_id",
                        column: x => x.unique_identifier_id,
                        principalSchema: "portal",
                        principalTable: "unique_identifiers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_identity_providers",
                schema: "portal",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_provider_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_identity_providers", x => new { x.company_id, x.identity_provider_id });
                    table.ForeignKey(
                        name: "fk_company_identity_providers_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_identity_providers_identity_providers_identity_prov",
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
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    firstname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    lastlogin = table.Column<byte[]>(type: "bytea", nullable: true),
                    lastname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_user_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_users_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_users_company_user_statuses_company_user_status_id",
                        column: x => x.company_user_status_id,
                        principalSchema: "portal",
                        principalTable: "company_user_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "provider_company_details",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    auto_setup_url = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_company_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_provider_company_details_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_business_partners",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_partner_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_business_partners", x => new { x.company_user_id, x.business_partner_number });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_business_partners_company_users_compa",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    document_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    document_content = table.Column<byte[]>(type: "bytea", nullable: false),
                    document_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    document_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_documents_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_documents_document_status_document_status_id",
                        column: x => x.document_status_id,
                        principalSchema: "portal",
                        principalTable: "document_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_documents_document_types_document_type_id",
                        column: x => x.document_type_id,
                        principalSchema: "portal",
                        principalTable: "document_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                        name: "fk_iam_users_company_users_company_user_id",
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
                        name: "fk_invitations_company_applications_company_application_id",
                        column: x => x.company_application_id,
                        principalSchema: "portal",
                        principalTable: "company_applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_invitations_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_invitations_invitation_statuses_invitation_status_id",
                        column: x => x.invitation_status_id,
                        principalSchema: "portal",
                        principalTable: "invitation_statuses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    notification_type_id = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_company_users_creator_id",
                        column: x => x.creator_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_company_users_receiver_id",
                        column: x => x.receiver_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_notification_type_notification_type_id",
                        column: x => x.notification_type_id,
                        principalSchema: "portal",
                        principalTable: "notification_type",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offers",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_released = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    marketing_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    contact_number = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    offer_type_id = table.Column<int>(type: "integer", nullable: false),
                    sales_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    offer_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offers", x => x.id);
                    table.ForeignKey(
                        name: "fk_offers_companies_provider_company_id",
                        column: x => x.provider_company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offers_company_users_sales_manager_id",
                        column: x => x.sales_manager_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offers_offer_statuses_offer_status_id",
                        column: x => x.offer_status_id,
                        principalSchema: "portal",
                        principalTable: "offer_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_offers_offer_types_offer_type_id",
                        column: x => x.offer_type_id,
                        principalSchema: "portal",
                        principalTable: "offer_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connectors",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    connector_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    host_id = table.Column<Guid>(type: "uuid", nullable: true),
                    self_description_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    daps_registration_successful = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connectors", x => x.id);
                    table.ForeignKey(
                        name: "fk_connectors_companies_host_id",
                        column: x => x.host_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_connectors_companies_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_connectors_connector_statuses_status_id",
                        column: x => x.status_id,
                        principalSchema: "portal",
                        principalTable: "connector_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_connectors_connector_types_type_id",
                        column: x => x.type_id,
                        principalSchema: "portal",
                        principalTable: "connector_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_connectors_countries_location_temp_id1",
                        column: x => x.location_id,
                        principalSchema: "portal",
                        principalTable: "countries",
                        principalColumn: "alpha2code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_connectors_documents_self_description_document_id",
                        column: x => x.self_description_document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
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
                        name: "fk_consents_agreements_agreement_id",
                        column: x => x.agreement_id,
                        principalSchema: "portal",
                        principalTable: "agreements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consents_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consents_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consents_consent_statuses_consent_status_id",
                        column: x => x.consent_status_id,
                        principalSchema: "portal",
                        principalTable: "consent_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consents_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
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
                    table.PrimaryKey("pk_company_user_assigned_app_favourites", x => new { x.company_user_id, x.app_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_app_favourites_company_users_company_",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_app_favourites_offers_app_id",
                        column: x => x.app_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_assigned_documents",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_assigned_documents", x => new { x.offer_id, x.document_id });
                    table.ForeignKey(
                        name: "fk_offer_assigned_documents_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "portal",
                        principalTable: "documents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_assigned_documents_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_assigned_licenses",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_license_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_assigned_licenses", x => new { x.offer_id, x.offer_license_id });
                    table.ForeignKey(
                        name: "fk_offer_assigned_licenses_offer_licenses_offer_license_id",
                        column: x => x.offer_license_id,
                        principalSchema: "portal",
                        principalTable: "offer_licenses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_assigned_licenses_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_descriptions",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description_long = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    description_short = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_descriptions", x => new { x.offer_id, x.language_short_name });
                    table.ForeignKey(
                        name: "fk_offer_descriptions_languages_language_short_name",
                        column: x => x.language_short_name,
                        principalSchema: "portal",
                        principalTable: "languages",
                        principalColumn: "short_name");
                    table.ForeignKey(
                        name: "fk_offer_descriptions_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_detail_images",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_detail_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_detail_images_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_subscriptions",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_subscription_status_id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_company_users_requester_id",
                        column: x => x.requester_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_offer_subscription_statuses_offer_subsc",
                        column: x => x.offer_subscription_status_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscription_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offer_subscriptions_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offer_tags",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_tags", x => new { x.offer_id, x.tag_name });
                    table.ForeignKey(
                        name: "fk_offer_tags_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "service_assigned_service_types",
                schema: "portal",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_assigned_service_types", x => new { x.service_id, x.service_type_id });
                    table.ForeignKey(
                        name: "fk_service_assigned_service_types_offers_service_id",
                        column: x => x.service_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_service_assigned_service_types_service_types_service_type_id",
                        column: x => x.service_type_id,
                        principalSchema: "portal",
                        principalTable: "service_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consent_assigned_offers",
                schema: "portal",
                columns: table => new
                {
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_assigned_offers", x => new { x.consent_id, x.offer_id });
                    table.ForeignKey(
                        name: "fk_consent_assigned_offers_consents_consent_id",
                        column: x => x.consent_id,
                        principalSchema: "portal",
                        principalTable: "consents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consent_assigned_offers_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "portal",
                        principalTable: "offers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    service_account_owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    company_service_account_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_service_account_status_id = table.Column<int>(type: "integer", nullable: false),
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_service_accounts_companies_service_account_owner_id",
                        column: x => x.service_account_owner_id,
                        principalSchema: "portal",
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_service_accounts_company_service_account_statuses_c",
                        column: x => x.company_service_account_status_id,
                        principalSchema: "portal",
                        principalTable: "company_service_account_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_service_accounts_company_service_account_types_comp",
                        column: x => x.company_service_account_type_id,
                        principalSchema: "portal",
                        principalTable: "company_service_account_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_service_accounts_offer_subscriptions_offer_subscrip",
                        column: x => x.offer_subscription_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscriptions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "consent_assigned_offer_subscriptions",
                schema: "portal",
                columns: table => new
                {
                    offer_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_assigned_offer_subscriptions", x => new { x.consent_id, x.offer_subscription_id });
                    table.ForeignKey(
                        name: "fk_consent_assigned_offer_subscriptions_consents_consent_id",
                        column: x => x.consent_id,
                        principalSchema: "portal",
                        principalTable: "consents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_consent_assigned_offer_subscriptions_offer_subscriptions_of",
                        column: x => x.offer_subscription_id,
                        principalSchema: "portal",
                        principalTable: "offer_subscriptions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_user_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_user_assigned_roles", x => new { x.company_user_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_company_user_assigned_roles_company_users_company_user_id",
                        column: x => x.company_user_id,
                        principalSchema: "portal",
                        principalTable: "company_users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_user_assigned_roles_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_role_assigned_collections",
                schema: "portal",
                columns: table => new
                {
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_collection_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_assigned_collections", x => new { x.user_role_id, x.user_role_collection_id });
                    table.ForeignKey(
                        name: "fk_user_role_assigned_collections_user_role_collections_user_r",
                        column: x => x.user_role_collection_id,
                        principalSchema: "portal",
                        principalTable: "user_role_collections",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_role_assigned_collections_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_role_descriptions",
                schema: "portal",
                columns: table => new
                {
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_short_name = table.Column<string>(type: "character(2)", maxLength: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
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
                name: "company_service_account_assigned_roles",
                schema: "portal",
                columns: table => new
                {
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_service_account_assigned_roles", x => new { x.company_service_account_id, x.user_role_id });
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_company_service_acco",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_service_account_assigned_roles_user_roles_user_role",
                        column: x => x.user_role_id,
                        principalSchema: "portal",
                        principalTable: "user_roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "iam_service_accounts",
                schema: "portal",
                columns: table => new
                {
                    client_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    client_client_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    company_service_account_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iam_service_accounts", x => x.client_id);
                    table.ForeignKey(
                        name: "fk_iam_service_accounts_company_service_accounts_company_servi",
                        column: x => x.company_service_account_id,
                        principalSchema: "portal",
                        principalTable: "company_service_accounts",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "agreement_categories",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CX_FRAME_CONTRACT" },
                    { 2, "APP_CONTRACT" },
                    { 3, "DATA_CONTRACT" },
                    { 4, "SERVICE_CONTRACT" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "audit_operation",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INSERT" },
                    { 2, "UPDATE" },
                    { 3, "DELETE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_application_statuses",
                columns: new[] { "id", "label" },
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
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE_PARTICIPANT" },
                    { 2, "APP_PROVIDER" },
                    { 3, "SERVICE_PROVIDER" },
                    { 4, "OPERATOR" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_service_account_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_service_account_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "MANAGED" },
                    { 2, "OWN" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "REJECTED" },
                    { 4, "INACTIVE" },
                    { 5, "DELETED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "company_user_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" },
                    { 3, "DELETED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "connector_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "connector_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COMPANY_CONNECTOR" },
                    { 2, "CONNECTOR_AS_A_SERVICE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "consent_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_status",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "LOCKED" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "document_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CX_FRAME_CONTRACT" },
                    { 2, "COMMERCIAL_REGISTER_EXTRACT" },
                    { 3, "APP_CONTRACT" },
                    { 4, "APP_DATA_DETAILS" },
                    { 5, "ADDITIONAL_DETAILS" },
                    { 6, "APP_LEADIMAGE" },
                    { 7, "APP_IMAGE" },
                    { 8, "SELF_DESCRIPTION" },
                    { 9, "APP_TECHNICAL_INFORMATION" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "identity_provider_categories",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "KEYCLOAK_SHARED" },
                    { 2, "KEYCLOAK_OIDC" },
                    { 3, "KEYCLOAK_SAML" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "invitation_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "PENDING" },
                    { 3, "ACCEPTED" },
                    { 4, "DECLINED" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_topic",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INFO" },
                    { 2, "ACTION" },
                    { 3, "OFFER" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "notification_type",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "INFO" },
                    { 2, "ACTION" },
                    { 3, "WELCOME" },
                    { 4, "WELCOME_USE_CASES" },
                    { 5, "WELCOME_SERVICE_PROVIDER" },
                    { 6, "WELCOME_CONNECTOR_REGISTRATION" },
                    { 7, "WELCOME_APP_MARKETPLACE" },
                    { 8, "APP_SUBSCRIPTION_REQUEST" },
                    { 9, "APP_SUBSCRIPTION_ACTIVATION" },
                    { 10, "CONNECTOR_REGISTERED" },
                    { 11, "APP_RELEASE_REQUEST" },
                    { 12, "TECHNICAL_USER_CREATION" },
                    { 13, "SERVICE_REQUEST" },
                    { 14, "SERVICE_ACTIVATION" },
                    { 15, "APP_ROLE_ADDED" },
                    { 16, "APP_RELEASE_APPROVAL" },
                    { 17, "SERVICE_RELEASE_REQUEST" },
                    { 18, "SERVICE_RELEASE_APPROVAL" },
                    { 19, "APP_RELEASE_REJECTION" },
                    { 20, "SERVICE_RELEASE_REJECTION" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "offer_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CREATED" },
                    { 2, "IN_REVIEW" },
                    { 3, "ACTIVE" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "offer_subscription_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "offer_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "APP" },
                    { 2, "CORE_COMPONENT" },
                    { 3, "SERVICE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "service_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "DATASPACE_SERVICE" },
                    { 2, "CONSULTANCE_SERVICE" }
                });

            migrationBuilder.InsertData(
                schema: "portal",
                table: "unique_identifiers",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "COMMERCIAL_REG_NUMBER" },
                    { 2, "VAT_ID" },
                    { 3, "LEI_CODE" },
                    { 4, "VIES" },
                    { 5, "EORI" }
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
                name: "ix_agreement_assigned_documents_document_id",
                schema: "portal",
                table: "agreement_assigned_documents",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_offer_types_offer_type_id",
                schema: "portal",
                table: "agreement_assigned_offer_types",
                column: "offer_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreement_assigned_offers_offer_id",
                schema: "portal",
                table: "agreement_assigned_offers",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_agreements_agreement_category_id",
                schema: "portal",
                table: "agreements",
                column: "agreement_category_id");

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
                name: "ix_app_assigned_use_cases_use_case_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "use_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_instances_app_id",
                schema: "portal",
                table: "app_instances",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_instances_iam_client_id",
                schema: "portal",
                table: "app_instances",
                column: "iam_client_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_languages_language_short_name",
                schema: "portal",
                table: "app_languages",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_app_subscription_details_app_instance_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "app_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_app_subscription_details_offer_subscription_id",
                schema: "portal",
                table: "app_subscription_details",
                column: "offer_subscription_id",
                unique: true);

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
                name: "ix_companies_self_description_document_id",
                schema: "portal",
                table: "companies",
                column: "self_description_document_id");

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
                name: "ix_company_identifiers_unique_identifier_id",
                schema: "portal",
                table: "company_identifiers",
                column: "unique_identifier_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_identity_providers_identity_provider_id",
                schema: "portal",
                table: "company_identity_providers",
                column: "identity_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_role_assigned_role_collections_user_role_collection",
                schema: "portal",
                table: "company_role_assigned_role_collections",
                column: "user_role_collection_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_role_descriptions_language_short_name",
                schema: "portal",
                table: "company_role_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_account_assigned_roles_user_role_id",
                schema: "portal",
                table: "company_service_account_assigned_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_status_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_company_service_account_type_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "company_service_account_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_offer_subscription_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "offer_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_service_accounts_service_account_owner_id",
                schema: "portal",
                table: "company_service_accounts",
                column: "service_account_owner_id");

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
                name: "ix_connectors_host_id",
                schema: "portal",
                table: "connectors",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_location_id",
                schema: "portal",
                table: "connectors",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_provider_id",
                schema: "portal",
                table: "connectors",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_self_description_document_id",
                schema: "portal",
                table: "connectors",
                column: "self_description_document_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_connectors_status_id",
                schema: "portal",
                table: "connectors",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectors_type_id",
                schema: "portal",
                table: "connectors",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_assigned_offer_subscriptions_offer_subscription_id",
                schema: "portal",
                table: "consent_assigned_offer_subscriptions",
                column: "offer_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_assigned_offers_offer_id",
                schema: "portal",
                table: "consent_assigned_offers",
                column: "offer_id");

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
                name: "ix_country_assigned_identifier_unique_identifier_id",
                schema: "portal",
                table: "country_assigned_identifier",
                column: "unique_identifier_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_company_user_id",
                schema: "portal",
                table: "documents",
                column: "company_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_document_status_id",
                schema: "portal",
                table: "documents",
                column: "document_status_id");

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
                name: "ix_iam_service_accounts_client_client_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "client_client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_company_service_account_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "company_service_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iam_service_accounts_user_entity_id",
                schema: "portal",
                table: "iam_service_accounts",
                column: "user_entity_id",
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
                name: "ix_notification_type_assigned_topics_notification_topic_id",
                schema: "portal",
                table: "notification_type_assigned_topics",
                column: "notification_topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_type_assigned_topics_notification_type_id",
                schema: "portal",
                table: "notification_type_assigned_topics",
                column: "notification_type_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_creator_user_id",
                schema: "portal",
                table: "notifications",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_notification_type_id",
                schema: "portal",
                table: "notifications",
                column: "notification_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_receiver_user_id",
                schema: "portal",
                table: "notifications",
                column: "receiver_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_assigned_documents_document_id",
                schema: "portal",
                table: "offer_assigned_documents",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_assigned_licenses_offer_license_id",
                schema: "portal",
                table: "offer_assigned_licenses",
                column: "offer_license_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_descriptions_language_short_name",
                schema: "portal",
                table: "offer_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_offer_detail_images_offer_id",
                schema: "portal",
                table: "offer_detail_images",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_company_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_offer_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_offer_subscription_status_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "offer_subscription_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_subscriptions_requester_id",
                schema: "portal",
                table: "offer_subscriptions",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_offer_status_id",
                schema: "portal",
                table: "offers",
                column: "offer_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_offer_type_id",
                schema: "portal",
                table: "offers",
                column: "offer_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_provider_company_id",
                schema: "portal",
                table: "offers",
                column: "provider_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_sales_manager_id",
                schema: "portal",
                table: "offers",
                column: "sales_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_provider_company_details_company_id",
                schema: "portal",
                table: "provider_company_details",
                column: "company_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_assigned_service_types_service_type_id",
                schema: "portal",
                table: "service_assigned_service_types",
                column: "service_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assigned_collections_user_role_collection_id",
                schema: "portal",
                table: "user_role_assigned_collections",
                column: "user_role_collection_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_collection_descriptions_language_short_name",
                schema: "portal",
                table: "user_role_collection_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_descriptions_language_short_name",
                schema: "portal",
                table: "user_role_descriptions",
                column: "language_short_name");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_offer_id",
                schema: "portal",
                table: "user_roles",
                column: "offer_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreement_assigned_company_roles_agreements_agreement_id",
                schema: "portal",
                table: "agreement_assigned_company_roles",
                column: "agreement_id",
                principalSchema: "portal",
                principalTable: "agreements",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreement_assigned_documents_agreements_agreement_id",
                schema: "portal",
                table: "agreement_assigned_documents",
                column: "agreement_id",
                principalSchema: "portal",
                principalTable: "agreements",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreement_assigned_documents_documents_document_id",
                schema: "portal",
                table: "agreement_assigned_documents",
                column: "document_id",
                principalSchema: "portal",
                principalTable: "documents",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreement_assigned_offer_types_agreements_agreement_id",
                schema: "portal",
                table: "agreement_assigned_offer_types",
                column: "agreement_id",
                principalSchema: "portal",
                principalTable: "agreements",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreement_assigned_offers_agreements_agreement_id",
                schema: "portal",
                table: "agreement_assigned_offers",
                column: "agreement_id",
                principalSchema: "portal",
                principalTable: "agreements",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreement_assigned_offers_offers_offer_id",
                schema: "portal",
                table: "agreement_assigned_offers",
                column: "offer_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_agreements_companies_issuer_company_id",
                schema: "portal",
                table: "agreements",
                column: "issuer_company_id",
                principalSchema: "portal",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_assigned_use_cases_offers_app_id",
                schema: "portal",
                table: "app_assigned_use_cases",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_instances_offers_app_id",
                schema: "portal",
                table: "app_instances",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_app_languages_offers_app_id",
                schema: "portal",
                table: "app_languages",
                column: "app_id",
                principalSchema: "portal",
                principalTable: "offers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_app_subscription_details_offer_subscriptions_offer_subscrip",
                schema: "portal",
                table: "app_subscription_details",
                column: "offer_subscription_id",
                principalSchema: "portal",
                principalTable: "offer_subscriptions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_companies_documents_self_description_document_id",
                schema: "portal",
                table: "companies",
                column: "self_description_document_id",
                principalSchema: "portal",
                principalTable: "documents",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.offer_subscription_id, \r\n  OLD.app_instance_id, \r\n  OLD.app_subscription_url, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL AFTER DELETE\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.offer_subscription_id, \r\n  NEW.app_instance_id, \r\n  NEW.app_subscription_url, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL AFTER INSERT\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$\r\nBEGIN\r\n  INSERT INTO portal.audit_app_subscription_detail20221118 (\"id\", \"offer_subscription_id\", \"app_instance_id\", \"app_subscription_url\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.offer_subscription_id, \r\n  NEW.app_instance_id, \r\n  NEW.app_subscription_url, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL AFTER UPDATE\r\nON portal.app_subscription_details\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.date_last_changed, \r\n  OLD.application_status_id, \r\n  OLD.company_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION AFTER DELETE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION AFTER INSERT\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_application20221005 (\"id\", \"date_created\", \"date_last_changed\", \"application_status_id\", \"company_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.date_last_changed, \r\n  NEW.application_status_id, \r\n  NEW.company_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION AFTER UPDATE\r\nON portal.company_applications\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.date_created, \r\n  OLD.email, \r\n  OLD.firstname, \r\n  OLD.lastlogin, \r\n  OLD.lastname, \r\n  OLD.company_id, \r\n  OLD.company_user_status_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSER AFTER DELETE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSER AFTER INSERT\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user20221005 (\"id\", \"date_created\", \"email\", \"firstname\", \"lastlogin\", \"lastname\", \"company_id\", \"company_user_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.date_created, \r\n  NEW.email, \r\n  NEW.firstname, \r\n  NEW.lastlogin, \r\n  NEW.lastname, \r\n  NEW.company_id, \r\n  NEW.company_user_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSER AFTER UPDATE\r\nON portal.company_users\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.company_user_id, \r\n  OLD.user_role_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE AFTER DELETE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE AFTER INSERT\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_company_user_assigned_role20221018 (\"company_user_id\", \"user_role_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.company_user_id, \r\n  NEW.user_role_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE AFTER UPDATE\r\nON portal.company_user_assigned_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20221013 (\"id\", \"name\", \"date_created\", \"date_released\", \"thumbnail_url\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.name, \r\n  OLD.date_created, \r\n  OLD.date_released, \r\n  OLD.thumbnail_url, \r\n  OLD.marketing_url, \r\n  OLD.contact_email, \r\n  OLD.contact_number, \r\n  OLD.provider, \r\n  OLD.offer_type_id, \r\n  OLD.sales_manager_id, \r\n  OLD.provider_company_id, \r\n  OLD.offer_status_id, \r\n  OLD.date_last_changed, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFER AFTER DELETE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20221013 (\"id\", \"name\", \"date_created\", \"date_released\", \"thumbnail_url\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.thumbnail_url, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFER AFTER INSERT\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFER$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer20221013 (\"id\", \"name\", \"date_created\", \"date_released\", \"thumbnail_url\", \"marketing_url\", \"contact_email\", \"contact_number\", \"provider\", \"offer_type_id\", \"sales_manager_id\", \"provider_company_id\", \"offer_status_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.name, \r\n  NEW.date_created, \r\n  NEW.date_released, \r\n  NEW.thumbnail_url, \r\n  NEW.marketing_url, \r\n  NEW.contact_email, \r\n  NEW.contact_number, \r\n  NEW.provider, \r\n  NEW.offer_type_id, \r\n  NEW.sales_manager_id, \r\n  NEW.provider_company_id, \r\n  NEW.offer_status_id, \r\n  NEW.date_last_changed, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFER$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFER AFTER UPDATE\r\nON portal.offers\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFER();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.company_id, \r\n  OLD.offer_id, \r\n  OLD.offer_subscription_status_id, \r\n  OLD.display_name, \r\n  OLD.description, \r\n  OLD.requester_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION AFTER DELETE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION AFTER INSERT\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$\r\nBEGIN\r\n  INSERT INTO portal.audit_offer_subscription20221005 (\"id\", \"company_id\", \"offer_id\", \"offer_subscription_status_id\", \"display_name\", \"description\", \"requester_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.company_id, \r\n  NEW.offer_id, \r\n  NEW.offer_subscription_status_id, \r\n  NEW.display_name, \r\n  NEW.description, \r\n  NEW.requester_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION AFTER UPDATE\r\nON portal.offer_subscriptions\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_DELETE_USERROLE() RETURNS trigger as $LC_TRIGGER_AFTER_DELETE_USERROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_user_role20221017 (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT OLD.id, \r\n  OLD.user_role, \r\n  OLD.offer_id, \r\n  OLD.last_editor_id, \r\n  gen_random_uuid(), \r\n  3, \r\n  CURRENT_DATE, \r\n  OLD.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_DELETE_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_DELETE_USERROLE AFTER DELETE\r\nON portal.user_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_DELETE_USERROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_INSERT_USERROLE() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_USERROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_user_role20221017 (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.user_role, \r\n  NEW.offer_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_USERROLE AFTER INSERT\r\nON portal.user_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_INSERT_USERROLE();");

            migrationBuilder.Sql("CREATE FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_USERROLE() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_USERROLE$\r\nBEGIN\r\n  INSERT INTO portal.audit_user_role20221017 (\"id\", \"user_role\", \"offer_id\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.id, \r\n  NEW.user_role, \r\n  NEW.offer_id, \r\n  NEW.last_editor_id, \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.last_editor_id;\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_USERROLE$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_USERROLE AFTER UPDATE\r\nON portal.user_roles\r\nFOR EACH ROW EXECUTE PROCEDURE portal.LC_TRIGGER_AFTER_UPDATE_USERROLE();");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_APPSUBSCRIPTIONDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_APPSUBSCRIPTIONDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_APPSUBSCRIPTIONDETAIL() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYAPPLICATION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_COMPANYUSERASSIGNEDROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFER() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_OFFERSUBSCRIPTION() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_DELETE_USERROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_INSERT_USERROLE() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION portal.LC_TRIGGER_AFTER_UPDATE_USERROLE() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_addresses_countries_country_temp_id",
                schema: "portal",
                table: "addresses");

            migrationBuilder.DropForeignKey(
                name: "fk_companies_documents_self_description_document_id",
                schema: "portal",
                table: "companies");

            migrationBuilder.DropTable(
                name: "agreement_assigned_company_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_assigned_documents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_assigned_offer_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_assigned_offers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_languages",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_subscription_details",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_app_subscription_detail20221118",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_application20221005",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_user_assigned_role20221018",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_company_user20221005",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_offer_subscription20221005",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_offer20221013",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_operation",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "audit_user_role20221017",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_assigned_use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_identifiers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_role_assigned_role_collections",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_role_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_role_registration_data",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_account_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_app_favourites",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_business_partners",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_assigned_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "connectors",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consent_assigned_offer_subscriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consent_assigned_offers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "country_assigned_identifier",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_users",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "invitations",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "notification_type_assigned_topics",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_assigned_documents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_assigned_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_detail_images",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_tags",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "provider_company_details",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_assigned_service_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_assigned_collections",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_collection_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_descriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "app_instances",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "connector_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "connector_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "unique_identifiers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_providers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_accounts",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_applications",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "invitation_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "notification_topic",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "notification_type",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_licenses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "service_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_role_collections",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "languages",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "iam_clients",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreements",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "consent_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "identity_provider_categories",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_account_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_service_account_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_subscriptions",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_application_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "agreement_categories",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "use_cases",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_subscription_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offers",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "offer_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "countries",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_users",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "document_status",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "document_types",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_user_statuses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "addresses",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "company_statuses",
                schema: "portal");
        }
    }
}
