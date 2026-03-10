using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
{
    public class ClientesController : Controller
    {
        private readonly SqlConnection _connection;

        public ClientesController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            List<Cliente> lista = new List<Cliente>();

            _connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Clientes", _connection);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    IdCliente = (int)reader["IdCliente"],
                    Nombre = reader["Nombre"].ToString(),
                    Telefono = reader["Telefono"].ToString(),
                    Email = reader["Email"].ToString(),
                    Direccion = reader["Direccion"].ToString()
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
        public IActionResult Crear(Cliente cliente)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Clientes (Nombre,Telefono,Email,Direccion) VALUES (@Nombre,@Telefono,@Email,@Direccion)",
                _connection);

            cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
            cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
            cmd.Parameters.AddWithValue("@Email", cliente.Email);
            cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Eliminar(int id)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand("DELETE FROM Clientes WHERE IdCliente=@Id", _connection);
            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            Cliente cliente = new Cliente();

            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "SELECT * FROM Clientes WHERE IdCliente=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Id", id);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                cliente.IdCliente = (int)reader["IdCliente"];
                cliente.Nombre = reader["Nombre"].ToString();
                cliente.Telefono = reader["Telefono"].ToString();
                cliente.Email = reader["Email"].ToString();
                cliente.Direccion = reader["Direccion"].ToString();
            }

            _connection.Close();

            return View(cliente);
        }

        [HttpPost]
        public IActionResult Editar(Cliente cliente)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "UPDATE Clientes SET Nombre=@Nombre, Telefono=@Telefono, Email=@Email, Direccion=@Direccion WHERE IdCliente=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
            cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
            cmd.Parameters.AddWithValue("@Email", cliente.Email);
            cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion);
            cmd.Parameters.AddWithValue("@Id", cliente.IdCliente);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }
    }
}