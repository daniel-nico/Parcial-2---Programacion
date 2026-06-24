using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SimuladorDronApp.Domain;
using SimuladorDronApp.Data;
using SimuladorDronApp.Presentation;

namespace SimuladorDronApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Cargar la configuración desde la RAÍZ del proyecto tal como pide el enunciado
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                IConfiguration config = builder.Build();
                string? connectionString = config.GetConnectionString("PostgreSQL");

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("[-] Error crítico: Falta la cadena de conexión en appsettings.json.");
                    return;
                }

                AccesoPostgre db = new AccesoPostgre(connectionString);
                db.InicializarBaseDeDatos();

                var (n, inicio) = InterfazConsola.SolicitarDatosEntrada();

                SimuladorVuelo simulador = new SimuladorVuelo(n, inicio);
                bool exito = simulador.Resolver();

                if (!exito)
                {
                    InterfazConsola.DibujarMatrizSinSolucion(inicio, n);
                    Console.WriteLine("\nPresione cualquier tecla para finalizar...");
                    Console.ReadKey();
                    return;
                }

                InterfazConsola.DibujarMatriz(simulador.MatrizResultante, n);

                var secuencia = InterfazConsola.ExtraerSecuenciaCronologica(
                    simulador.MatrizResultante,
                    n,
                    simulador.TotalAlcanzables
                );

                Console.WriteLine("\n[+] Guardando secuencia transaccional en PostgreSQL...");
                int masterId = db.GuardarSimulacion(n, inicio, secuencia);
                Console.WriteLine($"[+] Guardado exitoso. ID de Cabecera (tb_master_control): {masterId}");

                Console.WriteLine("\n=======================================================");
                Console.WriteLine("    REPORTE INVERSO (ÚLTIMOS 5 PASOS RECONSTRUIDOS)     ");
                Console.WriteLine("=======================================================");

                List<string> reporte = db.ObtenerReporteInverso(masterId);

                int idx = 0;
                while (idx < reporte.Count)
                {
                    Console.WriteLine(reporte[idx]);
                    idx++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[-] Ocurrió un error inesperado: {ex.Message}");
            }

            Console.WriteLine("\nPresione cualquier tecla para finalizar...");
            Console.ReadKey();
        }
    }
}