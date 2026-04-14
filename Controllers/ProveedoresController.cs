using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
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
            var proveedores = new List<Proveedor>();

            string query = @"SELECT IdProveedor, CodigoProveedor, TipoProveedor, TipoDocumento, NumeroDocumento,
                                    RazonSocial, NombreComercial, NombreContacto, Telefono, Telefono2, Email,
                                    Direccion, Ciudad, Departamento, Pais, CondicionPago, CupoCredito,
                                    EstadoProveedor, Observaciones, FechaRegistro
                             FROM Proveedores";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    proveedores.Add(new Proveedor
                    {
                        IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                        CodigoProveedor = reader["CodigoProveedor"].ToString() ?? "",
                        TipoProveedor = reader["TipoProveedor"].ToString() ?? "",
                        TipoDocumento = reader["TipoDocumento"].ToString() ?? "",
                        NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                        RazonSocial = reader["RazonSocial"].ToString() ?? "",
                        NombreComercial = reader["NombreComercial"] as string,
                        NombreContacto = reader["NombreContacto"] as string,
                        Telefono = reader["Telefono"] as string,
                        Telefono2 = reader["Telefono2"] as string,
                        Email = reader["Email"] as string,
                        Direccion = reader["Direccion"] as string,
                        Ciudad = reader["Ciudad"] as string,
                        Departamento = reader["Departamento"] as string,
                        Pais = reader["Pais"].ToString() ?? "Colombia",
                        CondicionPago = reader["CondicionPago"].ToString() ?? "CONTADO",
                        CupoCredito = Convert.ToDecimal(reader["CupoCredito"]),
                        EstadoProveedor = reader["EstadoProveedor"].ToString() ?? "ACTIVO",
                        Observaciones = reader["Observaciones"] as string,
                        FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
                    });
                }

                _connection.Close();
            }

            return View(proveedores);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Proveedor proveedor)
        {
            if (!ModelState.IsValid)
                return View(proveedor);

            string query = @"INSERT INTO Proveedores
                            (CodigoProveedor, TipoProveedor, TipoDocumento, NumeroDocumento, RazonSocial,
                             NombreComercial, NombreContacto, Telefono, Telefono2, Email, Direccion,
                             Ciudad, Departamento, Pais, CondicionPago, CupoCredito, EstadoProveedor,
                             Observaciones, FechaRegistro)
                             VALUES
                            (@CodigoProveedor, @TipoProveedor, @TipoDocumento, @NumeroDocumento, @RazonSocial,
                             @NombreComercial, @NombreContacto, @Telefono, @Telefono2, @Email, @Direccion,
                             @Ciudad, @Departamento, @Pais, @CondicionPago, @CupoCredito, @EstadoProveedor,
                             @Observaciones, @FechaRegistro)";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@CodigoProveedor", proveedor.CodigoProveedor);
                cmd.Parameters.AddWithValue("@TipoProveedor", proveedor.TipoProveedor);
                cmd.Parameters.AddWithValue("@TipoDocumento", proveedor.TipoDocumento);
                cmd.Parameters.AddWithValue("@NumeroDocumento", proveedor.NumeroDocumento);
                cmd.Parameters.AddWithValue("@RazonSocial", proveedor.RazonSocial);
                cmd.Parameters.AddWithValue("@NombreComercial", (object?)proveedor.NombreComercial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreContacto", (object?)proveedor.NombreContacto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", (object?)proveedor.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono2", (object?)proveedor.Telefono2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)proveedor.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Direccion", (object?)proveedor.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ciudad", (object?)proveedor.Ciudad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Departamento", (object?)proveedor.Departamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Pais", proveedor.Pais ?? "Colombia");
                cmd.Parameters.AddWithValue("@CondicionPago", proveedor.CondicionPago);
                cmd.Parameters.AddWithValue("@CupoCredito", proveedor.CupoCredito);
                cmd.Parameters.AddWithValue("@EstadoProveedor", proveedor.EstadoProveedor);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)proveedor.Observaciones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Proveedor? proveedor = null;

            string query = @"SELECT * FROM Proveedores WHERE IdProveedor = @IdProveedor";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProveedor", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    proveedor = new Proveedor
                    {
                        IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                        CodigoProveedor = reader["CodigoProveedor"].ToString() ?? "",
                        TipoProveedor = reader["TipoProveedor"].ToString() ?? "",
                        TipoDocumento = reader["TipoDocumento"].ToString() ?? "",
                        NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                        RazonSocial = reader["RazonSocial"].ToString() ?? "",
                        NombreComercial = reader["NombreComercial"] as string,
                        NombreContacto = reader["NombreContacto"] as string,
                        Telefono = reader["Telefono"] as string,
                        Telefono2 = reader["Telefono2"] as string,
                        Email = reader["Email"] as string,
                        Direccion = reader["Direccion"] as string,
                        Ciudad = reader["Ciudad"] as string,
                        Departamento = reader["Departamento"] as string,
                        Pais = reader["Pais"].ToString() ?? "Colombia",
                        CondicionPago = reader["CondicionPago"].ToString() ?? "CONTADO",
                        CupoCredito = Convert.ToDecimal(reader["CupoCredito"]),
                        EstadoProveedor = reader["EstadoProveedor"].ToString() ?? "ACTIVO",
                        Observaciones = reader["Observaciones"] as string,
                        FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
                    };
                }

                _connection.Close();
            }

            if (proveedor == null)
                return NotFound();

            return View(proveedor);
        }

        [HttpPost]
        public IActionResult Edit(Proveedor proveedor)
        {
            if (!ModelState.IsValid)
                return View(proveedor);

            string query = @"UPDATE Proveedores SET
                                CodigoProveedor = @CodigoProveedor,
                                TipoProveedor = @TipoProveedor,
                                TipoDocumento = @TipoDocumento,
                                NumeroDocumento = @NumeroDocumento,
                                RazonSocial = @RazonSocial,
                                NombreComercial = @NombreComercial,
                                NombreContacto = @NombreContacto,
                                Telefono = @Telefono,
                                Telefono2 = @Telefono2,
                                Email = @Email,
                                Direccion = @Direccion,
                                Ciudad = @Ciudad,
                                Departamento = @Departamento,
                                Pais = @Pais,
                                CondicionPago = @CondicionPago,
                                CupoCredito = @CupoCredito,
                                EstadoProveedor = @EstadoProveedor,
                                Observaciones = @Observaciones
                             WHERE IdProveedor = @IdProveedor";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProveedor", proveedor.IdProveedor);
                cmd.Parameters.AddWithValue("@CodigoProveedor", proveedor.CodigoProveedor);
                cmd.Parameters.AddWithValue("@TipoProveedor", proveedor.TipoProveedor);
                cmd.Parameters.AddWithValue("@TipoDocumento", proveedor.TipoDocumento);
                cmd.Parameters.AddWithValue("@NumeroDocumento", proveedor.NumeroDocumento);
                cmd.Parameters.AddWithValue("@RazonSocial", proveedor.RazonSocial);
                cmd.Parameters.AddWithValue("@NombreComercial", (object?)proveedor.NombreComercial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreContacto", (object?)proveedor.NombreContacto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", (object?)proveedor.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono2", (object?)proveedor.Telefono2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)proveedor.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Direccion", (object?)proveedor.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ciudad", (object?)proveedor.Ciudad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Departamento", (object?)proveedor.Departamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Pais", proveedor.Pais ?? "Colombia");
                cmd.Parameters.AddWithValue("@CondicionPago", proveedor.CondicionPago);
                cmd.Parameters.AddWithValue("@CupoCredito", proveedor.CupoCredito);
                cmd.Parameters.AddWithValue("@EstadoProveedor", proveedor.EstadoProveedor);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)proveedor.Observaciones ?? DBNull.Value);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            Proveedor? proveedor = null;

            string query = @"SELECT * FROM Proveedores WHERE IdProveedor = @IdProveedor";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProveedor", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    proveedor = new Proveedor
                    {
                        IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                        CodigoProveedor = reader["CodigoProveedor"].ToString() ?? "",
                        RazonSocial = reader["RazonSocial"].ToString() ?? "",
                        NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                        Telefono = reader["Telefono"] as string,
                        Email = reader["Email"] as string
                    };
                }

                _connection.Close();
            }

            if (proveedor == null)
                return NotFound();

            return View(proveedor);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            string query = @"DELETE FROM Proveedores WHERE IdProveedor = @IdProveedor";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProveedor", id);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }
    }
}