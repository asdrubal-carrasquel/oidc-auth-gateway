# 🔧 Fix: Error 401 Unauthorized al hacer login

## Problema
El cliente `angular-spa` estaba configurado como **confidencial** (`publicClient: false`) cuando debería ser **público** (`publicClient: true`) porque es una SPA que no puede mantener secretos de forma segura.

## Solución Aplicada
Se actualizó `keycloak/realm-export.json` para configurar el cliente como público.

## ⚡ Solución Rápida (Recomendada)

### Opción A: Usar el Script Automático

```powershell
# Espera 30-60 segundos para que Keycloak termine de inicializar
# Luego ejecuta:
.\fix-keycloak-client.ps1
```

### Opción B: Configuración Manual en Keycloak (Más Simple)

1. **Abre Keycloak Admin Console:**
   - URL: `http://localhost:8082`
   - Usuario: `admin`
   - Contraseña: `admin`

2. **Selecciona el Realm:**
   - En el dropdown superior izquierdo, selecciona `auth-gateway-realm`

3. **Ve a Clients:**
   - En el menú lateral, haz clic en **Clients**
   - Busca y haz clic en **angular-spa**

4. **Actualiza la Configuración:**
   - En la pestaña **Settings**, busca **Access Type**
   - Cambia de `confidential` a `public`
   - Haz clic en **Save**

5. **Verifica los Flujos:**
   - Asegúrate de que estén habilitados:
     - ✅ **Standard Flow Enabled** (ON)
     - ✅ **Direct Access Grants Enabled** (ON)

6. **Guarda los cambios**

7. **Prueba el Login:**
   - Ve a `http://localhost:4200`
   - Haz clic en "Login"
   - Usa: `admin` / `admin123`

## Pasos Alternativos (Si lo anterior no funciona)

### Opción 1: Reiniciar Keycloak con reimportación (Recomendado)

```bash
# Detener y eliminar el contenedor de Keycloak
docker-compose stop keycloak
docker-compose rm -f keycloak

# Eliminar el volumen de Keycloak (esto eliminará el realm existente)
docker volume ls | grep keycloak
docker volume rm <nombre-del-volumen-keycloak>

# O simplemente eliminar todos los volúmenes no usados
docker volume prune -f

# Levantar Keycloak nuevamente (reimportará el realm)
docker-compose up -d keycloak
```

### Opción 2: Configurar manualmente en Keycloak Admin Console

1. Abre `http://localhost:8082`
2. Login con `admin` / `admin`
3. Ve a **Clients** → **angular-spa**
4. En la pestaña **Settings**:
   - Cambia **Access Type** a `public`
   - Guarda los cambios
5. Verifica que **Standard Flow Enabled** esté activado
6. Verifica que **Direct Access Grants Enabled** esté activado (para login directo)

### Opción 3: Usar la API de Keycloak

```bash
# Obtener token de admin
TOKEN=$(curl -X POST http://localhost:8082/realms/master/protocol/openid-connect/token \
  -d "client_id=admin-cli" \
  -d "username=admin" \
  -d "password=admin" \
  -d "grant_type=password" | jq -r '.access_token')

# Actualizar el cliente
curl -X PUT http://localhost:8082/admin/realms/auth-gateway-realm/clients/$(curl -s -H "Authorization: Bearer $TOKEN" http://localhost:8082/admin/realms/auth-gateway-realm/clients?clientId=angular-spa | jq -r '.[0].id') \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "publicClient": true,
    "clientAuthenticatorType": "client-secret",
    "standardFlowEnabled": true,
    "directAccessGrantsEnabled": true
  }'
```

## Verificación

Después de aplicar el fix:

1. Espera 30-60 segundos para que Keycloak termine de inicializar
2. Abre `http://localhost:4200`
3. Haz clic en "Login"
4. Deberías poder loguearte con:
   - `admin` / `admin123`
   - `user` / `user123`
   - `support` / `support123`

## Notas Importantes

- **Clientes Públicos**: No requieren client secret porque el código corre en el navegador
- **PKCE**: Se habilita automáticamente con `responseType: 'code'` en Angular
- **Direct Access Grants**: Permite login directo con usuario/contraseña (útil para desarrollo)
