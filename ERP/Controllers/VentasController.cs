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

            SqlCommand cmd = new SqlCommand(@"
        SELECT v.IdVenta,
               c.Nombre AS Cliente,
               p.Nombre AS Producto,
               v.Cantidad,
               v.Total,
               v.Fecha
        FROM Ventas v
        INNER JOIN Clientes c ON v.IdCliente = c.IdCliente
        INNER JOIN Productos p ON v.IdProducto = p.IdProducto
    ", _connection);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Venta
                {
                    IdVenta = (int)reader["IdVenta"],
                    NombreCliente = reader["Cliente"].ToString(),
                    NombreProducto = reader["Producto"].ToString(),
                    Cantidad = (int)reader["Cantidad"],
                    Total = Convert.ToDecimal(reader["Total"]),
                    Fecha = (DateTime)reader["Fecha"]
                });
            }

            _connection.Close();

            return View(lista);
        }


        public IActionResult Crear()
        {
            List<Cliente> clientes = new List<Cliente>();
            List<Producto> productos = new List<Producto>();

            _connection.Open();

            SqlCommand cmdClientes = new SqlCommand("SELECT IdCliente, Nombre FROM Clientes", _connection);
            SqlDataReader readerClientes = cmdClientes.ExecuteReader();

            while (readerClientes.Read())
            {
                clientes.Add(new Cliente
                {
                    IdCliente = (int)readerClientes["IdCliente"],
                    Nombre = readerClientes["Nombre"].ToString()
                });
            }

            readerClientes.Close();

            SqlCommand cmdProductos = new SqlCommand("SELECT IdProducto, Nombre FROM Productos", _connection);
            SqlDataReader readerProductos = cmdProductos.ExecuteReader();

            while (readerProductos.Read())
            {
                productos.Add(new Producto
                {
                    IdProducto = (int)readerProductos["IdProducto"],
                    Nombre = readerProductos["Nombre"].ToString()
                });
            }

            _connection.Close();

            ViewBag.Clientes = clientes;
            ViewBag.Productos = productos;

            return View();
        }

        [HttpPost]
        public IActionResult Crear(Venta venta)
        {
            _connection.Open();

            decimal precio = 0;

            SqlCommand precioCmd = new SqlCommand(
                "SELECT Precio FROM Productos WHERE IdProducto=@id",
                _connection);

            precioCmd.Parameters.AddWithValue("@id", venta.IdProducto);

            SqlDataReader reader = precioCmd.ExecuteReader();

            if (reader.Read())
            {
                precio = Convert.ToDecimal(reader["Precio"]);
            }

            reader.Close();

            decimal total = precio * venta.Cantidad;

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Ventas (IdCliente,IdProducto,Cantidad,Fecha,Total) VALUES (@cliente,@producto,@cantidad,@fecha,@total)",
                _connection);

            cmd.Parameters.AddWithValue("@cliente", venta.IdCliente);
            cmd.Parameters.AddWithValue("@producto", venta.IdProducto);
            cmd.Parameters.AddWithValue("@cantidad", venta.Cantidad);
            cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@total", total);

            cmd.ExecuteNonQuery();

            SqlCommand stockCmd = new SqlCommand(
                "UPDATE Productos SET Stock = Stock - @cantidad WHERE IdProducto=@producto",
                _connection);

            stockCmd.Parameters.AddWithValue("@cantidad", venta.Cantidad);
            stockCmd.Parameters.AddWithValue("@producto", venta.IdProducto);

            stockCmd.ExecuteNonQuery();

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