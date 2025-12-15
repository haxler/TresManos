using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TresManos.Shared.Entities;

public class Partida
{
    public int PartidaId { get; set; }

    // Relación con Usuario (Jugador 1)
    [Required]
    public int UsuarioId_Jugador1 { get; set; }

    public virtual Usuario Jugador1 { get; set; }

    public int UsuarioId_Jugador2 { get; set; }

    public virtual Usuario Jugador2 { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; } = DateTime.Now;

    public DateTime? FechaFin { get; set; }

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } // EN_CURSO, FINALIZADA


    public int? GanadorUsuarioId { get; set; }


    public virtual Usuario Ganador { get; set; }

    public int? PerdedorUsuarioId { get; set; }

    [InverseProperty(nameof(Usuario.PartidasPerdidas))]
    public virtual Usuario Perdedor { get; set; }

    // Campos de revancha
    [Required]
    public bool EsRevancha { get; set; } = false;


    public int? PartidaOriginalId { get; set; }


    public virtual Partida PartidaOriginal { get; set; }


    public virtual ICollection<Partida> Revanchas { get; set; }

    public bool? RevanchaAceptadaPorPerdedor { get; set; }

    public DateTime? FechaAceptacionRevancha { get; set; }

    public virtual ICollection<Ronda> Rondas { get; set; }

}