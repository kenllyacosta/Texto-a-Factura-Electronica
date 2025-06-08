namespace Factura_Electronica.Models
{
    internal class ItemTxt
    {
        /// <summary>
        /// 1 – Ítem de venta.
        /// 2 – Anulación de ítem de venta.
        /// 3 – Descuento por ítem.
        /// 4 – Recargo por ítem.
        /// </summary>
        public string? Tipo { get; set; } //1
        /// <summary>
        /// Cantidad (N) 5,3
        /// </summary>
        public string? Cantidad { get; set; } //2
        /// <summary>
        /// Descripción del ítem (RT) max 400
        /// </summary>
        public string? Descripcion { get; set; } //3
        /// <summary>
        /// Precio unitario (N) 7,2
        /// </summary>
        public string? Precio { get; set; } //4
        /// <summary>
        /// Tasa de ITBIS (N) 2,2
        /// </summary>
        public string? TasaImpuesto { get; set; } //5

        public string? Codigo { get; set; } //6

        public string? UnidadDeMedida { get; set; }//7
    }
}