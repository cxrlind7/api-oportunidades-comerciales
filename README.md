# API Oportunidades Comerciales

API REST en ASP.NET Core (.NET 8) con SQL Server para registrar, consultar y cambiar el estado de oportunidades comerciales.

## 1. Lo que necesitas instalado

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). Para revisar: `dotnet --version` (debe empezar con 8)
- SQL Server. Puede ser LocalDB (viene con Visual Studio o se instala con SQL Server Express), Express o Developer
- Algo para correr scripts SQL: SSMS o VS Code con la extensión SQL Server (mssql)

## 2. Crear la base de datos

Correr los scripts de la carpeta `database/` **en este orden**:

1. `01_database.sql` - crea la base `OportunidadesComerciales` y la tabla `Oportunidades`
2. `02_storedProcedures.sql` - crea los 4 stored procedures

Los dos se pueden correr varias veces sin error.

**Con VS Code (extensión mssql):** conectarse a `(localdb)\MSSQLLocalDB` con Windows Authentication, abrir cada script y darle `Ctrl+Shift+E`.

**Con SSMS:** conectarse al servidor, abrir cada script y darle Execute (F5).

### Tabla Oportunidades

| Columna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | llave primaria |
| Nombre | NVARCHAR(100) | |
| Cliente | NVARCHAR(120) | |
| MontoEstimado | DECIMAL(18,2) | mayor a 0 |
| Estado | VARCHAR(20) | default `Nueva`. Solo Nueva, EnSeguimiento, Ganada o Perdida |
| FechaCreacionUtc | DATETIME2 | default `SYSUTCDATETIME()` |

### Stored procedures

- `sp_Oportunidades_Crear` - recibe nombre, cliente y monto. Regresa la fila creada
- `sp_Oportunidades_ObtenerPorId` - regresa la oportunidad o nada si no existe
- `sp_Oportunidades_Listar` - filtro de estado opcional
- `sp_Oportunidades_ActualizarEstado` - solo actualiza si el estado sigue siendo el esperado (concurrencia optimista). Regresa `FilasAfectadas`; si es 0 la API responde 409

## 3. Configurar la API

La cadena de conexión, la clave del JWT y los passwords **no están en el repo**. Van en `Oportunidades.Api/appsettings.Development.json`, que está en `.gitignore`.

Crearlo a partir del ejemplo:

```powershell
Copy-Item Oportunidades.Api/appsettings.Development.example.json Oportunidades.Api/appsettings.Development.json
```

Y llenar en `appsettings.Development.json`:

| Campo | Qué poner |
|---|---|
| `ConnectionStrings:DefaultConnection` | Ya viene para LocalDB. Para otro servidor: `Server=localhost;Database=OportunidadesComerciales;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True` |
| `Jwt:Key` | Una clave de **mínimo 32 caracteres** |
| `Auth:Usuarios:consulta:Password` | El password que quieras para el usuario `consulta` |
| `Auth:Usuarios:editor:Password` | El password que quieras para el usuario `editor` |

## 4. Correr la API

```powershell
dotnet run --project Oportunidades.Api --launch-profile http
```

Abrir Swagger en el navegador: **<http://localhost:5006/swagger>**

Para detenerla: `Ctrl+C` en la terminal.

## 5. Autenticarse

Todos los endpoints piden token, menos el login. Hay dos usuarios:

| Usuario | Rol | Puede |
|---|---|---|
| consulta | Consulta | solo GET |
| editor | Editor | todo |

El password de cada uno es el que pusiste en el paso 3.

En Swagger:

1. Abrir `POST /api/auth/login` → **Try it out** y mandar:

   ```json
   { "usuario": "editor", "password": "TU_PASSWORD" }
   ```

2. Copiar el `accessToken` de la respuesta
3. Darle al botón **Authorize** (arriba a la derecha), pegar el token y darle Authorize
4. Ya se pueden probar los demás endpoints

## 6. Endpoints

| Método | Ruta | Rol | Respuestas |
|---|---|---|---|
| POST | /api/oportunidades | Editor | 201, 400, 401, 403 |
| GET | /api/oportunidades/{id} | Consulta, Editor | 200, 400, 401, 404 |
| GET | /api/oportunidades?estado=EnSeguimiento | Consulta, Editor | 200, 400, 401 |
| PATCH | /api/oportunidades/{id}/estado | Editor | 200, 400, 401, 403, 404, 409 |

