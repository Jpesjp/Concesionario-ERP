namespace ERP.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public int IdSucursal { get; set; }
        public int IdEmpleadoVendedor { get; set; }
        public DateTime FechaVenta { get; set; }
        public string TipoVenta { get; set; } = string.Empty;
        public string EstadoVenta { get; set; } = "PENDIENTE";
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
    }
}