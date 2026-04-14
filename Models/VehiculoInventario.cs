using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class VehiculoInventario
    {
        public int IdUnidadVehiculo { get; set; }

        [Display(Name = "Versión Vehículo")]
        public int IdVersionVehiculo { get; set; }

        [Display(Name = "Color")]
        public int IdColor { get; set; }

        [Display(Name = "Almacén")]
        public int IdAlmacen { get; set; }

        [Display(Name = "Ubicación")]
        public int IdUbicacion { get; set; }

        public string VIN { get; set; } = string.Empty;

        [Display(Name = "Número Motor")]
        public string? NumeroMotor { get; set; }

        public string? Chasis { get; set; }
        public string? Placa { get; set; }

        [Display(Name = "Año Fabricación")]
        public int AnioFabricacion { get; set; }

        [Display(Name = "Año Modelo")]
        public int AnioModelo { get; set; }

        [Display(Name = "Tipo Ingreso")]
        public string TipoIngreso { get; set; } = string.Empty;

        [Display(Name = "Condición Vehículo")]
        public string CondicionVehiculo { get; set; } = string.Empty;

        public int Kilometraje { get; set; }

        [Display(Name = "Costo Ingreso")]
        public decimal CostoIngreso { get; set; }

        [Display(Name = "Gastos Acondicionamiento")]
        public decimal GastosAcondicionamiento { get; set; }

        [Display(Name = "Precio Lista")]
        public decimal PrecioLista { get; set; }

        [Display(Name = "Precio Mínimo Venta")]
        public decimal PrecioMinimoVenta { get; set; }

        [Display(Name = "Estado Unidad")]
        public string EstadoUnidad { get; set; } = "DISPONIBLE";

        [Display(Name = "Fecha Ingreso")]
        public DateTime FechaIngreso { get; set; }

        [Display(Name = "Fecha Venta")]
        public DateTime? FechaVenta { get; set; }

        public string? Observaciones { get; set; }
    }
}