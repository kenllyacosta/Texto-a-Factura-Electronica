using DGIIFacturaElectronica;
using DGIIFacturaElectronica.Models;
using Factura_Electronica.Models;
using System.Text.Json;

namespace Factura_Electronica
{
    public static class Program
    {
        private static DocumentosElectronicos? documentosElectronicos;
        private static EmpresaConfig? Empresa;
        public static void Main(string[] args)
        {
            //Read the config file
            Empresa = JsonSerializer.Deserialize<EmpresaConfig>(File.ReadAllText("Empresa.json"))!;
            documentosElectronicos = new DocumentosElectronicos(Empresa.Usuario, Empresa.Clave, Empresa.URLBase);

            //Example: certificado C:\\Electronico\\certificado.pfx clave123456 RNCEmisor
            if (args.Contains("certificado"))
            {
                Task.Run(async () =>
                {
                    await SubirCertificado(Directory.GetCurrentDirectory() + "\\" + args[1], args[2], Empresa.RNCEmisor!.Trim());
                }).Wait();
                LogMessage("Subida de certificado completada. Verifique el archivo certificado.txt para los resultados.");
                return;
            }

            //Example:trackid 132071751 E310000001615
            if (args.Contains("trackid"))
            {
                Task.Run(async () =>
                {
                    await ConsultarTrackId(args[1], args[2]);
                }).Wait();
                LogMessage("Consulta de TrackId completada. Verifique el archivo trackid.txt para los resultados.");
                return;
            }

            ///Example: timbre 05-06-2025 132071751 E310000001615
            if (args.Contains("timbre"))
            {
                Task.Run(async () =>
                {
                    await ConsultarTimbre(args[1], args[2], args[3]);
                }).Wait();
                LogMessage("Consulta de Timbre completada. Verifique el archivo timbre.txt para los resultados.");
                return;
            }

            // Example: estatus e8367911-a1f3-450a-9cb2-cbf5490d069c
            if (args.Contains("estatus"))
            {
                Task.Run(async () =>
                {
                    await ConsultarEstatus(args[1]);
                }).Wait();
                LogMessage("Consulta de estatus completada. Verifique el archivo estatus.txt para los resultados.");
                return;
            }

            // Example: infocertificado
            if (args.Contains("infocertificado"))
            {
                Task.Run(async () =>
                {
                    await InformacionDeCertificado();
                }).Wait();
                LogMessage("Consulta de información de certificado completada.");
                return;
            }

            // Example: infotoken
            if (args.Contains("infotoken"))
            {
                Task.Run(async () =>
                {
                    await InformacionDelToken();
                }).Wait();
                LogMessage("Consulta de información de token completada.");
                return;
            }

            // Example: directorio 132071751.
            if (args.Contains("directorio"))
            {
                Task.Run(async () =>
                {
                    await ConsultarDirectorio(args[1]);
                }).Wait();
                LogMessage("Consulta de directorio completada.");
                return;
            }

            // Example: ncfrecibidos 01-01-2023 31-12-2023
            if (args.Contains("ncfrecibidos"))
            {
                Task.Run(async () =>
                {
                    await NCFRecibidos(DateTime.Parse(args[1]), DateTime.Parse(args[2]));
                }).Wait();
                LogMessage("Consulta de NCF recibidos completada. Verifique el archivo e-log.txt para los resultados.");
                return;
            }

            // Example: aprobaciones 01-01-2023 31-12-2023
            if (args.Contains("aprobaciones"))
            {
                Task.Run(async () =>
                {
                    await AprobacionesComerciales(DateTime.Parse(args[1]), DateTime.Parse(args[2]));
                }).Wait();
                LogMessage("Consulta de aprobaciones comerciales completada. Verifique el archivo e-log.txt para los resultados.");
                return;
            }

            // Example: enviaraprobacion 132071751 E310000001615 05-06-2025 132071751 E310000001615 1000.00 true "Aprobado por el comprador"
            if (args.Contains("enviaraprobacion"))
            {
                Task.Run(async () =>
                {
                    await EnviarAprobacionComercialAComprador(args[1], args[2], args[3], args[4], args[5], args[6], decimal.Parse(args[7]), bool.Parse(args[8]), args[9]);
                }).Wait();
                LogMessage("Envío de aprobación comercial completado. Verifique el archivo e-log.txt para los resultados.");
                return;
            }

            // Example: descargarxml 132071751 E310000000001
            if (args.Contains("descargarxml"))
            {
                Task.Run(async () =>
                {
                    await DescargarXML(args[1], args[2]);
                }).Wait();
                LogMessage("Descarga de XML completada. Verifique el archivo e-log.txt para los resultados.");
                return;
            }

            string ruta = @"C:\Electronico\factura.txt";
            if (!File.Exists(ruta))
            {
                LogMessage("El archivo de la factura no existe en la ruta correcta.\n\nC:\\Electronico\\Factura.txt");

                Console.Beep(); Console.Beep(); Console.Beep();
                return;
            }
            string[] Factura = File.ReadAllLines(ruta, System.Text.Encoding.Latin1);

            //Creamos el objeto factura y los Items
            FacturaTxt FacturaFiscal = CrearFacturaFiscalDesdeTexto(Factura);
            List<ItemTxt> items = CrearItemsFiscalDesdeTexto(Factura);

            FacturaElectronica facturaElectronica = CrearFacturaElectronica(FacturaFiscal, items);

            //Example: enviaracomprador https://example.com/autenticacion https://example.com/recepcion
            if (args.Contains("enviaracomprador"))
            {
                Task.Run(async () =>
                {
                    await EnviarFacturaAComprador(facturaElectronica, args[1], args[2]);
                }).Wait();
                LogMessage("Envío de factura a comprador completado. Verifique el archivo e-log.txt para los resultados.");
                return;
            }

            string estatus = string.Empty;
            Task.Run(async () =>
            {
                estatus = await EnviarFactura(facturaElectronica);
            }).Wait();

            if (estatus.Contains("Aceptado"))
            {
                //Consultamos su Timbre
                Task.Run(async () =>
                {
                    await ConsultarTimbre(facturaElectronica.FechaEmision, Empresa.RNCEmisor!, facturaElectronica.SecuenciaGOB!);
                }).Wait();
            }
            else
            {
                LogMessage($"La factura no fue aceptada. Estatus: {estatus}");
                Console.Beep(); Console.Beep(); Console.Beep();
                return;
            }

            //Copy end replace the Factura.txt file with the new one in a new folder
            string newFolder = Path.Combine(Directory.GetCurrentDirectory(), "FacturasElectronicas", DateTime.Now.ToString("ddMMyyyy"));
            if (!Directory.Exists(newFolder))
                Directory.CreateDirectory(newFolder);

            string newFilePath = Path.Combine(newFolder, $"{Empresa.RNCEmisor!.Trim()}{facturaElectronica.SecuenciaGOB}.txt");
            File.WriteAllText(newFilePath, File.ReadAllText(ruta));
        }

