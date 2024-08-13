---------------------------------------------------------------
-- Copyright (c) 2024 Contributors to the Eclipse Foundation
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
---------------------------------------------------------------

SELECT application_id, act.label, date_created, date_last_changed, acs.label, comment 
FROM portal.application_checklist as ac
JOIN portal.application_checklist_types as act ON act.id = ac.application_checklist_entry_type_id
JOIN portal.application_checklist_statuses as acs ON acs.id = ac.application_checklist_entry_status_id
WHERE application_id in (
	SELECT id 
	FROM portal.company_applications 
	WHERE company_id in (
		SELECT id 
		FROM portal.companies
		WHERE business_partner_number = 'BPNL0000000TESTE'
	)
)