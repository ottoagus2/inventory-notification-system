# Sistema de Notificaciones de Inventario

Sistema de microservicios para gestión de inventario con notificaciones en tiempo real usando RabbitMQ.

## Arquitectura

```
┌─────────────────┐    RabbitMQ     ┌─────────────────────┐
│   InventoryAPI  │ ──────────────► │ NotificationService │
│   (Publisher)   │                 │    (Consumer)       │
└─────────────────┘                 └─────────────────────┘
         │                                       │
         ▼                                      ▼
  [SQLite: Products]                    [SQLite: Logs]


--------------------------------
 Guía de Instalación Paso a Paso
--------------------------------


## Sistema de Notificaciones de Inventario


Al finalizar esta guía el proyecto tendrá:
-  3 servicios corriendo en Docker
-  RabbitMQ Management UI funcionando
-  2 APIs con documentación Swagger
-  Sistema completo de mensajería en tiempo real

---

## Tiempo Estimado

- **Primera instalación**: 15-30 minutos
- **Ejecuciones posteriores**: 2-3 minutos

---

## PARTE 1: PRERREQUISITOS 

### 1.1 Verificar Sistema Operativo

#### Windows 10/11
-  Versión mínima: Windows 10 versión 2004 o superior
-  Arquitectura: 64-bit

#### Mac
- macOS 10.15 o superior
- Chip Intel o Apple Silicon

#### Linux
- Ubuntu 18.04+, CentOS 7+, Debian 9+

### 1.2 Instalar Docker Desktop

#### En Windows:

**Paso 1**: Ve a https://www.docker.com/products/docker-desktop
**Paso 2**: Click en "Download for Windows"
**Paso 3**: Ejecuta el archivo descargado `Docker Desktop Installer.exe`
**Paso 4**: Sigue el asistente:
   - Marca "Use WSL 2 instead of Hyper-V"
   - Marca "Add shortcut to desktop"
**Paso 5**: **REINICIA tu computadora** cuando se complete
**Paso 6**: Después del reinicio, abre "Docker Desktop"
**Paso 7**: Acepta los términos de servicio
**Paso 8**: **ESPERA** hasta ver el ícono de docker en la bandeja del menu de inicio

#### En Mac:

**Paso 1**: Ve a https://www.docker.com/products/docker-desktop
**Paso 2**: Click en "Download for Mac"
**Paso 3**: Arrastra Docker a la carpeta Applications
**Paso 4**: Abre Docker desde Applications
**Paso 5**: Permite las configuraciones de seguridad si te lo pide
**Paso 6**: Espera hasta que aparezca el ícono de docker en la barra superior

#### En Linux:

# Ubuntu/Debian
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Reiniciar sesión o ejecutar:
newgrp docker

# Instalar Docker Compose
sudo apt install docker-compose-plugin


### 1.3 WSL2 (Solo Windows)

Si tienes Windows y Docker te muestra errores de WSL:

#### Método Automático:
**Paso 1**: Presiona `Windows + X`
**Paso 2**: Click en "Windows PowerShell (Admin)" o "Terminal (Admin)"
**Paso 3**: Ejecuta:

wsl --install

**Paso 4**: **REINICIA Windows** cuando termine
**Paso 5**: Después del reinicio, ejecuta:

wsl --set-default-version 2


#### Si el método automático falla:
**Paso 1**: Abre PowerShell como Administrador
**Paso 2**: Ejecuta estos comandos uno por uno:

dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart

dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart

**Paso 3**: **REINICIA Windows**
**Paso 4**: Descarga manualmente desde: https://aka.ms/wsl2kernel
**Paso 5**: Instala el archivo `wsl_update_x64.msi`
**Paso 6**: Ejecuta: `wsl --set-default-version 2`

### 1.4 Verificar Instalación de Docker

**Paso 1**: Abre Command Prompt, PowerShell o Terminal
**Paso 2**: Ejecuta:

docker --version

**Resultado esperado**: `Docker version 24.x.x, build xxxxx`

**Paso 3**: Ejecuta:

docker-compose --version

**Resultado esperado**: `Docker Compose version v2.x.x`

**Paso 4**: Prueba que Docker funcione:

docker run hello-world

**Resultado esperado**: Mensaje "Hello from Docker!"
---

## PARTE 2: OBTENER EL CÓDIGO

### Opción A: Con Git (Recomendado)

**Paso 1**: Instalar Git si no lo tienes:
- **Windows**: https://git-scm.com/download/win
- **Mac**: `brew install git` o desde App Store
- **Linux**: `sudo apt install git`

**Paso 2**: Clonar el repositorio:

git clone https://github.com/tu-usuario/inventory-notification-system.git

**Paso 3**: Entrar al directorio:

cd inventory-notification-system

### Opción B: Descargar ZIP

**Paso 1**: Ve al repositorio en GitHub
**Paso 2**: Click en el botón verde "Code"
**Paso 3**: Click en "Download ZIP"
**Paso 4**: Extrae el archivo en una carpeta de tu preferencia
**Paso 5**: Abre Command Prompt/PowerShell/Terminal en esa carpeta
---

## PARTE 3: VERIFICAR ESTRUCTURA DE ARCHIVOS

Asegurate de tener esta estructura exacta:

inventory-notification-system/
├── docker-compose.yml        
├── README.md
├── InventoryAPI/
│   ├── Dockerfile              
│   ├── InventoryAPI.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   └── ... (otros archivos)
├── NotificationService/
│   ├── Dockerfile              
│   ├── NotificationService.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   └── ... (otros archivos)


**IMPORTANTE**: Si falta algún `Dockerfile` o `docker-compose.yml`, el sistema no funcionará.

---

## PARTE 4: EJECUTAR EL SISTEMA

### 4.1 Primera Ejecución (Puede tardar 5-10 minutos)

**Paso 1**: Navega a la carpeta del proyecto:

cd /ruta/completa/a/inventory-notification-system


**Paso 2**: Ejecuta Docker Compose:

docker-compose up --build


** Lo que verás durante la primera ejecución:**

1. **Descarga de imágenes (2-3 minutos)**:

[+] Pulling rabbitmq
[+] Pulling inventory-api
```

