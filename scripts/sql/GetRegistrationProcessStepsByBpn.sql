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

SELECT ps.id, pst.label, pss.label, date_created, date_last_changed, process_id, message
FROM portal.process_steps as ps
JOIN portal.process_step_types as pst ON pst.id = ps.process_step_type_id
JOIN portal.process_step_statuses as pss ON pss.id = ps.process_step_status_id
WHERE ps.process_id in (
	SELECT checklist_process_id 
	FROM portal.company_applications 
	WHERE company_id in (
		SELECT id 
		FROM portal.companies
		WHERE business_partner_number = 'BPNL0000000TESTE'
	)
)
order by date_created desc