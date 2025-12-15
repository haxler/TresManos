using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TresManos.Shared.Entities;

public class Ronda
{
    public int RondaId { get; set; }

    public int PartidaId { get; set; }

    public virtual Partida Partida { get; set; }

    [Required]
    public int NumeroRonda { get; set; }

    [Required]
    [StringLength(1)]
    [RegularExpression("^[PAT]$", ErrorMessage = "Movimiento debe ser P (Piedra), A (pApel) o T (Tijeras)")]
    public string MovimientoJugador1 { get; set; } // P, A, T

    [Required]
    [StringLength(1)]
    [RegularExpression("^[PAT]$", ErrorMessage = "Movimiento debe ser P (Piedra), A (pApel) o T (Tijeras)")]
    public string MovimientoJugador2 { get; set; } // P, A, T

    [Required]
    [StringLength(1)]
    [RegularExpression("^[E12]$", ErrorMessage = "Resultado debe ser E (Empate), 1 (Gana J1) o 2 (Gana J2)")]
    public string Resultado { get; set; } // E, 1, 2

    public int? GanadorUsuarioId { get; set; }

    public virtual Usuario GanadorUsuario { get; set; }

    public int? PerdedorUsuarioId { get; set; }

    public virtual Usuario PerdedorUsuario { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}