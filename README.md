# AD01 - Comercio y distribución multibodega (nivel básico)

Práctica 1 de Aplicaciones Distribuidas, Unidad 1. Hecha en C# con ASP.NET Core Web API, Entity Framework Core y SQL Server (instancia localhost), siguiendo el nivel de los ejemplos de clase: modelos, contextos, controladores, `Program.cs`, Swagger y Postman.

## 1. Requisitos

- Windows con **Visual Studio** (carga de trabajo "ASP.NET y desarrollo web") o .NET SDK 10.
- **SQL Server** instalado y ejecutandose como `localhost` (autenticacion de Windows), con SSMS para visualizar tablas.
- Postman para importar la colección.

Esta versión utiliza .NET 10, igual que el ZIP de referencia. No requiere Docker, microservicios con mensajería, repositorios genéricos ni autenticación.

## 2. Proyecto y bases de datos

| API | Puerto HTTP | Base de datos | Tablas |
| --- | --- | --- | --- |
| Catalogo.Api | 5001 | CatalogoDB_AD01_Basico | Categoria, Marca, Producto |
| Inventario.Api | 5002 | InventarioDB1_AD01_Basico | Bodega, Existencia, MovimientoInventario, ReservaStock |
| Pedidos.Api | 5003 | PedidosDB1_AD01_Basico | Cliente, Pedido, DetallePedido |

Cada API abre su propia conexión SQL Server. La base y las tablas se crean automáticamente con `EnsureCreated()` en su primer arranque, si el servidor `localhost` está disponible. Esto se hace para evitar pasos avanzados de migraciones en la práctica. **Si ya tienes tablas con esos nombres y otro diseño, debes usar bases nuevas**; `EnsureCreated` no actualiza esquemas existentes.


## Bases de datos: creación y visualización

Cada API utiliza una base diferente, con nombres propios para no sobrescribir bases de otros ejercicios.

1. Ejecuta `PREPARAR_Y_EJECUTAR.bat` desde la carpeta principal. Si se encuentra `sqlcmd`, creará las bases y tablas antes de iniciar las API.
2. Si no tienes `sqlcmd`, abre en **SQL Server Management Studio** los tres archivos `BASES_DE_DATOS/01_CatalogoDB.sql`, `02_InventarioDB.sql` y `03_PedidosDB.sql`; ejecútalos con **F5**. Los scripts no eliminan datos y omiten tablas ya existentes.
3. También puede iniciarse cada API directamente: el proyecto usa `Database.EnsureCreated()` para crear tablas cuando la base aún está vacía o no existe.
4. En SSMS ejecuta `BASES_DE_DATOS/04_VISUALIZAR_TODAS_LAS_TABLAS.sql` para consultar las **10 tablas**. Si están vacías, crea datos desde Swagger o Postman y ejecuta la consulta otra vez.
5. En el Explorador de objetos de SSMS, presiona **Actualizar** sobre Bases de datos para mostrar las tres bases.

**Importante:** `EnsureCreated()` no actualiza tablas que ya existen ni migra esquemas antiguos. Los scripts corresponden a este proyecto básico; no los utilices sobre una base preexistente con otra estructura.

## 3. Cómo abrirlo en Visual Studio

1. Extrae el ZIP.
2. Abre `ComercioMultibodega.slnx` en Visual Studio con soporte para .NET 10.
3. Espera la restauración de paquetes NuGet.
4. Inicia **Catalogo.Api**, después **Inventario.Api** y después **Pedidos.Api**. Puedes configurar la solución con varios proyectos de inicio, o abrir tres terminales.
5. Swagger estará en:

   - Catálogo: http://localhost:5001/swagger
   - Inventario: http://localhost:5002/swagger
   - Pedidos: http://localhost:5003/swagger

Desde una terminal situada en la carpeta principal, también puedes iniciar cada proyecto así (cada comando en una terminal diferente):

```bash
dotnet restore ComercioMultibodega.slnx
dotnet run --project Catalogo.Api --launch-profile http
dotnet run --project Inventario.Api --launch-profile http
dotnet run --project Pedidos.Api --launch-profile http
```

Si tu instancia no se llama `localhost`, cambia `Server=localhost` en los tres `appsettings.json` y en `PREPARAR_Y_EJECUTAR.bat` (los tres comandos `sqlcmd -S localhost`). Cada API debe utilizar su propia base de datos.

## 4. Orden para probarlo (Swagger o Postman)

1. Catálogo: crear una **Categoria** y una **Marca**; luego crear un **Producto** que use sus IDs.
2. Inventario: crear una **Bodega**; crear una **Existencia** con el ID de ese Producto y de esa Bodega.
3. Inventario: registrar una **ENTRADA** o **SALIDA** en Movimientos; el stock se actualiza automáticamente. Crear una reserva **ACTIVA** para descontarla del stock disponible (no del stock físico).
4. Pedidos: crear un **Cliente**, después un **Pedido**, y finalmente un **Detalle** usando el producto del Catálogo. El **Total** del pedido se calcula cuando se agrega, edita o borra un detalle.
5. Pedidos: `GET /api/Pedidos/{id}/disponibilidad` consulta el stock disponible de Inventario.Api por HTTP, para mostrar si hay unidades suficientes. **Este GET no reserva ni descuenta stock**.
6. Para pruebas rápidas, importa `Postman/AD01_Comercio_Multibodega.postman_collection.json` y ejecuta las carpetas **01, 02 y 03 en orden**. La colección guarda los IDs creados para las peticiones siguientes. Las carpetas **04 y 05** tienen pruebas negativas y borrados opcionales.

