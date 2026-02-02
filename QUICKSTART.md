# 🚀 Quick Start Guide

## Inicio Rápido (5 minutos)

### 1. Levantar Servicios

```bash
docker-compose up -d
```

Espera 30-60 segundos para que Keycloak termine de inicializar.

### 2. Verificar Servicios

```bash
# Keycloak
curl http://localhost:8082

# Auth Gateway
curl http://localhost:5003/api/health

# Orders API
curl http://localhost:5004/health

# Admin API
curl http://localhost:5005/health
```

### 3. Configurar Frontend

```bash
cd frontend/angular-spa
npm install
npm start
```

### 4. Probar el Sistema

1. Abre `http://localhost:4200`
2. Click en "Login"
3. Usa las credenciales:
   - **Admin:** `admin` / `admin123`
   - **User:** `user` / `user123`
   - **Support:** `support` / `support123`

### 5. Probar Endpoints

#### Desde el navegador (Angular SPA)
- Navega a las diferentes secciones: Orders, Admin, Reports

#### Desde curl (necesitas un token)

```bash
# 1. Obtén un token desde Keycloak o desde la consola del navegador
# 2. Usa el token en las requests

curl -H "Authorization: Bearer <tu-token>" http://localhost:5003/api/orders
```

## Usuarios de Prueba

| Usuario | Password | Rol | Acceso |
|---------|----------|-----|--------|
| admin | admin123 | Admin | Todo (con restricciones ABAC) |
| user | user123 | User | Solo GET /api/orders |
| support | support123 | Support | Solo GET /api/orders |

## Endpoints Principales

- `GET http://localhost:5003/api/orders` - Requiere: User + country=CL
- `POST http://localhost:5003/api/orders` - Requiere: Admin
- `GET http://localhost:5003/api/admin` - Requiere: Admin + IT + CL
- `GET http://localhost:5003/api/admin/reports` - Requiere: Admin + IT + Working Hours (12:00-22:00 UTC)

## Troubleshooting

### Keycloak no responde
```bash
docker-compose logs keycloak
```

### Reiniciar servicios
```bash
docker-compose restart
```

### Detener todo
```bash
docker-compose down
```

### Ver logs
```bash
docker-compose logs -f
```

## Próximos Pasos

Lee el [README.md](README.md) completo para:
- Entender la arquitectura
- Aprender sobre OAuth2/OIDC
- Entender RBAC vs ABAC
- Ver ejemplos de JWT
- Configuración avanzada