2. **Construcción de aplicaciones (2-3 minutos)**:

[+] Building inventory-api
[+] Building notification-service


3. **Inicio de servicios (30-60 segundos)**:

[+] Running 4/4
✓ Network inventory_network Created
✓ Container rabbitmq Started
✓ Container inventory-api Started
✓ Container notification-service Started


** Señales de que todo está funcionando:**
- No hay mensajes de error en rojo
- Ves logs de los 3 servicios
- Los mensajes incluyen "Application started" o similar

### 4.2 Ejecuciones Posteriores (1-2 minutos)

Para las siguientes veces que ejecutes el sistema:


# Opción 1: Ver logs en tiempo real
docker-compose up

# Opción 2: Ejecutar en background
docker-compose up -d


### 4.3 Comandos Útiles


# Ver estado de los contenedores
docker-compose ps

# Ver logs de todos los servicios
docker-compose logs

# Ver logs de un servicio específico
docker-compose logs inventory-api
docker-compose logs notification-service
docker-compose logs rabbitmq

# Ver logs en tiempo real
docker-compose logs -f

# Detener todos los servicios
docker-compose down

# Detener y eliminar todo (incluyendo volúmenes)
docker-compose down -v

# Reconstruir desde cero
docker-compose build --no-cache
docker-compose up

---

## PARTE 5: ACCEDER A LAS INTERFACES

Una vez que el sistema esté corriendo, abre estas URLs en tu navegador:

### 5.1 RabbitMQ Management UI
- **URL**: http://localhost:15672
- **Usuario**: `admin`
- **Contraseña**: `admin123`
- **Qué verás**: Dashboard de RabbitMQ con exchanges y colas

### 5.2 InventoryAPI Swagger
- **URL**: http://localhost:7022/swagger
- **Qué verás**: Documentación interactiva de la API de inventario

### 5.3 NotificationService Swagger
- **URL**: http://localhost:7002/swagger
- **Qué verás**: Documentación interactiva de la API de notificaciones

** Si alguna URL no responde:**
1. Espera 30 segundos más
2. Verifica que no hay errores en los logs
3. Ejecuta `docker-compose ps` para ver el estado

---

##  PARTE 6: VERIFICAR QUE TODO FUNCIONE

### 6.1 Verificar Contenedores

docker-compose ps


**Resultado esperado:**

