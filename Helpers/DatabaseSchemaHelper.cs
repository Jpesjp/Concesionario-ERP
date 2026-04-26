using Microsoft.Data.SqlClient;

namespace ERPConcesionario.Helpers
{
    public static class DatabaseSchemaHelper
    {
        public static void EnsureVentasFacturacionTables(SqlConnection connection)
        {
            string query = @"
                IF OBJECT_ID('dbo.Ventas', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Ventas (
                        IdVenta INT IDENTITY(1,1) PRIMARY KEY,
                        NumeroVenta VARCHAR(30) NOT NULL UNIQUE,
                        IdCliente INT NOT NULL,
                        IdSucursal INT NOT NULL,
                        IdEmpleadoVendedor INT NOT NULL,
                        FechaVenta DATETIME NOT NULL DEFAULT GETDATE(),
                        TipoVenta VARCHAR(20) NOT NULL
                            CHECK (TipoVenta IN ('VEHICULO','REPUESTOS','MIXTA')),
                        EstadoVenta VARCHAR(20) NOT NULL DEFAULT 'PENDIENTE'
                            CHECK (EstadoVenta IN ('PENDIENTE','FACTURADA','PAGADA','ANULADA','ENTREGADA')),
                        Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Subtotal >= 0),
                        Descuento DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Descuento >= 0),
                        Impuesto DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Impuesto >= 0),
                        Total DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Total >= 0),
                        Observaciones NVARCHAR(500) NULL,
                        CONSTRAINT FK_Ventas_Clientes
                            FOREIGN KEY (IdCliente) REFERENCES Clientes(IdCliente),
                        CONSTRAINT FK_Ventas_Sucursales
                            FOREIGN KEY (IdSucursal) REFERENCES Sucursales(IdSucursal),
                        CONSTRAINT FK_Ventas_Empleados
                            FOREIGN KEY (IdEmpleadoVendedor) REFERENCES Empleados(IdEmpleado)
                    );
                END

                IF OBJECT_ID('dbo.VentaDetalle', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.VentaDetalle (
                        IdVentaDetalle INT IDENTITY(1,1) PRIMARY KEY,
                        IdVenta INT NOT NULL,
                        IdProducto INT NULL,
                        IdUnidadVehiculo INT NULL,
                        DescripcionItem NVARCHAR(200) NOT NULL,
                        Cantidad INT NOT NULL CHECK (Cantidad > 0),
                        PrecioUnitario DECIMAL(18,2) NOT NULL CHECK (PrecioUnitario >= 0),
                        DescuentoLinea DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (DescuentoLinea >= 0),
                        IVA DECIMAL(5,2) NOT NULL DEFAULT 19 CHECK (IVA >= 0),
                        SubtotalLinea DECIMAL(18,2) NOT NULL CHECK (SubtotalLinea >= 0),
                        CONSTRAINT FK_VentaDetalle_Ventas
                            FOREIGN KEY (IdVenta) REFERENCES Ventas(IdVenta),
                        CONSTRAINT FK_VentaDetalle_Productos
                            FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto),
                        CONSTRAINT FK_VentaDetalle_Vehiculos
                            FOREIGN KEY (IdUnidadVehiculo) REFERENCES VehiculosInventario(IdUnidadVehiculo),
                        CONSTRAINT CK_VentaDetalle_Item
                            CHECK (
                                (IdProducto IS NOT NULL AND IdUnidadVehiculo IS NULL)
                                OR
                                (IdProducto IS NULL AND IdUnidadVehiculo IS NOT NULL)
                            )
                    );
                END

                IF OBJECT_ID('dbo.FacturasVenta', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.FacturasVenta (
                        IdFacturaVenta INT IDENTITY(1,1) PRIMARY KEY,
                        IdVenta INT NOT NULL UNIQUE,
                        NumeroFactura VARCHAR(30) NOT NULL UNIQUE,
                        FechaEmision DATETIME NOT NULL DEFAULT GETDATE(),
                        EstadoFactura VARCHAR(20) NOT NULL DEFAULT 'EMITIDA'
                            CHECK (EstadoFactura IN ('EMITIDA','PAGADA','ANULADA','NOTA_CREDITO')),
                        Subtotal DECIMAL(18,2) NOT NULL CHECK (Subtotal >= 0),
                        Impuesto DECIMAL(18,2) NOT NULL CHECK (Impuesto >= 0),
                        Total DECIMAL(18,2) NOT NULL CHECK (Total >= 0),
                        Observaciones NVARCHAR(300) NULL,
                        CONSTRAINT FK_FacturasVenta_Ventas
                            FOREIGN KEY (IdVenta) REFERENCES Ventas(IdVenta)
                    );
                END";

            bool cerrarConexion = connection.State != System.Data.ConnectionState.Open;

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                if (cerrarConexion)
                    connection.Open();

                cmd.ExecuteNonQuery();

                if (cerrarConexion)
                    connection.Close();
            }
        }
    }
}
