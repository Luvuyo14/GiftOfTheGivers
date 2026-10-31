# Database migrations (Azure SQL Database)

The schema of the Gift of the Givers database is managed as code and deployed by the Azure Pipeline.

## Structure
| Path | Purpose |
|---|---|
| `migrations/NNN_description.sql` | One forward migration per schema change, applied in number order. |
| `rollback/NNN_rollback.sql` | The matching undo script for each migration. |
| `deploy-database.ps1` | Applies pending migrations and records them in `dbo.SchemaVersions`. |

## How deployment works
1. The pipeline's **Deploy database schema** job runs `deploy-database.ps1` with the server, database and secret SQL login.
2. The script creates `dbo.SchemaVersions` if it does not exist and reads which versions are already applied.
3. Each pending migration runs inside a transaction. If a statement fails, that migration is rolled back and the job fails.
4. Applied versions are stored with a timestamp and who applied them, so running the pipeline again changes nothing.

## Adding a schema change
1. Create `migrations/00N_short_description.sql` using the next number. Make it safe to re-run (`IF OBJECT_ID(...) IS NULL`, `IF NOT EXISTS(...)`).
2. Create `rollback/00N_rollback.sql` that undoes it and deletes its row from `dbo.SchemaVersions`.
3. Commit and push to `main`. The pipeline applies it automatically after the tests pass.

## Rolling back a change
- **A schema change that has no data worth keeping:** run the matching `rollback/NNN_rollback.sql` in the Azure portal Query editor (or with any SQL client) as the server admin. It undoes the change and removes the version record. The next pipeline run will re-apply the migration if its file is still in `migrations/`, so delete or fix that file first.
- **Data was lost or damaged:** use Azure SQL point-in-time restore (Azure portal -> SQL database -> Restore). The Basic tier keeps backups for 7 days. The restore creates a new database, so check the data there before switching the application over.

## Notes and limits
- The deployment uses the SQL server admin login stored as secret pipeline variables. A production system should use a dedicated deployment login with only the permissions it needs.
- The database is on a small tier (Basic), so migrations are kept lightweight.
