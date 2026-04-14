namespace ERP.Models
{
    public class OrdenCompra
    {
        public int IdOrdenCompra { get; set; }
        public string NumeroOrdenCompra { get; set; } = string.Empty;
        public int IdProveedor { get; set; }
        public int IdSucursal { get; set; }
        public int IdEmpleadoSolicita { get; set; }
        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEstimadaRecepcion { get; set; }
        public string EstadoOrden { get; set; } = "BORRADOR";
        public string Moneda { get; set; } = "COP";
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
    }
}