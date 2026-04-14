using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }

        [Display(Name = "Código Proveedor")]
        public string CodigoProveedor { get; set; } = string.Empty;

        [Display(Name = "Tipo Proveedor")]
        public string TipoProveedor { get; set; } = string.Empty;

        [Display(Name = "Tipo Documento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [Display(Name = "Número Documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; } = string.Empty;

        [Display(Name = "Nombre Comercial")]
        public string? NombreComercial { get; set; }

        [Display(Name = "Nombre Contacto")]
        public string? NombreContacto { get; set; }

        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Departamento { get; set; }
        public string Pais { get; set; } = "Colombia";

        [Display(Name = "Condición Pago")]
        public string CondicionPago { get; set; } = "CONTADO";

        [Display(Name = "Cupo Crédito")]
        public decimal CupoCredito { get; set; }

        [Display(Name = "Estado Proveedor")]
        public string EstadoProveedor { get; set; } = "ACTIVO";

        public string? Observaciones { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}