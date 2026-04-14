using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly SqlConnection _connection;

        public VehiculosController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var vehiculos = new List<VehiculoInventario>();

            string query = @"SELECT IdUnidadVehiculo, IdVersionVehiculo, IdColor, IdAlmacen, IdUbicacion,
                                    VIN, NumeroMotor, Chasis, Placa, AnioFabricacion, AnioModelo,
                                    TipoIngreso, CondicionVehiculo, Kilometraje, CostoIngreso,
                                    GastosAcondicionamiento, PrecioLista, PrecioMinimoVenta,
                                    EstadoUnidad, FechaIngreso, FechaVenta, Observaciones
                             FROM VehiculosInventario";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    vehiculos.Add(new VehiculoInventario
                    {
                        IdUnidadVehiculo = Convert.ToInt32(reader["IdUnidadVehiculo"]),
                        IdVersionVehiculo = Convert.ToInt32(reader["IdVersionVehiculo"]),
                        IdColor = Convert.ToInt32(reader["IdColor"]),
                        IdAlmacen = Convert.ToInt32(reader["IdAlmacen"]),
                        IdUbicacion = Convert.ToInt32(reader["IdUbicacion"]),
                        VIN = reader["VIN"].ToString() ?? "",
                        NumeroMotor = reader["NumeroMotor"] as string,
                        Chasis = reader["Chasis"] as string,
                        Placa = reader["Placa"] as string,
                        AnioFabricacion = Convert.ToInt32(reader["AnioFabricacion"]),
                        AnioModelo = Convert.ToInt32(reader["AnioModelo"]),
                        TipoIngreso = reader["TipoIngreso"].ToString() ?? "",
                        CondicionVehiculo = reader["CondicionVehiculo"].ToString() ?? "",
                        Kilometraje = Convert.ToInt32(reader["Kilometraje"]),
                        CostoIngreso = Convert.ToDecimal(reader["CostoIngreso"]),
                        GastosAcondicionamiento = Convert.ToDecimal(reader["GastosAcondicionamiento"]),
                        PrecioLista = Convert.ToDecimal(reader["PrecioLista"]),
                        PrecioMinimoVenta = Convert.ToDecimal(reader["PrecioMinimoVenta"]),
                        EstadoUnidad = reader["EstadoUnidad"].ToString() ?? "DISPONIBLE",
                        FechaIngreso = Convert.ToDateTime(reader["FechaIngreso"]),
                        FechaVenta = reader["FechaVenta"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaVenta"]),
                        Observaciones = reader["Observaciones"] as string
                    });
                }

                _connection.Close();
            }

            return View(vehiculos);
        }

        public IActionResult Create()
        {
            CargarCombos();
            return View();
        }

        [HttpPost]
        public IActionResult Create(VehiculoInventario vehiculo)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(vehiculo);
            }

            string query = @"INSERT INTO VehiculosInventario
                            (IdVersionVehiculo, IdColor, IdAlmacen, IdUbicacion, VIN, NumeroMotor,
                             Chasis, Placa, AnioFabricacion, AnioModelo, TipoIngreso,
                             CondicionVehiculo, Kilometraje, CostoIngreso, GastosAcondicionamiento,
                             PrecioLista, PrecioMinimoVenta, EstadoUnidad, FechaIngreso, FechaVenta,
                             Observaciones)
                             VALUES
                            (@IdVersionVehiculo, @IdColor, @IdAlmacen, @IdUbicacion, @VIN, @NumeroMotor,
                             @Chasis, @Placa, @AnioFabricacion, @AnioModelo, @TipoIngreso,
                             @CondicionVehiculo, @Kilometraje, @CostoIngreso, @GastosAcondicionamiento,
                             @PrecioLista, @PrecioMinimoVenta, @EstadoUnidad, @FechaIngreso, @FechaVenta,
                             @Observaciones)";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdVersionVehiculo", vehiculo.IdVersionVehiculo);
                cmd.Parameters.AddWithValue("@IdColor", vehiculo.IdColor);
                cmd.Parameters.AddWithValue("@IdAlmacen", vehiculo.IdAlmacen);
                cmd.Parameters.AddWithValue("@IdUbicacion", vehiculo.IdUbicacion);
                cmd.Parameters.AddWithValue("@VIN", vehiculo.VIN);
                cmd.Parameters.AddWithValue("@NumeroMotor", (object?)vehiculo.NumeroMotor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Chasis", (object?)vehiculo.Chasis ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Placa", (object?)vehiculo.Placa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AnioFabricacion", vehiculo.AnioFabricacion);
                cmd.Parameters.AddWithValue("@AnioModelo", vehiculo.AnioModelo);
                cmd.Parameters.AddWithValue("@TipoIngreso", vehiculo.TipoIngreso);
                cmd.Parameters.AddWithValue("@CondicionVehiculo", vehiculo.CondicionVehiculo);
                cmd.Parameters.AddWithValue("@Kilometraje", vehiculo.Kilometraje);
                cmd.Parameters.AddWithValue("@CostoIngreso", vehiculo.CostoIngreso);
                cmd.Parameters.AddWithValue("@GastosAcondicionamiento", vehiculo.GastosAcondicionamiento);
                cmd.Parameters.AddWithValue("@PrecioLista", vehiculo.PrecioLista);
                cmd.Parameters.AddWithValue("@PrecioMinimoVenta", vehiculo.PrecioMinimoVenta);
                cmd.Parameters.AddWithValue("@EstadoUnidad", vehiculo.EstadoUnidad);
                cmd.Parameters.AddWithValue("@FechaIngreso", vehiculo.FechaIngreso == default ? DateTime.Now : vehiculo.FechaIngreso);
                cmd.Parameters.AddWithValue("@FechaVenta", (object?)vehiculo.FechaVenta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)vehiculo.Observaciones ?? DBNull.Value);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            VehiculoInventario? vehiculo = null;

            string query = @"SELECT * FROM VehiculosInventario WHERE IdUnidadVehiculo = @IdUnidadVehiculo";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdUnidadVehiculo", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    vehiculo = new VehiculoInventario
                    {
                        IdUnidadVehiculo = Convert.ToInt32(reader["IdUnidadVehiculo"]),
                        IdVersionVehiculo = Convert.ToInt32(reader["IdVersionVehiculo"]),
                        IdColor = Convert.ToInt32(reader["IdColor"]),
                        IdAlmacen = Convert.ToInt32(reader["IdAlmacen"]),
                        IdUbicacion = Convert.ToInt32(reader["IdUbicacion"]),
                        VIN = reader["VIN"].ToString() ?? "",
                        NumeroMotor = reader["NumeroMotor"] as string,
                        Chasis = reader["Chasis"] as string,
                        Placa = reader["Placa"] as string,
                        AnioFabricacion = Convert.ToInt32(reader["AnioFabricacion"]),
                        AnioModelo = Convert.ToInt32(reader["AnioModelo"]),
                        TipoIngreso = reader["TipoIngreso"].ToString() ?? "",
                        CondicionVehiculo = reader["CondicionVehiculo"].ToString() ?? "",
                        Kilometraje = Convert.ToInt32(reader["Kilometraje"]),
                        CostoIngreso = Convert.ToDecimal(reader["CostoIngreso"]),
                        GastosAcondicionamiento = Convert.ToDecimal(reader["GastosAcondicionamiento"]),
                        PrecioLista = Convert.ToDecimal(reader["PrecioLista"]),
                        PrecioMinimoVenta = Convert.ToDecimal(reader["PrecioMinimoVenta"]),
                        EstadoUnidad = reader["EstadoUnidad"].ToString() ?? "DISPONIBLE",
                        FechaIngreso = Convert.ToDateTime(reader["FechaIngreso"]),
                        FechaVenta = reader["FechaVenta"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaVenta"]),
                        Observaciones = reader["Observaciones"] as string
                    };
                }

                _connection.Close();
            }

            if (vehiculo == null)
                return NotFound();

            CargarCombos();
            return View(vehiculo);
        }

        [HttpPost]
        public IActionResult Edit(VehiculoInventario vehiculo)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(vehiculo);
            }

            string query = @"UPDATE VehiculosInventario SET
                                IdVersionVehiculo = @IdVersionVehiculo,
                                IdColor = @IdColor,
                                IdAlmacen = @IdAlmacen,
                                IdUbicacion = @IdUbicacion,
                                VIN = @VIN,
                                NumeroMotor = @NumeroMotor,
                                Chasis = @Chasis,
                                Placa = @Placa,
                                AnioFabricacion = @AnioFabricacion,
                                AnioModelo = @AnioModelo,
                                TipoIngreso = @TipoIngreso,
                                CondicionVehiculo = @CondicionVehiculo,
                                Kilometraje = @Kilometraje,
                                CostoIngreso = @CostoIngreso,
                                GastosAcondicionamiento = @GastosAcondicionamiento,
                                PrecioLista = @PrecioLista,
                                PrecioMinimoVenta = @PrecioMinimoVenta,
                                EstadoUnidad = @EstadoUnidad,
                                FechaIngreso = @FechaIngreso,
                                FechaVenta = @FechaVenta,
                                Observaciones = @Observaciones
                             WHERE IdUnidadVehiculo = @IdUnidadVehiculo";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdUnidadVehiculo", vehiculo.IdUnidadVehiculo);
                cmd.Parameters.AddWithValue("@IdVersionVehiculo", vehiculo.IdVersionVehiculo);
                cmd.Parameters.AddWithValue("@IdColor", vehiculo.IdColor);
                cmd.Parameters.AddWithValue("@IdAlmacen", vehiculo.IdAlmacen);
                cmd.Parameters.AddWithValue("@IdUbicacion", vehiculo.IdUbicacion);
                cmd.Parameters.AddWithValue("@VIN", vehiculo.VIN);
                cmd.Parameters.AddWithValue("@NumeroMotor", (object?)vehiculo.NumeroMotor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Chasis", (object?)vehiculo.Chasis ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Placa", (object?)vehiculo.Placa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AnioFabricacion", vehiculo.AnioFabricacion);
                cmd.Parameters.AddWithValue("@AnioModelo", vehiculo.AnioModelo);
                cmd.Parameters.AddWithValue("@TipoIngreso", vehiculo.TipoIngreso);
                cmd.Parameters.AddWithValue("@CondicionVehiculo", vehiculo.CondicionVehiculo);
                cmd.Parameters.AddWithValue("@Kilometraje", vehiculo.Kilometraje);
                cmd.Parameters.AddWithValue("@CostoIngreso", vehiculo.CostoIngreso);
                cmd.Parameters.AddWithValue("@GastosAcondicionamiento", vehiculo.GastosAcondicionamiento);
                cmd.Parameters.AddWithValue("@PrecioLista", vehiculo.PrecioLista);
                cmd.Parameters.AddWithValue("@PrecioMinimoVenta", vehiculo.PrecioMinimoVenta);
                cmd.Parameters.AddWithValue("@EstadoUnidad", vehiculo.EstadoUnidad);
                cmd.Parameters.AddWithValue("@FechaIngreso", vehiculo.FechaIngreso);
                cmd.Parameters.AddWithValue("@FechaVenta", (object?)vehiculo.FechaVenta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)vehiculo.Observaciones ?? DBNull.Value);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            VehiculoInventario? vehiculo = null;

            string query = @"SELECT * FROM VehiculosInventario WHERE IdUnidadVehiculo = @IdUnidadVehiculo";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdUnidadVehiculo", id);

                _connection.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    vehiculo = new VehiculoInventario
                    {
                        IdUnidadVehiculo = Convert.ToInt32(reader["IdUnidadVehiculo"]),
                        VIN = reader["VIN"].ToString() ?? "",
                        Placa = reader["Placa"] as string,
                        AnioModelo = Convert.ToInt32(reader["AnioModelo"]),
                        PrecioLista = Convert.ToDecimal(reader["PrecioLista"]),
                        EstadoUnidad = reader["EstadoUnidad"].ToString() ?? "DISPONIBLE"
                    };
                }

                _connection.Close();
            }

            if (vehiculo == null)
                return NotFound();

            return View(vehiculo);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            string query = @"DELETE FROM VehiculosInventario WHERE IdUnidadVehiculo = @IdUnidadVehiculo";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@IdUnidadVehiculo", id);

                _connection.Open();
                cmd.ExecuteNonQuery();
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        private void CargarCombos()
        {
            ViewBag.Versiones = ObtenerVersiones();
            ViewBag.Colores = ObtenerColores();
            ViewBag.Almacenes = ObtenerAlmacenes();
            ViewBag.Ubicaciones = ObtenerUbicaciones();
        }

        private List<SelectListItem> ObtenerVersiones()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT IdVersionVehiculo, NombreVersion FROM VersionesVehiculo ORDER BY NombreVersion";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = reader["IdVersionVehiculo"].ToString(),
                        Text = reader["NombreVersion"].ToString()
                    });
                }

                _connection.Close();
            }

            return lista;
        }

        private List<SelectListItem> ObtenerColores()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT IdColor, Nombre FROM ColoresVehiculo ORDER BY Nombre";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = reader["IdColor"].ToString(),
                        Text = reader["Nombre"].ToString()
                    });
                }

                _connection.Close();
            }

            return lista;
        }

        private List<SelectListItem> ObtenerAlmacenes()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT IdAlmacen, Nombre FROM Almacenes ORDER BY Nombre";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = reader["IdAlmacen"].ToString(),
                        Text = reader["Nombre"].ToString()
                    });
                }

                _connection.Close();
            }

            return lista;
        }

        private List<SelectListItem> ObtenerUbicaciones()
        {
            var lista = new List<SelectListItem>();
            string query = "SELECT IdUbicacion, Nombre FROM Ubicaciones ORDER BY Nombre";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = reader["IdUbicacion"].ToString(),
                        Text = reader["Nombre"].ToString()
                    });
                }

                _connection.Close();
            }

            return lista;
        }
    }
}