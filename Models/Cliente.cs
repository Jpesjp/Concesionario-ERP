using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        [Display(Name = "Código Cliente")]
        public string CodigoCliente { get; set; } = string.Empty;

        [Display(Name = "Tipo Cliente")]
        public string TipoCliente { get; set; } = string.Empty;

        [Display(Name = "Tipo Documento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [Display(Name = "Número Documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }

        [Display(Name = "Razón Social")]
        public string? RazonSocial { get; set; }

        [Display(Name = "Nombre Comercial")]
        public string? NombreComercial { get; set; }

        [Display(Name = "Fecha Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        public string? Genero { get; set; }

        [Display(Name = "Teléfono Principal")]
        public string TelefonoPrincipal { get; set; } = string.Empty;

        [Display(Name = "Teléfono Secundario")]
        public string? TelefonoSecundario { get; set; }

        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Departamento { get; set; }
        public string Pais { get; set; } = "Colombia";

        [Display(Name = "Empresa donde Labora")]
        public string? EmpresaLabora { get; set; }

        public string? Cargo { get; set; }

        [Display(Name = "Ingreso Mensual")]
        public decimal? IngresoMensual { get; set; }

        [Display(Name = "Límite Crédito")]
        public decimal LimiteCredito { get; set; }

        [Display(Name = "Estado Cliente")]
        public string EstadoCliente { get; set; } = "ACTIVO";

        public DateTime FechaRegistro { get; set; }
        public string? Observaciones { get; set; }
    }
}