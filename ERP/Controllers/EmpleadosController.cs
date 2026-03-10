using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly SqlConnection _connection;

        public EmpleadosController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            List<Empleado> lista = new List<Empleado>();

            _connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT * FROM Empleados", _connection);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Empleado
                {
                    IdEmpleado = (int)reader["IdEmpleado"],
                    Nombre = reader["Nombre"].ToString(),
                    Cargo = reader["Cargo"].ToString(),
                    Salario = (decimal)reader["Salario"]
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
        public IActionResult Crear(Empleado empleado)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Empleados (Nombre,Cargo,Salario) VALUES (@Nombre,@Cargo,@Salario)",
                _connection);

            cmd.Parameters.AddWithValue("@Nombre", empleado.Nombre);
            cmd.Parameters.AddWithValue("@Cargo", empleado.Cargo);
            cmd.Parameters.AddWithValue("@Salario", empleado.Salario);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            Empleado empleado = new Empleado();

            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "SELECT * FROM Empleados WHERE IdEmpleado=@id",
                _connection);

            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                empleado.IdEmpleado = (int)reader["IdEmpleado"];
                empleado.Nombre = reader["Nombre"].ToString();
                empleado.Cargo = reader["Cargo"].ToString();
                empleado.Salario = (decimal)reader["Salario"];
            }

            _connection.Close();

            return View(empleado);
        }

        [HttpPost]
        public IActionResult Editar(Empleado empleado)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                @"UPDATE Empleados
          SET Nombre=@Nombre,
              Cargo=@Cargo,
              Salario=@Salario
          WHERE IdEmpleado=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Nombre", empleado.Nombre);
            cmd.Parameters.AddWithValue("@Cargo", empleado.Cargo);
            cmd.Parameters.AddWithValue("@Salario", empleado.Salario);
            cmd.Parameters.AddWithValue("@Id", empleado.IdEmpleado);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }
        public IActionResult Eliminar(int id)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "DELETE FROM Empleados WHERE IdEmpleado=@id",
                _connection);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

    }
}