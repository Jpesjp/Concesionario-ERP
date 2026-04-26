using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class CompraProductoFormViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
        public int IdProducto { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor.")]
        public int IdProveedor { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un almacen.")]
        public int IdAlmacen { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una ubicacion.")]
        public int IdUbicacion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "El costo no puede ser negativo.")]
        public decimal CostoUnitario { get; set; }

        public string? Observaciones { get; set; }
    }
}
