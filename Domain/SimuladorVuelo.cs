using System.Collections.Generic;
using System.Linq;

namespace SimuladorDronApp.Domain
{
    public class SimuladorVuelo
    {
        private readonly int _n;
        private readonly Coordenada _inicio;
        private readonly int[,] _matriz;
        private int _totalAlcanzables;

        // Patrón estricto en "L": 2 en una dirección y 1 en perpendicular
        private readonly int[] _dx = { -2, -2, -1, -1, 1, 1, 2, 2 };
        private readonly int[] _dy = { -1, 1, -2, 2, -2, 2, -1, 1 };

        public int[,] MatrizResultante => _matriz;
        public int TotalAlcanzables => _totalAlcanzables;

        public SimuladorVuelo(int n, Coordenada inicio)
        {
            _n = n;
            _inicio = inicio;
            _matriz = new int[_n, _n];

            int i = 0;
            while (i < _n)
            {
                int j = 0;
                while (j < _n)
                {
                    _matriz[i, j] = -1; // -1 significa libre o no pisada
                    j++;
                }
                i++;
            }
        }

        public bool Resolver()
        {
            // Determina la cantidad real de celdas que pertenecen a la isla conectada
            _totalAlcanzables = CalcularTotalAlcanzables(_inicio);

            // Coloca el dron en la posición de despegue (Paso 0)
            _matriz[_inicio.X, _inicio.Y] = 0;

            if (BuscarCamino(_inicio, 1))
                return true;

            _matriz[_inicio.X, _inicio.Y] = -1;
            return false;
        }

        private bool BuscarCamino(Coordenada actual, int pasoActual)
        {
            // Éxito: cubrimos todas las parcelas de la isla conectada (M)
            if (pasoActual == _totalAlcanzables)
                return true;

            // Heurística de menor grado: se evalúan primero las casillas con menos salidas
            List<DestinoCandidato> candidatos = ObtenerCandidatosOrdenados(actual);

            int idx = 0;
            while (idx < candidatos.Count)
            {
                var candidato = candidatos[idx];
                Coordenada sig = candidato.Posicion;

                _matriz[sig.X, sig.Y] = pasoActual; // Bloquear parcela

                if (BuscarCamino(sig, pasoActual + 1))
                    return true;

                _matriz[sig.X, sig.Y] = -1; // Backtracking si cae en un callejón
                idx++;
            }

            return false;
        }

        private List<DestinoCandidato> ObtenerCandidatosOrdenados(Coordenada actual)
        {
            List<DestinoCandidato> lista = new List<DestinoCandidato>();

            int i = 0;
            while (i < 8)
            {
                int nx = actual.X + _dx[i];
                int ny = actual.Y + _dy[i];

                if (EsValido(nx, ny) && _matriz[nx, ny] == -1)
                {
                    Coordenada candidata = new Coordenada(nx, ny);
                    int grado = CalcularGrado(candidata);
                    lista.Add(new DestinoCandidato(candidata, grado));
                }
                i++;
            }

            return lista.OrderBy(c => c.Grado).ToList();
        }

        private int CalcularGrado(Coordenada c)
        {
            int salidasLibres = 0;
            int i = 0;
            while (i < 8)
            {
                int nx = c.X + _dx[i];
                int ny = c.Y + _dy[i];
                if (EsValido(nx, ny) && _matriz[nx, ny] == -1)
                    salidasLibres++;
                i++;
            }
            return salidasLibres;
        }

        private int CalcularTotalAlcanzables(Coordenada inicio)
        {
            HashSet<Coordenada> visitados = new HashSet<Coordenada>();
            Queue<Coordenada> fila = new Queue<Coordenada>();

            fila.Enqueue(inicio);
            visitados.Add(inicio);

            while (fila.Count > 0)
            {
                Coordenada curr = fila.Dequeue();
                int i = 0;
                while (i < 8)
                {
                    int nx = curr.X + _dx[i];
                    int ny = curr.Y + _dy[i];

                    if (EsValido(nx, ny))
                    {
                        Coordenada vecina = new Coordenada(nx, ny);
                        if (!visitados.Contains(vecina))
                        {
                            visitados.Add(vecina);
                            fila.Enqueue(vecina);
                        }
                    }
                    i++;
                }
            }
            return visitados.Count;
        }

        private bool EsValido(int x, int y)
        {
            return x >= 0 && x < _n && y >= 0 && y < _n;
        }
    }
}