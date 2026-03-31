-- =============================================
-- Fix reference tables in local dev DB
--
-- Problems fixed:
--   1. code column in ref_incident_report_reference was set to derived names
--      (e.g. 'plan_permit') instead of the reference type code (e.g. 'documenttype')
--   2. ref_doc_type was never populated (DBInitializer put doc items in wrong table)
--   3. ref_investigation_reference was never populated
--
-- Run this once against the local dev database.
-- Safe to re-run: all INSERTs use per-row EXISTS checks.
-- =============================================

USE [stronghold_safety_dev];
GO

-- =============================================
-- STEP 1: Fix codes in ref_incident_report_reference
-- Set code = reference type code for all existing rows
-- =============================================
UPDATE r
SET    r.code       = rt.code,
       r.updated_at = GETUTCDATE()
FROM   safety.ref_incident_report_reference r
JOIN   safety.ref_reference_type            rt ON r.reference_type_id = rt.id;
GO

-- =============================================
-- STEP 2: Populate ref_doc_type  (documenttype items)
-- DBInitializer put these in ref_incident_report_reference by mistake.
-- We keep them there AND also seed the correct table here.
-- =============================================

DECLARE @rt_documenttype UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype');

IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Plan Permit')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Plan Permit', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Equipment Inspection')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Equipment Inspection', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Citation')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Citation', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Strongcard/FLHA')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Strongcard/FLHA', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Policy Report')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Policy Report', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Statement of all Involved')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Statement of all Involved', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Daily Pre-Shift/Toolbox')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Daily Pre-Shift/Toolbox', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Third Party Data')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Third Party Data', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Photograph')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Photograph', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'JHA')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'JHA', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Training Records')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Training Records', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_doc_type WHERE reference_type_id = @rt_documenttype AND name = N'Project Inspection')
    INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_documenttype, 'documenttype', N'Project Inspection', 1, GETUTCDATE(), GETUTCDATE());
GO

-- =============================================
-- STEP 3: Fill missing items in ref_incident_report_reference
-- Types: environmentaltype, equipmentdamage, incidenttype,
--        injurydetail, injurytype, motorvehicleaccident, policydeviation
-- =============================================

DECLARE @rt_environmentaltype  UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'environmentaltype');
DECLARE @rt_equipmentdamage    UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage');
DECLARE @rt_incidenttype       UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype');
DECLARE @rt_injurydetail       UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'injurydetail');
DECLARE @rt_injurytype         UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype');
DECLARE @rt_motorvehicle       UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'motorvehicleaccident');
DECLARE @rt_policydeviation    UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'policydeviation');

-- environmentaltype
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_environmentaltype AND name = N'Emission')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_environmentaltype, 'environmentaltype', N'Emission', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_environmentaltype AND name = N'Release')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_environmentaltype, 'environmentaltype', N'Release', 1, GETUTCDATE(), GETUTCDATE());

-- equipmentdamage
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_equipmentdamage AND name = N'Backing Up')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_equipmentdamage, 'equipmentdamage', N'Backing Up', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_equipmentdamage AND name = N'Fire')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_equipmentdamage, 'equipmentdamage', N'Fire', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_equipmentdamage AND name = N'Malfunction')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_equipmentdamage, 'equipmentdamage', N'Malfunction', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_equipmentdamage AND name = N'Unreported Damage')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_equipmentdamage, 'equipmentdamage', N'Unreported Damage', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_equipmentdamage AND name = N'Windshield')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_equipmentdamage, 'equipmentdamage', N'Windshield', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_equipmentdamage AND name = N'Explosion')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_equipmentdamage, 'equipmentdamage', N'Explosion', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_equipmentdamage AND name = N'Struck By')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_equipmentdamage, 'equipmentdamage', N'Struck By', 1, GETUTCDATE(), GETUTCDATE());

-- incidenttype
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Transportation Compliance')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Transportation Compliance', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Injury')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Injury', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Environmental')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Environmental', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Policy Deviation')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Policy Deviation', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Security')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Security', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Equipment Damage')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Equipment Damage', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Motor Vehicle Accident')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Motor Vehicle Accident', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_incidenttype AND name = N'Life Support')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_incidenttype, 'incidenttype', N'Life Support', 1, GETUTCDATE(), GETUTCDATE());

