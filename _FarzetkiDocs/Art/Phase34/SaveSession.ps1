param([ValidateSet('Backup','Restore')][string]$Mode, [string]$ManifestPath)
$ErrorActionPreference = 'Stop'
$phaseSaveRoot = 'C:\Users\Farzetki\AppData\LocalLow\TELEFERIC\SmasherRoyale'
if ($Mode -eq 'Backup') {
    $phaseSessionRoot = Join-Path 'C:\Users\Farzetki\.codex\local-backups' ('Phase34-Play-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    New-Item -ItemType Directory -Path $phaseSessionRoot -Force | Out-Null
    $phaseEntries = foreach ($phaseFile in Get-ChildItem -LiteralPath $phaseSaveRoot -Recurse -File -Filter '*.json') {
        $phaseRelative = $phaseFile.FullName.Substring($phaseSaveRoot.Length + 1)
        $phaseDestination = Join-Path $phaseSessionRoot $phaseRelative
        New-Item -ItemType Directory -Path (Split-Path -Parent $phaseDestination) -Force | Out-Null
        Copy-Item -LiteralPath $phaseFile.FullName -Destination $phaseDestination
        [ordered]@{relativePath=$phaseRelative;backup=$phaseDestination;sha256=(Get-FileHash -LiteralPath $phaseFile.FullName -Algorithm SHA256).Hash}
    }
    $phaseManifest = [ordered]@{capturedUtc=[DateTime]::UtcNow.ToString('o');saveRoot=$phaseSaveRoot;backupRoot=$phaseSessionRoot;files=@($phaseEntries)}
    $phaseManifest | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $ManifestPath -Encoding utf8
    [ordered]@{manifest=$ManifestPath;count=@($phaseEntries).Count;backupRoot=$phaseSessionRoot} | ConvertTo-Json
} else {
    $phaseManifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
    if ([IO.Path]::GetFullPath($phaseManifest.saveRoot).TrimEnd('\') -ne [IO.Path]::GetFullPath($phaseSaveRoot).TrimEnd('\')) { throw 'Save root mismatch' }
    foreach ($phaseEntry in $phaseManifest.files) {
        $phaseDestination = [IO.Path]::GetFullPath((Join-Path $phaseSaveRoot $phaseEntry.relativePath))
        if (!$phaseDestination.StartsWith($phaseSaveRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Restore target escapes save root' }
        Copy-Item -LiteralPath $phaseEntry.backup -Destination $phaseDestination
    }
    $phaseExtraFiles = @(Get-ChildItem -LiteralPath $phaseSaveRoot -Recurse -File -Filter '*.json' | Where-Object {$phaseManifest.files.relativePath -notcontains $_.FullName.Substring($phaseSaveRoot.Length+1)})
    foreach ($phaseExtra in $phaseExtraFiles) {
        $phaseTarget = [IO.Path]::GetFullPath($phaseExtra.FullName)
        if (!$phaseTarget.StartsWith($phaseSaveRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'New-file cleanup escapes save root' }
        Remove-Item -LiteralPath $phaseTarget
    }
    $phaseChecks = foreach ($phaseEntry in $phaseManifest.files) {
        $phaseHash = (Get-FileHash -LiteralPath (Join-Path $phaseSaveRoot $phaseEntry.relativePath) -Algorithm SHA256).Hash
        [ordered]@{file=$phaseEntry.relativePath;restored=($phaseHash -eq $phaseEntry.sha256);sha256=$phaseHash}
    }
    $phaseReport=[ordered]@{restoredUtc=[DateTime]::UtcNow.ToString('o');manifest=$ManifestPath;newFilesRemoved=$phaseExtraFiles.Count;allPassed=(@($phaseChecks|Where-Object{!$_.restored}).Count -eq 0);files=@($phaseChecks)}
    $phaseReport | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath ($ManifestPath.Replace('-backup.json','-restoration.json')) -Encoding utf8
    $phaseReport | ConvertTo-Json -Depth 7
    if (!$phaseReport.allPassed) { throw 'Save hash restore failed' }
}
