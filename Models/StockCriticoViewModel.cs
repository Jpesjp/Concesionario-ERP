namespace ERPConcesionario.Models
{
    public class StockCriticoViewModel
    {
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int StockMinimo { get; set; }
        public int StockActual { get; set; }
        public string Almacen { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public string EstadoAlerta =>
            StockActual == 0 ? "SIN STOCK" :
            StockActual <= StockMinimo ? "STOCK CRÍTICO" :
            "NORMAL";
    }
}