Estados: `Nueva` → `EnSeguimiento` → `Ganada` o `Perdida`. Toda oportunidad empieza en `Nueva`.

### Ejemplos con curl

Primero sacar el token con el login y reemplazar `TOKEN` en los demás:

```bash
curl -X POST http://localhost:5006/api/auth/login -H "Content-Type: application/json" -d '{"usuario":"editor","password":"TU_PASSWORD"}'

curl -X POST http://localhost:5006/api/oportunidades -H "Authorization: Bearer TOKEN" -H "Content-Type: application/json" -d '{"nombre":"ERP","cliente":"ACME","montoEstimado":1500.50}'

curl http://localhost:5006/api/oportunidades/1 -H "Authorization: Bearer TOKEN"

curl "http://localhost:5006/api/oportunidades?estado=Nueva" -H "Authorization: Bearer TOKEN"

curl -X PATCH http://localhost:5006/api/oportunidades/1/estado -H "Authorization: Bearer TOKEN" -H "Content-Type: application/json" -d '{"estado":"EnSeguimiento"}'
```

## 7. Pruebas

```powershell
dotnet test
```

Las pruebas de integración crean su propia base (`OportunidadesComerciales_Tests`) en `(localdb)\MSSQLLocalDB` con los mismos scripts y la borran al terminar. No tocan la base normal y no necesitan el `appsettings.Development.json`.

## Cómo funciona

### Endpoints

- **POST /api/oportunidades**: recibe `nombre`, `cliente` y `montoEstimado`. Regresa **201** con la oportunidad creada y el header `Location` con su URL. Siempre empieza en estado `Nueva`.
- **GET /api/oportunidades/{id}**: regresa **200** con la oportunidad o **404** si no existe.
- **GET /api/oportunidades?estado=...**: regresa **200** con una lista. El filtro es opcional; si no hay coincidencias regresa una lista vacía `[]`.
- **PATCH /api/oportunidades/{id}/estado**: recibe `estado`. Regresa **200** con la oportunidad, **404** si no existe o **409** si el cambio no se puede aplicar (transición no permitida o alguien más la cambió al mismo tiempo). Si mandas el mismo estado que ya tiene, la regresa sin cambios.

### Validaciones y errores

- Regresa **400** cuando los datos están mal: nombre vacío o de más de 100 caracteres, cliente vacío o de más de 120, monto menor o igual a 0, con más de 2 decimales o más de 16 enteros, id menor o igual a 0, o un estado que no existe (en el body o en el filtro).
- Todos los errores (400, 404, 409 y 500) regresan en formato **ProblemDetails**. En los 400 viene `errors` con los campos que hay que corregir, por ejemplo:

  ```json
  {
    "status": 400,
    "title": "Hay datos inválidos en la petición.",
    "errors": { "montoEstimado": ["El monto debe ser mayor a 0."] }
  }
  ```

- Los errores inesperados se manejan en un solo lugar (`GlobalExceptionHandler`): el detalle se guarda en el log y al cliente se le regresa un **500** con un mensaje general.

### Seguridad

- Los cuatro endpoints piden token **JWT Bearer**. Se valida la firma, el emisor (issuer), la audiencia (audience) y la expiración.
- El rol **Consulta** solo puede usar los GET. El rol **Editor** puede consultar, crear y cambiar el estado.
- Sin token o con un token inválido o vencido regresa **401**. Si el rol no tiene permiso regresa **403**.
- La clave del JWT y los passwords no están en el repo, van en `appsettings.Development.json` (paso 3). Los tokens se sacan con el login (paso 5) y duran 60 minutos.

## Estructura

- `database/` - scripts SQL
- `Oportunidades.Domain` - entidad, estados y reglas de cambio de estado
- `Oportunidades.Application` - validaciones y servicio
- `Oportunidades.Infrastructure` - repositorio que llama a los stored procedures
- `Oportunidades.Api` - controllers, login/JWT y manejo de errores
- `Oportunidades.Tests` - pruebas unitarias y de integración
