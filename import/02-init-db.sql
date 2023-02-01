--
-- Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
--
-- See the NOTICE file(s) distributed with this work for additional
-- information regarding copyright ownership.
--
-- This program and the accompanying materials are made available under the
-- terms of the Apache License, Version 2.0 which is available at
-- https://www.apache.org/licenses/LICENSE-2.0.
--
-- Unless required by applicable law or agreed to in writing, software
-- distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
-- WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
-- License for the specific language governing permissions and limitations
-- under the License.
--
-- SPDX-License-Identifier: Apache-2.0
--

CREATE SCHEMA portal;
ALTER SCHEMA portal OWNER TO portal;
CREATE SCHEMA provisioning;
ALTER SCHEMA provisioning OWNER TO provisioning;
CREATE TABLE public.__efmigrations_history_portal (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);
ALTER TABLE public.__efmigrations_history_portal OWNER TO portal;
CREATE TABLE public.__efmigrations_history_provisioning (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL
);
ALTER TABLE public.__efmigrations_history_provisioning OWNER TO provisioning;
