# Arc42 – ERP de Concesionario de Vehículos

# 1. Introducción & objetivos

**Producto:**

ERP educativo para **concesionarios de vehículos**.

**Objetivo:**

Gestionar inventario de vehículos por sucursal, compras a proveedores o fabricantes, ventas de vehículos, facturación y recursos humanos con trazabilidad básica y simplicidad.

**Usuarios clave:**

- Administrador del sistema
- Encargado de inventario
- Vendedor de vehículos
- Área de compras
- Recursos Humanos

## Aplicaciones implementadas

- Autenticación y autorización completa con **ASP.NET Core Identity**, roles y políticas por módulo y capacidad.
- Control de la interfaz de usuario mostrando u ocultando acciones según permisos en **Razor Pages**.

### RRHH extendido

Incluye:

- Contratos
- TipoContrato
- Parámetros de Nómina
- cálculo de nómina
- liquidación laboral

### Inventario mejorado

- Kardex de movimientos de vehículos
- control de stock por sucursal
- alertas de bajo inventario

### Notificaciones

- modelo de notificaciones
- servicio de notificaciones
- listado de notificaciones
- opción de marcar como leídas o borrar

### Dashboards

- panel de inicio para RRHH
- panel de inventario
- métricas y gráficos del sistema

# 2. Restricciones

**Tecnología**

- ASP.NET Core (Razor Pages)
- Entity Framework Core
- SQL Server

**Idioma / Moneda**

- Español (Colombia)
- montos en `decimal(18,2)`

## Arquitectura

Arquitectura **monolítica modular** por áreas:

- Inventario
- Proveedores
- Ventas
- Facturación
- Recursos Humanos

## Entorno

- **IDE:** Visual Studio
- **Servidor de desarrollo:** Kestrel
- **Base de datos:** SQL Server LocalDB

## Limitaciones

- Sin integraciones externas (pagos o contabilidad)
- Proyecto académico
- Se prioriza **simplicidad sobre completitud**

# 3. Contexto & alcance

## Contexto interno

- Interfaz web construida con **Razor Pages**
- acceso a datos mediante **Entity Framework Core**

## Contexto externo

Actualmente el sistema **no se integra con sistemas externos** como:

- plataformas de pago
- sistemas contables
- sistemas gubernamentales

## Alcance funcional

### Inventario

- registro de vehículos
- stock por sucursal
- kardex de inventario
- alertas por bajo stock

### Proveedores

- registro de proveedores
- relación **vehículo – proveedor**
- historial de precios de compra

### Ventas

- registro de clientes
- condiciones de pago
- cotizaciones
- pedidos de venta

### Facturación

- facturas de venta
- registro de cobros
- secuencia de documentos
- descarga automática de inventario
- notificaciones por bajo stock

### Recursos Humanos

- empleados
- departamentos
- cargos
- contratos laborales
- tipo de contrato
- parámetros de nómina
- cálculo de nómina
- liquidación laboral

# 4. Estrategia de solución

### Modelo de datos normalizado

Se utiliza un modelo de datos estructurado con **claves compuestas para garantizar trazabilidad**.

### Servicios de dominio

Servicios principales del sistema:

- `InventarioService`
- `NominaService`
- `NotificacionService`

### Autenticación

Uso de **ASP.NET Core Identity** con:

- roles
- policies
- autorización granular

### Validaciones

- DataAnnotations
- reglas de negocio en Razor Pages

### Transacciones

Se utilizan en operaciones críticas como:

- venta de vehículos
- actualización de inventario
- liquidación de nómina

### Seeds iniciales

Datos iniciales para:

- roles
- tipos de contrato
- parámetros de nómina

### Dashboards

Uso de consultas optimizadas mediante:

- `AsNoTracking`
- DTOs
- consultas agregadas

# 5. Estructura (Building Blocks)

## Capas principales

```
Pages (UI)
   ↓
Services (Lógica de negocio)
   ↓
DbContext / EF Core (Datos)
```

## Contextos

### AppDbContext

Maneja las operaciones del ERP.

### ApplicationDbContext

Gestiona usuarios, roles y autenticación mediante Identity.

## Servicios registrados

- `IInventarioService`
- `INominaService`
- `INotificacionService`

# Módulos del sistema

# Inventario

### Vehiculo

- VIN
- Marca
- Modelo
- Año
- Tipo
- Precio
- Impuesto

### Almacen

Representa la sucursal o patio donde se encuentran los vehículos.

### Existencia

Controla el stock de vehículos por sucursal.

Clave primaria:

```
VehiculoId + AlmacenId
```

### MovimientoInventario (Kardex)

Registra:

- ingreso de vehículos
- venta
- traslado
- ajuste

# Proveedores

### Proveedor

- nombre
- NIT
- contacto

### VehiculoProveedor

Clave primaria:

```
VehiculoId + ProveedorId + FechaDesde
```

Incluye:

- proveedor principal
- precio de compra

# Ventas

### Cliente

- documento
- nombre
- teléfono
- correo

### PedidoVenta

Registro de solicitud de compra.

### PedidoVentaLinea

Contiene snapshot de:

- precio
- impuesto

