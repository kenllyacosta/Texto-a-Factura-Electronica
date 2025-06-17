# 📄 Documentación de Comandos - Factura_Electronica.exe

Este repositorio contiene la documentación detallada de los comandos disponibles en la herramienta `Factura_Electronica.exe`, una utilidad de línea de comandos para gestionar comprobantes fiscales electrónicos (facturas electrónicas) en República Dominicana. Desde un archivo .txt puedes convertirlo a una factura electrónica válida.

## 📌 Funcionalidades documentadas

- Consulta de tracking de NCF
- Descarga de archivos XML de facturas
- Timbrado electrónico (firma y validación)
- Estado de transacciones por UUID
- Información de certificados y tokens
- Consulta de directorios de receptores
- Revisión de NCF recibidos y aprobaciones
- Envío de aprobaciones y comunicación con compradores por API

## 📁 Contenido

- `Documentacion_Comandos_Factura_Electronica.md`: Versión en Markdown lista para visualizar en GitHub.
- `Documentacion_Comandos_Factura_Electronica.docx`: Versión en Word para impresión o uso offline.

## 🚀 Uso

Cada comando está explicado con:

- Descripción funcional
- Lista de parámetros
- Resultado esperado

Esto permite a desarrolladores e integradores comprender fácilmente cómo utilizar la herramienta desde scripts o aplicaciones externas.

```bash
.\Factura_Electronica.exe trackid 123456789 E310000000001
.\Factura_Electronica.exe descargarxml 123456789 E310000000001
.\Factura_Electronica.exe timbre 05-06-2025 123456789 E310000000001
.\Factura_Electronica.exe estatus e8367911-a1f3-450a-9cb2-cbf5490d069d
.\Factura_Electronica.exe infocertificado
.\Factura_Electronica.exe infotoken
.\Factura_Electronica.exe directorio 123456789
.\Factura_Electronica.exe ncfrecibidos 01-01-2023 31-12-2023
.\Factura_Electronica.exe aprobaciones 01-01-2023 31-12-2023
.\Factura_Electronica.exe enviaraprobacion 123456789 E310000000001 05-06-2025 123456789 E310000000001 1000.00 true "Aprobado por el comprador"
.\Factura_Electronica.exe descargarxml 123456789 E310000000001
.\Factura_Electronica.exe enviaracomprador https://example.com/autenticacion https://example.com/recepcion
```
