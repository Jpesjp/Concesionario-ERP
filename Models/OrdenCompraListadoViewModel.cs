namespace ERPConcesionario.Models
{
    public class OrdenCompraListadoViewModel
    {
        public int IdOrdenCompra { get; set; }
        public string NumeroOrdenCompra { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEstimadaRecepcion { get; set; }
        public string EstadoOrden { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
        public string? Observaciones { get; set; }
    }
}
