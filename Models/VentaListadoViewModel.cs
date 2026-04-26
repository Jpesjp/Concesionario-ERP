namespace ERPConcesionario.Models
{
    public class VentaListadoViewModel
    {
        public int IdVenta { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public string TipoVenta { get; set; } = "REPUESTOS";
        public string EstadoVenta { get; set; } = "PENDIENTE";
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
        public bool TieneFactura { get; set; }
    }
}
