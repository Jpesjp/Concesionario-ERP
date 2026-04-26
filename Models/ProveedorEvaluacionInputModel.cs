using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class ProveedorEvaluacionInputModel
    {
        public int IdProveedor { get; set; }
        public string CodigoProveedor { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "La puntualidad debe estar entre 1 y 5.")]
        public int PuntualidadEntrega { get; set; } = 5;

        [Range(1, 5, ErrorMessage = "La calidad debe estar entre 1 y 5.")]
        public int CalidadProductos { get; set; } = 5;

        public string? Observaciones { get; set; }
    }
}
