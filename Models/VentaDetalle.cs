namespace ERP.Models
{
    public class VentaDetalle
    {
        public int IdVentaDetalle { get; set; }
        public int IdVenta { get; set; }
        public int? IdProducto { get; set; }
        public int? IdUnidadVehiculo { get; set; }
        public string DescripcionItem { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal DescuentoLinea { get; set; }
        public decimal IVA { get; set; }
        public decimal SubtotalLinea { get; set; }
    }
}