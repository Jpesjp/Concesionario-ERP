namespace ERPConcesionario.Models
{
    public class ProveedorEvaluacionViewModel
    {
        public int Posicion { get; set; }
        public int IdProveedor { get; set; }
        public string CodigoProveedor { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string TipoProveedor { get; set; } = string.Empty;
        public int CantidadEvaluaciones { get; set; }
        public decimal PromedioPuntualidad { get; set; }
        public decimal PromedioCalidad { get; set; }
        public decimal PuntajeRanking { get; set; }
        public DateTime? UltimaEvaluacion { get; set; }
        public string Clasificacion { get; set; } = "Sin datos";
    }
}
