namespace TresManos.Backend.Dtos;

public class UsuarioCreateDto
{
    public string NombreUsuario { get; set; } = string.Empty;

    // Estadísticas iniciales (pueden venir del front o se ignoran y se inicializan en 0)
    public int RondasGanadas { get; set; } = 0;
    public int RondasPerdidas { get; set; } = 0;
    public int PartidasGanadas { get; set; } = 0;
    public int PartidasPerdidas { get; set; } = 0;
    public int PartidasComoJugador1 { get; set; } = 0;
    public int PartidasComoJugador2 { get; set; } = 0;
}
