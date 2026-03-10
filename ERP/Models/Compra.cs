namespace ERP.Models
{
    public class Compra
    {
        public int IdCompra { get; set; }

        public int IdProveedor { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string? NombreProveedor { get; set; }
    }
}