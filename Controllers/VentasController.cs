using ERPConcesionario.Helpers;
using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
{
    public class VentasController : Controller
    {
        private readonly SqlConnection _connection;

        public VentasController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Ventas");
            if (acceso != null) return acceso;

            DatabaseSchemaHelper.EnsureVentasFacturacionTables(_connection);
            return View(ObtenerVentas());
        }

        public IActionResult Create(int? idInventarioProducto = null)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Ventas");
            if (acceso != null) return acceso;

            DatabaseSchemaHelper.EnsureVentasFacturacionTables(_connection);
            CargarCombos();

            var model = new VentaProductoFormViewModel();
            if (idInventarioProducto.HasValue)
            {
                var producto = ObtenerProductoInventario(idInventarioProducto.Value);
                if (producto != null)
                {
                    model.IdInventarioProducto = producto.IdInventarioProducto;
                    model.PrecioUnitario = producto.PrecioVenta;
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(VentaProductoFormViewModel model)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Ventas");
            if (acceso != null) return acceso;

            DatabaseSchemaHelper.EnsureVentasFacturacionTables(_connection);

            var producto = ObtenerProductoInventario(model.IdInventarioProducto);
            if (producto == null)
                ModelState.AddModelError(nameof(model.IdInventarioProducto), "Producto no encontrado en inventario.");
            else
            {
                if (producto.StockActual < model.Cantidad)
                    ModelState.AddModelError(nameof(model.Cantidad), "No hay stock suficiente para realizar la venta.");

                if (model.PrecioUnitario <= 0)
                    model.PrecioUnitario = producto.PrecioVenta;
            }

            int? idEmpleado = ObtenerPrimerEmpleado();
            int? idSucursal = producto == null ? null : ObtenerSucursalPorInventario(producto.IdInventarioProducto);

            if (!idEmpleado.HasValue || !idSucursal.HasValue)
                ModelState.AddModelError("", "Debe existir al menos una sucursal y un empleado para registrar la venta.");

            if (!ModelState.IsValid || producto == null || !idEmpleado.HasValue || !idSucursal.HasValue)
            {
                CargarCombos();
                return View(model);
            }

            try
            {
                int idVenta = RegistrarVentaProducto(model, producto, idSucursal.Value, idEmpleado.Value);
                TempData["Mensaje"] = "Venta registrada correctamente: #" + idVenta;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No fue posible registrar la venta: " + ex.Message);
                CargarCombos();
                return View(model);
            }
        }

        private List<VentaListadoViewModel> ObtenerVentas()
        {
            var ventas = new List<VentaListadoViewModel>();

            string query = @"
                SELECT
                    v.IdVenta,
                    v.NumeroVenta,
                    v.FechaVenta,
                    v.TipoVenta,
                    v.EstadoVenta,
                    v.Subtotal,
                    v.Impuesto,
                    v.Total,
                    CASE
                        WHEN c.TipoCliente = 'NATURAL'
                            THEN LTRIM(RTRIM(ISNULL(c.Nombres, '') + ' ' + ISNULL(c.Apellidos, '')))
                        ELSE ISNULL(c.RazonSocial, ISNULL(c.NombreComercial, 'Cliente empresa'))
                    END AS Cliente,
                    LTRIM(RTRIM(ISNULL(e.PrimerNombre, '') + ' ' + ISNULL(e.PrimerApellido, ''))) AS Vendedor,
                    COUNT(vd.IdVentaDetalle) AS TotalItems,
                    CASE WHEN fv.IdFacturaVenta IS NULL THEN 0 ELSE 1 END AS TieneFactura
                FROM Ventas v
                INNER JOIN Clientes c ON v.IdCliente = c.IdCliente
                INNER JOIN Empleados e ON v.IdEmpleadoVendedor = e.IdEmpleado
                LEFT JOIN VentaDetalle vd ON v.IdVenta = vd.IdVenta
                LEFT JOIN FacturasVenta fv ON v.IdVenta = fv.IdVenta
                GROUP BY v.IdVenta, v.NumeroVenta, v.FechaVenta, v.TipoVenta, v.EstadoVenta,
                         v.Subtotal, v.Impuesto, v.Total, c.TipoCliente, c.Nombres, c.Apellidos,
                         c.RazonSocial, c.NombreComercial, e.PrimerNombre, e.PrimerApellido, fv.IdFacturaVenta
                ORDER BY v.FechaVenta DESC;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ventas.Add(new VentaListadoViewModel
                    {
                        IdVenta = Convert.ToInt32(reader["IdVenta"]),
                        NumeroVenta = reader["NumeroVenta"].ToString() ?? "",
                        FechaVenta = Convert.ToDateTime(reader["FechaVenta"]),
                        TipoVenta = reader["TipoVenta"].ToString() ?? "",
                        EstadoVenta = reader["EstadoVenta"].ToString() ?? "",
                        Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                        Impuesto = Convert.ToDecimal(reader["Impuesto"]),
                        Total = Convert.ToDecimal(reader["Total"]),
                        Cliente = reader["Cliente"].ToString() ?? "",
                        Vendedor = reader["Vendedor"].ToString() ?? "",
                        TotalItems = Convert.ToInt32(reader["TotalItems"]),
                        TieneFactura = Convert.ToInt32(reader["TieneFactura"]) == 1
                    });
                }

                _connection.Close();
            }

            return ventas;
        }

        private int RegistrarVentaProducto(VentaProductoFormViewModel model, VentaInventarioOptionViewModel producto, int idSucursal, int idEmpleado)
        {
            string numeroVenta = GenerarNumeroDocumento("VT");
            decimal bruto = model.Cantidad * model.PrecioUnitario;
            decimal descuento = Math.Min(model.DescuentoLinea, bruto);
            decimal subtotal = bruto - descuento;
            decimal impuesto = Math.Round(subtotal * producto.IVA / 100, 2);
            decimal total = subtotal + impuesto;

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                int stockActual;
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT StockActual
                    FROM InventarioProductos WITH (UPDLOCK, ROWLOCK)
                    WHERE IdInventarioProducto = @IdInventarioProducto;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdInventarioProducto", producto.IdInventarioProducto);
                    object? value = cmd.ExecuteScalar();
                    if (value == null || value == DBNull.Value)
                        throw new InvalidOperationException("El registro de inventario ya no existe.");

                    stockActual = Convert.ToInt32(value);
                }

                if (stockActual < model.Cantidad)
                    throw new InvalidOperationException("El stock disponible cambio y ya no alcanza para la venta.");

                int idVenta;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Ventas
                        (NumeroVenta, IdCliente, IdSucursal, IdEmpleadoVendedor, FechaVenta, TipoVenta,
                         EstadoVenta, Subtotal, Descuento, Impuesto, Total, Observaciones)
                    OUTPUT INSERTED.IdVenta
                    VALUES
                        (@NumeroVenta, @IdCliente, @IdSucursal, @IdEmpleado, GETDATE(), 'REPUESTOS',
                         'PENDIENTE', @Subtotal, @Descuento, @Impuesto, @Total, @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@NumeroVenta", numeroVenta);
                    cmd.Parameters.AddWithValue("@IdCliente", model.IdCliente);
                    cmd.Parameters.AddWithValue("@IdSucursal", idSucursal);
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Descuento", descuento);
                    cmd.Parameters.AddWithValue("@Impuesto", impuesto);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Observaciones", (object?)model.Observaciones ?? DBNull.Value);
                    idVenta = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO VentaDetalle
                        (IdVenta, IdProducto, IdUnidadVehiculo, DescripcionItem, Cantidad, PrecioUnitario,
                         DescuentoLinea, IVA, SubtotalLinea)
                    VALUES
                        (@IdVenta, @IdProducto, NULL, @DescripcionItem, @Cantidad, @PrecioUnitario,
                         @DescuentoLinea, @IVA, @SubtotalLinea);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                    cmd.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                    cmd.Parameters.AddWithValue("@DescripcionItem", producto.NombreProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@PrecioUnitario", model.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@DescuentoLinea", descuento);
                    cmd.Parameters.AddWithValue("@IVA", producto.IVA);
                    cmd.Parameters.AddWithValue("@SubtotalLinea", subtotal);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE InventarioProductos
                    SET StockActual = StockActual - @Cantidad,
                        FechaUltimoMovimiento = GETDATE()
                    WHERE IdInventarioProducto = @IdInventarioProducto;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@IdInventarioProducto", producto.IdInventarioProducto);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO MovimientosInventario
                        (FechaMovimiento, TipoMovimiento, ModuloOrigen, IdProducto, Cantidad,
                         IdAlmacenOrigen, IdUbicacionOrigen, CostoUnitario, DocumentoReferencia,
                         Observaciones, IdEmpleado)
                    SELECT
                        GETDATE(), 'SALIDA', 'VENTAS', i.IdProducto, @Cantidad,
                        i.IdAlmacen, i.IdUbicacion, p.CostoPromedio, @DocumentoReferencia,
                        @Observaciones, @IdEmpleado
                    FROM InventarioProductos i
                    INNER JOIN Productos p ON i.IdProducto = p.IdProducto
                    WHERE i.IdInventarioProducto = @IdInventarioProducto;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@DocumentoReferencia", numeroVenta);
                    cmd.Parameters.AddWithValue("@Observaciones", "Salida por venta de producto.");
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.Parameters.AddWithValue("@IdInventarioProducto", producto.IdInventarioProducto);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return idVenta;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        private void CargarCombos()
        {
            ViewBag.Clientes = ObtenerClientes();
            ViewBag.ProductosInventario = ObtenerProductosInventarioDisponibles();
        }

        private List<SelectListItem> ObtenerClientes()
        {
            var clientes = new List<SelectListItem>();

            string query = @"
                SELECT
                    IdCliente,
                    CodigoCliente,
                    CASE
                        WHEN TipoCliente = 'NATURAL'
                            THEN LTRIM(RTRIM(ISNULL(Nombres, '') + ' ' + ISNULL(Apellidos, '')))
                        ELSE ISNULL(RazonSocial, ISNULL(NombreComercial, 'Cliente empresa'))
                    END AS NombreCliente
                FROM Clientes
                WHERE EstadoCliente = 'ACTIVO'
                ORDER BY NombreCliente;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clientes.Add(new SelectListItem
                    {
                        Value = reader["IdCliente"].ToString(),
                        Text = (reader["CodigoCliente"].ToString() ?? "") + " - " + (reader["NombreCliente"].ToString() ?? "")
                    });
                }

                _connection.Close();
            }

            return clientes;
        }

        private List<VentaInventarioOptionViewModel> ObtenerProductosInventarioDisponibles()
        {
            var productos = new List<VentaInventarioOptionViewModel>();

            string query = @"
                SELECT
                    i.IdInventarioProducto,
                    p.IdProducto,
                    p.CodigoProducto,
                    p.Nombre AS NombreProducto,
                    a.Nombre AS Almacen,
                    u.Nombre AS Ubicacion,
                    i.StockActual,
                    p.PrecioVenta,
                    p.IVA
                FROM InventarioProductos i
                INNER JOIN Productos p ON i.IdProducto = p.IdProducto
                INNER JOIN Almacenes a ON i.IdAlmacen = a.IdAlmacen
                INNER JOIN Ubicaciones u ON i.IdUbicacion = u.IdUbicacion
                WHERE p.EstadoProducto = 'ACTIVO'
                  AND p.AfectaInventario = 1
                  AND i.StockActual > 0
                ORDER BY p.Nombre, a.Nombre, u.Nombre;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productos.Add(new VentaInventarioOptionViewModel
                    {
                        IdInventarioProducto = Convert.ToInt32(reader["IdInventarioProducto"]),
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        NombreProducto = reader["NombreProducto"].ToString() ?? "",
                        Almacen = reader["Almacen"].ToString() ?? "",
                        Ubicacion = reader["Ubicacion"].ToString() ?? "",
                        StockActual = Convert.ToInt32(reader["StockActual"]),
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        IVA = Convert.ToDecimal(reader["IVA"])
                    });
                }

                _connection.Close();
            }

            return productos;
        }

        private VentaInventarioOptionViewModel? ObtenerProductoInventario(int idInventarioProducto)
        {
            string query = @"
                SELECT
                    i.IdInventarioProducto,
                    p.IdProducto,
                    p.CodigoProducto,
                    p.Nombre AS NombreProducto,
                    a.Nombre AS Almacen,
                    u.Nombre AS Ubicacion,
                    i.StockActual,
                    p.PrecioVenta,
                    p.IVA
                FROM InventarioProductos i
                INNER JOIN Productos p ON i.IdProducto = p.IdProducto
                INNER JOIN Almacenes a ON i.IdAlmacen = a.IdAlmacen
                INNER JOIN Ubicaciones u ON i.IdUbicacion = u.IdUbicacion
                WHERE i.IdInventarioProducto = @IdInventarioProducto
                  AND p.EstadoProducto = 'ACTIVO'
                  AND p.AfectaInventario = 1;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdInventarioProducto", idInventarioProducto);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                VentaInventarioOptionViewModel? producto = null;
                if (reader.Read())
                {
                    producto = new VentaInventarioOptionViewModel
                    {
                        IdInventarioProducto = Convert.ToInt32(reader["IdInventarioProducto"]),
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        NombreProducto = reader["NombreProducto"].ToString() ?? "",
                        Almacen = reader["Almacen"].ToString() ?? "",
                        Ubicacion = reader["Ubicacion"].ToString() ?? "",
                        StockActual = Convert.ToInt32(reader["StockActual"]),
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        IVA = Convert.ToDecimal(reader["IVA"])
                    };
                }

                _connection.Close();
                return producto;
            }
        }

        private int? ObtenerPrimerEmpleado()
        {
            string query = @"
                SELECT TOP 1 IdEmpleado
                FROM Empleados
                ORDER BY CASE WHEN EstadoEmpleado = 'ACTIVO' THEN 0 ELSE 1 END, IdEmpleado;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                object? value = cmd.ExecuteScalar();
                _connection.Close();
                return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
            }
        }

        private int? ObtenerSucursalPorInventario(int idInventarioProducto)
        {
            string query = @"
                SELECT a.IdSucursal
                FROM InventarioProductos i
                INNER JOIN Almacenes a ON i.IdAlmacen = a.IdAlmacen
                WHERE i.IdInventarioProducto = @IdInventarioProducto;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdInventarioProducto", idInventarioProducto);

                _connection.Open();
                object? value = cmd.ExecuteScalar();
                _connection.Close();
                return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
            }
        }

        private static string GenerarNumeroDocumento(string prefijo)
        {
            return prefijo + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
