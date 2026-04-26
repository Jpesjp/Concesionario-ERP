using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ERPConcesionario.Helpers;

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
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
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

        public IActionResult Ranking()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;

            EnsureEvaluacionesProveedorTable();
            var ranking = new List<ProveedorEvaluacionViewModel>();

            string query = @"
                SELECT
                    p.IdProveedor,
                    p.CodigoProveedor,
                    p.RazonSocial,
                    p.TipoProveedor,
                    COUNT(e.IdEvaluacionProveedor) AS CantidadEvaluaciones,
                    ISNULL(AVG(CAST(e.PuntualidadEntrega AS DECIMAL(10,2))), 0) AS PromedioPuntualidad,
                    ISNULL(AVG(CAST(e.CalidadProductos AS DECIMAL(10,2))), 0) AS PromedioCalidad,
                    MAX(e.FechaEvaluacion) AS UltimaEvaluacion
                FROM Proveedores p
                LEFT JOIN EvaluacionesProveedor e
                    ON p.IdProveedor = e.IdProveedor
                GROUP BY p.IdProveedor, p.CodigoProveedor, p.RazonSocial, p.TipoProveedor;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int cantidadEvaluaciones = Convert.ToInt32(reader["CantidadEvaluaciones"]);
                    decimal puntualidad = Convert.ToDecimal(reader["PromedioPuntualidad"]);
                    decimal calidad = Convert.ToDecimal(reader["PromedioCalidad"]);
                    decimal puntaje = cantidadEvaluaciones == 0 ? 0 : Math.Round(((puntualidad + calidad) / 2) * 20, 2);

                    ranking.Add(new ProveedorEvaluacionViewModel
                    {
                        IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                        CodigoProveedor = reader["CodigoProveedor"].ToString() ?? "",
                        RazonSocial = reader["RazonSocial"].ToString() ?? "",
                        TipoProveedor = reader["TipoProveedor"].ToString() ?? "",
                        CantidadEvaluaciones = cantidadEvaluaciones,
                        PromedioPuntualidad = puntualidad,
                        PromedioCalidad = calidad,
                        PuntajeRanking = puntaje,
                        UltimaEvaluacion = reader["UltimaEvaluacion"] == DBNull.Value ? null : Convert.ToDateTime(reader["UltimaEvaluacion"]),
                        Clasificacion = ObtenerClasificacionProveedor(cantidadEvaluaciones, puntaje)
                    });
                }

                _connection.Close();
            }

            ranking = ranking
                .OrderByDescending(p => p.PuntajeRanking)
                .ThenBy(p => p.RazonSocial)
                .ToList();

            for (int i = 0; i < ranking.Count; i++)
                ranking[i].Posicion = i + 1;

            return View(ranking);
        }

        public IActionResult Evaluar(int id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;

            var proveedor = ObtenerProveedorParaEvaluacion(id);
            if (proveedor == null)
                return NotFound();

            return View(proveedor);
        }

        [HttpPost]
        public IActionResult Evaluar(ProveedorEvaluacionInputModel model)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;

            var proveedor = ObtenerProveedorParaEvaluacion(model.IdProveedor);
            if (proveedor == null)
                return NotFound();

            model.CodigoProveedor = proveedor.CodigoProveedor;
            model.RazonSocial = proveedor.RazonSocial;

            if (!ModelState.IsValid)
                return View(model);

            EnsureEvaluacionesProveedorTable();

            string query = @"
                INSERT INTO EvaluacionesProveedor
                    (IdProveedor, FechaEvaluacion, PuntualidadEntrega, CalidadProductos, Observaciones)
                VALUES
                    (@IdProveedor, GETDATE(), @PuntualidadEntrega, @CalidadProductos, @Observaciones);";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProveedor", model.IdProveedor);
                cmd.Parameters.AddWithValue("@PuntualidadEntrega", model.PuntualidadEntrega);
                cmd.Parameters.AddWithValue("@CalidadProductos", model.CalidadProductos);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)model.Observaciones ?? DBNull.Value);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            TempData["Mensaje"] = "Evaluacion registrada correctamente.";
            return RedirectToAction("Ranking");
        }

        public IActionResult Create()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Proveedor proveedor)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
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
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
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
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
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
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
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
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Compras");
            if (acceso != null) return acceso;
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

        private void EnsureEvaluacionesProveedorTable()
        {
            string query = @"
                IF OBJECT_ID('dbo.EvaluacionesProveedor', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.EvaluacionesProveedor (
                        IdEvaluacionProveedor INT IDENTITY(1,1) PRIMARY KEY,
                        IdProveedor INT NOT NULL,
                        FechaEvaluacion DATETIME NOT NULL DEFAULT GETDATE(),
                        PuntualidadEntrega INT NOT NULL CHECK (PuntualidadEntrega BETWEEN 1 AND 5),
                        CalidadProductos INT NOT NULL CHECK (CalidadProductos BETWEEN 1 AND 5),
                        Observaciones NVARCHAR(500) NULL,
                        CONSTRAINT FK_EvaluacionesProveedor_Proveedores
                            FOREIGN KEY (IdProveedor) REFERENCES Proveedores(IdProveedor)
                    );
                END";

            bool cerrarConexion = _connection.State != System.Data.ConnectionState.Open;

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                if (cerrarConexion)
                    _connection.Open();

                cmd.ExecuteNonQuery();

                if (cerrarConexion)
                    _connection.Close();
            }
        }

        private ProveedorEvaluacionInputModel? ObtenerProveedorParaEvaluacion(int id)
        {
            ProveedorEvaluacionInputModel? proveedor = null;

            string query = @"
                SELECT IdProveedor, CodigoProveedor, RazonSocial
                FROM Proveedores
                WHERE IdProveedor = @IdProveedor;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdProveedor", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    proveedor = new ProveedorEvaluacionInputModel
                    {
                        IdProveedor = Convert.ToInt32(reader["IdProveedor"]),
                        CodigoProveedor = reader["CodigoProveedor"].ToString() ?? "",
                        RazonSocial = reader["RazonSocial"].ToString() ?? ""
                    };
                }

                _connection.Close();
            }

            return proveedor;
        }

        private static string ObtenerClasificacionProveedor(int cantidadEvaluaciones, decimal puntaje)
        {
            if (cantidadEvaluaciones == 0)
                return "Sin datos";

            if (puntaje >= 90)
                return "Excelente";

            if (puntaje >= 75)
                return "Confiable";

            if (puntaje >= 60)
                return "En seguimiento";

            return "Riesgo";
        }
    }
}
