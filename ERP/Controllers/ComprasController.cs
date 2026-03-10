using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERP.Models;

namespace ERP.Controllers
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
            List<Compra> lista = new List<Compra>();

            _connection.Open();

            SqlCommand cmd = new SqlCommand(@"
                SELECT c.IdCompra,
                       p.Nombre as Proveedor,
                       c.Fecha,
                       c.Total
                FROM Compras c
                INNER JOIN Proveedores p
                ON c.IdProveedor = p.IdProveedor
            ", _connection);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Compra
                {
                    IdCompra = (int)reader["IdCompra"],
                    NombreProveedor = reader["Proveedor"].ToString(),
                    Fecha = (DateTime)reader["Fecha"],
                    Total = (decimal)reader["Total"]
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
        public IActionResult Crear(Compra compra)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                @"INSERT INTO Compras (IdProveedor,Fecha,Total)
          VALUES (@Proveedor,@Fecha,@Total)",
                _connection);

            cmd.Parameters.AddWithValue("@Proveedor", compra.IdProveedor);
            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@Total", compra.Total);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            Compra compra = new Compra();

            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "SELECT * FROM Compras WHERE IdCompra=@id",
                _connection);

            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                compra.IdCompra = (int)reader["IdCompra"];
                compra.IdProveedor = (int)reader["IdProveedor"];
                compra.Total = (decimal)reader["Total"];
            }

            _connection.Close();

            return View(compra);
        }

        [HttpPost]
        public IActionResult Editar(Compra compra)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                @"UPDATE Compras 
          SET IdProveedor=@Proveedor,
              Total=@Total
          WHERE IdCompra=@Id",
                _connection);

            cmd.Parameters.AddWithValue("@Proveedor", compra.IdProveedor);
            cmd.Parameters.AddWithValue("@Total", compra.Total);
            cmd.Parameters.AddWithValue("@Id", compra.IdCompra);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }

        public IActionResult Eliminar(int id)
        {
            _connection.Open();

            SqlCommand cmd = new SqlCommand(
                "DELETE FROM Compras WHERE IdCompra=@id",
                _connection);

            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();

            _connection.Close();

            return RedirectToAction("Index");
        }
    }
}