using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERPConcesionario.Helpers;

namespace ERPConcesionario.Controllers
{
    public class ProductosController : Controller
    {
        private readonly SqlConnection _connection;

        public ProductosController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            var productos = new List<Producto>();

            string query = @"SELECT IdProducto, IdCategoriaProducto, CodigoProducto, CodigoBarras, Nombre,
                                    Descripcion, Marca, UnidadMedida, TipoProducto, AfectaInventario,
                                    StockMinimo, StockMaximo, CostoPromedio, PrecioVenta, IVA,
                                    EstadoProducto, FechaCreacion
                             FROM Productos";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    productos.Add(new Producto
                    {
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        IdCategoriaProducto = Convert.ToInt32(reader["IdCategoriaProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        CodigoBarras = reader["CodigoBarras"] as string,
                        Nombre = reader["Nombre"].ToString() ?? "",
                        Descripcion = reader["Descripcion"] as string,
                        Marca = reader["Marca"] as string,
                        UnidadMedida = reader["UnidadMedida"].ToString() ?? "UND",
                        TipoProducto = reader["TipoProducto"].ToString() ?? "",
                        AfectaInventario = Convert.ToBoolean(reader["AfectaInventario"]),
                        StockMinimo = Convert.ToInt32(reader["StockMinimo"]),
                        StockMaximo = reader["StockMaximo"] == DBNull.Value ? null : Convert.ToInt32(reader["StockMaximo"]),
                        CostoPromedio = Convert.ToDecimal(reader["CostoPromedio"]),
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        IVA = Convert.ToDecimal(reader["IVA"]),
                        EstadoProducto = reader["EstadoProducto"].ToString() ?? "ACTIVO",
                        FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                    });
                }

                _connection.Close();
            }

            return View(productos);
        }

        public IActionResult Create()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
                return View(producto);

            string query = @"INSERT INTO Productos
                            (IdCategoriaProducto, CodigoProducto, CodigoBarras, Nombre, Descripcion, Marca,
                             UnidadMedida, TipoProducto, AfectaInventario, StockMinimo, StockMaximo,
                             CostoPromedio, PrecioVenta, IVA, EstadoProducto, FechaCreacion)
                             VALUES
                            (@IdCategoriaProducto, @CodigoProducto, @CodigoBarras, @Nombre, @Descripcion, @Marca,
                             @UnidadMedida, @TipoProducto, @AfectaInventario, @StockMinimo, @StockMaximo,
                             @CostoPromedio, @PrecioVenta, @IVA, @EstadoProducto, @FechaCreacion)";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdCategoriaProducto", producto.IdCategoriaProducto);
                cmd.Parameters.AddWithValue("@CodigoProducto", producto.CodigoProducto);
                cmd.Parameters.AddWithValue("@CodigoBarras", (object?)producto.CodigoBarras ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Marca", (object?)producto.Marca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UnidadMedida", producto.UnidadMedida);
                cmd.Parameters.AddWithValue("@TipoProducto", producto.TipoProducto);
                cmd.Parameters.AddWithValue("@AfectaInventario", producto.AfectaInventario);
                cmd.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                cmd.Parameters.AddWithValue("@StockMaximo", (object?)producto.StockMaximo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CostoPromedio", producto.CostoPromedio);
                cmd.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta);
                cmd.Parameters.AddWithValue("@IVA", producto.IVA);
                cmd.Parameters.AddWithValue("@EstadoProducto", producto.EstadoProducto);
                cmd.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            Producto? producto = null;

            string query = @"SELECT * FROM Productos WHERE IdProducto = @IdProducto";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProducto", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    producto = new Producto
                    {
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        IdCategoriaProducto = Convert.ToInt32(reader["IdCategoriaProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        CodigoBarras = reader["CodigoBarras"] as string,
                        Nombre = reader["Nombre"].ToString() ?? "",
                        Descripcion = reader["Descripcion"] as string,
                        Marca = reader["Marca"] as string,
                        UnidadMedida = reader["UnidadMedida"].ToString() ?? "UND",
                        TipoProducto = reader["TipoProducto"].ToString() ?? "",
                        AfectaInventario = Convert.ToBoolean(reader["AfectaInventario"]),
                        StockMinimo = Convert.ToInt32(reader["StockMinimo"]),
                        StockMaximo = reader["StockMaximo"] == DBNull.Value ? null : Convert.ToInt32(reader["StockMaximo"]),
                        CostoPromedio = Convert.ToDecimal(reader["CostoPromedio"]),
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        IVA = Convert.ToDecimal(reader["IVA"]),
                        EstadoProducto = reader["EstadoProducto"].ToString() ?? "ACTIVO",
                        FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                    };
                }

                _connection.Close();
            }

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        [HttpPost]
        public IActionResult Edit(Producto producto)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            if (!ModelState.IsValid)
                return View(producto);

            string query = @"UPDATE Productos SET
                                IdCategoriaProducto = @IdCategoriaProducto,
                                CodigoProducto = @CodigoProducto,
                                CodigoBarras = @CodigoBarras,
                                Nombre = @Nombre,
                                Descripcion = @Descripcion,
                                Marca = @Marca,
                                UnidadMedida = @UnidadMedida,
                                TipoProducto = @TipoProducto,
                                AfectaInventario = @AfectaInventario,
                                StockMinimo = @StockMinimo,
                                StockMaximo = @StockMaximo,
                                CostoPromedio = @CostoPromedio,
                                PrecioVenta = @PrecioVenta,
                                IVA = @IVA,
                                EstadoProducto = @EstadoProducto
                             WHERE IdProducto = @IdProducto";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                cmd.Parameters.AddWithValue("@IdCategoriaProducto", producto.IdCategoriaProducto);
                cmd.Parameters.AddWithValue("@CodigoProducto", producto.CodigoProducto);
                cmd.Parameters.AddWithValue("@CodigoBarras", (object?)producto.CodigoBarras ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Marca", (object?)producto.Marca ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UnidadMedida", producto.UnidadMedida);
                cmd.Parameters.AddWithValue("@TipoProducto", producto.TipoProducto);
                cmd.Parameters.AddWithValue("@AfectaInventario", producto.AfectaInventario);
                cmd.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                cmd.Parameters.AddWithValue("@StockMaximo", (object?)producto.StockMaximo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CostoPromedio", producto.CostoPromedio);
                cmd.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta);
                cmd.Parameters.AddWithValue("@IVA", producto.IVA);
                cmd.Parameters.AddWithValue("@EstadoProducto", producto.EstadoProducto);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            Producto? producto = null;

            string query = @"SELECT * FROM Productos WHERE IdProducto = @IdProducto";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProducto", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    producto = new Producto
                    {
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        Nombre = reader["Nombre"].ToString() ?? "",
                        Marca = reader["Marca"] as string,
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        EstadoProducto = reader["EstadoProducto"].ToString() ?? "ACTIVO"
                    };
                }

                _connection.Close();
            }

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            string query = @"DELETE FROM Productos WHERE IdProducto = @IdProducto";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProducto", id);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }
    }
}