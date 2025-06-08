namespace Factura_Electronica.Models
{
    public class EmpresaConfig
    {
        public string? Usuario { get; set; }
        public string? Clave { get; set; }
        public string? URLBase { get; set; }
        public string? uRlAutenticacion { get; set; }
        public string? uRlRecepcion { get; set; }
        public string? uRlAprobacionComercial { get; set; }
        public string? RNCEmisor { get; set; }
        public string? RazonSocialEmisor { get; set; }
        public string? DirecionEmisor { get; set; }
        public string? Municipio { get; set; }
        public string? Provincia { get; set; }
    }
}