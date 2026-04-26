namespace ERPConcesionario.Models
{
    public class FacturaElectronicaViewModel
    {
        public int IdVenta { get; set; }
        public int? IdFacturaVenta { get; set; }
        public string NumeroVenta { get; set; } = string.Empty;
        public string? NumeroFactura { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public DateTime? FechaEmision { get; set; }
        public string EstadoVenta { get; set; } = string.Empty;
        public string? EstadoFactura { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string? Cufe { get; set; }
        public string? FirmaDigital { get; set; }
        public string? EstadoEnvioDian { get; set; }
        public DateTime? FechaEnvioDian { get; set; }
        public string? RespuestaDian { get; set; }
        public bool TieneFactura => IdFacturaVenta.HasValue;
    }
}
