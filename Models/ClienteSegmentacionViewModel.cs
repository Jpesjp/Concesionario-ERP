namespace ERPConcesionario.Models
{
    public class ClienteSegmentacionViewModel
    {
        public int IdCliente { get; set; }
        public string CodigoCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = string.Empty;
        public int CantidadCompras { get; set; }
        public decimal ValorHistoricoCompras { get; set; }
        public DateTime? FechaUltimaCompra { get; set; }
        public decimal Puntaje { get; set; }
        public string Segmento { get; set; } = "C";
        public string RecomendacionMarketing { get; set; } = string.Empty;
    }
}
