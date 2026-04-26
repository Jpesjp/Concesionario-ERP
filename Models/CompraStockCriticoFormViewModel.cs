using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class CompraStockCriticoFormViewModel
    {
        public int IdInventarioProducto { get; set; }
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int? StockMaximo { get; set; }
        public int IdAlmacen { get; set; }
        public int IdUbicacion { get; set; }
        public string Almacen { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Seleccione un proveedor.")]
        public int IdProveedor { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo.")]
        public decimal CostoUnitario { get; set; }

        public string? Observaciones { get; set; }
    }
}
