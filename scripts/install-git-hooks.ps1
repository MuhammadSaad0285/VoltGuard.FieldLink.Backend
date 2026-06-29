$ErrorActionPreference = "Stop"

git config core.hooksPath .githooks
Write-Host "Git hooks enabled. Commits and pushes will run the test suite first."
