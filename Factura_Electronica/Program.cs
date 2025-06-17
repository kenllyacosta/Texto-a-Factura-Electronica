using DGIIFacturaElectronica;
using DGIIFacturaElectronica.Models;
using EpsonESCP;
using Factura_Electronica.Models;
using QRCoder;
using System.IO;
using System.Text.Json;
using static QRCoder.PayloadGenerator.SwissQrCode;

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

            //Consultamos el TrackId de la factura enviada
            Task.Run(async () =>
            {
                await ConsultarTrackId(facturaElectronica.RNCEmisor, facturaElectronica.SecuenciaGOB!);
            }).Wait();

            if (estatus.Contains("Aceptado"))
            {
                Timbre timbre = null!;
                //Consultamos su Timbre
                Task.Run(async () =>
                {
                    timbre = await ConsultarTimbre(facturaElectronica.FechaEmision, Empresa.RNCEmisor!, facturaElectronica.SecuenciaGOB!);
                }).Wait();

                //Create a QR Code using the Timbre information
                string qrCodePath = CrearCodigoQR(timbre);

                //Crear Print con la información del Timbre y la Factura
                PrintDocument(facturaElectronica, timbre, qrCodePath);

                //Descargamos el XML de la factura
                Task.Run(async () =>
                {
                    await DescargarXML(Empresa.RNCEmisor!, facturaElectronica.SecuenciaGOB!);
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

            string newFilePath = Path.Combine(newFolder, $"{Empresa.RNCEmisor!.Trim()}{facturaElectronica.SecuenciaGOB}_factura.txt");
            File.WriteAllText(newFilePath, File.ReadAllText(ruta));
        }

        private static void PrintDocument(FacturaElectronica facturaElectronica, Timbre timbre, string qrCodePath)
        {
            List<string> Lineas = new();
            string template = "{0,-24:N2}{1,14:N2}{2,18:N2}";
            int tamanoImpresionNoFiscal = 56;

            Lineas.AddRange(EncabezadoLineas());
            Lineas.Add(" ");
            if (timbre.fechaFirma != null)
                Lineas.Add($"{timbre.fechaFirma}");
            else
                Lineas.Add($"{facturaElectronica.FechaEmision} {DateTime.Now.ToString("HH:mm:ss")}");
            Lineas.Add($"e-NCF: {facturaElectronica.SecuenciaGOB}");

            if (!facturaElectronica.SecuenciaGOB.StartsWith("E32"))
            {
                Lineas.Add($"{facturaElectronica.RNCComprador}");
                Lineas.Add($"{facturaElectronica.RazonSocialComprador}");
            }

            string raya = new string('-', tamanoImpresionNoFiscal);
            Lineas.Add(raya);
            if (facturaElectronica.SecuenciaGOB.StartsWith("E31"))
                Lineas.Add("Factura de Crédito Fiscal Electronica".StringCenter(tamanoImpresionNoFiscal));
            if (facturaElectronica.SecuenciaGOB.StartsWith("E32"))
                Lineas.Add("Factura de Consumo Electronica".StringCenter(tamanoImpresionNoFiscal));
            if (facturaElectronica.SecuenciaGOB.StartsWith("E44"))
                Lineas.Add("Regimenes Especiales Electronico".StringCenter(tamanoImpresionNoFiscal));
            if (facturaElectronica.SecuenciaGOB.StartsWith("E45"))
                Lineas.Add("Gubernamental Electronico".StringCenter(tamanoImpresionNoFiscal));
            if (facturaElectronica.SecuenciaGOB.StartsWith("E46"))
                Lineas.Add("Comprobante de Exportaciones Electronico".StringCenter(tamanoImpresionNoFiscal));
            if (facturaElectronica.SecuenciaGOB.StartsWith("E41"))
                Lineas.Add("Compras Electronico".StringCenter(tamanoImpresionNoFiscal));
            if (facturaElectronica.SecuenciaGOB.StartsWith("E43"))
                Lineas.Add("Gastos Menores Electronico".StringCenter(tamanoImpresionNoFiscal));
            if (facturaElectronica.SecuenciaGOB.StartsWith("E47"))
                Lineas.Add("Comprobante para Pagos al Exterior Electronico".StringCenter(tamanoImpresionNoFiscal));
            Lineas.Add(raya);
            Lineas.Add(string.Format(template, "Descripcion", "Imp.", "Valor"));
            Lineas.Add(raya);

            decimal impuestos = 0, total = 0;
            foreach (var item in facturaElectronica.Detalles)
            {
                Lineas.Add($"{item.Cantidad} x {item.Precio}");
                decimal cantidadPorPrecio = item.Cantidad * item.Precio;
                if (facturaElectronica.IndicadorMontoGravado == 0)//no tienen ITBIS incluido
                {
                    if (item.IndicadorFacturacion == 1) //18%
                        Lineas.Add(string.Format(template, item.Descripcion.Trim(), $"{1.18m}", item.Cantidad * item.Precio));
                }
                else //si tienen ITBIS incluido
                {
                    if (item.IndicadorFacturacion == 1) //18%
                    {
                        decimal imp = cantidadPorPrecio - (cantidadPorPrecio / 1.18m);
                        Lineas.Add(string.Format(template, item.Descripcion.Trim(), $"{imp:N2}", $"{cantidadPorPrecio:N2}"));
                        impuestos += imp;
                    }
                }
                total += cantidadPorPrecio;
            }
            Lineas.Add(raya);
            Lineas.Add(string.Format(template, "Subtotal", string.Format("{0:N2}", impuestos), string.Format("{0:N2}", total)));
            Lineas.Add(string.Format(template, "Desc/Rec.", "", "0.00"));
            Lineas.Add(string.Format(template, "Total", string.Format("{0:N2}", impuestos), string.Format("{0:N2}", total)));
            Lineas.Add(" ");

            //1: Contado 2: Crédito
            if (facturaElectronica.TipoPago != null)
            {
                if (facturaElectronica.TipoPago == 1)
                    Lineas.Add("Contado");
                else
                    Lineas.Add("Credito");
            }

            Lineas.Add(" ");
            decimal totalCobrado = 0;
            if (facturaElectronica.FormaPago != null && facturaElectronica.MontoPago != null)
                for (int i = 0; i < facturaElectronica.FormaPago.Length; i++)
                {
                    totalCobrado += facturaElectronica.MontoPago[i];
                    if (facturaElectronica.FormaPago[i] == 1) //Efectivo
                        Lineas.Add(string.Format(template, "Efectivo", "", $"{facturaElectronica.MontoPago[i]:N2}"));
                    if (facturaElectronica.FormaPago[i] == 2) //Cheque/Transferencia/Depósito 
                        Lineas.Add(string.Format(template, "Cheq/Trans/Dep", "", $"{facturaElectronica.MontoPago[i]:N2}"));
                    if (facturaElectronica.FormaPago[i] == 3) //Tarjeta de Débito/Crédito
                        Lineas.Add(string.Format(template, "Tarjeta", "", $"{facturaElectronica.MontoPago[i]:N2}"));
                    if (facturaElectronica.FormaPago[i] == 4) //Venta a Crédito
                        Lineas.Add(string.Format(template, "Venta a Crédito", "", $"{facturaElectronica.MontoPago[i]:N2}"));

                    if (facturaElectronica.FormaPago[i] == 5) //Bonos o Certificados de regalo
                        Lineas.Add(string.Format(template, "Bonos o Certificados", "", $"{facturaElectronica.MontoPago[i]:N2}"));
                    if (facturaElectronica.FormaPago[i] == 6) //Permuta
                        Lineas.Add(string.Format(template, "Permuta", "", $"{facturaElectronica.MontoPago[i]:N2}"));
                    if (facturaElectronica.FormaPago[i] == 7) //Nota de crédito
                        Lineas.Add(string.Format(template, "Nota de crédito", "", $"{facturaElectronica.MontoPago[i]:N2}"));
                    if (facturaElectronica.FormaPago[i] == 8) //Otras Formas de pago
                        Lineas.Add(string.Format(template, "Otras Formas de pago", "", $"{facturaElectronica.MontoPago[i]:N2}"));
                }

            Lineas.Add(" ");
            if (totalCobrado > total)
                Lineas.Add(string.Format(template, "Cambio", "", $"{totalCobrado - total:N2}"));

            string newFolder = Path.Combine(Directory.GetCurrentDirectory(), "FacturasElectronicas", DateTime.Now.ToString("ddMMyyyy"));
            if (!Directory.Exists(newFolder))
                Directory.CreateDirectory(newFolder);

            foreach (var item in Lineas)
                LogLines(item, newFolder, $"{timbre.rncEmisor}{facturaElectronica.SecuenciaGOB}.txt");

            if (!string.IsNullOrWhiteSpace(Empresa!.PrinterPOS))
                ImpresionDeLineas(Lineas, qrCodePath.Replace("jpg", "png"), timbre.codigoSeguridad, timbre.fechaFirma!);
        }

        private static IEnumerable<string> EncabezadoLineas()
        {
            List<string> lineas = new();
            #region Reporte completo            
            byte tamanoImpresionNoFiscal = 56;

            if (!string.IsNullOrWhiteSpace(Empresa!.RazonSocialEmisor))
                lineas.Add(Empresa.NombreComercialEmisor!.Trim().ToUpper().StringCenter(44));

            if (!string.IsNullOrWhiteSpace(Empresa.DirecionEmisor))
                lineas.Add(Empresa.DirecionEmisor.Trim().StringCenter(tamanoImpresionNoFiscal));
            if (!string.IsNullOrWhiteSpace(Empresa.RNCEmisor))
                lineas.Add($"{Empresa.RNCEmisor.Trim()}".StringCenter(tamanoImpresionNoFiscal));

            if (!string.IsNullOrWhiteSpace(Empresa.Telefono))
                lineas.Add(Empresa.Telefono.Trim().StringCenter(tamanoImpresionNoFiscal));
            #endregion

            return lineas;
        }

        internal static void ImpresionDeLineas(List<string> lineas)
        {
            ImpresionESC impresionESC = new(Empresa!.PrinterPOS!.Trim());
            foreach (var linea in lineas)
            {
                if (linea.Trim().StartsWith("1|") || linea.Trim().StartsWith("2|"))
                {
                    string[] valores = linea.Split(new string[] { "|" }, StringSplitOptions.None);
                    bool negrita = valores[1] != "0";
                    string line = linea.Trim().StartsWith("2|") ? valores[1] : valores[7];

                    if (negrita)
                        impresionESC.AddNewLine(line, Formatos.NormalNegrita);
                }
                else
                {
                    impresionESC.AddNewLine(linea, Formatos.NormalNegrita);
                }
            }

            impresionESC.CortarPapel();

            impresionESC.Print();
            impresionESC.Close();
        }

        internal static void ImpresionDeLineas(List<string> lineas, string qrPath, string code, string fechaFirma)
        {
            ImpresionESC impresionESC = new(Empresa!.PrinterPOS!.Trim());
            foreach (var linea in lineas)
            {
                if (linea.Trim().StartsWith("1|") || linea.Trim().StartsWith("2|"))
                {
                    string[] valores = linea.Split(new string[] { "|" }, StringSplitOptions.None);
                    bool negrita = valores[1] != "0";
                    string line = linea.Trim().StartsWith("2|") ? valores[1] : valores[7];

                    if (negrita)
                        impresionESC.AddNewLine(line, Formatos.NormalNegrita);
                }
                else
                {
                    impresionESC.AddNewLine(linea, Formatos.NormalNegrita);
                }
            }

            if (string.IsNullOrWhiteSpace(qrPath))
                impresionESC.CortarPapel();

            impresionESC.Print();
            impresionESC.Close();

            if (!string.IsNullOrWhiteSpace(qrPath))
            {
                impresionESC = new ImpresionESC(Empresa!.PrinterPOS!.Trim());
                impresionESC.AddNewLine("", Formatos.NormalNegrita);
                impresionESC.ImprimirImagen(qrPath, Empresa.QrWidth, Empresa.QrHeight, 0, Empresa.QrBlankColumnsBefore);

                impresionESC.AddNewLine($"Codigo de Seguridad: {code}".StringCenter(56));
                impresionESC.AddNewLine($"Fecha de Firma Digital: {fechaFirma}".StringCenter(56));

                impresionESC.CortarPapel();
                impresionESC.Print();
                impresionESC.Close();
            }
        }

        private static string CrearCodigoQR(Timbre timbre)
        {
            string qrCodePath = string.Empty;
            if (timbre == null || string.IsNullOrWhiteSpace(timbre.urlImage))
            {
                qrCodePath = "No se pudo crear el código QR porque no se obtuvo un Timbre válido.";
                LogMessage(qrCodePath);
                return qrCodePath;
            }
            //Create a QR Code using the Timbre information
            qrCodePath = Path.Combine(Directory.GetCurrentDirectory(), "QR", $"{timbre.rncEmisor}{timbre.encf}.jpg");
            if (!Directory.Exists(Path.GetDirectoryName(qrCodePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(qrCodePath)!);

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(timbre.urlImage, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(qrCodeData);
            byte[] pngBytes = pngQrCode.GetGraphic(20);
            File.WriteAllBytes(qrCodePath.Replace(".jpg", ".png"), pngBytes);

            LogMessage($"Código QR generado en: {qrCodePath}");
            return qrCodePath;
        }

        private static FacturaElectronica CrearFacturaElectronica(FacturaTxt facturaFiscal, List<ItemTxt> items)
        {
            List<Detalle> Detalles = new();
            foreach (var item in items)
                Detalles.Add(new Detalle(byte.Parse(item.Tipo!), decimal.Parse(item.Precio!), item.Descripcion!.Trim().Length > 24 ? item.Descripcion.Trim().Substring(0, 24) : item.Descripcion.Trim(), item.Codigo, decimal.Parse(item.Cantidad!)));

            var result = new FacturaElectronica(Detalles.ToArray(),
                facturaFiscal.NCF, facturaFiscal.FechaVencimientoSecuencia, facturaFiscal.TipoDeIngreso,
                Empresa!.RNCEmisor, Empresa.RazonSocialEmisor,
                Empresa.DirecionEmisor, facturaFiscal.FechaEmision, facturaFiscal.RNCDelComprador,
                facturaFiscal.RazonSocialDelComprador, byte.Parse(facturaFiscal.Condiciones!),
                byte.Parse(facturaFiscal.IndicadorMontoGravado!),
                Empresa.Municipio, facturaFiscal.Municipio, Empresa.Provincia, facturaFiscal.Provincia);

            //1: Efectivo
            //2: Cheque/Transferencia/Depósito
            //3: Tarjeta de Débito/Crédito
            //4: Venta a Crédito
            //5: Bonos o Certificados de regalo
            //6: Permuta
            //7: Nota de crédito
            //8: Otras Formas de pago
            List<byte> tiposDePago = new();
            List<decimal> montosDePago = new();
            if (!string.IsNullOrWhiteSpace(facturaFiscal.Efectivo))
            {
                tiposDePago.Add(1);
                montosDePago.Add(decimal.Parse(facturaFiscal.Efectivo));
            }
            if (!string.IsNullOrWhiteSpace(facturaFiscal.ChequeTransferenciaDeposito))
            {
                tiposDePago.Add(2);
                montosDePago.Add(decimal.Parse(facturaFiscal.ChequeTransferenciaDeposito));
            }
            if (!string.IsNullOrWhiteSpace(facturaFiscal.TarjetaDebitoCredito))
            {
                tiposDePago.Add(3);
                montosDePago.Add(decimal.Parse(facturaFiscal.TarjetaDebitoCredito));
            }
            if (!string.IsNullOrWhiteSpace(facturaFiscal.VentaCredito))
            {
                tiposDePago.Add(4);
                montosDePago.Add(decimal.Parse(facturaFiscal.VentaCredito));
            }
            if (!string.IsNullOrWhiteSpace(facturaFiscal.BonosOCertificadosDeRegalo))
            {
                tiposDePago.Add(5);
                montosDePago.Add(decimal.Parse(facturaFiscal.BonosOCertificadosDeRegalo));
            }

            if (!string.IsNullOrWhiteSpace(facturaFiscal.Permuta))
            {
                tiposDePago.Add(6);
                montosDePago.Add(decimal.Parse(facturaFiscal.Permuta));
            }

            if (!string.IsNullOrWhiteSpace(facturaFiscal.NotaCredito))
            {
                tiposDePago.Add(7);
                montosDePago.Add(decimal.Parse(facturaFiscal.NotaCredito));
            }
            if (!string.IsNullOrWhiteSpace(facturaFiscal.OtrasFormasDePago))
            {
                tiposDePago.Add(8);
                montosDePago.Add(decimal.Parse(facturaFiscal.OtrasFormasDePago));
            }

            if (tiposDePago.Count != 0 && montosDePago.Count != 0)
            {
                result.FormaPago = [.. tiposDePago];
                result.MontoPago = [.. montosDePago];
            }

            return result;
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

            //Copy end replace the Factura.txt file with the new one in a new folder
            string newFolder = Path.Combine(Directory.GetCurrentDirectory(), "FacturasElectronicas", DateTime.Now.ToString("ddMMyyyy"));
            if (!Directory.Exists(newFolder))
                Directory.CreateDirectory(newFolder);

            string newFilePath = Path.Combine(newFolder, $"{rncEmisor}{eNcf}_TrackId.txt");
            await File.WriteAllTextAsync(newFilePath, JsonSerializer.Serialize(resultado));

            return resultado;
        }

        private static async Task<Timbre> ConsultarTimbre(string fecha, string rnc, string ncf)
        {
            Timbre resultado = await documentosElectronicos!.ConsultarTimbre(fecha, rnc, ncf);
            await LogMessageAsync(JsonSerializer.Serialize(resultado));

            if (resultado.fechaFirma != null && resultado.fechaEmision != null && resultado.encf != null && resultado.rncComprador != null && resultado.codigoSeguridad != null && resultado.montoTotal != null && resultado.urlImage != null && resultado.rncEmisor != null)
                await LogMessageAsync($"Fecha de Firma: {resultado.fechaFirma}\nfechaEmision: {resultado.fechaEmision}\nencf: {resultado.encf}\nRNC Comprador: {resultado.rncComprador}\nCódigo de Seguridad: {resultado.codigoSeguridad}\nMonto Total: {resultado.montoTotal}\nRrl Image: {resultado.urlImage}\nRNC Emisor: {resultado.rncEmisor}");

            //Copy end replace the Factura.txt file with the new one in a new folder
            string newFolder = Path.Combine(Directory.GetCurrentDirectory(), "FacturasElectronicas", DateTime.Now.ToString("ddMMyyyy"));
            if (!Directory.Exists(newFolder))
                Directory.CreateDirectory(newFolder);

            string newFilePath = Path.Combine(newFolder, $"{rnc}{ncf}_timbre.txt");
            await File.WriteAllTextAsync(newFilePath, JsonSerializer.Serialize(resultado));

            return resultado!;
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
            
            if (!string.IsNullOrWhiteSpace(resultado))
            {
                //Copy end replace the Factura.txt file with the new one in a new folder
                string newFolder = Path.Combine(Directory.GetCurrentDirectory(), "FacturasElectronicas", DateTime.Now.ToString("ddMMyyyy"));
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

        private static void LogLines(string message, string path, string fileName)
        {
            using (StreamWriter sw = new($"{path}\\{fileName}", append: true))
                sw.WriteLine($"{message}");
        }
    }
}