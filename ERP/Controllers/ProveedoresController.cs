using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly SqlConnection _connection;

        public ProveedoresController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            List<Proveedor> lista = new List<Proveedor>();

            _connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Proveedores", _connection);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Proveedor
                {
                    IdProveedor = (int)reader["IdProveedor"],
                    Nombre = reader["Nombre"].ToString(),
                    Telefono = reader["Telefono"].ToString(),
                    Email = reader["Email"].ToString()
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
        public IActionResult Crear(Proveedor proveedor)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Proveedores (Nombre,Telefono,Email) VALUES (@Nombre,@Telefono,@Email)",
                _connection);

            cmd.Parameters.AddWithValue("@Nombre", proveedor.Nombre);
            cmd.Parameters.AddWithValue("@Telefono", proveedor.Telefono);
            cmd.Parameters.AddWithValue("@Email", proveedor.Email);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Eliminar(int id)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "DELETE FROM Proveedores WHERE IdProveedor=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            Proveedor proveedor = new Proveedor();

            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "SELECT * FROM Proveedores WHERE IdProveedor=@id",
                _connection);

            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                proveedor.IdProveedor = (int)reader["IdProveedor"];
                proveedor.Nombre = reader["Nombre"].ToString();
                proveedor.Telefono = reader["Telefono"].ToString();
                proveedor.Email = reader["Email"].ToString();
            }

            _connection.Close();

            return View(proveedor);
        }

        [HttpPost]
        public IActionResult Editar(Proveedor proveedor)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                @"UPDATE Proveedores 
          SET Nombre=@Nombre,
              Telefono=@Telefono,
              Email=@Email
          WHERE IdProveedor=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Nombre", proveedor.Nombre);
            cmd.Parameters.AddWithValue("@Telefono", proveedor.Telefono);
            cmd.Parameters.AddWithValue("@Email", proveedor.Email);
            cmd.Parameters.AddWithValue("@Id", proveedor.IdProveedor);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }



    }
}