namespace ERPConcesionario.Models
{
    public class VentaInventarioOptionViewModel
    {
        public int IdInventarioProducto { get; set; }
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public string Almacen { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal IVA { get; set; }

        public string Texto =>
            CodigoProducto + " - " + NombreProducto + " | Stock: " + StockActual + " | " + Almacen + " / " + Ubicacion;
    }
}
