# Contract lifecycle synchronization

No database schema changes are required. The Contract service marks signed contracts as expired when their return date passes. It then asks the Premise service to release any rows for that contract from the existing `rented_premise` table and set linked premises back to `Available`, preserving `Maintenance`.

The lifecycle worker runs every 15 seconds. Contract termination also attempts an immediate release; if the Premise service is unavailable, the worker retries ended contracts.

Configure both services with the same private key using environment variables or a secret manager:

```text
InternalApi__Key=<shared-secret>
```

For local development, Contract and Premise currently share a `UserSecretsId`. From the repository root in PowerShell, generate and save one random key; both services will read it automatically when run in the Development environment:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "InternalApi:Key" $key --project Backend\Module\Contract\Contract.csproj
```

Configure the Contract service with the Premise service's reachable base URL:

```text
ModuleUrls__Premise=https://<premise-service-host>
```

Locations do not have a persisted active/hidden field in the current schema. The location management action therefore permanently deletes a location, and the API refuses to delete one that is still referenced by a premise.
