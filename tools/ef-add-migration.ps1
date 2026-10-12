# Adds an EF Core migration to one UmbCheckout context for both SQL Server and SQLite, e.g.:
#   ./tools/ef-add-migration.ps1 -Project src/UmbCheckout.Core -Context UmbCheckoutDbContext -Name AddSomething -OutputDir Persistence/Migrations -Namespace UmbCheckout.Core.Persistence.Migrations
# Closed (obfuscated) projects: afterwards run python tools/write-obfuscation-exclusions.py <project>/Persistence/Migrations in the parent repo.
param(
    [Parameter(Mandatory)][string]$Project,     # e.g. UmbCheckout/src/UmbCheckout.Core
    [Parameter(Mandatory)][string]$Context,     # e.g. UmbCheckoutDbContext
    [Parameter(Mandatory)][string]$Name,        # migration name
    [Parameter(Mandatory)][string]$OutputDir,   # e.g. Persistence/Migrations (provider folder appended)
    [Parameter(Mandatory)][string]$Namespace,   # e.g. UmbCheckout.Core.Persistence.Migrations (provider appended)
    [string[]]$Providers = @('SqlServer', 'Sqlite'))
# Adds one migration per provider. EF finds an existing snapshot file by name and would overwrite the other provider's,
# so each provider's snapshot is renamed <Context><Provider>ModelSnapshot and kept in that provider's folder.
$ErrorActionPreference = 'Stop'
foreach ($provider in $Providers) {
    $env:UMBCHECKOUT_EF_PROVIDER = $provider
    $out = "$OutputDir/$provider"
    $ns = "$Namespace.$provider"
    dotnet ef migrations add $Name --project $Project --context $Context --output-dir $out --namespace $ns 2>&1 | Select-Object -Last 2
    if ($LASTEXITCODE -ne 0) { throw "dotnet ef failed for $provider" }
    $target = Join-Path $Project "$out/$Context${provider}ModelSnapshot.cs"
    $generated = Get-ChildItem -Path $Project -Recurse -Filter "${Context}ModelSnapshot.cs" | Select-Object -First 1
    if ($generated) {
        $text = (Get-Content $generated.FullName -Raw).Replace("class ${Context}ModelSnapshot", "class $Context${provider}ModelSnapshot")
        Set-Content -Path $target -Value $text -NoNewline
        Remove-Item $generated.FullName
        # remove the empty folders EF created from the namespace
        $dir = $generated.Directory
        while ($dir.FullName -ne (Resolve-Path $Project).Path -and -not (Get-ChildItem $dir.FullName -Force)) { $parent = $dir.Parent; Remove-Item $dir.FullName; $dir = $parent }
    }
    "snapshot -> $target"
}
