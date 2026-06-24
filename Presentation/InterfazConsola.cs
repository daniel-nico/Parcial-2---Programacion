using System;
using System.Collections.Generic;
using SimuladorDronApp.Domain;

namespace SimuladorDronApp.Presentation
{
    public static class InterfazConsola
    {
        public static (int n, Coordenada inicio) SolicitarDatosEntrada()
        {
            Console.WriteLine("=== SIMULADOR DE TRAYECTORIA DE DRON AUTOMATIZADO ===\n");

            int n;
            while (true)
            {
                Console.Write("Ingrese la dimensión del terreno N (>= 1): ");
                if (int.TryParse(Console.ReadLine(), out n) && n >= 1) break;
                Console.WriteLine("[-] Entrada inválida. Debe ser entero mayor o igual a 1.");
            }

            int x, y;
            while (true)
            {
                Console.Write($"Ingrese coordenada inicial Fila X (0 a {n - 1}): ");
                if (int.TryParse(Console.ReadLine(), out x) && x >= 0 && x < n) break;
                Console.WriteLine($"[-] Fuera de rango. Debe estar entre 0 y {n - 1}.");
            }

            while (true)
            {
                Console.Write($"Ingrese coordenada inicial Columna Y (0 a {n - 1}): ");
                if (int.TryParse(Console.ReadLine(), out y) && y >= 0 && y < n) break;
                Console.WriteLine($"[-] Fuera de rango. Debe estar entre 0 y {n - 1}.");
            }

            return (n, new Coordenada(x, y));
        }

        public static void DibujarMatriz(int[,] matriz, int n)
        {
            Console.WriteLine("\n[+] MATRIZ DE RECORRIDO CALCULADA:");
            Console.WriteLine(new string('-', n * 6));

            int i = 0;
            while (i < n)
            {
                int j = 0;
                while (j < n)
                {
                    if (matriz[i, j] == -1)
                        Console.Write("   . ");
                    else
                        Console.Write($"{matriz[i, j],4} ");
                    j++;
                }
                Console.WriteLine();
                i++;
            }
            Console.WriteLine(new string('-', n * 6));
        }

        public static void DibujarMatrizSinSolucion(Coordenada inicio, int n)
        {
            Console.WriteLine("\n[RESULTADO] SIN SOLUCIÓN: Alcanza las parcelas pero no existe una ruta sin repetir.");

            int fila = 0;
            while (fila < n)
            {
                int columna = 0;
                while (columna < n)
                {
                    if (fila == inicio.X && columna == inicio.Y)
                        Console.Write("   0 ");
                    else
                        Console.Write("   . ");
                    columna++;
                }
                Console.WriteLine();
                fila++;
            }
        }

        public static List<(int paso, Coordenada coord)> ExtraerSecuenciaCronologica(int[,] matriz, int n, int totalM)
        {
            var secuenciaPlana = new (int paso, Coordenada coord)[totalM];

            int i = 0;
            while (i < n)
            {
                int j = 0;
                while (j < n)
                {
                    int val = matriz[i, j];
                    if (val >= 0 && val < totalM)
                    {
                        secuenciaPlana[val] = (val, new Coordenada(i, j));
                    }
                    j++;
                }
                i++;
            }

            return new List<(int, Coordenada)>(secuenciaPlana);
        }
    }
}