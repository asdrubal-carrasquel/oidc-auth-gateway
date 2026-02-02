# 🔐 URLs de Keycloak

## Consola de Administración

**URL Principal:**
```
http://localhost:8082/admin
```

**Credenciales:**
- Usuario: `admin`
- Contraseña: `admin`

## Acceso Directo a Realms

### Master Realm (Administración)
```
http://localhost:8082/realms/master
```

### Auth Gateway Realm (Nuestro Realm)
```
http://localhost:8082/realms/auth-gateway-realm
```

## Endpoints Importantes

### Discovery Document (OIDC)
```
http://localhost:8082/realms/auth-gateway-realm/.well-known/openid-configuration
```

### Token Endpoint
```
http://localhost:8082/realms/auth-gateway-realm/protocol/openid-connect/token
```

### Authorization Endpoint
```
http://localhost:8082/realms/auth-gateway-realm/protocol/openid-connect/auth
```

## Flujo Normal

1. **Para administrar Keycloak:**
   - Ve a `http://localhost:8082/admin`
   - Login con `admin` / `admin`
   - Selecciona el realm `auth-gateway-realm` en el dropdown superior izquierdo

2. **Para probar el login desde la aplicación:**
   - Ve a `http://localhost:4200` (Angular SPA)
   - Haz clic en "Login"
   - Serás redirigido automáticamente a Keycloak para autenticarte

## Nota Importante

Cuando accedes directamente a `http://localhost:8082`, Keycloak te redirige al login porque no hay una página de inicio por defecto. Esto es **normal y esperado**.

Para acceder a la consola de administración, siempre usa:
```
http://localhost:8082/admin
```
