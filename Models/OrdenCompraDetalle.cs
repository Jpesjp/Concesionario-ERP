namespace ERP.Models
{
    public class OrdenCompraDetalle
    {
        public int IdOrdenCompraDetalle { get; set; }
        public int IdOrdenCompra { get; set; }
        public int? IdProducto { get; set; }
        public int? IdVersionVehiculo { get; set; }
        public string DescripcionItem { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal IVA { get; set; }
        public decimal Descuento { get; set; }
        public decimal Subtotal { get; set; }
    }
}