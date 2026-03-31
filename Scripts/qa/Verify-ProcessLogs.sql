/*
  Purpose:
  Validate that key process log records are written during live QA gates.

  Usage example (PowerShell):
    sqlcmd -S <server> -d <database> -U <user> -P <password> -i Scripts\qa\Verify-ProcessLogs.sql
*/

SELECT TOP (100)
    process_name,
    process_type,
    log_type,
    incident_report_id,
    message,
    related_object,
    run_id,
    logged_at
FROM safety.process_log
ORDER BY logged_at DESC;
