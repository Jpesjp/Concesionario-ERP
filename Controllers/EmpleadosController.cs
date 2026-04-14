using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
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
            var empleados = new List<Empleado>();

            string query = @"SELECT * FROM Empleados";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    empleados.Add(MapEmpleado(reader));
                }

                _connection.Close();
            }

            return View(empleados);
        }

        public IActionResult Create()
        {
            CargarCombos();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Empleado empleado)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(empleado);
            }

            string query = @"INSERT INTO Empleados
                            (CodigoEmpleado, IdSucursal, IdCargo, PrimerNombre, SegundoNombre, PrimerApellido, SegundoApellido,
                             TipoDocumento, NumeroDocumento, FechaNacimiento, Sexo, EstadoCivil, Nacionalidad, Direccion,
                             Ciudad, Departamento, Pais, TelefonoPersonal, TelefonoSecundario, EmailPersonal, EmailCorporativo,
                             NombreContactoEmergencia, ParentescoContactoEmergencia, TelefonoEmergencia, FechaIngreso, FechaSalida,
                             TipoContrato, SalarioBase, AuxilioTransporte, ComisionBase, Banco, TipoCuentaBancaria,
                             NumeroCuentaBancaria, EPS, AFP, ARL, CajaCompensacion, TieneLicenciaConducir, CategoriaLicencia,
                             FechaVencimientoLicencia, TallaCamisa, TallaPantalon, TallaCalzado, EstadoEmpleado, FotoUrl,
                             Observaciones, FechaCreacion)
                             VALUES
                            (@CodigoEmpleado, @IdSucursal, @IdCargo, @PrimerNombre, @SegundoNombre, @PrimerApellido, @SegundoApellido,
                             @TipoDocumento, @NumeroDocumento, @FechaNacimiento, @Sexo, @EstadoCivil, @Nacionalidad, @Direccion,
                             @Ciudad, @Departamento, @Pais, @TelefonoPersonal, @TelefonoSecundario, @EmailPersonal, @EmailCorporativo,
                             @NombreContactoEmergencia, @ParentescoContactoEmergencia, @TelefonoEmergencia, @FechaIngreso, @FechaSalida,
                             @TipoContrato, @SalarioBase, @AuxilioTransporte, @ComisionBase, @Banco, @TipoCuentaBancaria,
                             @NumeroCuentaBancaria, @EPS, @AFP, @ARL, @CajaCompensacion, @TieneLicenciaConducir, @CategoriaLicencia,
                             @FechaVencimientoLicencia, @TallaCamisa, @TallaPantalon, @TallaCalzado, @EstadoEmpleado, @FotoUrl,
                             @Observaciones, @FechaCreacion)";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                AddEmpleadoParameters(cmd, empleado, false);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Empleado? empleado = null;

            string query = @"SELECT * FROM Empleados WHERE IdEmpleado = @IdEmpleado";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdEmpleado", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    empleado = MapEmpleado(reader);
                }

                _connection.Close();
            }

            if (empleado == null)
                return NotFound();

            CargarCombos();
            return View(empleado);
        }

        [HttpPost]
        public IActionResult Edit(Empleado empleado)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(empleado);
            }

            string query = @"UPDATE Empleados SET
                                CodigoEmpleado = @CodigoEmpleado,
                                IdSucursal = @IdSucursal,
                                IdCargo = @IdCargo,
                                PrimerNombre = @PrimerNombre,
                                SegundoNombre = @SegundoNombre,
                                PrimerApellido = @PrimerApellido,
                                SegundoApellido = @SegundoApellido,
                                TipoDocumento = @TipoDocumento,
                                NumeroDocumento = @NumeroDocumento,
                                FechaNacimiento = @FechaNacimiento,
                                Sexo = @Sexo,
                                EstadoCivil = @EstadoCivil,
                                Nacionalidad = @Nacionalidad,
                                Direccion = @Direccion,
                                Ciudad = @Ciudad,
                                Departamento = @Departamento,
                                Pais = @Pais,
                                TelefonoPersonal = @TelefonoPersonal,
                                TelefonoSecundario = @TelefonoSecundario,
                                EmailPersonal = @EmailPersonal,
                                EmailCorporativo = @EmailCorporativo,
                                NombreContactoEmergencia = @NombreContactoEmergencia,
                                ParentescoContactoEmergencia = @ParentescoContactoEmergencia,
                                TelefonoEmergencia = @TelefonoEmergencia,
                                FechaIngreso = @FechaIngreso,
                                FechaSalida = @FechaSalida,
                                TipoContrato = @TipoContrato,
                                SalarioBase = @SalarioBase,
                                AuxilioTransporte = @AuxilioTransporte,
                                ComisionBase = @ComisionBase,
                                Banco = @Banco,
                                TipoCuentaBancaria = @TipoCuentaBancaria,
                                NumeroCuentaBancaria = @NumeroCuentaBancaria,
                                EPS = @EPS,
                                AFP = @AFP,
                                ARL = @ARL,
                                CajaCompensacion = @CajaCompensacion,
                                TieneLicenciaConducir = @TieneLicenciaConducir,
                                CategoriaLicencia = @CategoriaLicencia,
                                FechaVencimientoLicencia = @FechaVencimientoLicencia,
                                TallaCamisa = @TallaCamisa,
                                TallaPantalon = @TallaPantalon,
                                TallaCalzado = @TallaCalzado,
                                EstadoEmpleado = @EstadoEmpleado,
                                FotoUrl = @FotoUrl,
                                Observaciones = @Observaciones
                             WHERE IdEmpleado = @IdEmpleado";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                AddEmpleadoParameters(cmd, empleado, true);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            Empleado? empleado = null;

            string query = @"SELECT * FROM Empleados WHERE IdEmpleado = @IdEmpleado";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdEmpleado", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    empleado = new Empleado
                    {
                        IdEmpleado = Convert.ToInt32(reader["IdEmpleado"]),
                        CodigoEmpleado = reader["CodigoEmpleado"].ToString() ?? "",
                        PrimerNombre = reader["PrimerNombre"].ToString() ?? "",
                        PrimerApellido = reader["PrimerApellido"].ToString() ?? "",
                        NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                        TelefonoPersonal = reader["TelefonoPersonal"].ToString() ?? "",
                        EstadoEmpleado = reader["EstadoEmpleado"].ToString() ?? "ACTIVO"
                    };
                }

                _connection.Close();
            }

            if (empleado == null)
                return NotFound();

            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            string query = @"DELETE FROM Empleados WHERE IdEmpleado = @IdEmpleado";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdEmpleado", id);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        private void CargarCombos()
        {
            ViewBag.Sucursales = ObtenerSucursales();
            ViewBag.Cargos = ObtenerCargos();
        }

        private List<SelectListItem> ObtenerSucursales()
        {
            var lista = new List<SelectListItem>();

            string query = "SELECT IdSucursal, Nombre FROM Sucursales ORDER BY Nombre";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = reader["IdSucursal"].ToString(),
                        Text = reader["Nombre"].ToString()
                    });
                }

                _connection.Close();
            }

            return lista;
        }

        private List<SelectListItem> ObtenerCargos()
        {
            var lista = new List<SelectListItem>();

            string query = "SELECT IdCargo, Nombre FROM Cargos ORDER BY Nombre";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = reader["IdCargo"].ToString(),
                        Text = reader["Nombre"].ToString()
                    });
                }

                _connection.Close();
            }

            return lista;
        }

        private Empleado MapEmpleado(SqlDataReader reader)
        {
            return new Empleado
            {
                IdEmpleado = Convert.ToInt32(reader["IdEmpleado"]),
                CodigoEmpleado = reader["CodigoEmpleado"].ToString() ?? "",
                IdSucursal = Convert.ToInt32(reader["IdSucursal"]),
                IdCargo = Convert.ToInt32(reader["IdCargo"]),
                PrimerNombre = reader["PrimerNombre"].ToString() ?? "",
                SegundoNombre = reader["SegundoNombre"] as string,
                PrimerApellido = reader["PrimerApellido"].ToString() ?? "",
                SegundoApellido = reader["SegundoApellido"] as string,
                TipoDocumento = reader["TipoDocumento"].ToString() ?? "",
                NumeroDocumento = reader["NumeroDocumento"].ToString() ?? "",
                FechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"]),
                Sexo = reader["Sexo"].ToString() ?? "",
                EstadoCivil = reader["EstadoCivil"].ToString() ?? "",
                Nacionalidad = reader["Nacionalidad"].ToString() ?? "Colombiana",
                Direccion = reader["Direccion"].ToString() ?? "",
                Ciudad = reader["Ciudad"].ToString() ?? "",
                Departamento = reader["Departamento"].ToString() ?? "",
                Pais = reader["Pais"].ToString() ?? "Colombia",
                TelefonoPersonal = reader["TelefonoPersonal"].ToString() ?? "",
                TelefonoSecundario = reader["TelefonoSecundario"] as string,
                EmailPersonal = reader["EmailPersonal"] as string,
                EmailCorporativo = reader["EmailCorporativo"] as string,
                NombreContactoEmergencia = reader["NombreContactoEmergencia"].ToString() ?? "",
                ParentescoContactoEmergencia = reader["ParentescoContactoEmergencia"].ToString() ?? "",
                TelefonoEmergencia = reader["TelefonoEmergencia"].ToString() ?? "",
                FechaIngreso = Convert.ToDateTime(reader["FechaIngreso"]),
                FechaSalida = reader["FechaSalida"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaSalida"]),
                TipoContrato = reader["TipoContrato"].ToString() ?? "",
                SalarioBase = Convert.ToDecimal(reader["SalarioBase"]),
                AuxilioTransporte = Convert.ToDecimal(reader["AuxilioTransporte"]),
                ComisionBase = Convert.ToDecimal(reader["ComisionBase"]),
                Banco = reader["Banco"] as string,
                TipoCuentaBancaria = reader["TipoCuentaBancaria"] as string,
                NumeroCuentaBancaria = reader["NumeroCuentaBancaria"] as string,
                EPS = reader["EPS"] as string,
                AFP = reader["AFP"] as string,
                ARL = reader["ARL"] as string,
                CajaCompensacion = reader["CajaCompensacion"] as string,
                TieneLicenciaConducir = Convert.ToBoolean(reader["TieneLicenciaConducir"]),
                CategoriaLicencia = reader["CategoriaLicencia"] as string,
                FechaVencimientoLicencia = reader["FechaVencimientoLicencia"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaVencimientoLicencia"]),
                TallaCamisa = reader["TallaCamisa"] as string,
                TallaPantalon = reader["TallaPantalon"] as string,
                TallaCalzado = reader["TallaCalzado"] as string,
                EstadoEmpleado = reader["EstadoEmpleado"].ToString() ?? "ACTIVO",
                FotoUrl = reader["FotoUrl"] as string,
                Observaciones = reader["Observaciones"] as string,
                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
            };
        }

        private void AddEmpleadoParameters(SqlCommand cmd, Empleado empleado, bool includeId)
        {
            if (includeId)
                cmd.Parameters.AddWithValue("@IdEmpleado", empleado.IdEmpleado);

            cmd.Parameters.AddWithValue("@CodigoEmpleado", empleado.CodigoEmpleado);
            cmd.Parameters.AddWithValue("@IdSucursal", empleado.IdSucursal);
            cmd.Parameters.AddWithValue("@IdCargo", empleado.IdCargo);
            cmd.Parameters.AddWithValue("@PrimerNombre", empleado.PrimerNombre);
            cmd.Parameters.AddWithValue("@SegundoNombre", (object?)empleado.SegundoNombre ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrimerApellido", empleado.PrimerApellido);
            cmd.Parameters.AddWithValue("@SegundoApellido", (object?)empleado.SegundoApellido ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TipoDocumento", empleado.TipoDocumento);
            cmd.Parameters.AddWithValue("@NumeroDocumento", empleado.NumeroDocumento);
            cmd.Parameters.AddWithValue("@FechaNacimiento", empleado.FechaNacimiento);
            cmd.Parameters.AddWithValue("@Sexo", empleado.Sexo);
            cmd.Parameters.AddWithValue("@EstadoCivil", empleado.EstadoCivil);
            cmd.Parameters.AddWithValue("@Nacionalidad", empleado.Nacionalidad);
            cmd.Parameters.AddWithValue("@Direccion", empleado.Direccion);
            cmd.Parameters.AddWithValue("@Ciudad", empleado.Ciudad);
            cmd.Parameters.AddWithValue("@Departamento", empleado.Departamento);
            cmd.Parameters.AddWithValue("@Pais", empleado.Pais);
            cmd.Parameters.AddWithValue("@TelefonoPersonal", empleado.TelefonoPersonal);
            cmd.Parameters.AddWithValue("@TelefonoSecundario", (object?)empleado.TelefonoSecundario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmailPersonal", (object?)empleado.EmailPersonal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmailCorporativo", (object?)empleado.EmailCorporativo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NombreContactoEmergencia", empleado.NombreContactoEmergencia);
            cmd.Parameters.AddWithValue("@ParentescoContactoEmergencia", empleado.ParentescoContactoEmergencia);
            cmd.Parameters.AddWithValue("@TelefonoEmergencia", empleado.TelefonoEmergencia);
            cmd.Parameters.AddWithValue("@FechaIngreso", empleado.FechaIngreso);
            cmd.Parameters.AddWithValue("@FechaSalida", (object?)empleado.FechaSalida ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TipoContrato", empleado.TipoContrato);
            cmd.Parameters.AddWithValue("@SalarioBase", empleado.SalarioBase);
            cmd.Parameters.AddWithValue("@AuxilioTransporte", empleado.AuxilioTransporte);
            cmd.Parameters.AddWithValue("@ComisionBase", empleado.ComisionBase);
            cmd.Parameters.AddWithValue("@Banco", (object?)empleado.Banco ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TipoCuentaBancaria", (object?)empleado.TipoCuentaBancaria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NumeroCuentaBancaria", (object?)empleado.NumeroCuentaBancaria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EPS", (object?)empleado.EPS ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AFP", (object?)empleado.AFP ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ARL", (object?)empleado.ARL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CajaCompensacion", (object?)empleado.CajaCompensacion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TieneLicenciaConducir", empleado.TieneLicenciaConducir);
            cmd.Parameters.AddWithValue("@CategoriaLicencia", (object?)empleado.CategoriaLicencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaVencimientoLicencia", (object?)empleado.FechaVencimientoLicencia ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TallaCamisa", (object?)empleado.TallaCamisa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TallaPantalon", (object?)empleado.TallaPantalon ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TallaCalzado", (object?)empleado.TallaCalzado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EstadoEmpleado", empleado.EstadoEmpleado);
            cmd.Parameters.AddWithValue("@FotoUrl", (object?)empleado.FotoUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)empleado.Observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
        }
    }
}