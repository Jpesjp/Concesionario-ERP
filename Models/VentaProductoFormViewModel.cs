using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class VentaProductoFormViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
        public int IdCliente { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto con inventario disponible.")]
        public int IdInventarioProducto { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        public decimal PrecioUnitario { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo.")]
        public decimal DescuentoLinea { get; set; }

        public string? Observaciones { get; set; }
    }
}
