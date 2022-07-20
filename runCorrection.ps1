param(
    $FileName = 'dummy.csv'
)

$url ="http://localhost:7071/api/StartCorrectionOrchestrator/$FileName"
$props = "instanceId", "runtimeStatus", "customStatus" , "output"

# Start the correction orchestrator
$r = Invoke-RestMethod $url

if (-not $r) {
    Write-Host "Failed to start correction orchestrator"
    break
}

# # Cancel
# Invoke-RestMethod $r.terminatePostUri.replace("{text}","Operation cancelled by user") -Method post

# # Check the status of the correction orchestrator
# $status = Invoke-RestMethod $r.statusQueryGetUri
# $status | Select-Object -Property $props


# # Start the correction orchestrator
# $r = Invoke-RestMethod $url


# Check the status of the correction orchestrator
$status = Invoke-RestMethod $r.statusQueryGetUri
$status | Select-Object -Property $props

# Approve the correction for processing
$requestParams = @{
    Uri = $r.sendEventPostUri.replace("{eventName}",'EventApproveCorrection')
    Method = [Microsoft.PowerShell.Commands.WebRequestMethod]::POST
    Body = @{Approved=$true} | ConvertTo-Json
    Headers = @{'content-type'='application/json'}
}
Invoke-RestMethod @requestParams

# Check the status of the correction orchestrator
do {
    $status = Invoke-RestMethod $r.statusQueryGetUri
    Start-Sleep -Milliseconds 500
} until ($status.runtimeStatus -ne 'Running')
$status | Select-Object -Property $props