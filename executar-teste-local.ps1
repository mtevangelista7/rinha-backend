# Use este script para executar testes locais

$RESULTS_WORKSPACE = "$(Get-Location)\load-test\user-files\results"
$GATLING_BIN_DIR = "$env:GATLING_HOME\bin"
$GATLING_WORKSPACE = "$(Get-Location)\load-test\user-files"

function Run-Gatling {

    try {
        & "$GATLING_BIN_DIR\gatling.bat" -rm local -s RinhaBackendCrebitosSimulation `
            -rd "Rinha de Backend - 2024/Q1: Crébito" `
            -rf $RESULTS_WORKSPACE `
            -sf "$GATLING_WORKSPACE/simulations"
    }
    catch {
        Write-Output "Erro ao executar o teste de carga"
        Write-Output $_.Exception.Message
        exit 1
    }
}

function Start-Test {
    for ($i = 1; $i -le 20; $i++) {
        Write-Output $i
        try {
            # 2 requests to wake the 2 API instances up :)
            Invoke-RestMethod -Uri "http://localhost:9999/clientes/1/extrato" -ErrorAction Stop
            Write-Host "teste api aaa"
            Invoke-RestMethod -Uri "http://localhost:9999/clientes/1/extrato" -ErrorAction Stop
            Write-Host "teste api bbb"
            Run-Gatling
            break
        } catch {
            Write-Output $_.Exception.Message
            Start-Sleep -Seconds 2
        }
    }
}

Start-Test
