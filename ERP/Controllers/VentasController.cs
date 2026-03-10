using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
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
            List<Venta> lista = new List<Venta>();

            _connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Ventas", _connection);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Venta
                {
                    IdVenta = (int)reader["IdVenta"],
                    IdCliente = (int)reader["IdCliente"],
                    IdProducto = (int)reader["IdProducto"],
                    Cantidad = (int)reader["Cantidad"],
                    Fecha = (DateTime)reader["Fecha"],
                    Total = reader["Total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Total"])
                });
            }

            _connection.Close();

            return View(lista);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Venta venta)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Ventas (IdCliente,IdProducto,Cantidad,Fecha) VALUES (@Cliente,@Producto,@Cantidad,@Fecha)",
                _connection);

            cmd.Parameters.AddWithValue("@Cliente", venta.IdCliente);
            cmd.Parameters.AddWithValue("@Producto", venta.IdProducto);
            cmd.Parameters.AddWithValue("@Cantidad", venta.Cantidad);
            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Eliminar(int id)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "DELETE FROM Ventas WHERE IdVenta=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            Venta venta = new Venta();

            _connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Ventas WHERE IdVenta=@id", _connection);
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                venta.IdVenta = (int)reader["IdVenta"];
                venta.IdCliente = (int)reader["IdCliente"];
                venta.IdProducto = (int)reader["IdProducto"];
                venta.Cantidad = (int)reader["Cantidad"];
                venta.Total = reader["Total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Total"]);
            }

            _connection.Close();

            return View(venta);
        }

        [HttpPost]
        public IActionResult Editar(Venta venta)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "UPDATE Ventas SET IdCliente=@cliente, IdProducto=@producto, Cantidad=@cantidad, Total=@total WHERE IdVenta=@id",
                _connection);

            cmd.Parameters.AddWithValue("@cliente", venta.IdCliente);
            cmd.Parameters.AddWithValue("@producto", venta.IdProducto);
            cmd.Parameters.AddWithValue("@cantidad", venta.Cantidad);
            cmd.Parameters.AddWithValue("@total", venta.Total);
            cmd.Parameters.AddWithValue("@id", venta.IdVenta);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }
    }
}