namespace Factura_Electronica.Models
{
    internal class FacturaTxt
    {
        /// <summary>
        /// 
        /// </summary>
        public string? TipoDeFactura { get; set; } //1
        
        /// <summary>
        /// NCF de la factura (A,O) 19
        /// </summary>
        public string? NCF { get; set; } //7
        /// <summary>
        /// Razón social del comprador (RT,O) 40
        /// </summary>
        public string? RazonSocialDelComprador { get; set; } //8
        /// <summary>
        /// RNC del comprador (A,O) 11
        /// </summary>
        public string? RNCDelComprador { get; set; } //9
        /// <summary>
        /// NCF de referencia (A,O) 19
        /// Opcional para las facturas
        /// </summary>
        public string? NCFReferencia { get; set; } //10
        /// <summary>
        /// Para realizar un descuento a la factura
        /// </summary>
        public string? Descuento { get; set; } //11
        /// <summary>
        /// Para realizar un recargo a la factura
        /// </summary>
        public string? Recargo { get; set; } //12
        /// <summary>
        /// Para cargarle un monto de propina a la factura
        /// </summary>
        public string? Propina { get; set; } //13
        /// <summary>
        /// Comentarios en la factura de hasta 4,000 caracteres
        /// </summary>
        public string? Comentarios { get; set; } //15
        
        //Tipos de pago para la factura
        public string? Efectivo { get; set; }
        public string? ChequeTransferenciaDeposito { get; set; }
        public string? TarjetaDebitoCredito { get; set; }
        public string? VentaCredito { get; set; }
        public string? BonosOCertificadosDeRegalo { get; set; }
        public string? Permuta { get; set; }
        public string? NotaCredito { get; set; }
        public string? OtrasFormasDePago { get; set; }

        //Opciones electronicas
        public string? FechaVencimientoSecuencia { get; set; }
        public string? TipoDeIngreso { get; set; }
        public string? FechaEmision { get; set; }
        public string? Condiciones { get; set; }
        public string? IndicadorMontoGravado { get; set; }
        public string? Municipio { get; set; }
        public string? Provincia { get; set; }
    }
}