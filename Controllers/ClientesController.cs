using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
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
            var clientes = new List<Cliente>();

            string query = @"SELECT IdCliente, CodigoCliente, TipoCliente, TipoDocumento, NumeroDocumento,
                                    Nombres, Apellidos, RazonSocial, NombreComercial, FechaNacimiento,
                                    Genero, TelefonoPrincipal, TelefonoSecundario, Email, Direccion,
                                    Ciudad, Departamento, Pais, EmpresaLabora, Cargo, IngresoMensual,
                                    LimiteCredito, EstadoCliente, FechaRegistro, Observaciones
                             FROM Clientes";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clientes.Add(new Cliente
                    {
                        IdCliente = Convert.ToInt32(reader["IdCliente"]),
                        CodigoCliente = reader["CodigoCliente"].ToString() ?? "",
                        TipoCliente = reader["TipoCliente"].ToString() ?? "",
                        TipoDocumento = reader["TipoDocumento"].ToString() ?? "",
                        NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                        Nombres = reader["Nombres"] as string,
                        Apellidos = reader["Apellidos"] as string,
                        RazonSocial = reader["RazonSocial"] as string,
                        NombreComercial = reader["NombreComercial"] as string,
                        FechaNacimiento = reader["FechaNacimiento"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaNacimiento"]),
                        Genero = reader["Genero"] as string,
                        TelefonoPrincipal = reader["TelefonoPrincipal"].ToString() ?? "",
                        TelefonoSecundario = reader["TelefonoSecundario"] as string,
                        Email = reader["Email"] as string,
                        Direccion = reader["Direccion"] as string,
                        Ciudad = reader["Ciudad"] as string,
                        Departamento = reader["Departamento"] as string,
                        Pais = reader["Pais"].ToString() ?? "Colombia",
                        EmpresaLabora = reader["EmpresaLabora"] as string,
                        Cargo = reader["Cargo"] as string,
                        IngresoMensual = reader["IngresoMensual"] == DBNull.Value ? null : Convert.ToDecimal(reader["IngresoMensual"]),
                        LimiteCredito = Convert.ToDecimal(reader["LimiteCredito"]),
                        EstadoCliente = reader["EstadoCliente"].ToString() ?? "ACTIVO",
                        FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
                        Observaciones = reader["Observaciones"] as string
                    });
                }

                _connection.Close();
            }

            return View(clientes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return View(cliente);

            string query = @"INSERT INTO Clientes
                            (CodigoCliente, TipoCliente, TipoDocumento, NumeroDocumento, Nombres, Apellidos,
                             RazonSocial, NombreComercial, FechaNacimiento, Genero, TelefonoPrincipal,
                             TelefonoSecundario, Email, Direccion, Ciudad, Departamento, Pais,
                             EmpresaLabora, Cargo, IngresoMensual, LimiteCredito, EstadoCliente,
                             FechaRegistro, Observaciones)
                             VALUES
                            (@CodigoCliente, @TipoCliente, @TipoDocumento, @NumeroDocumento, @Nombres, @Apellidos,
                             @RazonSocial, @NombreComercial, @FechaNacimiento, @Genero, @TelefonoPrincipal,
                             @TelefonoSecundario, @Email, @Direccion, @Ciudad, @Departamento, @Pais,
                             @EmpresaLabora, @Cargo, @IngresoMensual, @LimiteCredito, @EstadoCliente,
                             @FechaRegistro, @Observaciones)";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@CodigoCliente", cliente.CodigoCliente);
                cmd.Parameters.AddWithValue("@TipoCliente", cliente.TipoCliente);
                cmd.Parameters.AddWithValue("@TipoDocumento", cliente.TipoDocumento);
                cmd.Parameters.AddWithValue("@NumeroDocumento", cliente.NumeroDocumento);
                cmd.Parameters.AddWithValue("@Nombres", (object?)cliente.Nombres ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Apellidos", (object?)cliente.Apellidos ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RazonSocial", (object?)cliente.RazonSocial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreComercial", (object?)cliente.NombreComercial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaNacimiento", (object?)cliente.FechaNacimiento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Genero", (object?)cliente.Genero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TelefonoPrincipal", cliente.TelefonoPrincipal);
                cmd.Parameters.AddWithValue("@TelefonoSecundario", (object?)cliente.TelefonoSecundario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)cliente.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Direccion", (object?)cliente.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ciudad", (object?)cliente.Ciudad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Departamento", (object?)cliente.Departamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Pais", cliente.Pais ?? "Colombia");
                cmd.Parameters.AddWithValue("@EmpresaLabora", (object?)cliente.EmpresaLabora ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cargo", (object?)cliente.Cargo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IngresoMensual", (object?)cliente.IngresoMensual ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LimiteCredito", cliente.LimiteCredito);
                cmd.Parameters.AddWithValue("@EstadoCliente", cliente.EstadoCliente);
                cmd.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)cliente.Observaciones ?? DBNull.Value);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Cliente? cliente = null;

            string query = @"SELECT * FROM Clientes WHERE IdCliente = @IdCliente";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdCliente", id);

                _connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    cliente = new Cliente
                    {
                        IdCliente = Convert.ToInt32(reader["IdCliente"]),
                        CodigoCliente = reader["CodigoCliente"].ToString() ?? "",
                        TipoCliente = reader["TipoCliente"].ToString() ?? "",
                        TipoDocumento = reader["TipoDocumento"].ToString() ?? "",
                        NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                        Nombres = reader["Nombres"] as string,
                        Apellidos = reader["Apellidos"] as string,
                        RazonSocial = reader["RazonSocial"] as string,
                        NombreComercial = reader["NombreComercial"] as string,
                        FechaNacimiento = reader["FechaNacimiento"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaNacimiento"]),
                        Genero = reader["Genero"] as string,
                        TelefonoPrincipal = reader["TelefonoPrincipal"].ToString() ?? "",
                        TelefonoSecundario = reader["TelefonoSecundario"] as string,
                        Email = reader["Email"] as string,
                        Direccion = reader["Direccion"] as string,
                        Ciudad = reader["Ciudad"] as string,
                        Departamento = reader["Departamento"] as string,
                        Pais = reader["Pais"].ToString() ?? "Colombia",
                        EmpresaLabora = reader["EmpresaLabora"] as string,
                        Cargo = reader["Cargo"] as string,
                        IngresoMensual = reader["IngresoMensual"] == DBNull.Value ? null : Convert.ToDecimal(reader["IngresoMensual"]),
                        LimiteCredito = Convert.ToDecimal(reader["LimiteCredito"]),
                        EstadoCliente = reader["EstadoCliente"].ToString() ?? "ACTIVO",
                        FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
                        Observaciones = reader["Observaciones"] as string
                    };
                }

                _connection.Close();
            }

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        public IActionResult Edit(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return View(cliente);

            string query = @"UPDATE Clientes SET
                                CodigoCliente = @CodigoCliente,
                                TipoCliente = @TipoCliente,
                                TipoDocumento = @TipoDocumento,
                                NumeroDocumento = @NumeroDocumento,
                                Nombres = @Nombres,
                                Apellidos = @Apellidos,
                                RazonSocial = @RazonSocial,
                                NombreComercial = @NombreComercial,
                                FechaNacimiento = @FechaNacimiento,
                                Genero = @Genero,
                                TelefonoPrincipal = @TelefonoPrincipal,
                                TelefonoSecundario = @TelefonoSecundario,
                                Email = @Email,
                                Direccion = @Direccion,
                                Ciudad = @Ciudad,
                                Departamento = @Departamento,
                                Pais = @Pais,
                                EmpresaLabora = @EmpresaLabora,
                                Cargo = @Cargo,
                                IngresoMensual = @IngresoMensual,
                                LimiteCredito = @LimiteCredito,
                                EstadoCliente = @EstadoCliente,
                                Observaciones = @Observaciones
                             WHERE IdCliente = @IdCliente";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
                cmd.Parameters.AddWithValue("@CodigoCliente", cliente.CodigoCliente);
                cmd.Parameters.AddWithValue("@TipoCliente", cliente.TipoCliente);
                cmd.Parameters.AddWithValue("@TipoDocumento", cliente.TipoDocumento);
                cmd.Parameters.AddWithValue("@NumeroDocumento", cliente.NumeroDocumento);
                cmd.Parameters.AddWithValue("@Nombres", (object?)cliente.Nombres ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Apellidos", (object?)cliente.Apellidos ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RazonSocial", (object?)cliente.RazonSocial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NombreComercial", (object?)cliente.NombreComercial ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaNacimiento", (object?)cliente.FechaNacimiento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Genero", (object?)cliente.Genero ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TelefonoPrincipal", cliente.TelefonoPrincipal);
                cmd.Parameters.AddWithValue("@TelefonoSecundario", (object?)cliente.TelefonoSecundario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)cliente.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Direccion", (object?)cliente.Direccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Ciudad", (object?)cliente.Ciudad ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Departamento", (object?)cliente.Departamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Pais", cliente.Pais ?? "Colombia");
                cmd.Parameters.AddWithValue("@EmpresaLabora", (object?)cliente.EmpresaLabora ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Cargo", (object?)cliente.Cargo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IngresoMensual", (object?)cliente.IngresoMensual ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LimiteCredito", cliente.LimiteCredito);
                cmd.Parameters.AddWithValue("@EstadoCliente", cliente.EstadoCliente);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)cliente.Observaciones ?? DBNull.Value);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            Cliente? cliente = null;

            string query = @"SELECT * FROM Clientes WHERE IdCliente = @IdCliente";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdCliente", id);

                _connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    cliente = new Cliente
                    {
                        IdCliente = Convert.ToInt32(reader["IdCliente"]),
                        CodigoCliente = reader["CodigoCliente"].ToString() ?? "",
                        TipoCliente = reader["TipoCliente"].ToString() ?? "",
                        TipoDocumento = reader["TipoDocumento"].ToString() ?? "",
                        NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                        Nombres = reader["Nombres"] as string,
                        Apellidos = reader["Apellidos"] as string,
                        RazonSocial = reader["RazonSocial"] as string,
                        NombreComercial = reader["NombreComercial"] as string,
                        TelefonoPrincipal = reader["TelefonoPrincipal"].ToString() ?? "",
                        Email = reader["Email"] as string
                    };
                }

                _connection.Close();
            }

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            string query = @"DELETE FROM Clientes WHERE IdCliente = @IdCliente";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdCliente", id);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }
    }
}