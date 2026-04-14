using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }

        [Display(Name = "Categoría")]
        public int IdCategoriaProducto { get; set; }

        [Display(Name = "Código Producto")]
        public string CodigoProducto { get; set; } = string.Empty;

        [Display(Name = "Código Barras")]
        public string? CodigoBarras { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Marca { get; set; }

        [Display(Name = "Unidad Medida")]
        public string UnidadMedida { get; set; } = "UND";

        [Display(Name = "Tipo Producto")]
        public string TipoProducto { get; set; } = string.Empty;

        [Display(Name = "Afecta Inventario")]
        public bool AfectaInventario { get; set; }

        [Display(Name = "Stock Mínimo")]
        public int StockMinimo { get; set; }

        [Display(Name = "Stock Máximo")]
        public int? StockMaximo { get; set; }

        [Display(Name = "Costo Promedio")]
        public decimal CostoPromedio { get; set; }

        [Display(Name = "Precio Venta")]
        public decimal PrecioVenta { get; set; }

        public decimal IVA { get; set; }

        [Display(Name = "Estado Producto")]
        public string EstadoProducto { get; set; } = "ACTIVO";

        public DateTime FechaCreacion { get; set; }
    }
}