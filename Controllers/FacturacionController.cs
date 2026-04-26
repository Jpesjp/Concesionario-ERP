using System.Security.Cryptography;
using System.Text;
using ERPConcesionario.Helpers;
using ERPConcesionario.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Controllers
{
    public class FacturacionController : Controller
    {
        private readonly SqlConnection _connection;

        public FacturacionController(SqlConnection connection)
        {
            _connection = connection;
        }

        public IActionResult Index()
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Ventas");
            if (acceso != null) return acceso;

            DatabaseSchemaHelper.EnsureVentasFacturacionTables(_connection);
            EnsureFacturasElectronicasTable();

            var facturas = new List<FacturaElectronicaViewModel>();

            string query = @"
                SELECT
                    v.IdVenta,
                    v.NumeroVenta,
                    v.FechaVenta,
                    v.EstadoVenta,
                    v.Subtotal,
                    v.Impuesto,
                    v.Total,
                    CASE
                        WHEN c.TipoCliente = 'NATURAL'
                            THEN LTRIM(RTRIM(ISNULL(c.Nombres, '') + ' ' + ISNULL(c.Apellidos, '')))
                        ELSE ISNULL(c.RazonSocial, ISNULL(c.NombreComercial, 'Cliente empresa'))
                    END AS Cliente,
                    f.IdFacturaVenta,
                    f.NumeroFactura,
                    f.FechaEmision,
                    f.EstadoFactura,
                    fe.CUFE,
                    fe.FirmaDigital,
                    fe.EstadoEnvioDian,
                    fe.FechaEnvioDian,
                    fe.RespuestaDian
                FROM Ventas v
                INNER JOIN Clientes c ON v.IdCliente = c.IdCliente
                LEFT JOIN FacturasVenta f ON v.IdVenta = f.IdVenta
                LEFT JOIN FacturasElectronicas fe ON f.IdFacturaVenta = fe.IdFacturaVenta
                ORDER BY v.FechaVenta DESC;";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                _connection.Open();
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    facturas.Add(new FacturaElectronicaViewModel
                    {
                        IdVenta = Convert.ToInt32(reader["IdVenta"]),
                        NumeroVenta = reader["NumeroVenta"].ToString() ?? "",
                        FechaVenta = Convert.ToDateTime(reader["FechaVenta"]),
                        EstadoVenta = reader["EstadoVenta"].ToString() ?? "",
                        Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                        Impuesto = Convert.ToDecimal(reader["Impuesto"]),
                        Total = Convert.ToDecimal(reader["Total"]),
                        Cliente = reader["Cliente"].ToString() ?? "",
                        IdFacturaVenta = reader["IdFacturaVenta"] == DBNull.Value ? null : Convert.ToInt32(reader["IdFacturaVenta"]),
                        NumeroFactura = reader["NumeroFactura"] as string,
                        FechaEmision = reader["FechaEmision"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaEmision"]),
                        EstadoFactura = reader["EstadoFactura"] as string,
                        Cufe = reader["CUFE"] as string,
                        FirmaDigital = reader["FirmaDigital"] as string,
                        EstadoEnvioDian = reader["EstadoEnvioDian"] as string,
                        FechaEnvioDian = reader["FechaEnvioDian"] == DBNull.Value ? null : Convert.ToDateTime(reader["FechaEnvioDian"]),
                        RespuestaDian = reader["RespuestaDian"] as string
                    });
                }

                _connection.Close();
            }

            return View(facturas);
        }

        [HttpPost]
        public IActionResult Emitir(int id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Ventas");
            if (acceso != null) return acceso;

            DatabaseSchemaHelper.EnsureVentasFacturacionTables(_connection);
            EnsureFacturasElectronicasTable();

            try
            {
                int idFactura = EmitirFacturaElectronica(id);
                TempData["Mensaje"] = "Factura emitida correctamente: #" + idFactura;
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "No fue posible emitir la factura: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult EnviarDian(int id)
        {
            var acceso = AutorizacionHelper.ValidarSesionYRol(this, "Admin", "Ventas");
            if (acceso != null) return acceso;

            DatabaseSchemaHelper.EnsureVentasFacturacionTables(_connection);
            EnsureFacturasElectronicasTable();

            try
            {
                EnviarFacturaADian(id);
                TempData["Mensaje"] = "Factura enviada y aceptada por el servicio simulado DIAN.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "No fue posible enviar la factura: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private int EmitirFacturaElectronica(int idVenta)
        {
            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var venta = ObtenerVentaParaFacturar(idVenta, transaction);
                if (venta == null)
                    throw new InvalidOperationException("Venta no encontrada.");

                int? idFacturaExistente = ObtenerIdFacturaPorVenta(idVenta, transaction);
                int idFactura = idFacturaExistente ?? InsertarFacturaVenta(venta.Value, transaction);

                CrearRegistroElectronicoSiNoExiste(idFactura, venta.Value.NumeroVenta, venta.Value.Total, transaction);

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Ventas
                    SET EstadoVenta = 'FACTURADA'
                    WHERE IdVenta = @IdVenta
                      AND EstadoVenta <> 'ANULADA';",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return idFactura;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        private void EnviarFacturaADian(int idFacturaVenta)
        {
            _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                var factura = ObtenerFacturaParaEnvio(idFacturaVenta, transaction);
                if (factura == null)
                    throw new InvalidOperationException("Factura no encontrada.");

                CrearRegistroElectronicoSiNoExiste(idFacturaVenta, factura.Value.NumeroVenta, factura.Value.Total, transaction);

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE FacturasElectronicas
                    SET EstadoEnvioDian = 'ACEPTADA',
                        FechaEnvioDian = GETDATE(),
                        RespuestaDian = @RespuestaDian
                    WHERE IdFacturaVenta = @IdFacturaVenta;",
                    _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@IdFacturaVenta", idFacturaVenta);
                    cmd.Parameters.AddWithValue("@RespuestaDian", "Documento validado por servicio simulado DIAN.");
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

        private (int IdVenta, string NumeroVenta, decimal Subtotal, decimal Impuesto, decimal Total)? ObtenerVentaParaFacturar(int idVenta, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT IdVenta, NumeroVenta, Subtotal, Impuesto, Total
                FROM Ventas
                WHERE IdVenta = @IdVenta
                  AND EstadoVenta <> 'ANULADA';",
                _connection, transaction))
            {
                cmd.Parameters.AddWithValue("@IdVenta", idVenta);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return null;

                return (
                    Convert.ToInt32(reader["IdVenta"]),
                    reader["NumeroVenta"].ToString() ?? "",
                    Convert.ToDecimal(reader["Subtotal"]),
                    Convert.ToDecimal(reader["Impuesto"]),
                    Convert.ToDecimal(reader["Total"]));
            }
        }

        private (string NumeroVenta, decimal Total)? ObtenerFacturaParaEnvio(int idFacturaVenta, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT v.NumeroVenta, f.Total
                FROM FacturasVenta f
                INNER JOIN Ventas v ON f.IdVenta = v.IdVenta
                WHERE f.IdFacturaVenta = @IdFacturaVenta;",
                _connection, transaction))
            {
                cmd.Parameters.AddWithValue("@IdFacturaVenta", idFacturaVenta);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return null;

                return (
                    reader["NumeroVenta"].ToString() ?? "",
                    Convert.ToDecimal(reader["Total"]));
            }
        }

        private int? ObtenerIdFacturaPorVenta(int idVenta, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT IdFacturaVenta FROM FacturasVenta WHERE IdVenta = @IdVenta;", _connection, transaction))
            {
                cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                object? value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
            }
        }

        private int InsertarFacturaVenta((int IdVenta, string NumeroVenta, decimal Subtotal, decimal Impuesto, decimal Total) venta, SqlTransaction transaction)
        {
            string numeroFactura = GenerarNumeroDocumento("FV");

            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO FacturasVenta
                    (IdVenta, NumeroFactura, FechaEmision, EstadoFactura, Subtotal, Impuesto, Total, Observaciones)
                OUTPUT INSERTED.IdFacturaVenta
                VALUES
                    (@IdVenta, @NumeroFactura, GETDATE(), 'EMITIDA', @Subtotal, @Impuesto, @Total, @Observaciones);",
                _connection, transaction))
            {
                cmd.Parameters.AddWithValue("@IdVenta", venta.IdVenta);
                cmd.Parameters.AddWithValue("@NumeroFactura", numeroFactura);
                cmd.Parameters.AddWithValue("@Subtotal", venta.Subtotal);
                cmd.Parameters.AddWithValue("@Impuesto", venta.Impuesto);
                cmd.Parameters.AddWithValue("@Total", venta.Total);
                cmd.Parameters.AddWithValue("@Observaciones", "Factura generada desde modulo de facturacion electronica.");
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void CrearRegistroElectronicoSiNoExiste(int idFacturaVenta, string numeroVenta, decimal total, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM FacturasElectronicas WHERE IdFacturaVenta = @IdFacturaVenta;", _connection, transaction))
            {
                cmd.Parameters.AddWithValue("@IdFacturaVenta", idFacturaVenta);
                int existe = Convert.ToInt32(cmd.ExecuteScalar());
                if (existe > 0)
                    return;
            }

            string cufe = CalcularHash(idFacturaVenta + "|" + numeroVenta + "|" + total.ToString("0.00") + "|" + DateTime.Today.ToString("yyyyMMdd"));
            string firma = CalcularHash(cufe + "|ERPConcesionario|FirmaDigitalSimulada");

            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO FacturasElectronicas
                    (IdFacturaVenta, CUFE, FirmaDigital, EstadoEnvioDian, RespuestaDian)
                VALUES
                    (@IdFacturaVenta, @CUFE, @FirmaDigital, 'GENERADA', @RespuestaDian);",
                _connection, transaction))
            {
                cmd.Parameters.AddWithValue("@IdFacturaVenta", idFacturaVenta);
                cmd.Parameters.AddWithValue("@CUFE", cufe);
                cmd.Parameters.AddWithValue("@FirmaDigital", firma);
                cmd.Parameters.AddWithValue("@RespuestaDian", "Factura electronica generada; pendiente de envio al ente regulador.");
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureFacturasElectronicasTable()
        {
            string query = @"
                IF OBJECT_ID('dbo.FacturasElectronicas', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.FacturasElectronicas (
                        IdFacturaElectronica INT IDENTITY(1,1) PRIMARY KEY,
                        IdFacturaVenta INT NOT NULL UNIQUE,
                        CUFE VARCHAR(128) NOT NULL,
                        FirmaDigital VARCHAR(256) NOT NULL,
                        EstadoEnvioDian VARCHAR(20) NOT NULL DEFAULT 'GENERADA',
                        FechaEnvioDian DATETIME NULL,
                        RespuestaDian NVARCHAR(500) NULL,
                        CONSTRAINT FK_FacturasElectronicas_FacturasVenta
                            FOREIGN KEY (IdFacturaVenta) REFERENCES FacturasVenta(IdFacturaVenta)
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

        private static string CalcularHash(string texto)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }

        private static string GenerarNumeroDocumento(string prefijo)
        {
            return prefijo + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
