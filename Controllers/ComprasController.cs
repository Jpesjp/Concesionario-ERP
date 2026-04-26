using ERPConcesionario.Helpers;
using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
{
    public class ComprasController : Controller
    {
        private readonly SqlConnection _connection;

        public ComprasController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;

            ViewBag.StockCritico = ObtenerStockCritico();
            return View(ObtenerOrdenesCompra());
        }

        public IActionResult ComprarStockCritico(int id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras", "Inventario");
            if (acceso != null) return acceso;

            var item = ObtenerStockCritico(id).FirstOrDefault();
            if (item == null)
                return NotFound();

            ViewBag.Proveedores = ObtenerProveedores();

            var model = new CompraStockCriticoFormViewModel
            {
                IdInventarioProducto = item.IdInventarioProducto,
                IdProducto = item.IdProducto,
                CodigoProducto = item.CodigoProducto,
                NombreProducto = item.NombreProducto,
                StockActual = item.StockActual,
                StockMinimo = item.StockMinimo,
                StockMaximo = item.StockMaximo,
                IdAlmacen = item.IdAlmacen,
                IdUbicacion = item.IdUbicacion,
                Almacen = item.Almacen,
                Ubicacion = item.Ubicacion,
                Cantidad = item.CantidadSugerida,
                CostoUnitario = item.CostoPromedio > 0 ? item.CostoPromedio : item.PrecioVenta
            };

            return View(model);
        }

        public IActionResult ComprarProducto(int? idProducto = null, int? idInventarioProducto = null)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras", "Inventario");
            if (acceso != null) return acceso;

            CargarCombosCompraLibre();

            var model = new CompraProductoFormViewModel();

            if (idInventarioProducto.HasValue)
            {
                var item = ObtenerStockCritico(idInventarioProducto.Value).FirstOrDefault();
                if (item != null)
                {
                    model.IdProducto = item.IdProducto;
                    model.IdAlmacen = item.IdAlmacen;
                    model.IdUbicacion = item.IdUbicacion;
                    model.Cantidad = item.CantidadSugerida;
                    model.CostoUnitario = item.CostoPromedio > 0 ? item.CostoPromedio : item.PrecioVenta;
                }
            }
            else if (idProducto.HasValue)
            {
                var producto = ObtenerProductoCompraInfo(idProducto.Value);
                if (producto != null)
                {
                    model.IdProducto = producto.IdProducto;
                    model.CostoUnitario = producto.CostoPromedio > 0 ? producto.CostoPromedio : producto.PrecioVenta;
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ComprarProducto(CompraProductoFormViewModel model)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras", "Inventario");
            if (acceso != null) return acceso;

            var producto = ObtenerProductoCompraInfo(model.IdProducto);
            if (producto == null)
                ModelState.AddModelError(nameof(model.IdProducto), "Producto no encontrado o inactivo.");
            else if (model.CostoUnitario <= 0)
                model.CostoUnitario = producto.CostoPromedio > 0 ? producto.CostoPromedio : producto.PrecioVenta;

            if (!UbicacionPerteneceAlmacen(model.IdUbicacion, model.IdAlmacen))
                ModelState.AddModelError(nameof(model.IdUbicacion), "La ubicacion seleccionada no pertenece al almacen.");

            int? idSucursal = ObtenerSucursalPorAlmacen(model.IdAlmacen);
            int? idEmpleado = ObtenerPrimerEmpleado();

            if (!idSucursal.HasValue || !idEmpleado.HasValue)
                ModelState.AddModelError("", "Debe existir al menos una sucursal y un empleado para registrar la compra.");

            if (!ModelState.IsValid || producto == null || !idSucursal.HasValue || !idEmpleado.HasValue)
            {
                CargarCombosCompraLibre();
                return View(model);
            }

            try
            {
                int idOrden = RegistrarCompraProductoLibre(model, producto, idSucursal.Value, idEmpleado.Value);
                TempData["Mensaje"] = "Compra registrada correctamente: #" + idOrden;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No fue posible registrar la compra: " + ex.Message);
                CargarCombosCompraLibre();
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult ComprarStockCritico(CompraStockCriticoFormViewModel model)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras", "Inventario");
            if (acceso != null) return acceso;

            var item = ObtenerStockCritico(model.IdInventarioProducto).FirstOrDefault();
            if (item == null)
            {
                TempData["MensajeError"] = "El producto ya no se encuentra en stock critico.";
                return RedirectToAction("StockCritico", "Inventario");
            }

            CompletarDatosFormularioCompra(model, item);

            if (!ModelState.IsValid)
            {
                ViewBag.Proveedores = ObtenerProveedores();
                return View(model);
            }

            int? idSucursal = ObtenerSucursalPorAlmacen(item.IdAlmacen);
            int? idEmpleado = ObtenerPrimerEmpleado();

            if (!idSucursal.HasValue || !idEmpleado.HasValue)
            {
                ModelState.AddModelError("", "Debe existir al menos una sucursal y un empleado para registrar la compra.");
                ViewBag.Proveedores = ObtenerProveedores();
                return View(model);
            }

            try
            {
                RegistrarCompraRecibida(item, model, idSucursal.Value, idEmpleado.Value);
                TempData["Mensaje"] = "Compra recibida y stock actualizado correctamente.";
                return RedirectToAction("StockCritico", "Inventario");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No fue posible registrar la compra: " + ex.Message);
                ViewBag.Proveedores = ObtenerProveedores();
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult GenerarBorradorStockCritico(int? id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras", "Inventario");
            if (acceso != null) return acceso;

            var items = ObtenerStockCritico(id);
            if (!items.Any())
            {
                TempData["MensajeError"] = "No hay alertas de stock critico para generar ordenes.";
                return RedirectToAction("Index");
            }

            int? idProveedor = ObtenerPrimerProveedor();
            int? idEmpleado = ObtenerPrimerEmpleado();
            int? idSucursal = ObtenerSucursalPorAlmacen(items.First().IdAlmacen);

            if (!idProveedor.HasValue || !idEmpleado.HasValue || !idSucursal.HasValue)
            {
                TempData["MensajeError"] = "Debe existir al menos un proveedor activo, una sucursal y un empleado para generar la orden.";
                return RedirectToAction("Index");
            }

            try
            {
                int idOrden = CrearOrdenCompraBorrador(items, idProveedor.Value, idSucursal.Value, idEmpleado.Value);
                TempData["Mensaje"] = "Borrador de orden de compra generado: #" + idOrden;
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "No fue posible generar el borrador: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private List<OrdenCompraListadoViewModel> ObtenerOrdenesCompra()
        {
            var ordenes = new List<OrdenCompraListadoViewModel>();

            string query = @"
                SELECT
                    oc.IdOrdenCompra,
                    oc.NumeroOrdenCompra,
                    p.RazonSocial AS Proveedor,
                    oc.FechaOrden,
                    oc.FechaEstimadaRecepcion,
                    oc.EstadoOrden,
                    oc.Total,
                    oc.Observaciones,
                    COUNT(od.IdOrdenCompraDetalle) AS TotalItems
                FROM OrdenesCompra oc
                INNER JOIN Proveedores p ON oc.IdProveedor = p.IdProveedor
                LEFT JOIN OrdenCompraDetalle od ON oc.IdOrdenCompra = od.IdOrdenCompra
                GROUP BY oc.IdOrdenCompra, oc.NumeroOrdenCompra, p.RazonSocial, oc.FechaOrden,
                         oc.FechaEstimadaRecepcion, oc.EstadoOrden, oc.Total, oc.Observaciones
                ORDER BY oc.FechaOrden DESC;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ordenes.Add(new OrdenCompraListadoViewModel
                    {
                        IdOrdenCompra = Convert.ToInt32(reader["IdOrdenCompra"]),
                        NumeroOrdenCompra = reader["NumeroOrdenCompra"].ToString() ?? "",
                        Proveedor = reader["Proveedor"].ToString() ?? "",
                        FechaOrden = Convert.ToDateTime(reader["FechaOrden"]),
                        FechaEstimadaRecepcion = reader["FechaEstimadaRecepcion"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaEstimadaRecepcion"]),
                        EstadoOrden = reader["EstadoOrden"].ToString() ?? "",
                        Total = Convert.ToDecimal(reader["Total"]),
                        Observaciones = reader["Observaciones"] as string,
                        TotalItems = Convert.ToInt32(reader["TotalItems"])
                    });
                }

                _connection.Close();
            }

            return ordenes;
        }

        private List<CompraCriticaViewModel> ObtenerStockCritico(int? idInventarioProducto = null)
        {
            var lista = new List<CompraCriticaViewModel>();

            string query = @"
                SELECT
                    i.IdInventarioProducto,
                    p.IdProducto,
                    p.CodigoProducto,
                    p.Nombre AS NombreProducto,
                    p.StockMinimo,
                    p.StockMaximo,
                    i.StockActual,
                    i.IdAlmacen,
                    a.Nombre AS Almacen,
                    i.IdUbicacion,
                    u.Nombre AS Ubicacion,
                    p.CostoPromedio,
                    p.PrecioVenta,
                    p.IVA
                FROM InventarioProductos i
                INNER JOIN Productos p ON i.IdProducto = p.IdProducto
                INNER JOIN Almacenes a ON i.IdAlmacen = a.IdAlmacen
                INNER JOIN Ubicaciones u ON i.IdUbicacion = u.IdUbicacion
                WHERE p.AfectaInventario = 1
                  AND i.StockActual <= p.StockMinimo";

            if (idInventarioProducto.HasValue)
                query += " AND i.IdInventarioProducto = @IdInventarioProducto";

            query += " ORDER BY i.StockActual ASC, p.Nombre ASC;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                if (idInventarioProducto.HasValue)
                    cmd.Parameters.AddWithValue("@IdInventarioProducto", idInventarioProducto.Value);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int stockActual = Convert.ToInt32(reader["StockActual"]);
                    int stockMinimo = Convert.ToInt32(reader["StockMinimo"]);
                    int? stockMaximo = reader["StockMaximo"] == DBNull.Value ? null : Convert.ToInt32(reader["StockMaximo"]);

                    lista.Add(new CompraCriticaViewModel
                    {
                        IdInventarioProducto = Convert.ToInt32(reader["IdInventarioProducto"]),
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        NombreProducto = reader["NombreProducto"].ToString() ?? "",
                        StockMinimo = stockMinimo,
                        StockMaximo = stockMaximo,
                        StockActual = stockActual,
                        IdAlmacen = Convert.ToInt32(reader["IdAlmacen"]),
                        Almacen = reader["Almacen"].ToString() ?? "",
                        IdUbicacion = Convert.ToInt32(reader["IdUbicacion"]),
                        Ubicacion = reader["Ubicacion"].ToString() ?? "",
                        CostoPromedio = Convert.ToDecimal(reader["CostoPromedio"]),
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        IVA = Convert.ToDecimal(reader["IVA"]),
                        CantidadSugerida = CalcularCantidadSugerida(stockActual, stockMinimo, stockMaximo)
                    });
                }

                _connection.Close();
            }

            return lista;
        }

        private void RegistrarCompraRecibida(CompraCriticaViewModel item, CompraStockCriticoFormViewModel model, int idSucursal, int idEmpleado)
        {
            string numeroOrden = GenerarNumeroDocumento("OC");
            string numeroRecepcion = GenerarNumeroDocumento("RC");
            decimal subtotal = model.Cantidad * model.CostoUnitario;
            decimal impuesto = Math.Round(subtotal * item.IVA / 100, 2);
            decimal total = subtotal + impuesto;
            decimal nuevoCostoPromedio = CalcularNuevoCostoPromedio(item.StockActual, item.CostoPromedio, model.Cantidad, model.CostoUnitario);

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                int idOrden;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO OrdenesCompra
                        (NumeroOrdenCompra, IdProveedor, IdSucursal, IdEmpleadoSolicita, FechaOrden,
                         FechaEstimadaRecepcion, EstadoOrden, Moneda, Subtotal, Descuento, Impuesto, Total, Observaciones)
                    OUTPUT INSERTED.IdOrdenCompra
                    VALUES
                        (@NumeroOrdenCompra, @IdProveedor, @IdSucursal, @IdEmpleado, GETDATE(),
                         @FechaEstimadaRecepcion, 'RECIBIDA', 'COP', @Subtotal, 0, @Impuesto, @Total, @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@NumeroOrdenCompra", numeroOrden);
                    cmd.Parameters.AddWithValue("@IdProveedor", model.IdProveedor);
                    cmd.Parameters.AddWithValue("@IdSucursal", idSucursal);
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.Parameters.AddWithValue("@FechaEstimadaRecepcion", DateTime.Today);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Impuesto", impuesto);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Observaciones", (object?)model.Observaciones ?? "Compra directa desde alerta de stock critico.");
                    idOrden = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int idDetalle;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO OrdenCompraDetalle
                        (IdOrdenCompra, IdProducto, IdVersionVehiculo, DescripcionItem, Cantidad, CostoUnitario, IVA, Descuento, Subtotal)
                    OUTPUT INSERTED.IdOrdenCompraDetalle
                    VALUES
                        (@IdOrdenCompra, @IdProducto, NULL, @DescripcionItem, @Cantidad, @CostoUnitario, @IVA, 0, @Subtotal);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdOrdenCompra", idOrden);
                    cmd.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                    cmd.Parameters.AddWithValue("@DescripcionItem", item.NombreProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                    cmd.Parameters.AddWithValue("@IVA", item.IVA);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    idDetalle = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int idRecepcion;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO RecepcionesCompra
                        (NumeroRecepcion, IdOrdenCompra, IdEmpleadoRecibe, FechaRecepcion, EstadoRecepcion, Observaciones)
                    OUTPUT INSERTED.IdRecepcionCompra
                    VALUES
                        (@NumeroRecepcion, @IdOrdenCompra, @IdEmpleado, GETDATE(), 'RECIBIDA', @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@NumeroRecepcion", numeroRecepcion);
                    cmd.Parameters.AddWithValue("@IdOrdenCompra", idOrden);
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.Parameters.AddWithValue("@Observaciones", "Recepcion automatica por compra de stock critico.");
                    idRecepcion = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO RecepcionCompraDetalle
                        (IdRecepcionCompra, IdOrdenCompraDetalle, IdProducto, IdUnidadVehiculo, CantidadRecibida, CostoUnitario, Observaciones)
                    VALUES
                        (@IdRecepcionCompra, @IdOrdenCompraDetalle, @IdProducto, NULL, @Cantidad, @CostoUnitario, @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdRecepcionCompra", idRecepcion);
                    cmd.Parameters.AddWithValue("@IdOrdenCompraDetalle", idDetalle);
                    cmd.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                    cmd.Parameters.AddWithValue("@Observaciones", "Producto recibido y aplicado al inventario.");
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Productos
                    SET CostoPromedio = @CostoPromedio
                    WHERE IdProducto = @IdProducto;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@CostoPromedio", nuevoCostoPromedio);
                    cmd.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE InventarioProductos
                    SET StockActual = StockActual + @Cantidad,
                        CostoUltimaCompra = @CostoUnitario,
                        FechaUltimoMovimiento = GETDATE()
                    WHERE IdInventarioProducto = @IdInventarioProducto;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                    cmd.Parameters.AddWithValue("@IdInventarioProducto", item.IdInventarioProducto);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO MovimientosInventario
                        (FechaMovimiento, TipoMovimiento, ModuloOrigen, IdProducto, Cantidad,
                         IdAlmacenDestino, IdUbicacionDestino, CostoUnitario, DocumentoReferencia,
                         Observaciones, IdEmpleado)
                    VALUES
                        (GETDATE(), 'ENTRADA', 'COMPRAS', @IdProducto, @Cantidad,
                         @IdAlmacen, @IdUbicacion, @CostoUnitario, @DocumentoReferencia,
                         @Observaciones, @IdEmpleado);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@IdAlmacen", item.IdAlmacen);
                    cmd.Parameters.AddWithValue("@IdUbicacion", item.IdUbicacion);
                    cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                    cmd.Parameters.AddWithValue("@DocumentoReferencia", numeroOrden);
                    cmd.Parameters.AddWithValue("@Observaciones", "Entrada por compra de stock critico.");
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
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

        private int RegistrarCompraProductoLibre(CompraProductoFormViewModel model, ProductoCompraInfo producto, int idSucursal, int idEmpleado)
        {
            string numeroOrden = GenerarNumeroDocumento("OC");
            string numeroRecepcion = GenerarNumeroDocumento("RC");
            decimal subtotal = model.Cantidad * model.CostoUnitario;
            decimal impuesto = Math.Round(subtotal * producto.IVA / 100, 2);
            decimal total = subtotal + impuesto;

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                int stockActual = 0;
                int? idInventarioProducto = null;

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT IdInventarioProducto, StockActual
                    FROM InventarioProductos WITH (UPDLOCK, HOLDLOCK)
                    WHERE IdProducto = @IdProducto
                      AND IdAlmacen = @IdAlmacen
                      AND IdUbicacion = @IdUbicacion;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    cmd.Parameters.AddWithValue("@IdAlmacen", model.IdAlmacen);
                    cmd.Parameters.AddWithValue("@IdUbicacion", model.IdUbicacion);

                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        idInventarioProducto = Convert.ToInt32(reader["IdInventarioProducto"]);
                        stockActual = Convert.ToInt32(reader["StockActual"]);
                    }
                }

                decimal nuevoCostoPromedio = CalcularNuevoCostoPromedio(stockActual, producto.CostoPromedio, model.Cantidad, model.CostoUnitario);

                int idOrden;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO OrdenesCompra
                        (NumeroOrdenCompra, IdProveedor, IdSucursal, IdEmpleadoSolicita, FechaOrden,
                         FechaEstimadaRecepcion, EstadoOrden, Moneda, Subtotal, Descuento, Impuesto, Total, Observaciones)
                    OUTPUT INSERTED.IdOrdenCompra
                    VALUES
                        (@NumeroOrdenCompra, @IdProveedor, @IdSucursal, @IdEmpleado, GETDATE(),
                         @FechaEstimadaRecepcion, 'RECIBIDA', 'COP', @Subtotal, 0, @Impuesto, @Total, @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@NumeroOrdenCompra", numeroOrden);
                    cmd.Parameters.AddWithValue("@IdProveedor", model.IdProveedor);
                    cmd.Parameters.AddWithValue("@IdSucursal", idSucursal);
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.Parameters.AddWithValue("@FechaEstimadaRecepcion", DateTime.Today);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Impuesto", impuesto);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Observaciones", (object?)model.Observaciones ?? "Compra libre de producto.");
                    idOrden = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int idDetalle;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO OrdenCompraDetalle
                        (IdOrdenCompra, IdProducto, IdVersionVehiculo, DescripcionItem, Cantidad, CostoUnitario, IVA, Descuento, Subtotal)
                    OUTPUT INSERTED.IdOrdenCompraDetalle
                    VALUES
                        (@IdOrdenCompra, @IdProducto, NULL, @DescripcionItem, @Cantidad, @CostoUnitario, @IVA, 0, @Subtotal);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdOrdenCompra", idOrden);
                    cmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    cmd.Parameters.AddWithValue("@DescripcionItem", producto.NombreProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                    cmd.Parameters.AddWithValue("@IVA", producto.IVA);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    idDetalle = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int idRecepcion;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO RecepcionesCompra
                        (NumeroRecepcion, IdOrdenCompra, IdEmpleadoRecibe, FechaRecepcion, EstadoRecepcion, Observaciones)
                    OUTPUT INSERTED.IdRecepcionCompra
                    VALUES
                        (@NumeroRecepcion, @IdOrdenCompra, @IdEmpleado, GETDATE(), 'RECIBIDA', @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@NumeroRecepcion", numeroRecepcion);
                    cmd.Parameters.AddWithValue("@IdOrdenCompra", idOrden);
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.Parameters.AddWithValue("@Observaciones", "Recepcion de compra libre.");
                    idRecepcion = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO RecepcionCompraDetalle
                        (IdRecepcionCompra, IdOrdenCompraDetalle, IdProducto, IdUnidadVehiculo, CantidadRecibida, CostoUnitario, Observaciones)
                    VALUES
                        (@IdRecepcionCompra, @IdOrdenCompraDetalle, @IdProducto, NULL, @Cantidad, @CostoUnitario, @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdRecepcionCompra", idRecepcion);
                    cmd.Parameters.AddWithValue("@IdOrdenCompraDetalle", idDetalle);
                    cmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                    cmd.Parameters.AddWithValue("@Observaciones", "Producto recibido y aplicado al inventario.");
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Productos
                    SET CostoPromedio = @CostoPromedio
                    WHERE IdProducto = @IdProducto;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@CostoPromedio", nuevoCostoPromedio);
                    cmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    cmd.ExecuteNonQuery();
                }

                if (idInventarioProducto.HasValue)
                {
                    using (SqlCommand cmd = new SqlCommand(@"
                        UPDATE InventarioProductos
                        SET StockActual = StockActual + @Cantidad,
                            CostoUltimaCompra = @CostoUnitario,
                            FechaUltimoMovimiento = GETDATE()
                        WHERE IdInventarioProducto = @IdInventarioProducto;",
                        _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                        cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                        cmd.Parameters.AddWithValue("@IdInventarioProducto", idInventarioProducto.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO InventarioProductos
                            (IdProducto, IdAlmacen, IdUbicacion, StockActual, StockReservado, CostoUltimaCompra, FechaUltimoMovimiento)
                        VALUES
                            (@IdProducto, @IdAlmacen, @IdUbicacion, @Cantidad, 0, @CostoUnitario, GETDATE());",
                        _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                        cmd.Parameters.AddWithValue("@IdAlmacen", model.IdAlmacen);
                        cmd.Parameters.AddWithValue("@IdUbicacion", model.IdUbicacion);
                        cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                        cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO MovimientosInventario
                        (FechaMovimiento, TipoMovimiento, ModuloOrigen, IdProducto, Cantidad,
                         IdAlmacenDestino, IdUbicacionDestino, CostoUnitario, DocumentoReferencia,
                         Observaciones, IdEmpleado)
                    VALUES
                        (GETDATE(), 'ENTRADA', 'COMPRAS', @IdProducto, @Cantidad,
                         @IdAlmacen, @IdUbicacion, @CostoUnitario, @DocumentoReferencia,
                         @Observaciones, @IdEmpleado);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdProducto", model.IdProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@IdAlmacen", model.IdAlmacen);
                    cmd.Parameters.AddWithValue("@IdUbicacion", model.IdUbicacion);
                    cmd.Parameters.AddWithValue("@CostoUnitario", model.CostoUnitario);
                    cmd.Parameters.AddWithValue("@DocumentoReferencia", numeroOrden);
                    cmd.Parameters.AddWithValue("@Observaciones", "Entrada por compra libre de producto.");
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return idOrden;
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

        private int CrearOrdenCompraBorrador(List<CompraCriticaViewModel> items, int idProveedor, int idSucursal, int idEmpleado)
        {
            string numeroOrden = GenerarNumeroDocumento("OC");
            decimal subtotal = items.Sum(i => i.CantidadSugerida * ObtenerCostoCompra(i));
            decimal impuesto = items.Sum(i => Math.Round(i.CantidadSugerida * ObtenerCostoCompra(i) * i.IVA / 100, 2));
            decimal total = subtotal + impuesto;

            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                int idOrden;
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO OrdenesCompra
                        (NumeroOrdenCompra, IdProveedor, IdSucursal, IdEmpleadoSolicita, FechaOrden,
                         FechaEstimadaRecepcion, EstadoOrden, Moneda, Subtotal, Descuento, Impuesto, Total, Observaciones)
                    OUTPUT INSERTED.IdOrdenCompra
                    VALUES
                        (@NumeroOrdenCompra, @IdProveedor, @IdSucursal, @IdEmpleado, GETDATE(),
                         @FechaEstimadaRecepcion, 'BORRADOR', 'COP', @Subtotal, 0, @Impuesto, @Total, @Observaciones);",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@NumeroOrdenCompra", numeroOrden);
                    cmd.Parameters.AddWithValue("@IdProveedor", idProveedor);
                    cmd.Parameters.AddWithValue("@IdSucursal", idSucursal);
                    cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                    cmd.Parameters.AddWithValue("@FechaEstimadaRecepcion", DateTime.Today.AddDays(7));
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Impuesto", impuesto);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Observaciones", "Borrador generado automaticamente por alerta de stock critico.");
                    idOrden = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (var item in items)
                {
                    decimal costo = ObtenerCostoCompra(item);
                    decimal subtotalLinea = item.CantidadSugerida * costo;

                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO OrdenCompraDetalle
                            (IdOrdenCompra, IdProducto, IdVersionVehiculo, DescripcionItem, Cantidad, CostoUnitario, IVA, Descuento, Subtotal)
                        VALUES
                            (@IdOrdenCompra, @IdProducto, NULL, @DescripcionItem, @Cantidad, @CostoUnitario, @IVA, 0, @Subtotal);",
                        _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@IdOrdenCompra", idOrden);
                        cmd.Parameters.AddWithValue("@IdProducto", item.IdProducto);
                        cmd.Parameters.AddWithValue("@DescripcionItem", item.NombreProducto);
                        cmd.Parameters.AddWithValue("@Cantidad", item.CantidadSugerida);
                        cmd.Parameters.AddWithValue("@CostoUnitario", costo);
                        cmd.Parameters.AddWithValue("@IVA", item.IVA);
                        cmd.Parameters.AddWithValue("@Subtotal", subtotalLinea);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return idOrden;
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

        private List<SelectListItem> ObtenerProveedores()
        {
            var proveedores = new List<SelectListItem>();

            string query = @"
                SELECT IdProveedor, RazonSocial, TipoProveedor
                FROM Proveedores
                WHERE EstadoProveedor = 'ACTIVO'
                ORDER BY
                    CASE WHEN TipoProveedor IN ('REPUESTOS', 'MIXTO') THEN 0 ELSE 1 END,
                    RazonSocial;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    proveedores.Add(new SelectListItem
                    {
                        Value = reader["IdProveedor"].ToString(),
                        Text = (reader["RazonSocial"].ToString() ?? "") + " (" + (reader["TipoProveedor"].ToString() ?? "") + ")"
                    });
                }

                _connection.Close();
            }

            return proveedores;
        }

        private List<SelectListItem> ObtenerProductosCompra()
        {
            var productos = new List<SelectListItem>();

            string query = @"
                SELECT IdProducto, CodigoProducto, Nombre, CostoPromedio, PrecioVenta
                FROM Productos
                WHERE EstadoProducto = 'ACTIVO'
                  AND AfectaInventario = 1
                ORDER BY Nombre;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    decimal costo = Convert.ToDecimal(reader["CostoPromedio"]);
                    decimal precio = Convert.ToDecimal(reader["PrecioVenta"]);
                    decimal sugerido = costo > 0 ? costo : precio;

                    productos.Add(new SelectListItem
                    {
                        Value = reader["IdProducto"].ToString(),
                        Text = (reader["CodigoProducto"].ToString() ?? "") + " - " + (reader["Nombre"].ToString() ?? "") + " | Costo sugerido: " + sugerido.ToString("N0")
                    });
                }

                _connection.Close();
            }

            return productos;
        }

        private List<SelectListItem> ObtenerAlmacenes()
        {
            var almacenes = new List<SelectListItem>();
            string query = "SELECT IdAlmacen, Nombre FROM Almacenes WHERE Activo = 1 ORDER BY Nombre;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    almacenes.Add(new SelectListItem
                    {
                        Value = reader["IdAlmacen"].ToString(),
                        Text = reader["Nombre"].ToString()
                    });
                }

                _connection.Close();
            }

            return almacenes;
        }

        private List<SelectListItem> ObtenerUbicaciones()
        {
            var ubicaciones = new List<SelectListItem>();

            string query = @"
                SELECT u.IdUbicacion, u.Nombre AS Ubicacion, a.Nombre AS Almacen
                FROM Ubicaciones u
                INNER JOIN Almacenes a ON u.IdAlmacen = a.IdAlmacen
                WHERE u.Activa = 1
                ORDER BY a.Nombre, u.Nombre;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ubicaciones.Add(new SelectListItem
                    {
                        Value = reader["IdUbicacion"].ToString(),
                        Text = (reader["Almacen"].ToString() ?? "") + " / " + (reader["Ubicacion"].ToString() ?? "")
                    });
                }

                _connection.Close();
            }

            return ubicaciones;
        }

        private int? ObtenerPrimerProveedor()
        {
            string query = @"
                SELECT TOP 1 IdProveedor
                FROM Proveedores
                WHERE EstadoProveedor = 'ACTIVO'
                ORDER BY CASE WHEN TipoProveedor IN ('REPUESTOS', 'MIXTO') THEN 0 ELSE 1 END, RazonSocial;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                object? value = cmd.ExecuteScalar();
                _connection.Close();
                return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
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

        private ProductoCompraInfo? ObtenerProductoCompraInfo(int idProducto)
        {
            string query = @"
                SELECT IdProducto, CodigoProducto, Nombre, CostoPromedio, PrecioVenta, IVA
                FROM Productos
                WHERE IdProducto = @IdProducto
                  AND EstadoProducto = 'ACTIVO'
                  AND AfectaInventario = 1;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                ProductoCompraInfo? producto = null;
                if (reader.Read())
                {
                    producto = new ProductoCompraInfo
                    {
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        NombreProducto = reader["Nombre"].ToString() ?? "",
                        CostoPromedio = Convert.ToDecimal(reader["CostoPromedio"]),
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        IVA = Convert.ToDecimal(reader["IVA"])
                    };
                }

                _connection.Close();
                return producto;
            }
        }

        private int? ObtenerSucursalPorAlmacen(int idAlmacen)
        {
            string query = "SELECT IdSucursal FROM Almacenes WHERE IdAlmacen = @IdAlmacen;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdAlmacen", idAlmacen);

                _connection.Open();
                object? value = cmd.ExecuteScalar();
                _connection.Close();
                return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
            }
        }

        private bool UbicacionPerteneceAlmacen(int idUbicacion, int idAlmacen)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Ubicaciones
                WHERE IdUbicacion = @IdUbicacion
                  AND IdAlmacen = @IdAlmacen
                  AND Activa = 1;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdUbicacion", idUbicacion);
                cmd.Parameters.AddWithValue("@IdAlmacen", idAlmacen);

                _connection.Open();
                int total = Convert.ToInt32(cmd.ExecuteScalar());
                _connection.Close();
                return total > 0;
            }
        }

        private void CargarCombosCompraLibre()
        {
            ViewBag.Productos = ObtenerProductosCompra();
            ViewBag.Proveedores = ObtenerProveedores();
            ViewBag.Almacenes = ObtenerAlmacenes();
            ViewBag.Ubicaciones = ObtenerUbicaciones();
        }

        private void CompletarDatosFormularioCompra(CompraStockCriticoFormViewModel model, CompraCriticaViewModel item)
        {
            model.IdProducto = item.IdProducto;
            model.CodigoProducto = item.CodigoProducto;
            model.NombreProducto = item.NombreProducto;
            model.StockActual = item.StockActual;
            model.StockMinimo = item.StockMinimo;
            model.StockMaximo = item.StockMaximo;
            model.IdAlmacen = item.IdAlmacen;
            model.IdUbicacion = item.IdUbicacion;
            model.Almacen = item.Almacen;
            model.Ubicacion = item.Ubicacion;
        }

        private static int CalcularCantidadSugerida(int stockActual, int stockMinimo, int? stockMaximo)
        {
            int objetivo = stockMaximo.HasValue && stockMaximo.Value > stockMinimo
                ? stockMaximo.Value
                : Math.Max(stockMinimo * 2, stockMinimo + 1);

            return Math.Max(1, objetivo - stockActual);
        }

        private static decimal ObtenerCostoCompra(CompraCriticaViewModel item)
        {
            return item.CostoPromedio > 0 ? item.CostoPromedio : item.PrecioVenta;
        }

        private static decimal CalcularNuevoCostoPromedio(int stockActual, decimal costoActual, int cantidadCompra, decimal costoCompra)
        {
            int nuevoStock = stockActual + cantidadCompra;
            if (nuevoStock <= 0)
                return costoCompra;

            return Math.Round(((stockActual * costoActual) + (cantidadCompra * costoCompra)) / nuevoStock, 2);
        }

        private static string GenerarNumeroDocumento(string prefijo)
        {
            return prefijo + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private class ProductoCompraInfo
        {
            public int IdProducto { get; set; }
            public string CodigoProducto { get; set; } = string.Empty;
            public string NombreProducto { get; set; } = string.Empty;
            public decimal CostoPromedio { get; set; }
            public decimal PrecioVenta { get; set; }
            public decimal IVA { get; set; }
        }
    }
}