-- injurydetail
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurydetail AND name = N'Modified Work')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurydetail, 'injurydetail', N'Modified Work', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurydetail AND name = N'Alleged')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurydetail, 'injurydetail', N'Alleged', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurydetail AND name = N'Lost Time')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurydetail, 'injurydetail', N'Lost Time', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurydetail AND name = N'Medical Aid')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurydetail, 'injurydetail', N'Medical Aid', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurydetail AND name = N'Report Only')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurydetail, 'injurydetail', N'Report Only', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurydetail AND name = N'First-Aid')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurydetail, 'injurydetail', N'First-Aid', 1, GETUTCDATE(), GETUTCDATE());

-- injurytype
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Fall Next Level')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Fall Next Level', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Caught Between/Under')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Caught Between/Under', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Contact With')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Contact With', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Caught In')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Caught In', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Caught On')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Caught On', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Exposure')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Exposure', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Overstress Ergonomics')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Overstress Ergonomics', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Fall Same Level')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Fall Same Level', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Struck By')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Struck By', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_injurytype AND name = N'Struck Against')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_injurytype, 'injurytype', N'Struck Against', 1, GETUTCDATE(), GETUTCDATE());

-- motorvehicleaccident
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_motorvehicle AND name = N'Third Party')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_motorvehicle, 'motorvehicleaccident', N'Third Party', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_motorvehicle AND name = N'Single Vehicle')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_motorvehicle, 'motorvehicleaccident', N'Single Vehicle', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_motorvehicle AND name = N'Animal Strike')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_motorvehicle, 'motorvehicleaccident', N'Animal Strike', 1, GETUTCDATE(), GETUTCDATE());

-- policydeviation
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_policydeviation AND name = N'SHC Policy')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_policydeviation, 'policydeviation', N'SHC Policy', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_policydeviation AND name = N'Client Policy')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_policydeviation, 'policydeviation', N'Client Policy', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_incident_report_reference WHERE reference_type_id = @rt_policydeviation AND name = N'Life-Saving Rule Violation')
    INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_policydeviation, 'policydeviation', N'Life-Saving Rule Violation', 1, GETUTCDATE(), GETUTCDATE());
GO

-- =============================================
-- STEP 4: Populate ref_investigation_reference
-- Types: jobfactors, lifesupport, personnelfactors, rootcause,
--        security, transportcompliance, unsafeacts, unsafeconditions
-- =============================================

DECLARE @rt_jobfactors        UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors');
DECLARE @rt_lifesupport       UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport');
DECLARE @rt_personnelfactors  UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors');
DECLARE @rt_rootcause         UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause');
DECLARE @rt_security          UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'security');
DECLARE @rt_transportcomp     UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'transportcompliance');
DECLARE @rt_unsafeacts        UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts');
DECLARE @rt_unsafeconditions  UNIQUEIDENTIFIER = (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions');

-- jobfactors
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Bulletin Reviews')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Bulletin Reviews', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Equipment Inspections')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Equipment Inspections', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'DVIR/Pre-Trip')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'DVIR/Pre-Trip', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Strong Card')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Strong Card', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Policies')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Policies', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Site Inspection')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Site Inspection', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Master JHA')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Master JHA', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Communication/Reinforcement')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Communication/Reinforcement', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Standard/COP')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Standard/COP', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'SOP')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'SOP', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_jobfactors AND name = N'Employee Engagements')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_jobfactors, 'jobfactors', N'Employee Engagements', 1, GETUTCDATE(), GETUTCDATE());

-- lifesupport
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Pigtail')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Pigtail', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Helmet Malfunction')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Helmet Malfunction', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Harness')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Harness', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Trailer/Cube')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Trailer/Cube', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Gas Detection')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Gas Detection', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Helmet Damage')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Helmet Damage', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Egress Cylinder')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Egress Cylinder', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Module')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Module', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Egress Regulator')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Egress Regulator', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_lifesupport AND name = N'Umbilical')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_lifesupport, 'lifesupport', N'Umbilical', 1, GETUTCDATE(), GETUTCDATE());

-- personnelfactors
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Poor Brothers'' & Sisters'' Keeper')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Poor Brothers'' & Sisters'' Keeper', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Inadequate Policies')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Inadequate Policies', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Attitude')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Attitude', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Missing Training')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Missing Training', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Missing Policy')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Missing Policy', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Inadequate Culture Building')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Inadequate Culture Building', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Inadequate Training')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Inadequate Training', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Inadequate Job Planning')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Inadequate Job Planning', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Rogue Employee')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Rogue Employee', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Poor Communication')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Poor Communication', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_personnelfactors AND name = N'Inadequate Motivation')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_personnelfactors, 'personnelfactors', N'Inadequate Motivation', 1, GETUTCDATE(), GETUTCDATE());

