using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
            return View();
        }

        public IActionResult StockCritico()
        {
            var lista = new List<StockCriticoViewModel>();

            string query = @"
                SELECT 
                    p.IdProducto,
                    p.CodigoProducto,
                    p.Nombre AS NombreProducto,
                    p.StockMinimo,
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
                        IdProducto = Convert.ToInt32(reader["IdProducto"]),
                        CodigoProducto = reader["CodigoProducto"].ToString() ?? "",
                        NombreProducto = reader["NombreProducto"].ToString() ?? "",
                        StockMinimo = Convert.ToInt32(reader["StockMinimo"]),
                        StockActual = Convert.ToInt32(reader["StockActual"]),
                        Almacen = reader["Almacen"].ToString() ?? "",
                        Ubicacion = reader["Ubicacion"].ToString() ?? "",
                        PrecioVenta = Convert.ToDecimal(reader["PrecioVenta"])
                    });
                }

                _connection.Close();
            }

            return View(lista);
        }
    }
}