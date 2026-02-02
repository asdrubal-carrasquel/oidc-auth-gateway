# 🔧 Fix: Problema de Login en Consola de Administración de Keycloak

## Problema
Al intentar acceder a `http://localhost:8082/admin`, se redirige al login pero falla con credenciales incorrectas.

## Solución

### Importante: Dos Realms Diferentes

Keycloak tiene **dos realms diferentes** con usuarios diferentes:

1. **Realm `master`** (Administración de Keycloak)
   - Usuario: `admin`
   - Contraseña: `admin`
   - Se usa para administrar Keycloak mismo

2. **Realm `auth-gateway-realm`** (Nuestra aplicación)
   - Usuario: `admin` / Contraseña: `admin123`
   - Usuario: `user` / Contraseña: `user123`
   - Usuario: `support` / Contraseña: `support123`
   - Se usa para la aplicación

## Pasos para Acceder a la Consola de Administración

### Opción 1: Usar CLI de Keycloak (Recomendado)

```bash
# Acceder al contenedor
docker exec -it keycloak /bin/bash

# Usar la CLI de Keycloak
/opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user admin --password admin

# Verificar usuarios
/opt/keycloak/bin/kcadm.sh get users --realm auth-gateway-realm
```

### Opción 2: Acceder vía Web (Si el login funciona)

1. Ve a `http://localhost:8082/admin`
2. **IMPORTANTE:** Usa las credenciales del realm `master`:
   - Usuario: `admin`
   - Contraseña: `admin`
3. Una vez dentro, selecciona el realm `auth-gateway-realm` en el dropdown superior izquierdo

### Opción 3: Usar la API Directamente (Sin Consola Web)

Puedes gestionar todo mediante la API REST de Keycloak:

```powershell
# Obtener token de admin
$token = (Invoke-RestMethod -Uri "http://localhost:8082/realms/master/protocol/openid-connect/token" `
    -Method POST -ContentType "application/x-www-form-urlencoded" `
    -Body @{client_id="admin-cli"; username="admin"; password="admin"; grant_type="password"}).access_token

# Listar usuarios del realm auth-gateway-realm
Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/auth-gateway-realm/users" `
    -Headers @{Authorization="Bearer $token"}
```

## Verificar Credenciales del Realm Master

Si el login en la consola web no funciona, puedes verificar/resetear la contraseña del admin del realm master:

```powershell
# Verificar login
$test = Invoke-RestMethod -Uri "http://localhost:8082/realms/master/protocol/openid-connect/token" `
    -Method POST -ContentType "application/x-www-form-urlencoded" `
    -Body @{client_id="admin-cli"; username="admin"; password="admin"; grant_type="password"}

if($test.access_token) {
    Write-Host "✅ Credenciales correctas"
} else {
    Write-Host "❌ Credenciales incorrectas"
}
```

## Nota Importante

La consola de administración web de Keycloak puede tener problemas en modo desarrollo. Si no puedes acceder, usa la **API REST** o la **CLI** que son más confiables.

## URLs Útiles

- **Admin Console:** `http://localhost:8082/admin`
- **API Admin:** `http://localhost:8082/admin/realms/{realm}/...`
- **Token Endpoint:** `http://localhost:8082/realms/{realm}/protocol/openid-connect/token`
