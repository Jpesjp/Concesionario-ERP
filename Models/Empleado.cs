using System.ComponentModel.DataAnnotations;

namespace ERPConcesionario.Models
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }

        [Display(Name = "Código Empleado")]
        public string CodigoEmpleado { get; set; } = string.Empty;

        [Display(Name = "Sucursal")]
        public int IdSucursal { get; set; }

        [Display(Name = "Cargo")]
        public int IdCargo { get; set; }

        [Display(Name = "Primer Nombre")]
        public string PrimerNombre { get; set; } = string.Empty;

        [Display(Name = "Segundo Nombre")]
        public string? SegundoNombre { get; set; }

        [Display(Name = "Primer Apellido")]
        public string PrimerApellido { get; set; } = string.Empty;

        [Display(Name = "Segundo Apellido")]
        public string? SegundoApellido { get; set; }

        [Display(Name = "Tipo Documento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [Display(Name = "Número Documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Display(Name = "Fecha Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        public string Sexo { get; set; } = string.Empty;

        [Display(Name = "Estado Civil")]
        public string EstadoCivil { get; set; } = string.Empty;

        public string Nacionalidad { get; set; } = "Colombiana";
        public string Direccion { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Pais { get; set; } = "Colombia";

        [Display(Name = "Teléfono Personal")]
        public string TelefonoPersonal { get; set; } = string.Empty;

        [Display(Name = "Teléfono Secundario")]
        public string? TelefonoSecundario { get; set; }

        [Display(Name = "Email Personal")]
        public string? EmailPersonal { get; set; }

        [Display(Name = "Email Corporativo")]
        public string? EmailCorporativo { get; set; }

        [Display(Name = "Contacto Emergencia")]
        public string NombreContactoEmergencia { get; set; } = string.Empty;

        [Display(Name = "Parentesco Contacto Emergencia")]
        public string ParentescoContactoEmergencia { get; set; } = string.Empty;

        [Display(Name = "Teléfono Emergencia")]
        public string TelefonoEmergencia { get; set; } = string.Empty;

        [Display(Name = "Fecha Ingreso")]
        public DateTime FechaIngreso { get; set; }

        [Display(Name = "Fecha Salida")]
        public DateTime? FechaSalida { get; set; }

        [Display(Name = "Tipo Contrato")]
        public string TipoContrato { get; set; } = string.Empty;

        [Display(Name = "Salario Base")]
        public decimal SalarioBase { get; set; }

        [Display(Name = "Auxilio Transporte")]
        public decimal AuxilioTransporte { get; set; }

        [Display(Name = "Comisión Base")]
        public decimal ComisionBase { get; set; }

        public string? Banco { get; set; }

        [Display(Name = "Tipo Cuenta Bancaria")]
        public string? TipoCuentaBancaria { get; set; }

        [Display(Name = "Número Cuenta Bancaria")]
        public string? NumeroCuentaBancaria { get; set; }

        public string? EPS { get; set; }
        public string? AFP { get; set; }
        public string? ARL { get; set; }

        [Display(Name = "Caja Compensación")]
        public string? CajaCompensacion { get; set; }

        [Display(Name = "Tiene Licencia Conducir")]
        public bool TieneLicenciaConducir { get; set; }

        [Display(Name = "Categoría Licencia")]
        public string? CategoriaLicencia { get; set; }

        [Display(Name = "Fecha Vencimiento Licencia")]
        public DateTime? FechaVencimientoLicencia { get; set; }

        [Display(Name = "Talla Camisa")]
        public string? TallaCamisa { get; set; }

        [Display(Name = "Talla Pantalón")]
        public string? TallaPantalon { get; set; }

        [Display(Name = "Talla Calzado")]
        public string? TallaCalzado { get; set; }

        [Display(Name = "Estado Empleado")]
        public string EstadoEmpleado { get; set; } = "ACTIVO";

        [Display(Name = "Foto URL")]
        public string? FotoUrl { get; set; }

        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}