Si vuelves a ejecutar toda la colección en la misma base de datos, el SKU `LAP-LEN-AD01` y la cédula de ejemplo pueden repetirse: usa valores nuevos o elimina previamente los registros de prueba.

## 5. Reglas y validaciones

- **Categoria → Producto y Marca → Producto:** claves foráneas físicas en CatalogoDB.
- **Bodega → Existencia/Movimiento/Reserva:** claves foráneas físicas en InventarioDB.
- **Cliente → Pedido y Pedido → Detalle:** claves foráneas físicas en PedidosDB.
- **Producto en Inventario y DetallePedido:** referencia **lógica** a Catalogo.Api, ya que SQL Server no crea FK entre las bases de microservicios. Inventario y Pedidos verifican el producto con una consulta HTTP al catálogo al crear registros.
- El stock físico **nunca puede quedar por debajo de las reservas activas**. Las salidas restan stock y las entradas lo aumentan. No se permite cantidad negativa.
- Los movimientos tienen un historial de registro: **GET y POST**, sin PUT ni DELETE, para no alterar un movimiento pasado sin ajustar stock.
- Un pedido empieza `PENDIENTE`, se puede `CONFIRMAR` o `CANCELAR`, y no permite cambiar detalles cuando no está pendiente.
- Para evitar problemas, hay validaciones para impedir el borrado de categorías, marcas, clientes y bodegas con registros dependientes.
- HTTP **200**: consulta correcta; **201**: creación; **204**: actualización o eliminación; **400**: dato inválido o regla incumplida; **404**: recurso no encontrado; **503**: API de catálogo o inventario no disponible.

### Qué significa "independientes"

Cada microservicio tiene su propia base de datos y puede responder a consultas locales sin las otras API. Algunas operaciones **sí** se comunican por HTTP: crear existencias o movimientos en Inventario.Api, crear detalles en Pedidos.Api y consultar disponibilidad en Pedidos.Api requieren que las APIs correspondientes estén iniciadas.

**Limitación deliberada del ejercicio básico:** registrar un pedido no realiza un descuento ni una reserva automática de inventario. El documento enumera el registro de pedidos, las reservas y los movimientos como operaciones de dominios distintos; la reserva se hace explícitamente en Inventario.Api. No se implementan transacciones distribuidas ni control de concurrencia entre varias personas comprando al mismo tiempo. La comprobación de disponibilidad es informativa.

**Atención con DELETE de Producto:** SQL Server no impide físicamente que borres un producto de CatalogoDB que todavía está referenciado en InventarioDB o PedidosDB. Para respetar las referencias lógicas, **solo elimina productos sin existencias, movimientos, reservas ni detalles de pedidos**; en Postman la carpeta 05 crea y elimina un producto temporal independiente. En un sistema real haría falta más control entre servicios.

## 6. Archivos entregados

- `Catalogo.Api/` código fuente del servicio Catálogo.
- `Inventario.Api/` código fuente del servicio Inventario.
- `Pedidos.Api/` código fuente del servicio Pedidos.
- `ComercioMultibodega.slnx` solución de los 3 proyectos.
- `Postman/AD01_Comercio_Multibodega.postman_collection.json` colección importable de pruebas.
- `.gitignore` excluye `bin`, `obj`, `.vs`, archivos locales.
- `BASES_DE_DATOS/` incluye los scripts SQL de creación y consulta para SSMS.
- `PREPARAR_Y_EJECUTAR.bat`: prepara las bases con `sqlcmd` cuando está disponible e inicia las tres API.
- `Declaracion_IA_BORRADOR.pdf`: borrador de declaración; **no sustituye la plantilla oficial del docente**, que no fue adjuntada.

## 7. Subir el proyecto a GitHub

El PDF solicita el formato `DISTR-[NIVEL/PARALELO]-[BIMESTRE]-[ACTIVIDAD]-[TEMA]-[APELLIDO/NOMBRE]`. Sustituye tus propios datos de nivel/paralelo y apellido/nombre; no los adivines.

Ejemplo de comandos, luego de crear un repositorio vacío en tu cuenta:

```bash
git init
git add .
git commit -m "AD01: tres microservicios basicos"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/TU_REPOSITORIO.git
git push -u origin main
```

**Antes de entregar:** ejecuta las tres API, exporta las evidencias o la colección de Postman y completa la **plantilla oficial de declaración de uso de IA** con tus datos. No se dispone de tus credenciales ni se ha publicado nada en GitHub desde aquí.

## 8. Nota sobre las pruebas técnicas

Los archivos se revisaron estructuralmente y se generó la colección JSON de Postman, pero en el entorno de elaboración no está instalado el SDK de .NET ni SQL Server, por lo que **no se ha podido compilar ni ejecutar aquí contra LocalDB**. La validación final debe hacerse en tu Visual Studio.
