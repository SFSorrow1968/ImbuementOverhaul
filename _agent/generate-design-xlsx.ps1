$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$py = Join-Path $scriptDir "generate-design-xlsx.py"

if (!(Test-Path $py)) {
    throw "Missing generator script: $py"
}

python $py
