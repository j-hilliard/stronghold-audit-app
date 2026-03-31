-- Seed reference tables from Ref_Table_Extract.xlsx
-- Column mapping required by Thad:
--   Column 1 -> name
--   Column 2 -> applies_to (routing + ref_reference_type.applies_to)
--   Column 3 -> code
--
-- Target tables:
--   safety.ref_reference_type
--   safety.ref_doc_type
--   safety.ref_incident_report_reference
--   safety.ref_investigation_reference

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    -- 1) Ensure parent reference types exist and are aligned
    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'injurydetail')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'injurydetail', applies_to = 'incident_report', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'injurydetail';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'injurydetail', 'injurydetail', 'incident_report', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'motorvehicleaccident')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'motorvehicleaccident', applies_to = 'incident_report', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'motorvehicleaccident';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'motorvehicleaccident', 'motorvehicleaccident', 'incident_report', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'security')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'security', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'security';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'security', 'security', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'personnelfactors')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'personnelfactors', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'personnelfactors';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'personnelfactors', 'personnelfactors', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'equipmentdamage')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'equipmentdamage', applies_to = 'incident_report', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'equipmentdamage';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'equipmentdamage', 'equipmentdamage', 'incident_report', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'jobfactors')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'jobfactors', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'jobfactors';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'jobfactors', 'jobfactors', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'unsafeacts')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'unsafeacts', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'unsafeacts';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'unsafeacts', 'unsafeacts', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'injurytype')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'injurytype', applies_to = 'incident_report', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'injurytype';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'injurytype', 'injurytype', 'incident_report', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'documenttype')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'documenttype', applies_to = 'attachment', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'documenttype';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'documenttype', 'documenttype', 'attachment', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'transportcompliance')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'transportcompliance', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'transportcompliance';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'transportcompliance', 'transportcompliance', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'policydeviation')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'policydeviation', applies_to = 'incident_report', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'policydeviation';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'policydeviation', 'policydeviation', 'incident_report', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'unsafeconditions')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'unsafeconditions', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'unsafeconditions';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'unsafeconditions', 'unsafeconditions', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'lifesupport')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'lifesupport', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'lifesupport';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'lifesupport', 'lifesupport', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'environmentaltype')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'environmentaltype', applies_to = 'incident_report', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'environmentaltype';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'environmentaltype', 'environmentaltype', 'incident_report', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'incidenttype')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'incidenttype', applies_to = 'incident_report', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'incidenttype';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'incidenttype', 'incidenttype', 'incident_report', 1, GETUTCDATE(), GETUTCDATE());
    END

    IF EXISTS (SELECT 1 FROM safety.ref_reference_type WHERE code = 'rootcause')
    BEGIN
        UPDATE safety.ref_reference_type
        SET name = 'rootcause', applies_to = 'investigation', is_active = 1, updated_at = GETUTCDATE()
        WHERE code = 'rootcause';
    END
    ELSE
    BEGIN
        INSERT INTO safety.ref_reference_type (id, code, name, applies_to, is_active, created_at, updated_at)
        VALUES (NEWID(), 'rootcause', 'rootcause', 'investigation', 1, GETUTCDATE(), GETUTCDATE());
    END

    -- 2) Insert attachment rows
    -- safety.ref_doc_type
    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Citation'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Citation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Daily Pre-Shift/Toolbox'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Daily Pre-Shift/Toolbox',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Equipment Inspection'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Equipment Inspection',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'JHA'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'JHA',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Photograph'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Photograph',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Plan Permit'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Plan Permit',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Policy Report'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Policy Report',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Project Inspection'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Project Inspection',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Statement of all Involved'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Statement of all Involved',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Strongcard/FLHA'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Strongcard/FLHA',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Third Party Data'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Third Party Data',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_doc_type t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'documenttype' AND t.name = N'Training Records'
    )
    BEGIN
        INSERT INTO safety.ref_doc_type (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'documenttype'),
            'documenttype',
            N'Training Records',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    -- 3) Insert incident rows
    -- safety.ref_incident_report_reference
    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurydetail' AND t.name = N'Alleged'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurydetail'),
            'injurydetail',
            N'Alleged',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'motorvehicleaccident' AND t.name = N'Animal Strike'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'motorvehicleaccident'),
            'motorvehicleaccident',
            N'Animal Strike',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'equipmentdamage' AND t.name = N'Backing Up'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage'),
            'equipmentdamage',
            N'Backing Up',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Caught Between/Under'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Caught Between/Under',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Caught In'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Caught In',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Caught On'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Caught On',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'policydeviation' AND t.name = N'Client Policy'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'policydeviation'),
            'policydeviation',
            N'Client Policy',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Contact With'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Contact With',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'environmentaltype' AND t.name = N'Emission'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'environmentaltype'),
            'environmentaltype',
            N'Emission',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Environmental'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Environmental',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Equipment Damage'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Equipment Damage',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'equipmentdamage' AND t.name = N'Explosion'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage'),
            'equipmentdamage',
            N'Explosion',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Exposure'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Exposure',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Fall Next Level'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Fall Next Level',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Fall Same Level'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Fall Same Level',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'equipmentdamage' AND t.name = N'Fire'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage'),
            'equipmentdamage',
            N'Fire',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurydetail' AND t.name = N'First-Aid'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurydetail'),
            'injurydetail',
            N'First-Aid',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Injury'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Injury',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Life Support'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Life Support',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'policydeviation' AND t.name = N'Life-Saving Rule Violation'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'policydeviation'),
            'policydeviation',
            N'Life-Saving Rule Violation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurydetail' AND t.name = N'Lost Time'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurydetail'),
            'injurydetail',
            N'Lost Time',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'equipmentdamage' AND t.name = N'Malfunction'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage'),
            'equipmentdamage',
            N'Malfunction',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurydetail' AND t.name = N'Medical Aid'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurydetail'),
            'injurydetail',
            N'Medical Aid',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurydetail' AND t.name = N'Modified Work'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurydetail'),
            'injurydetail',
            N'Modified Work',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Motor Vehicle Accident'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Motor Vehicle Accident',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Overstress Ergonomics'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Overstress Ergonomics',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Policy Deviation'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Policy Deviation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'environmentaltype' AND t.name = N'Release'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'environmentaltype'),
            'environmentaltype',
            N'Release',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurydetail' AND t.name = N'Report Only'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurydetail'),
            'injurydetail',
            N'Report Only',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Security'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Security',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'policydeviation' AND t.name = N'SHC Policy'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'policydeviation'),
            'policydeviation',
            N'SHC Policy',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'motorvehicleaccident' AND t.name = N'Single Vehicle'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'motorvehicleaccident'),
            'motorvehicleaccident',
            N'Single Vehicle',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Struck Against'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Struck Against',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'equipmentdamage' AND t.name = N'Struck By'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage'),
            'equipmentdamage',
            N'Struck By',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'injurytype' AND t.name = N'Struck By'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'injurytype'),
            'injurytype',
            N'Struck By',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'motorvehicleaccident' AND t.name = N'Third Party'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'motorvehicleaccident'),
            'motorvehicleaccident',
            N'Third Party',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'incidenttype' AND t.name = N'Transportation Compliance'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'incidenttype'),
            'incidenttype',
            N'Transportation Compliance',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'equipmentdamage' AND t.name = N'Unreported Damage'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage'),
            'equipmentdamage',
            N'Unreported Damage',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_incident_report_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'equipmentdamage' AND t.name = N'Windshield'
    )
    BEGIN
        INSERT INTO safety.ref_incident_report_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'equipmentdamage'),
            'equipmentdamage',
            N'Windshield',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    -- 4) Insert investigation rows
    -- safety.ref_investigation_reference
    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'security' AND t.name = N'Arson'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'security'),
            'security',
            N'Arson',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Attitude'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Attitude',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'security' AND t.name = N'Break In'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'security'),
            'security',
            N'Break In',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Bulletin Reviews'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Bulletin Reviews',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Bypassing Safety Controls'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Bypassing Safety Controls',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'transportcompliance' AND t.name = N'Citations'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'transportcompliance'),
            'transportcompliance',
            N'Citations',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Communication/Reinforcement'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Communication/Reinforcement',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Condition Change'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Condition Change',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Congestion'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Congestion',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'CSE Violation'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'CSE Violation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'DVIR/Pre-Trip'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'DVIR/Pre-Trip',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Egress Cylinder'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Egress Cylinder',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Egress Regulator'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Egress Regulator',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Employee Engagements'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Employee Engagements',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Energy Isolation'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Energy Isolation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Equipment Inspections'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Equipment Inspections',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Failure to Identify or Control a Hazard'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Failure to Identify or Control a Hazard',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Failure to Inspect'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Failure to Inspect',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Failure to Secure Equipment'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Failure to Secure Equipment',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Fall Protection Violation'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Fall Protection Violation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Fire Hazard'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Fire Hazard',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Gas Detection'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Gas Detection',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Harness'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Harness',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Helmet Damage'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Helmet Damage',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Helmet Malfunction'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Helmet Malfunction',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Horseplay'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Horseplay',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Improper Storage'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Improper Storage',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Improper use of tools/equipment'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Improper use of tools/equipment',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Inadequate Culture Building'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Inadequate Culture Building',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Inadequate Culture Building'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Inadequate Culture Building',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Inadequate Guard'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Inadequate Guard',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Inadequate Job Planning'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Inadequate Job Planning',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Inadequate Job Planning'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Inadequate Job Planning',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Inadequate Motivation'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Inadequate Motivation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Inadequate Motivation'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Inadequate Motivation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Inadequate Policies'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Inadequate Policies',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Inadequate Policies'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Inadequate Policies',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Inadequate Procedure'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Inadequate Procedure',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Inadequate Training'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Inadequate Training',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Inadequate Training'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Inadequate Training',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Inadequate Training'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Inadequate Training',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Inadequate Warning'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Inadequate Warning',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Inattentive to Hazards'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Inattentive to Hazards',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Line of Fire Positioning'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Line of Fire Positioning',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Master JHA'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Master JHA',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Missing Policy'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Missing Policy',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Missing Training'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Missing Training',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Missing Training'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Missing Training',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Module'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Module',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Not Driving to Road Conditions'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Not Driving to Road Conditions',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Not Fit for Duty'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Not Fit for Duty',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Overconfident'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Overconfident',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Pigtail'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Pigtail',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Policies'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Policies',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Poor Brothers'' & Sisters'' Keeper'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Poor Brothers'' & Sisters'' Keeper',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Poor Brothers'' & Sisters'' Keeper Attitude'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Poor Brothers'' & Sisters'' Keeper Attitude',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Poor Communication'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Poor Communication',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Poor Communication'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Poor Communication',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Poor Housekeeping'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Poor Housekeeping',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Poor Lighting'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Poor Lighting',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Poor Ventilation'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Poor Ventilation',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'transportcompliance' AND t.name = N'Public Driving Complaint'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'transportcompliance'),
            'transportcompliance',
            N'Public Driving Complaint',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Road Conditions'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Road Conditions',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'transportcompliance' AND t.name = N'Roadside Inspection'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'transportcompliance'),
            'transportcompliance',
            N'Roadside Inspection',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'rootcause' AND t.name = N'Rogue Employee'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'rootcause'),
            'rootcause',
            N'Rogue Employee',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'personnelfactors' AND t.name = N'Rogue EmployeeRoot'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'personnelfactors'),
            'personnelfactors',
            N'Rogue EmployeeRoot',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Site Inspection'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Site Inspection',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'SOP'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'SOP',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Standard/COP'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Standard/COP',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'jobfactors' AND t.name = N'Strong Card'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'jobfactors'),
            'jobfactors',
            N'Strong Card',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'security' AND t.name = N'Theft'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'security'),
            'security',
            N'Theft',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Trailer/Cube'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Trailer/Cube',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Tripping Hazards'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Tripping Hazards',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Trying to Gain or Save Time'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Trying to Gain or Save Time',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'lifesupport' AND t.name = N'Umbilical'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'lifesupport'),
            'lifesupport',
            N'Umbilical',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Using Uninspected Equipment'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Using Uninspected Equipment',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'security' AND t.name = N'Vandelism'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'security'),
            'security',
            N'Vandelism',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeconditions' AND t.name = N'Weather'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeconditions'),
            'unsafeconditions',
            N'Weather',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Working without Authority / Permit /Incorrect Permit'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Working without Authority / Permit /Incorrect Permit',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    IF NOT EXISTS (
        SELECT 1 FROM safety.ref_investigation_reference t
        JOIN safety.ref_reference_type rt ON rt.id = t.reference_type_id
        WHERE rt.code = 'unsafeacts' AND t.name = N'Workplace ViolenceSystemic'
    )
    BEGIN
        INSERT INTO safety.ref_investigation_reference (id, reference_type_id, code, name, is_active, created_at, updated_at)
        VALUES (
            NEWID(),
            (SELECT id FROM safety.ref_reference_type WHERE code = 'unsafeacts'),
            'unsafeacts',
            N'Workplace ViolenceSystemic',
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH
GO

-- Verification
SELECT COUNT(*) AS ref_reference_type_count FROM safety.ref_reference_type;
SELECT COUNT(*) AS ref_doc_type_count FROM safety.ref_doc_type;
SELECT COUNT(*) AS ref_incident_report_reference_count FROM safety.ref_incident_report_reference;
SELECT COUNT(*) AS ref_investigation_reference_count FROM safety.ref_investigation_reference;
GO
