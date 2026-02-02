# Script para actualizar el cliente angular-spa a público en Keycloak

Write-Host "🔧 Actualizando cliente angular-spa en Keycloak..." -ForegroundColor Cyan

# Esperar a que Keycloak esté listo
Write-Host "⏳ Esperando a que Keycloak esté listo..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Obtener token de admin
Write-Host "🔑 Obteniendo token de admin..." -ForegroundColor Yellow
$tokenResponse = Invoke-RestMethod -Uri "http://localhost:8082/realms/master/protocol/openid-connect/token" `
    -Method POST `
    -ContentType "application/x-www-form-urlencoded" `
    -Body @{
        client_id = "admin-cli"
        username = "admin"
        password = "admin"
        grant_type = "password"
    }

$adminToken = $tokenResponse.access_token
Write-Host "✅ Token obtenido" -ForegroundColor Green

# Obtener el ID del cliente angular-spa
Write-Host "🔍 Buscando cliente angular-spa..." -ForegroundColor Yellow
$clientsResponse = Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/auth-gateway-realm/clients?clientId=angular-spa" `
    -Method GET `
    -Headers @{
        Authorization = "Bearer $adminToken"
    }

if ($clientsResponse.Count -eq 0) {
    Write-Host "❌ Cliente angular-spa no encontrado" -ForegroundColor Red
    exit 1
}

$clientId = $clientsResponse[0].id
Write-Host "✅ Cliente encontrado: $clientId" -ForegroundColor Green

# Obtener la configuración actual del cliente
Write-Host "📥 Obteniendo configuración actual..." -ForegroundColor Yellow
$currentConfig = Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/auth-gateway-realm/clients/$clientId" `
    -Method GET `
    -Headers @{
        Authorization = "Bearer $adminToken"
    }

# Actualizar el cliente a público
Write-Host "🔄 Actualizando cliente a público..." -ForegroundColor Yellow
$updateBody = @{
    publicClient = $true
    standardFlowEnabled = $true
    directAccessGrantsEnabled = $true
    redirectUris = @("http://localhost:4200/*", "http://localhost:4200")
    webOrigins = @("http://localhost:4200")
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/auth-gateway-realm/clients/$clientId" `
        -Method PUT `
        -Headers @{
            Authorization = "Bearer $adminToken"
            "Content-Type" = "application/json"
        } `
        -Body $updateBody | Out-Null
    
    Write-Host "✅ Cliente actualizado exitosamente!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Configuración aplicada:" -ForegroundColor Cyan
    Write-Host "   - Public Client: true" -ForegroundColor White
    Write-Host "   - Standard Flow Enabled: true" -ForegroundColor White
    Write-Host "   - Direct Access Grants Enabled: true" -ForegroundColor White
    Write-Host ""
    Write-Host "🎉 Ahora puedes intentar hacer login nuevamente!" -ForegroundColor Green
} catch {
    Write-Host "❌ Error al actualizar el cliente: $_" -ForegroundColor Red
    exit 1
}
