using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
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
            List<Producto> lista = new List<Producto>();

            _connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Productos", _connection);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Producto
                {
                    IdProducto = (int)reader["IdProducto"],
                    Nombre = reader["Nombre"].ToString(),
                    Precio = (decimal)reader["Precio"],
                    Stock = (int)reader["Stock"]
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
        public IActionResult Crear(Producto producto)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Productos (Nombre,Precio,Stock) VALUES (@Nombre,@Precio,@Stock)",
                _connection);

            cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("@Precio", producto.Precio);
            cmd.Parameters.AddWithValue("@Stock", producto.Stock);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Eliminar(int id)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "DELETE FROM Productos WHERE IdProducto=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            Producto producto = new Producto();

            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "SELECT * FROM Productos WHERE IdProducto=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Id", id);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                producto.IdProducto = (int)reader["IdProducto"];
                producto.Nombre = reader["Nombre"].ToString();
                producto.Precio = (decimal)reader["Precio"];
                producto.Stock = (int)reader["Stock"];
            }

            _connection.Close();

            return View(producto);
        }

        [HttpPost]
        public IActionResult Editar(Producto producto)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "UPDATE Productos SET Nombre=@Nombre, Precio=@Precio, Stock=@Stock WHERE IdProducto=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Id", producto.IdProducto);
            cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
            cmd.Parameters.AddWithValue("@Precio", producto.Precio);
            cmd.Parameters.AddWithValue("@Stock", producto.Stock);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }
    }
}
