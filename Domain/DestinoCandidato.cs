namespace SimuladorDronApp.Domain
{
    public class DestinoCandidato
    {
        public Coordenada Posicion { get; set; }
        public int Grado { get; set; }

        public DestinoCandidato(Coordenada posicion, int grado)
        {
            Posicion = posicion;
            Grado = grado;
        }
    }
}