NAME                    IMAGE               STATUS
inventory-rabbitmq      rabbitmq:3.13...    Up
inventory-api           ...                 Up
notification-service    ...                 Up
```

### 6.2 Verificar RabbitMQ

**Paso 1**: Ve a http://localhost:15672
**Paso 2**: Login con admin/admin123
**Paso 3**: Click en "Exchanges"
**Paso 4**: Deberías ver `inventory_exchange`
**Paso 5**: Click en "Queues and Streams"
**Paso 6**: Deberías ver 3 colas:
- `product.created`
- `product.updated`
- `product.deleted`

### 6.3 Probar Creación de Producto

**Paso 1**: Ve a http://localhost:7022/swagger
**Paso 2**: Expand "GET /api/products"
**Paso 3**: Click "Try it out" → "Execute"
**Paso 4**: Deberías ver productos de ejemplo

**Paso 5**: Expand "POST /api/products"
**Paso 6**: Click "Try it out"
**Paso 7**: Copia este JSON:
```json
{
  "name": "Producto de Prueba",
  "description": "Creado para verificar el sistema",
  "price": 99.99,
  "stock": 5,
  "category": "Test"
}
```
**Paso 8**: Click "Execute"
**Paso 9**: Deberías ver "201 Created"

### 6.4 Verificar Notificaciones

**Paso 1**: Ve a http://localhost:7002/swagger
**Paso 2**: Expand "GET /api/logs"
**Paso 3**: Click "Try it out" → "Execute"
**Paso 4**: Deberías ver un log del producto que acabas de crear

### 6.5 Verificar RabbitMQ (Final)

**Paso 1**: Vuelve a http://localhost:15672
**Paso 2**: Ve a "Queues and Streams"
**Paso 3**: La cola `product.created` debería mostrar que procesó 1 mensaje

---


## Tecnologías Utilizadas

- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - APIs REST
- **Entity Framework Core** - ORM
- **SQLite** - Base de datos
- **RabbitMQ** - Message Broker
- **Docker & Docker Compose** - Containerización
- **Swagger/OpenAPI** - Documentación de APIs

## Características

### InventoryAPI (Productor)
- CRUD completo de productos
- Publicación de eventos a RabbitMQ
- Circuit Breaker pattern
- Manejo de reintentos
- Documentación Swagger de APIs

### NotificationService (Consumidor)
- Consumo de mensajes de RabbitMQ
- Persistencia de logs de eventos
- Manejo de errores y dead letters
- API de consulta de logs
- Estadísticas de procesamiento

### RabbitMQ
- Exchange tipo 'direct' llamado 'inventory_exchange'
- Colas por tipo de evento (created, updated, deleted)
- Mensajes persistentes
- Management UI disponible

### URLs de Acceso

| Servicio                    | URL                           | Credenciales   |
|-----------------------------|-------------------------------|----------------|
| InventoryAPI Swagger        | http://localhost:7022/swagger |      -         |
| NotificationService Swagger | http://localhost:7002/swagger |      -         |
| RabbitMQ Management         | http://localhost:15672        | admin/admin123 |

## Pruebas

### 1. Crear un producto

curl -X POST "http://localhost:7022/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop Test",
    "description": "Laptop para pruebas",
    "price": 999.99,
    "stock": 10,
    "category": "Electronics"
  }'
```

### 2. Verificar logs de notificaciones

curl -X GET "http://localhost:7002/api/logs"
```

### 3. Ver estadísticas

curl -X GET "http://localhost:7002/api/logs/stats"
```

## Flujo de Datos

1. **Cliente** envía request a **InventoryAPI**
2. **InventoryAPI** procesa la operación en la base de datos
3. **InventoryAPI** publica evento a **RabbitMQ**
4. **NotificationService** consume el evento
5. **NotificationService** guarda log en su base de datos

## Configuración

### Variables de Entorno

#### InventoryAPI

ConnectionStrings__DefaultConnection=Data Source=/app/data/inventory.db
RabbitMQ__HostName=rabbitmq
RabbitMQ__Port=5672
RabbitMQ__UserName=admin
RabbitMQ__Password=admin123
RabbitMQ__ExchangeName=inventory_exchange
```

#### NotificationService

ConnectionStrings__DefaultConnection=Data Source=/app/data/notifications.db
RabbitMQ__HostName=rabbitmq
RabbitMQ__Port=5672
RabbitMQ__UserName=admin
RabbitMQ__Password=admin123
RabbitMQ__ExchangeName=inventory_exchange
```

##  Troubleshooting

### Error: "BrokerUnreachableException"
- Verificar que RabbitMQ esté corriendo
- Revisar configuración de red en Docker

### Error: "Database locked"
- Reiniciar los contenedores: `docker-compose restart`

### Ver logs de servicios

# Logs de todos los servicios
docker-compose logs

# Logs específicos
docker-compose logs inventory-api
docker-compose logs notification-service
docker-compose logs rabbitmq
```

## Características de Resiliencia

- **Circuit Breaker** en RabbitMQ Publisher
- **Reintentos automáticos** con backoff exponencial
- **Mensajes persistentes** para evitar pérdida de datos
- **Dead Letter Queues** para mensajes fallidos
- **Health Checks** en todos los servicios

## Monitoreo

- **RabbitMQ Management UI** para monitorear colas y mensajes
- **Swagger UI** para probar APIs
- **Logs estructurados** en todos los servicios
- **Endpoint /health** para health checks

## Autor

- **AGUSTIN CRUZ OTTONELLO** 