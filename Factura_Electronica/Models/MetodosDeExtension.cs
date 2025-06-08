namespace Factura_Electronica.Models
{
    public static class MetodosDeExtension
    {
        public static byte ToByte(this string valor)
        {
            if (byte.TryParse(valor, out byte Resultado))
                return Resultado;

            return Resultado;
        }
    }
}