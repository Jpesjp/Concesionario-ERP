using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERPConcesionario.Helpers;

namespace ERPConcesionario.Controllers
{
    public class InventarioController : Controller
    {
        private readonly SqlConnection _connection;

        public InventarioController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            return View();
        }

        public IActionResult StockCritico()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Inventario");
            if (acceso != null) return acceso;
            var lista = new List<StockCriticoViewModel>();

            string query = @"
                SELECT 
                    i.IdInventarioProducto,
                    p.IdProducto,
                    p.CodigoProducto,
                    p.Nombre AS NombreProducto,
                    p.StockMinimo,
                    p.StockMaximo,
                    i.StockActual,
                    a.Nombre AS Almacen,
                    u.Nombre AS Ubicacion,
                    p.PrecioVenta
                FROM InventarioProductos i
                INNER JOIN Productos p ON i.IdProducto = p.IdProducto
                INNER JOIN Almacenes a ON i.IdAlmacen = a.IdAlmacen
                INNER JOIN Ubicaciones u ON i.IdUbicacion = u.IdUbicacion
                WHERE i.StockActual <= p.StockMinimo
                ORDER BY i.StockActual ASC, p.Nombre ASC;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new StockCriticoViewModel
                    {
                        IdInventarioProducto = Convert.ToInt32(reader["IdInventarioProducto"]),
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        NombreProducto = reader["NombreProducto"].ToString() ?? "",
                        StockMinimo = Convert.ToInt32(reader["StockMinimo"]),
                        StockMaximo = reader["StockMaximo"] == DBNull.Value ? null : Convert.ToInt32(reader["StockMaximo"]),
                        StockActual = Convert.ToInt32(reader["StockActual"]),
                        Almacen = reader["Almacen"].ToString() ?? "",
                        Ubicacion = reader["Ubicacion"].ToString() ?? "",
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"]),
                        CantidadSugerida = CalcularCantidadSugerida(
                            Convert.ToInt32(reader["StockActual"]),
                            Convert.ToInt32(reader["StockMinimo"]),
                            reader["StockMaximo"] == DBNull.Value ? null : Convert.ToInt32(reader["StockMaximo"]))
                    });
                }

                _connection.Close();
            }

            return View(lista);
        }

        private static int CalcularCantidadSugerida(int stockActual, int stockMinimo, int? stockMaximo)
        {
            int objetivo = stockMaximo.HasValue && stockMaximo.Value > stockMinimo
                ? stockMaximo.Value
                : Math.Max(stockMinimo * 2, stockMinimo + 1);

            return Math.Max(1, objetivo - stockActual);
        }
    }
}
