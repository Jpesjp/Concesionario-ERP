namespace ERPConcesionario.Models
{
    public class NominaDetalleViewModel
    {
        public int IdNominaDetalle { get; set; }
        public int IdNomina { get; set; }
        public int IdEmpleado { get; set; }

        public string CodigoEmpleado { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public string TipoContrato { get; set; } = string.Empty;
        public string EstadoEmpleado { get; set; } = string.Empty;

        public decimal SalarioBase { get; set; }
        public decimal AuxilioTransporte { get; set; }
        public decimal Comisiones { get; set; }
        public decimal HorasExtra { get; set; }
        public decimal Bonificaciones { get; set; }
        public decimal SaludEmpleado { get; set; }
        public decimal PensionEmpleado { get; set; }
        public decimal Deducciones { get; set; }
        public decimal ParafiscalesEmpresa { get; set; }
        public decimal NetoPagar { get; set; }

        public string? Observaciones { get; set; }
    }
}