namespace TresManos.Backend.Dtos;

public class RondaCreateDto
{
    public int PartidaId { get; set; }
    public string MovimientoJugador1 { get; set; } = string.Empty;
    public string MovimientoJugador2 { get; set; } = string.Empty;
}