$base = "http://localhost:5005"

Write-Output "Fetching available questions..."
$questions = Invoke-RestMethod -Method Get -Uri "$base/api/v1/questions"
if (-not $questions -or $questions.Count -eq 0) { Write-Error "No questions available from API"; exit 1 }
$firstQuestion = $questions[0]

Write-Output "Creating empty session..."
$session = Invoke-RestMethod -Method Post -Uri "$base/api/v1/gamesessions" -Body (@{} | ConvertTo-Json) -ContentType "application/json"
Write-Output "Session created: $($session.Id)"

Write-Output "Fetching session..."
$session = Invoke-RestMethod -Method Get -Uri "$base/api/v1/gamesessions/1"
if (-not $session) { Write-Error "Failed to get session"; exit 1 }

Write-Output "Joining player..."
$player = @{ Name = "IntegrationTester"; Id = "itest-1" } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "$base/api/v1/gamesessions/1/join" -Body $player -ContentType "application/json"

Write-Output "Starting session..."
Invoke-RestMethod -Method Post -Uri "$base/api/v1/gamesessions/1/start" -Body (@{} | ConvertTo-Json) -ContentType "application/json"

Write-Output "Posting player state (no answers)..."
$psJson = '{"PlayerId":"itest-1","Name":"IntegrationTester","CurrentQuestionIndex":0,"LatestQuestionId":"","Answers":[] }'
Invoke-RestMethod -Method Post -Uri "$base/api/v1/gamesessions/1/playerstate" -Body $psJson -ContentType "application/json"

Write-Output "Getting current playerstates..."
$states = Invoke-RestMethod -Method Get -Uri "$base/api/v1/gamesessions/1/playerstates/current"
Write-Output "Player states count: $($states.Count)"

if ($states.Count -ge 1) { Write-Output "Integration test passed."; exit 0 } else { Write-Error "Integration test failed: no playerstates"; exit 2 }