-- rootcause
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Inadequate Job Planning')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Inadequate Job Planning', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Poor Brothers'' & Sisters'' Keeper Attitude')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Poor Brothers'' & Sisters'' Keeper Attitude', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Inadequate Culture Building')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Inadequate Culture Building', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Rogue Employee')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Rogue Employee', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Poor Communication')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Poor Communication', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Inadequate Motivation')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Inadequate Motivation', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Missing Training')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Missing Training', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Inadequate Training')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Inadequate Training', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_rootcause AND name = N'Inadequate Policies')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_rootcause, 'rootcause', N'Inadequate Policies', 1, GETUTCDATE(), GETUTCDATE());

-- security
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_security AND name = N'Break In')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_security, 'security', N'Break In', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_security AND name = N'Theft')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_security, 'security', N'Theft', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_security AND name = N'Vandalism')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_security, 'security', N'Vandalism', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_security AND name = N'Arson')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_security, 'security', N'Arson', 1, GETUTCDATE(), GETUTCDATE());

-- transportcompliance
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_transportcomp AND name = N'Citations')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_transportcomp, 'transportcompliance', N'Citations', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_transportcomp AND name = N'Public Driving Complaint')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_transportcomp, 'transportcompliance', N'Public Driving Complaint', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_transportcomp AND name = N'Roadside Inspection')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_transportcomp, 'transportcompliance', N'Roadside Inspection', 1, GETUTCDATE(), GETUTCDATE());

-- unsafeacts
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Bypassing Safety Controls')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Bypassing Safety Controls', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Horseplay')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Horseplay', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'CSE Violation')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'CSE Violation', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Energy Isolation')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Energy Isolation', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Failure to Secure Equipment')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Failure to Secure Equipment', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Not Driving to Road Conditions')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Not Driving to Road Conditions', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Overconfident')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Overconfident', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Failure to Inspect')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Failure to Inspect', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Failure to Identify or Control a Hazard')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Failure to Identify or Control a Hazard', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Fall Protection Violation')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Fall Protection Violation', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Improper use of tools/equipment')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Improper use of tools/equipment', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Inattentive to Hazards')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Inattentive to Hazards', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Line of Fire Positioning')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Line of Fire Positioning', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Not Fit for Duty')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Not Fit for Duty', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Trying to Gain or Save Time')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Trying to Gain or Save Time', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Using Uninspected Equipment')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Using Uninspected Equipment', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Workplace Violence/Systemic')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Workplace Violence/Systemic', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeacts AND name = N'Working without Authority / Permit / Incorrect Permit')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeacts, 'unsafeacts', N'Working without Authority / Permit / Incorrect Permit', 1, GETUTCDATE(), GETUTCDATE());

-- unsafeconditions
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Inadequate Procedure')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Inadequate Procedure', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Inadequate Training')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Inadequate Training', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Poor Housekeeping')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Poor Housekeeping', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Poor Ventilation')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Poor Ventilation', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Improper Storage')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Improper Storage', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Congestion')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Congestion', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Inadequate Warning')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Inadequate Warning', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Inadequate Guard')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Inadequate Guard', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Fire Hazard')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Fire Hazard', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Poor Lighting')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Poor Lighting', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Road Conditions')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Road Conditions', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Weather')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Weather', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Tripping Hazards')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Tripping Hazards', 1, GETUTCDATE(), GETUTCDATE());
IF NOT EXISTS (SELECT 1 FROM safety.ref_investigation_reference WHERE reference_type_id = @rt_unsafeconditions AND name = N'Condition Change')
    INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at) VALUES (NEWID(), @rt_unsafeconditions, 'unsafeconditions', N'Condition Change', 1, GETUTCDATE(), GETUTCDATE());
GO

-- =============================================
-- Verification
-- =============================================
SELECT 'ref_doc_type'                    AS tbl, COUNT(*) AS row_count FROM safety.ref_doc_type
UNION ALL
SELECT 'ref_incident_report_reference',           COUNT(*) FROM safety.ref_incident_report_reference
UNION ALL
SELECT 'ref_investigation_reference',             COUNT(*) FROM safety.ref_investigation_reference;

-- Expected: ref_doc_type=12, ref_incident_report_reference>=39, ref_investigation_reference=80
GO
