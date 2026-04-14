using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
{
    public class NominasController : Controller
    {
        private readonly SqlConnection _connection;

        public NominasController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var nominas = new List<dynamic>();

            string query = @"
                SELECT IdNomina, Periodo, FechaGeneracion, Estado, Observaciones
                FROM Nominas
                ORDER BY FechaGeneracion DESC, IdNomina DESC";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    nominas.Add(new
                    {
                        IdNomina = Convert.ToInt32(reader["IdNomina"]),
                        Periodo = reader["Periodo"].ToString() ?? "",
                        FechaGeneracion = Convert.ToDateTime(reader["FechaGeneracion"]),
                        Estado = reader["Estado"].ToString() ?? "",
                        Observaciones = reader["Observaciones"] as string
                    });
                }

                _connection.Close();
            }

            return View(nominas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string periodo, string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(periodo))
            {
                ModelState.AddModelError("", "Debes indicar el período, por ejemplo 2026-04.");
                return View();
            }

            if (ExisteNomina(periodo))
            {
                ModelState.AddModelError("", "Ya existe una nómina para ese período.");
                return View();
            }

            int idNomina = CrearNomina(periodo, observaciones);
            GenerarDetalleNomina(idNomina);

            return RedirectToAction("Detalle", new { id = idNomina });
        }

        public IActionResult Detalle(int id)
        {
            var detalle = new List<NominaDetalleViewModel>();

            string query = @"
                SELECT 
                    nd.IdNominaDetalle,
                    nd.IdNomina,
                    nd.IdEmpleado,
                    e.CodigoEmpleado,
                    e.PrimerNombre,
                    e.SegundoNombre,
                    e.PrimerApellido,
                    e.SegundoApellido,
                    e.TipoContrato,
                    e.EstadoEmpleado,
                    nd.SalarioBase,
                    nd.AuxilioTransporte,
                    nd.Comisiones,
                    nd.HorasExtra,
                    nd.Bonificaciones,
                    nd.SaludEmpleado,
                    nd.PensionEmpleado,
                    nd.Deducciones,
                    nd.ParafiscalesEmpresa,
                    nd.NetoPagar,
                    nd.Observaciones
                FROM NominaDetalle nd
                INNER JOIN Empleados e ON nd.IdEmpleado = e.IdEmpleado
                WHERE nd.IdNomina = @IdNomina
                ORDER BY e.PrimerApellido, e.PrimerNombre";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdNomina", id);

                _connection.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    detalle.Add(new NominaDetalleViewModel
                    {
                        IdNominaDetalle = Convert.ToInt32(reader["IdNominaDetalle"]),
                        IdNomina = Convert.ToInt32(reader["IdNomina"]),
                        IdEmpleado = Convert.ToInt32(reader["IdEmpleado"]),
                        CodigoEmpleado = reader["CodigoEmpleado"].ToString() ?? "",
                        NombreEmpleado =
                            $"{reader["PrimerNombre"]} {reader["SegundoNombre"]} {reader["PrimerApellido"]} {reader["SegundoApellido"]}"
                            .Replace("  ", " ").Trim(),
                        TipoContrato = reader["TipoContrato"].ToString() ?? "",
                        EstadoEmpleado = reader["EstadoEmpleado"].ToString() ?? "",
                        SalarioBase = Convert.ToDecimal(reader["SalarioBase"]),
                        AuxilioTransporte = Convert.ToDecimal(reader["AuxilioTransporte"]),
                        Comisiones = Convert.ToDecimal(reader["Comisiones"]),
                        HorasExtra = Convert.ToDecimal(reader["HorasExtra"]),
                        Bonificaciones = Convert.ToDecimal(reader["Bonificaciones"]),
                        SaludEmpleado = Convert.ToDecimal(reader["SaludEmpleado"]),
                        PensionEmpleado = Convert.ToDecimal(reader["PensionEmpleado"]),
                        Deducciones = Convert.ToDecimal(reader["Deducciones"]),
                        ParafiscalesEmpresa = Convert.ToDecimal(reader["ParafiscalesEmpresa"]),
                        NetoPagar = Convert.ToDecimal(reader["NetoPagar"]),
                        Observaciones = reader["Observaciones"] as string
                    });
                }

                _connection.Close();
            }

            ViewBag.IdNomina = id;
            return View(detalle);
        }

        public IActionResult Cerrar(int id)
        {
            string query = "UPDATE Nominas SET Estado = 'CERRADA' WHERE IdNomina = @IdNomina";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdNomina", id);
                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Detalle", new { id });
        }

        private bool ExisteNomina(string periodo)
        {
            string query = "SELECT COUNT(*) FROM Nominas WHERE Periodo = @Periodo";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Periodo", periodo);
                _connection.Open();
                int total = Convert.ToInt32(cmd.ExecuteScalar());
                _connection.Close();
                return total > 0;
            }
        }

        private int CrearNomina(string periodo, string? observaciones)
        {
            string query = @"
                INSERT INTO Nominas (Periodo, FechaGeneracion, Estado, Observaciones)
                OUTPUT INSERTED.IdNomina
                VALUES (@Periodo, @FechaGeneracion, 'BORRADOR', @Observaciones)";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@Periodo", periodo);
                cmd.Parameters.AddWithValue("@FechaGeneracion", DateTime.Now.Date);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)observaciones ?? DBNull.Value);

                _connection.Open();
                int idNomina = Convert.ToInt32(cmd.ExecuteScalar());
                _connection.Close();

                return idNomina;
            }
        }

        private void GenerarDetalleNomina(int idNomina)
        {
            var empleados = new List<Empleado>();

            string queryEmpleados = @"
                SELECT *
                FROM Empleados
                WHERE EstadoEmpleado = 'ACTIVO'
                ORDER BY PrimerApellido, PrimerNombre";

            using (SqlCommand cmd = new SqlCommand(queryEmpleados, _connection))
            {
                _connection.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    empleados.Add(new Empleado
                    {
                        IdEmpleado = Convert.ToInt32(reader["IdEmpleado"]),
                        TipoContrato = reader["TipoContrato"].ToString() ?? "",
                        SalarioBase = Convert.ToDecimal(reader["SalarioBase"]),
                        AuxilioTransporte = Convert.ToDecimal(reader["AuxilioTransporte"]),
                        ComisionBase = Convert.ToDecimal(reader["ComisionBase"]),
                        EstadoEmpleado = reader["EstadoEmpleado"].ToString() ?? ""
                    });
                }

                _connection.Close();
            }

            foreach (var emp in empleados)
            {
                decimal salario = emp.SalarioBase;
                decimal auxilio = emp.AuxilioTransporte;
                decimal comisiones = emp.ComisionBase;
                decimal horasExtra = 0;
                decimal bonificaciones = 0;

                decimal salud = Math.Round(salario * 0.04m, 2);
                decimal pension = Math.Round(salario * 0.04m, 2);

                decimal parafiscales = emp.TipoContrato == "PRACTICAS"
                    ? 0
                    : Math.Round(salario * 0.09m, 2);

                decimal otrasDeducciones = salud + pension;
                decimal neto = salario + auxilio + comisiones + horasExtra + bonificaciones - otrasDeducciones;

                string insert = @"
                    INSERT INTO NominaDetalle
                    (
                        IdNomina, IdEmpleado, SalarioBase, AuxilioTransporte, Comisiones, HorasExtra,
                        Bonificaciones, SaludEmpleado, PensionEmpleado, Deducciones, ParafiscalesEmpresa,
                        NetoPagar, Observaciones
                    )
                    VALUES
                    (
                        @IdNomina, @IdEmpleado, @SalarioBase, @AuxilioTransporte, @Comisiones, @HorasExtra,
                        @Bonificaciones, @SaludEmpleado, @PensionEmpleado, @Deducciones, @ParafiscalesEmpresa,
                        @NetoPagar, @Observaciones
                    )";

                using (SqlCommand cmdInsert = new SqlCommand(insert, _connection))
                {
                    cmdInsert.Parameters.AddWithValue("@IdNomina", idNomina);
                    cmdInsert.Parameters.AddWithValue("@IdEmpleado", emp.IdEmpleado);
                    cmdInsert.Parameters.AddWithValue("@SalarioBase", salario);
                    cmdInsert.Parameters.AddWithValue("@AuxilioTransporte", auxilio);
                    cmdInsert.Parameters.AddWithValue("@Comisiones", comisiones);
                    cmdInsert.Parameters.AddWithValue("@HorasExtra", horasExtra);
                    cmdInsert.Parameters.AddWithValue("@Bonificaciones", bonificaciones);
                    cmdInsert.Parameters.AddWithValue("@SaludEmpleado", salud);
                    cmdInsert.Parameters.AddWithValue("@PensionEmpleado", pension);
                    cmdInsert.Parameters.AddWithValue("@Deducciones", otrasDeducciones);
                    cmdInsert.Parameters.AddWithValue("@ParafiscalesEmpresa", parafiscales);
                    cmdInsert.Parameters.AddWithValue("@NetoPagar", neto);
                    cmdInsert.Parameters.AddWithValue("@Observaciones", $"Contrato: {emp.TipoContrato}");

                    _connection.Open();
                    cmdInsert.ExecuteNonQuery();
                    _connection.Close();
                }
            }
        }
    }
}