        private static FacturaElectronica CrearFacturaElectronica(FacturaTxt facturaFiscal, List<ItemTxt> items)
        {
            List<Detalle> Detalles = new();
            foreach (var item in items)
                Detalles.Add(new Detalle(byte.Parse(item.Tipo!), decimal.Parse(item.Precio!), item.Descripcion, item.Codigo, decimal.Parse(item.Cantidad!)));

            return new FacturaElectronica(Detalles.ToArray(),
                facturaFiscal.NCF, facturaFiscal.FechaVencimientoSecuencia, facturaFiscal.TipoDeIngreso,
                Empresa!.RNCEmisor, Empresa.RazonSocialEmisor,
                Empresa.DirecionEmisor, facturaFiscal.FechaEmision, facturaFiscal.RNCDelComprador,
                facturaFiscal.RazonSocialDelComprador, byte.Parse(facturaFiscal.Condiciones!),
                byte.Parse(facturaFiscal.IndicadorMontoGravado!),
                Empresa.Municipio, facturaFiscal.Municipio, Empresa.Provincia, facturaFiscal.Provincia);
        }

        private static List<ItemTxt> CrearItemsFiscalDesdeTexto(string[] Factura)
        {
            List<ItemTxt> Items = new();

            try
            {
                foreach (var linea in Factura)
                {
                    if (!linea.Contains("||"))
                        continue;

                    string[] datos = linea.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (datos[0] == "1")
                        Items.Add(new ItemTxt()
                        {
                            Tipo = datos[0].Replace(".", "").Replace(",", "").Trim(),
                            Cantidad = datos[1].Trim(),
                            Descripcion = datos[2].Trim(),
                            Precio = datos[3].Trim(),
                            TasaImpuesto = datos[4].Trim(),
                            Codigo = datos.Length > 5 ? datos[5].Replace(".", "").Replace(",", "").Trim() : Guid.NewGuid().ToString()[..5].ToUpper(),
                            UnidadDeMedida = datos.Length > 6 ? datos[6].Replace(".", "").Replace(",", "").Trim() : "UND"
                        });
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Linea de items: {ex.Message}");
            }
            return Items;
        }

        private static FacturaTxt CrearFacturaFiscalDesdeTexto(string[] Factura)
        {
            FacturaTxt Resultado = null!;

            try
            {
                string LineaFactura = Factura[0];
                string[] datosFactura = LineaFactura.Split(new string[] { "||" }, StringSplitOptions.None);

                //Validamos el RNC que solo sea numerico
                string rncValidadoSoloNumeros = "";
                System.Text.RegularExpressions.Regex reg = new("^[0-9]*$");

                if (reg.IsMatch(datosFactura[3].Trim().Replace("-", "")))
                    rncValidadoSoloNumeros = datosFactura[3].Trim().Replace("-", "");

                //Creamos el objeto Factura
                Resultado = new FacturaTxt()
                {
                    TipoDeFactura = datosFactura[0].Trim(), //0                   
                    NCF = datosFactura[1].Trim(),//1
                    RazonSocialDelComprador = datosFactura[2].Trim(),//2
                    RNCDelComprador = rncValidadoSoloNumeros,//3
                    NCFReferencia = datosFactura[4].Trim(),//4
                    Descuento = datosFactura[5].Replace(".", "").Replace(",", "").Replace("-", "").Trim(),//5
                    Recargo = datosFactura[6].Replace(".", "").Replace(",", "").Replace("-", "").Trim(),//6
                    Propina = datosFactura[7].Replace(".", "").Replace(",", "").Trim(),//7
                    Comentarios = datosFactura[8],//8

                    //Formas de pago
                    Efectivo = datosFactura[9], //9
                    ChequeTransferenciaDeposito = datosFactura[10],//10
                    TarjetaDebitoCredito = datosFactura[11], //11
                    VentaCredito = datosFactura[12],//12
                    BonosOCertificadosDeRegalo = datosFactura[13],//13
                    Permuta = datosFactura[14],//14
                    NotaCredito = datosFactura[15],//15
                    OtrasFormasDePago = datosFactura[16],//16 

                    //Campos electronicos
                    FechaVencimientoSecuencia = datosFactura[17],//17
                    TipoDeIngreso = datosFactura[18],//18
                    FechaEmision = datosFactura[19],//19
                    Condiciones = datosFactura[20],//20
                    IndicadorMontoGravado = datosFactura[21],//21
                    Municipio = datosFactura[22],//22
                    Provincia = datosFactura[23],//23
                };
            }
            catch (Exception ex)
            {
                LogMessage($"Linea 1: {ex.Message}");
            }

            return Resultado!;
        }

        private static async Task SubirCertificado(string ruta, string clave, string rncEmisor)
        {
            string resultado = await documentosElectronicos!.UploadCertificate(ruta, clave, rncEmisor);
            Console.WriteLine(resultado);

            //Escribimos el resultado en el archivo con su nombre
            using (StreamWriter sw = new("certificado.txt", true))
                await sw.WriteLineAsync(resultado);

            await LogMessageAsync(resultado);
        }

        private static async Task<string> EnviarFactura(FacturaElectronica facturaElectronica)
        {
            string resultado = string.Empty;
            try
            {
                documentosElectronicos!.Excepcion += DocumentosElectronicos_Excepcion;
                resultado = await documentosElectronicos!.EnviarFactura(facturaElectronica);
                documentosElectronicos.Excepcion -= DocumentosElectronicos_Excepcion;
                await LogMessageAsync(resultado);
            }
            catch (Exception ex)
            {
                await LogMessageAsync(ex.Message);
            }
            return resultado;
        }

        private static void DocumentosElectronicos_Excepcion(object sender, ExceptionEventArgs e)
        {
            LogMessage(message: $"Error: {e.Exception.Message}");
            if (e.Exception.InnerException != null)
                LogMessage(message: $"Error: {e.Exception.InnerException.Message}");
        }

        private static async Task<List<TrackId>> ConsultarTrackId(string rncEmisor, string eNcf)
        {
            List<TrackId> resultado = await documentosElectronicos!.ConsultarTrackId(rncEmisor, eNcf);
            Console.WriteLine(JsonSerializer.Serialize(resultado));

            //Escribimos el resultado en el archivo con su nombre
            using (StreamWriter sw = new("trackid.txt", true))
            {
                foreach (var item in resultado)
                {
                    await LogMessageAsync($"TrackId: {item.trackId}\nMensaje: {item.mensaje}\nEstado: {item.estado}\nFecha Recepcion: {item.fechaRecepcion}");
                    await sw.WriteLineAsync(JsonSerializer.Serialize(item));
                }
            }

            return resultado;
        }

        private static async Task<Timbre> ConsultarTimbre(string fecha, string rnc, string ncf)
        {
            Timbre resultado = await documentosElectronicos!.ConsultarTimbre(fecha, rnc, ncf);
            Console.WriteLine(JsonSerializer.Serialize(resultado));

            await LogMessageAsync($"Fecha de Firma: {resultado.fechaFirma}\nfechaEmision: {resultado.fechaEmision}\nencf: {resultado.encf}\nRNC Comprador: {resultado.rncComprador}\nCódigo de Seguridad: {resultado.codigoSeguridad}\nMonto Total: {resultado.montoTotal}\nRrl Image: {resultado.urlImage}\nRNC Emisor: {resultado.rncEmisor}");

            //Escribimos el resultado en el archivo con su nombre
            using (StreamWriter sw = new("timbre.txt", true))
                await sw.WriteLineAsync(JsonSerializer.Serialize(resultado));

            return resultado;
        }

        private static async Task<string> ConsultarEstatus(string trackId)
        {
            string resultado = await documentosElectronicos!.ConsultarEstatus(trackId);
            Console.WriteLine(resultado);

            //Escribimos el resultado en el archivo con su nombre
            using (StreamWriter sw = new("estatus.txt", true))
                await sw.WriteLineAsync(JsonSerializer.Serialize(resultado));

            return resultado;
        }

        private static async Task<CertInfo> InformacionDeCertificado()
        {
            CertInfo Resultado = await documentosElectronicos!.InformacionDeCertificado();
            Console.WriteLine(JsonSerializer.Serialize(Resultado));

            //Escribimos el resultado en el archivo con su nombre
            using (StreamWriter sw = new("infoCertificado.txt", true))
                await sw.WriteLineAsync(JsonSerializer.Serialize(Resultado));

            return Resultado;
        }

        private static async Task<Token> InformacionDelToken()
        {
            Token Resultado = await documentosElectronicos!.InformacionDelToken();
            Console.WriteLine(JsonSerializer.Serialize(Resultado));

            //Escribimos el resultado en el archivo con su nombre
            using (StreamWriter sw = new("infotoken.txt", true))
                await sw.WriteLineAsync(JsonSerializer.Serialize(Resultado));

            await LogMessageAsync(Resultado.token);
            return Resultado;
        }

        private static async Task<Directorio> ConsultarDirectorio(string rnc)
        {
            Directorio Resultado = await documentosElectronicos!.ConsultaDirectorio(rnc);
            Console.WriteLine(JsonSerializer.Serialize(Resultado));

            //Escribimos el resultado en el archivo con su nombre
            using (StreamWriter sw = new StreamWriter("directorio.txt", true))
                await sw.WriteLineAsync(JsonSerializer.Serialize(Resultado));

            await LogMessageAsync($"RNC: {Resultado.rnc}\n Nombre: {Resultado.nombre}\n URL de recepción: {Resultado.urlRecepcion}\n urlAceptacion: {Resultado.urlAceptacion}\n urlOpcional: {Resultado.urlOpcional}");
            return Resultado;
        }

        private static async Task<ARECF> EnviarFacturaAComprador(FacturaElectronica facturaElectronica, string uRlAutenticacion, string uRlRecepcion)
        {
            ARECF resultado = await documentosElectronicos!.EnviarNCFAComprador(facturaElectronica, uRlAutenticacion, uRlRecepcion);
            if (resultado != null)
                await LogMessageAsync($"Detalle de Acuse de Recibo: {resultado.detalleAcusedeRecibo}\nAny: {resultado.any}");
            else
                await LogMessageAsync($"No hay resultados de este envío para: URL Autenticación: {uRlAutenticacion}\nURL Recepción: {uRlRecepcion}");

            return resultado!;
        }

        private static async Task<List<eNCFRecibido>> NCFRecibidos(DateTime fecha1, DateTime fecha2)
        {
            List<eNCFRecibido> resultado = await documentosElectronicos!.NCFRecibidos(fecha1, fecha2);
            Console.WriteLine(JsonSerializer.Serialize(resultado));

            if (resultado.Count != 0)
            {
                foreach (var item in resultado)
                {
                    using (StreamWriter sw = new("NCFRecibidos.txt", true))
                        await sw.WriteLineAsync(JsonSerializer.Serialize(item));

                    await LogMessageAsync(JsonSerializer.Serialize(item));
                }
            }
            else
            {
                await LogMessageAsync("No hay resultados disponibles para esta consulta.");
            }

            return resultado;
        }

        private static async Task<List<AprobacionComercial>> AprobacionesComerciales(DateTime fecha1, DateTime fecha2)
        {
            List<AprobacionComercial> resultado = await documentosElectronicos!.AprobacionesComerciales(fecha1, fecha2);
            Console.WriteLine(JsonSerializer.Serialize(resultado));

            if (resultado.Count != 0)
            {
                foreach (var item in resultado)
                {
                    using (StreamWriter sw = new("AprobacionesComerciales.txt", true))
                        await sw.WriteLineAsync(JsonSerializer.Serialize(item));

                    await LogMessageAsync(JsonSerializer.Serialize(item));
                }
            }
            else
            {
                await LogMessageAsync("No hay resultados disponibles para esta consulta.");
            }

            return resultado;
        }

        private static async Task<string> EnviarAprobacionComercialAComprador(string uRlAprobacionComercial, string uRlAutenticacion, string rncEmisor, string rncComprador, string ncf, string fechaEmision, decimal montoTotal, bool aprobado, string comentarios)
        {
            string resultado = await documentosElectronicos!.EnviarAprobacionComercialAComprador(uRlAprobacionComercial, uRlAutenticacion, rncEmisor, rncComprador, ncf, fechaEmision, montoTotal, aprobado, comentarios);
            Console.WriteLine(resultado);

            if (!string.IsNullOrWhiteSpace(resultado))
            {
                //Escribimos el resultado en el archivo con su nombre
                using (StreamWriter sw = new("envioAComprador.txt", true))
                    await sw.WriteLineAsync(JsonSerializer.Serialize(resultado));

                await LogMessageAsync(resultado);
            }
            else
            {
                await LogMessageAsync("No hay resultados disponibles para esta consulta.");
            }

            return resultado;
        }

        private static async Task<string> DescargarXML(string rnc, string ncf)
        {
            string resultado = await documentosElectronicos!.DescargaXMLFirmado(rnc, ncf);
            Console.WriteLine(resultado);

            if (!string.IsNullOrWhiteSpace(resultado))
            {
                //Copy end replace the Factura.txt file with the new one in a new folder
                string newFolder = Path.Combine(Directory.GetCurrentDirectory(), "XMLs", DateTime.Now.ToString("ddMMyyyy"));
                if (!Directory.Exists(newFolder))
                    Directory.CreateDirectory(newFolder);

                string newFilePath = Path.Combine(newFolder, $"{rnc}{ncf}.xml");
                await File.WriteAllTextAsync(newFilePath, resultado);

                await LogMessageAsync(resultado);
            }
            else
            {
                await LogMessageAsync("No hay resultados disponibles para esta consulta.");
            }

            return resultado;
        }

        private static async Task LogMessageAsync(string message)
        {
            using (StreamWriter sw = new("e-log.txt", append: true))
                await sw.WriteLineAsync($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}    {message}");
        }

        private static void LogMessage(string message)
        {
            using (StreamWriter sw = new("e-log.txt", append: true))
                sw.WriteLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}    {message}");
        }
    }
}