# 🔐 Auth Gateway - OIDC/OAuth2 + RBAC/ABAC Enterprise Demo

Un proyecto enterprise-grade que implementa un sistema completo de autenticación y autorización usando OIDC/OAuth2, RBAC (Role-Based Access Control) y ABAC (Attribute-Based Access Control).

## 📋 Tabla de Contenidos

- [Arquitectura](#-arquitectura)
- [Stack Tecnológico](#-stack-tecnológico)
- [OAuth2 vs OIDC](#oauth2-vs-oidc)
- [Authorization Code Flow + PKCE](#authorization-code-flow--pkce)
- [RBAC vs ABAC](#rbac-vs-abac)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Instalación y Configuración](#-instalación-y-configuración)
- [Endpoints y Casos de Uso](#-endpoints-y-casos-de-uso)
- [Usuarios de Ejemplo](#-usuarios-de-ejemplo)
- [Ejemplos de JWT](#-ejemplos-de-jwt)
- [Testing](#-testing)

## 🏗 Arquitectura

```
┌─────────────────┐
│  Angular SPA    │
│  (Frontend)     │
└────────┬────────┘
         │
         │ OAuth2 Authorization Code + PKCE
         │
         ▼
┌─────────────────┐
│   Keycloak      │
│  (OIDC Provider)│
└────────┬────────┘
         │
         │ JWT Bearer Token
         │
         ▼
┌─────────────────┐
│  Auth Gateway   │
│  / BFF (.NET 8) │
│  + YARP Proxy   │
└────────┬────────┘
         │
         │ Reverse Proxy + Policy Enforcement
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌─────────┐ ┌─────────┐
│ Orders  │ │  Admin  │
│   API   │ │   API   │
└─────────┘ └─────────┘
```

### Componentes Principales

1. **Angular SPA**: Aplicación frontend que maneja el flujo OAuth2 con PKCE
2. **Keycloak**: Servidor de identidad que actúa como OIDC Provider
3. **Auth Gateway**: Backend for Frontend (BFF) que valida JWT y aplica políticas de autorización
4. **Orders API**: Microservicio protegido para gestión de órdenes
5. **Admin API**: Microservicio protegido para funciones administrativas

## 🛠 Stack Tecnológico

### Backend
- **.NET 8**: Framework principal
- **ASP.NET Core Web API**: Para los microservicios
- **JWT Bearer Authentication**: Validación de tokens
- **Authorization Policies**: Implementación de RBAC/ABAC
- **YARP (Yet Another Reverse Proxy)**: Proxy reverso para enrutamiento

### Frontend
- **Angular 17**: Framework frontend
- **angular-oauth2-oidc**: Librería para OAuth2/OIDC
- **Standalone Components**: Arquitectura moderna de Angular

### Infraestructura
- **Docker**: Containerización
- **Docker Compose**: Orquestación de servicios
- **Keycloak 24.0**: Servidor de identidad
- **PostgreSQL**: Base de datos para Keycloak

## OAuth2 vs OIDC

### OAuth2 (Open Authorization 2.0)
OAuth2 es un **framework de autorización** que permite a aplicaciones obtener acceso limitado a recursos de un usuario.

**Conceptos clave:**
- **Authorization Server**: Keycloak en nuestro caso
- **Resource Server**: Nuestros APIs (Orders, Admin)
- **Client**: Angular SPA
- **Access Token**: Token que permite acceder a recursos protegidos

**Flujos principales:**
- Authorization Code Flow (usado en este proyecto)
- Client Credentials Flow
- Implicit Flow (deprecated)
- Resource Owner Password Credentials (no recomendado)

### OIDC (OpenID Connect)
OIDC es una **capa de identidad** construida sobre OAuth2 que proporciona autenticación.

**Diferencias clave:**
- OAuth2 = **Autorización** (¿Qué puede hacer?)
- OIDC = **Autenticación** (¿Quién es?)

**Características de OIDC:**
- **ID Token**: Token JWT que contiene información de identidad del usuario
- **UserInfo Endpoint**: Endpoint para obtener información del usuario
- **Standard Claims**: Campos estándar (sub, name, email, etc.)

**En este proyecto:**
- Usamos OIDC para autenticación (saber quién es el usuario)
- Usamos OAuth2 para autorización (qué puede hacer el usuario)
- El ID Token contiene roles y claims personalizados

## Authorization Code Flow + PKCE

### Flujo Completo

```
1. Usuario → Angular SPA: "Quiero iniciar sesión"
2. Angular SPA → Keycloak: Redirect con code_challenge
3. Keycloak → Usuario: Login form
4. Usuario → Keycloak: Credenciales
5. Keycloak → Angular SPA: Authorization Code (redirect)
6. Angular SPA → Keycloak: Code + code_verifier
7. Keycloak → Angular SPA: Access Token + ID Token
8. Angular SPA → Auth Gateway: Request con Bearer Token
9. Auth Gateway → Keycloak: Valida token (JWKS)
10. Auth Gateway → Orders/Admin API: Request con headers propagados
```

### PKCE (Proof Key for Code Exchange)

**¿Por qué PKCE?**
- Protege contra ataques de interceptación de código
- Esencial para clientes públicos (SPAs)
- Recomendado por OAuth2.1

**Cómo funciona:**
1. **code_verifier**: String aleatorio generado por el cliente
2. **code_challenge**: SHA256(code_verifier) - enviado en el paso 1
3. **code_verifier**: Enviado en el paso 6 para verificación

**En nuestro código:**
```typescript
// auth.config.ts
useCodeChallenge: true,        // Habilita PKCE
codeChallengeMethod: 'S256'    // Usa SHA256
```

## RBAC vs ABAC

### RBAC (Role-Based Access Control)

**Definición:** Control de acceso basado en **roles** asignados a usuarios.

**En este proyecto:**
- **Roles:** User, Admin, Support
- **Implementación:** `[Authorize(Roles = "Admin")]`
- **Ejemplo:** Solo usuarios con rol "Admin" pueden crear órdenes

**Ventajas:**
- Simple de entender e implementar
- Fácil de auditar
- Escalable para organizaciones grandes

**Limitaciones:**
- No considera contexto (hora, ubicación, etc.)
- Puede ser demasiado permisivo o restrictivo

### ABAC (Attribute-Based Access Control)

**Definición:** Control de acceso basado en **atributos** del usuario, recurso, acción y entorno.

**En este proyecto:**
- **Atributos del usuario:** country, department, tenant, workingHours
- **Atributos del entorno:** hora actual (para working hours)
- **Implementación:** `policy.RequireClaim("country", "CL")`

**Ejemplos de políticas ABAC:**
```csharp
// Solo usuarios de Chile
policy.RequireClaim("country", "CL");

// Solo departamento IT
policy.RequireClaim("department", "IT");

// Horario laboral (12:00 - 22:00 UTC)
policy.RequireAssertion(ctx => {
    var hour = DateTime.UtcNow.Hour;
    return hour >= 12 && hour <= 22;
});
```

**Ventajas:**
- Muy flexible y granular
- Considera contexto dinámico
- Permite políticas complejas

**Limitaciones:**
- Más complejo de implementar
- Puede ser difícil de auditar
- Requiere más procesamiento

### RBAC + ABAC Combinados

**En este proyecto combinamos ambos:**

```csharp
// Admin + IT + Chile
options.AddPolicy("AdminChileIT", policy =>
{
    policy.RequireRole("Admin");              // RBAC
    policy.RequireClaim("country", "CL");      // ABAC
    policy.RequireClaim("department", "IT");  // ABAC
});

// Admin + IT + Horario laboral
options.AddPolicy("AdminITWorkingHours", policy =>
{
    policy.RequireRole("Admin");              // RBAC
    policy.RequireClaim("department", "IT");  // ABAC
    policy.RequireAssertion(ctx =>            // ABAC dinámico
    {
        var hour = DateTime.UtcNow.Hour;
        return hour >= 12 && hour <= 22;
    });
});
```

## 📁 Estructura del Proyecto

```
auth-gateway/
│
├── docker-compose.yml          # Orquestación de servicios
│
├── keycloak/
│   └── realm-export.json       # Configuración del realm de Keycloak
│
├── src/
│   ├── AuthGateway/            # Gateway / BFF
│   │   ├── Program.cs          # Configuración principal
│   │   ├── ReverseProxy/       # Configuración YARP
│   │   │   ├── ReverseProxyConfigProvider.cs
│   │   │   ├── UserContextMiddleware.cs
│   │   │   └── PathRewriteMiddleware.cs
│   │   └── Controllers/
│   │       └── HealthController.cs
│   │
│   ├── OrdersApi/              # Microservicio de órdenes
│   │   ├── Program.cs
│   │   └── Controllers/
│   │       ├── OrdersController.cs
│   │       └── HealthController.cs
│   │
│   └── AdminApi/               # Microservicio administrativo
│       ├── Program.cs
│       └── Controllers/
│           ├── AdminController.cs
│           ├── ReportsController.cs
│           └── HealthController.cs
│
├── frontend/
│   └── angular-spa/            # Aplicación Angular
│       ├── src/
│       │   ├── app/
│       │   │   ├── app.component.ts
│       │   │   ├── auth.config.ts
│       │   │   ├── home/
│       │   │   ├── orders/
│       │   │   ├── admin/
│       │   │   └── reports/
│       │   └── main.ts
│       └── package.json
│
└── README.md                    # Esta documentación
```

## 🚀 Instalación y Configuración

### Prerrequisitos

- Docker Desktop (Windows/Mac) o Docker + Docker Compose (Linux)
- .NET 8 SDK (para desarrollo local)
- Node.js 18+ y npm (para desarrollo del frontend)

### Paso 1: Clonar y Preparar

```bash
git clone <repo-url>
cd oidc-auth-gateway
```

### Paso 2: Levantar Servicios con Docker

```bash
docker-compose up -d
```

Esto levantará:
- Keycloak en `http://localhost:8082`
- Auth Gateway en `http://localhost:5003`
- Orders API en `http://localhost:5004`
- Admin API en `http://localhost:5005`
- PostgreSQL (interno)

**Espera 30-60 segundos** para que Keycloak termine de inicializar.

### Paso 3: Verificar Keycloak

1. Abre `http://localhost:8082`
2. Login con:
   - Usuario: `admin`
   - Contraseña: `admin`
3. Ve a "Realms" → "auth-gateway-realm"
4. Verifica que los clientes y usuarios estén configurados

### Paso 4: Configurar Frontend (Desarrollo Local)

```bash
cd frontend/angular-spa
npm install
npm start
```

La aplicación estará disponible en `http://localhost:4200`

### Paso 5: Probar el Sistema

1. Abre `http://localhost:4200`
2. Haz clic en "Login"
3. Usa las credenciales de ejemplo (ver sección de usuarios)
4. Navega por las diferentes secciones

## 🧪 Endpoints y Casos de Uso

### Endpoints del Auth Gateway

Todos los endpoints pasan por el Auth Gateway (`http://localhost:5003`):

| Endpoint | Método | Política | Descripción |
|----------|--------|----------|-------------|
| `/api/orders` | GET | `UserChile` | Listar órdenes (User + country=CL) |
| `/api/orders` | POST | `RequireAdmin` | Crear orden (Admin) |
| `/api/orders/{id}` | PUT/DELETE | `RequireAdmin` | Modificar/Eliminar orden (Admin) |
| `/api/admin` | GET | `AdminChileIT` | Info admin (Admin + IT + CL) |
| `/api/admin/users` | GET | `AdminChileIT` | Listar usuarios (Admin + IT + CL) |
| `/api/admin/reports` | GET | `AdminITWorkingHours` | Reportes (Admin + IT + 12:00-22:00 UTC) |

### Políticas Implementadas

#### RBAC Puro
```csharp
[Authorize(Roles = "Admin")]  // Solo Admin
```

#### ABAC Puro
```csharp
policy.RequireClaim("country", "CL")  // Solo Chile
```

#### RBAC + ABAC
```csharp
// UserChile: User/Admin/Support + country=CL
policy.RequireRole("User", "Admin", "Support");
policy.RequireClaim("country", "CL");

// AdminChileIT: Admin + IT + CL
policy.RequireRole("Admin");
policy.RequireClaim("country", "CL");
policy.RequireClaim("department", "IT");

// AdminITWorkingHours: Admin + IT + Horario
policy.RequireRole("Admin");
policy.RequireClaim("department", "IT");
policy.RequireAssertion(ctx => {
    var hour = DateTime.UtcNow.Hour;
    return hour >= 12 && hour <= 22;
});
```

## 👥 Usuarios de Ejemplo

### Usuario Admin
- **Username:** `admin`
- **Password:** `admin123`
- **Rol:** Admin
- **Claims:**
  - country: CL
  - department: IT
  - tenant: tenant-001
  - workingHours: 09:00-18:00

**Puede acceder a:**
- ✅ GET /api/orders (UserChile)
- ✅ POST /api/orders (RequireAdmin)
- ✅ GET /api/admin (AdminChileIT)
- ✅ GET /api/admin/reports (AdminITWorkingHours - solo 12:00-22:00 UTC)

### Usuario Regular
- **Username:** `user`
- **Password:** `user123`
- **Rol:** User
- **Claims:**
  - country: CL
  - department: Sales
  - tenant: tenant-002

**Puede acceder a:**
- ✅ GET /api/orders (UserChile)
- ❌ POST /api/orders (RequireAdmin)
- ❌ GET /api/admin (AdminChileIT - no es Admin)
- ❌ GET /api/admin/reports (AdminITWorkingHours)

### Usuario Support
- **Username:** `support`
- **Password:** `support123`
- **Rol:** Support
- **Claims:**
  - country: CL
  - department: IT
  - tenant: tenant-001

**Puede acceder a:**
- ✅ GET /api/orders (UserChile)
- ❌ POST /api/orders (RequireAdmin)
- ❌ GET /api/admin (AdminChileIT - no es Admin)

## 🔑 Ejemplos de JWT

### Estructura del Token

Un JWT típico emitido por Keycloak contiene:

```json
{
  "header": {
    "alg": "RS256",
    "typ": "JWT",
    "kid": "..."
  },
  "payload": {
    "sub": "12345678-1234-1234-1234-123456789012",
    "iss": "http://localhost:8082/realms/auth-gateway-realm",
    "aud": "auth-gateway-api",
    "exp": 1703123456,
    "iat": 1703120056,
    "auth_time": 1703120056,
    "session_state": "...",
    "acr": "1",
    "preferred_username": "admin",
    "email": "admin@example.com",
    "email_verified": true,
    "name": "Admin User",
    "given_name": "Admin",
    "family_name": "User",
    "realm_access": {
      "roles": ["Admin", "offline_access", "uma_authorization"]
    },
    "resource_access": {
      "auth-gateway-api": {
        "roles": []
      }
    },
    "roles": ["Admin"],
    "country": "CL",
    "department": "IT",
    "tenant": "tenant-001",
    "workingHours": "09:00-18:00"
  },
  "signature": "..."
}
```

### Claims Importantes

- **sub**: Subject (ID único del usuario)
- **preferred_username**: Nombre de usuario
- **roles**: Roles del usuario (array)
- **realm_access.roles**: Roles a nivel de realm
- **country**: Atributo personalizado (ABAC)
- **department**: Atributo personalizado (ABAC)
- **tenant**: Atributo personalizado (ABAC)
- **workingHours**: Atributo personalizado (ABAC)

### Decodificar JWT

Puedes decodificar un JWT en:
- https://jwt.io
- O usar la consola del navegador:
```javascript
const token = 'tu-token-aqui';
const payload = JSON.parse(atob(token.split('.')[1]));
console.log(payload);
```

## 🧪 Testing

### Testing Manual

1. **Probar GET /api/orders:**
   ```bash
   curl -H "Authorization: Bearer <token>" http://localhost:5003/api/orders
   ```

2. **Probar POST /api/orders:**
   ```bash
   curl -X POST \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     -d '{"customerName":"Test","product":"Product","amount":100}' \
     http://localhost:5003/api/orders
   ```

3. **Probar GET /api/admin:**
   ```bash
   curl -H "Authorization: Bearer <token>" http://localhost:5003/api/admin
   ```

4. **Probar GET /api/admin/reports:**
   ```bash
   curl -H "Authorization: Bearer <token>" http://localhost:5003/api/admin/reports
   ```
   ⚠️ Solo funciona entre 12:00-22:00 UTC

### Obtener Token desde Keycloak

```bash
# 1. Obtener authorization code (manual desde navegador)
# 2. Intercambiar code por token
curl -X POST \
  http://localhost:8082/realms/auth-gateway-realm/protocol/openid-connect/token \
  -d "grant_type=authorization_code" \
  -d "client_id=angular-spa" \
  -d "client_secret=angular-spa-secret" \
  -d "code=<authorization_code>" \
  -d "redirect_uri=http://localhost:4200" \
  -d "code_verifier=<code_verifier>"
```

### Testing desde Angular

La aplicación Angular incluye componentes para probar cada endpoint:
- **Home**: Muestra información del token
- **Orders**: Prueba GET /api/orders
- **Admin**: Prueba GET /api/admin
- **Reports**: Prueba GET /api/admin/reports

## 🔧 Configuración Avanzada

### Modificar Políticas

Edita `src/AuthGateway/Program.cs`:

```csharp
options.AddPolicy("MiPolitica", policy =>
{
    policy.RequireRole("Admin");
    policy.RequireClaim("custom-claim", "value");
    policy.RequireAssertion(ctx => {
        // Lógica personalizada
        return true;
    });
});
```

### Agregar Nuevos Claims

1. Edita `keycloak/realm-export.json`
2. Agrega protocol mappers para nuevos claims
3. Reinicia Keycloak: `docker-compose restart keycloak`

### Cambiar Horario Laboral

Edita `src/AuthGateway/Program.cs`:

```csharp
policy.RequireAssertion(ctx =>
{
    var hour = DateTime.UtcNow.Hour;
    // Cambiar rango aquí
    return hour >= 9 && hour <= 17; // 9 AM - 5 PM UTC
});
```

## 📚 Recursos Adicionales

- [OAuth2 Specification](https://oauth.net/2/)
- [OpenID Connect Specification](https://openid.net/connect/)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Angular OAuth2 OIDC](https://github.com/manfredsteyer/angular-oauth2-oidc)

## 🐛 Troubleshooting

### Keycloak no inicia
- Verifica que el puerto 8080 esté libre
- Revisa logs: `docker-compose logs keycloak`
- Espera más tiempo (puede tardar hasta 2 minutos)

### Error "Invalid token"
- Verifica que el token no haya expirado
- Confirma que el issuer sea correcto
- Revisa que el audience coincida

### Error 403 Forbidden
- Verifica los roles del usuario
- Confirma los claims requeridos
- Revisa la política de autorización

### CORS Errors
- Verifica `webOrigins` en Keycloak
- Confirma `redirectUris` en el cliente

## 📝 Notas de Producción

⚠️ **Este es un proyecto de demostración. Para producción:**

1. **HTTPS obligatorio** en todos los servicios
2. **Secrets management** (no hardcodear secrets)
3. **Rate limiting** en el gateway
4. **Logging y monitoreo** completo
5. **Token refresh** automático
6. **Validación de certificados** SSL
7. **Secrets rotation** periódico
8. **Audit logs** para seguridad

## 👨‍💻 Autor

Proyecto creado como demostración de arquitectura enterprise-grade para autenticación y autorización.

## 📄 Licencia

Este proyecto es de código abierto y está disponible para fines educativos y de demostración.

---

**¿Preguntas?** Abre un issue o consulta la documentación de cada componente.
