namespace ERPConcesionario.Models
{
    public class CompraCriticaViewModel
    {
        public int IdInventarioProducto { get; set; }
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int? StockMaximo { get; set; }
        public int IdAlmacen { get; set; }
        public string Almacen { get; set; } = string.Empty;
        public int IdUbicacion { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public decimal CostoPromedio { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal IVA { get; set; }
        public int CantidadSugerida { get; set; }
        public string EstadoAlerta => StockActual == 0 ? "SIN STOCK" : "STOCK CRITICO";
    }
}
