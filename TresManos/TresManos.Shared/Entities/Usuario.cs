using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TresManos.Shared.Entities;

public class Usuario
{
    public int UsuarioId { get; set; }

    [Required]
    [StringLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [InverseProperty(nameof(Partida.Jugador1))]
    public virtual ICollection<Partida> PartidasComoJugador1 { get; set; } = new HashSet<Partida>();

    [InverseProperty(nameof(Partida.Jugador2))]
    public virtual ICollection<Partida> PartidasComoJugador2 { get; set; } = new HashSet<Partida>();

    [InverseProperty(nameof(Partida.Ganador))]
    public virtual ICollection<Partida> PartidasGanadas { get; set; } = new HashSet<Partida>();

    [InverseProperty(nameof(Partida.Perdedor))]
    public virtual ICollection<Partida> PartidasPerdidas { get; set; } = new HashSet<Partida>();

    [InverseProperty(nameof(Ronda.GanadorUsuario))]
    public virtual ICollection<Ronda> RondasGanadas { get; set; } = new HashSet<Ronda>();

    [InverseProperty(nameof(Ronda.PerdedorUsuario))]
    public virtual ICollection<Ronda> RondasPerdidas { get; set; } = new HashSet<Ronda>();
}