# Notificaciones

### Notificacion

Eventos del sistema como:

- `LOW_STOCK`

### INotificacionService

Servicio para gestionar notificaciones.

# Recursos Humanos

### Empleado

Relación con:

- sucursal
- departamento
- cargo

### Contrato

Relación con **TipoContrato**

### ParametrosNomina

Variables usadas para el cálculo salarial.

### Nomina

Registro del pago salarial.

### PeriodoNomina

Periodo de cálculo de nómina.

### Liquidacion

Cierre del contrato laboral.

# UI

Componentes principales:

- `_Layout`
- `_SideMenu` controlado por roles y políticas

### Panel de notificaciones

Ruta:

```
/Notificaciones/Index
```

Opciones:

- marcar como leída
- eliminar

### Dashboards

- Dashboard de RRHH
- Dashboard de Inventario

Incluyen:

- KPIs
- gráficos

# 6. Vista de ejecución (Runtime View)

## Flujo de facturación

1. El usuario crea una factura.
2. Se guarda **FacturaVenta + líneas**.
3. `InventarioService.DescargarStockPorFacturaAsync` inicia una transacción.
4. Se valida disponibilidad de vehículos.
5. Se registra movimiento en kardex.
6. `NotificacionService.EvaluarBajoStockAsync` verifica inventario.
7. Si aplica, crea notificación **LOW_STOCK**.

## Flujo de RRHH

1. Se crea o edita un contrato.
2. `NominaService` genera nómina según `PeriodoNomina`.
3. Calcula:
- devengos
- deducciones
- provisiones
1. Guarda registros.

### Liquidación

Cuando termina el contrato:

- se cierra el contrato
- se calcula la liquidación final

# 7. Despliegue

## Entorno de desarrollo

- Kestrel
- SQL Server LocalDB

## Migraciones

```
Add-Migration AddParametrosNomina -Context AppDbContext
Update-Database -Context AppDbContext

Add-Migration IdentitySeedRoles -Context ApplicationDbContext
Update-Database -Context ApplicationDbContext
```

## Seeds

Datos iniciales:

- Roles
- TipoContrato

Ejemplo:

- Indefinido
- Fijo
- Prestación de servicios

## DevOps

- Git
- ramas feature
- pull requests hacia develop

## Despliegue futuro

- IIS
- Nginx
- Azure App Service

# 8. Conceptos transversales

### Autenticación y autorización

ASP.NET Core Identity + Policies.

### Transacciones

Uso de transacciones en operaciones críticas.

### Manejo de montos

```
decimal(18,2)
```

Impuestos:

```
decimal(5,2)
```

### Auditoría

Kardex de inventario registra trazabilidad de movimientos.

### Rendimiento

Uso de:

- `AsNoTracking`
- índices
- DTOs

### Validación

- reglas de negocio
- DataAnnotations

### Control de notificaciones duplicadas

No se crea una notificación **LOW_STOCK** del mismo vehículo en menos de **24 horas**.

# 9. Decisiones (ADRs)

## Decisiones originales

- Stock por sucursal mediante **Existencia con clave compuesta**.
- Histórico proveedor–vehículo con **FechaDesde**.
- Secuencias por documento:

```
FV = Factura de Venta
PV = Pedido de Venta
```

- Uso de **Razor Pages** en lugar de MVC o SPA.

## Nuevas decisiones

- Uso de **ASP.NET Identity con roles y policies**.
- Integración de notificaciones dentro de `InventarioService`.
- Persistencia de **ParametrosNomina**.
- Control de interfaz mediante políticas en Razor Pages.

# 10. Calidad & escenarios

### Integridad de inventario

Operaciones atómicas y validación de disponibilidad.

### Seguridad

Uso de `[Authorize]` en operaciones sensibles.

### Trazabilidad

Kardex enlazado con documento origen.

### Evolutividad

Módulos desacoplados y snapshots de precios.

### Usabilidad

- autocompletado
- botones condicionales según permisos

---

### Rendimiento

- consultas con DTOs
- paginación

---

### Notificaciones

- sin duplicados
- estado **visto / no visto**

---

# 11. Riesgos & deudas

- seguridad incompleta en algunos módulos
- falta auditoría de acciones críticas
- concurrencia no controlada
- validaciones de nómina incompletas
- impuestos simplificados
- falta de pruebas automáticas
- soft delete no implementado en todas las entidades

# 12. Glosario

**VIN**

Número único de identificación del vehículo.

**Kardex**

Historial de movimientos de inventario.

**FV**

Factura de venta.

**PV**

Pedido de venta.

**Snapshot**

Copia del precio del vehículo al momento de la venta.

**PeriodoNomina**

Periodo temporal de cálculo salarial.

**ParametrosNomina**

Variables globales de cálculo (salario mínimo, salud, pensión).

**Liquidacion**

Cierre del contrato laboral y cálculo final.

**Notificacion**

Evento del sistema (ej. `LOW_STOCK`) con severidad y estado visto/no visto.

**Identity**

Sistema de autenticación y roles.

**Policy**

Regla de autorización asociada a roles o claims.

**Badge**

Indicador visual de notificaciones no leídas.
