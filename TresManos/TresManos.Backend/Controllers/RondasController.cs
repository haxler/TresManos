using Microsoft.AspNetCore.Mvc;
using TresManos.Backend.Dtos;
using TresManos.Backend.Repositories.Interfaces;
using TresManos.Shared.Entities;

namespace TresManos.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RondasController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RondasController> _logger;

        public RondasController(IUnitOfWork unitOfWork, ILogger<RondasController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/rondas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ronda>>> GetAll()
        {
            try
            {
                var rondas = await _unitOfWork.Rondas.GetAllAsync();
                return Ok(rondas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las rondas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/rondas/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Ronda>> GetById(int id)
        {
            try
            {
                var ronda = await _unitOfWork.Rondas.GetByIdAsync(id);

                if (ronda == null)
                    return NotFound($"Ronda con Id {id} no encontrada");

                return Ok(ronda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ronda con Id {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/rondas/partida/5
        [HttpGet("partida/{partidaId:int}")]
        public async Task<ActionResult<IEnumerable<Ronda>>> GetByPartida(int partidaId)
        {
            try
            {
                var existe = await _unitOfWork.Partidas.ExistsAsync(partidaId);
                if (!existe)
                    return NotFound($"Partida con Id {partidaId} no encontrada");

                var rondas = await _unitOfWork.Rondas.GetRondasByPartidaIdAsync(partidaId);
                return Ok(rondas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rondas de la partida {PartidaId}", partidaId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/rondas
        // Acepta RondaCreateDto y mapea a la entidad Ronda
        [HttpPost]
        public async Task<ActionResult<Ronda>> Create([FromBody] RondaCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validar que la partida existe
                var partidaExiste = await _unitOfWork.Partidas.ExistsAsync(dto.PartidaId);
                if (!partidaExiste)
                    return NotFound($"Partida con Id {dto.PartidaId} no encontrada");

                // Obtener la partida con jugadores
                var partida = await _unitOfWork.Partidas.GetByIdWithJugadoresAsync(dto.PartidaId);
                if (partida == null)
                    return NotFound($"Partida con Id {dto.PartidaId} no encontrada");

                if (partida.Estado != "EN_CURSO")
                    return BadRequest("No se pueden agregar rondas a una partida que no está en curso");

                // Validar movimientos (P, A, T)
                var movimientosValidos = new[] { "P", "A", "T" };
                if (!movimientosValidos.Contains(dto.MovimientoJugador1) ||
                    !movimientosValidos.Contains(dto.MovimientoJugador2))
                    return BadRequest("Movimientos inválidos. Debe ser P (Piedra), A (pApel) o T (Tijeras)");

                // Calcular número de ronda automáticamente
                var rondasExistentes = await _unitOfWork.Rondas.GetRondasByPartidaIdAsync(dto.PartidaId);
                var numeroRonda = rondasExistentes.Count() + 1;

                // Calcular resultado automáticamente
                var resultado = CalcularResultado(dto.MovimientoJugador1, dto.MovimientoJugador2);

                // Mapear DTO -> Entidad
                var ronda = new Ronda
                {
                    PartidaId = dto.PartidaId,
                    NumeroRonda = numeroRonda,
                    MovimientoJugador1 = dto.MovimientoJugador1,
                    MovimientoJugador2 = dto.MovimientoJugador2,
                    Resultado = resultado,
                    FechaCreacion = DateTime.Now
                };

                // Asignar ganador y perdedor según resultado
                if (resultado == "1")
                {
                    ronda.GanadorUsuarioId = partida.UsuarioId_Jugador1;
                    ronda.PerdedorUsuarioId = partida.UsuarioId_Jugador2;
                }
                else if (resultado == "2")
                {
                    ronda.GanadorUsuarioId = partida.UsuarioId_Jugador2;
                    ronda.PerdedorUsuarioId = partida.UsuarioId_Jugador1;
                }
                else // Empate
                {
                    ronda.GanadorUsuarioId = null;
                    ronda.PerdedorUsuarioId = null;
                }

                // Crear la ronda
                var nuevaRonda = await _unitOfWork.Rondas.CreateAsync(ronda);
                await _unitOfWork.SaveChangesAsync();

                // Verificar si hay un ganador de la partida (2 rondas ganadas)
                var todasLasRondas = await _unitOfWork.Rondas.GetRondasByPartidaIdAsync(dto.PartidaId);
                var rondasGanadasJ1 = todasLasRondas.Count(r => r.GanadorUsuarioId == partida.UsuarioId_Jugador1);
                var rondasGanadasJ2 = todasLasRondas.Count(r => r.GanadorUsuarioId == partida.UsuarioId_Jugador2);

                // Si algún jugador ganó 2 rondas, finalizar la partida
                if (rondasGanadasJ1 >= 2)
                {
                    partida.Estado = "FINALIZADA";
                    partida.FechaFin = DateTime.Now;
                    partida.GanadorUsuarioId = partida.UsuarioId_Jugador1;
                    partida.PerdedorUsuarioId = partida.UsuarioId_Jugador2;
                    await _unitOfWork.Partidas.UpdateAsync(partida);
                    await _unitOfWork.SaveChangesAsync();
                }
                else if (rondasGanadasJ2 >= 2)
                {
                    partida.Estado = "FINALIZADA";
                    partida.FechaFin = DateTime.Now;
                    partida.GanadorUsuarioId = partida.UsuarioId_Jugador2;
                    partida.PerdedorUsuarioId = partida.UsuarioId_Jugador1;
                    await _unitOfWork.Partidas.UpdateAsync(partida);
                    await _unitOfWork.SaveChangesAsync();
                }

                return CreatedAtAction(nameof(GetById), new { id = nuevaRonda.RondaId }, nuevaRonda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ronda");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/rondas/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Ronda>> Update(int id, [FromBody] Ronda ronda)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != ronda.RondaId)
                    return BadRequest("El Id de la URL no coincide con el Id de la ronda");

                var existe = await _unitOfWork.Rondas.ExistsAsync(id);
                if (!existe)
                    return NotFound($"Ronda con Id {id} no encontrada");

                var rondaActualizada = await _unitOfWork.Rondas.UpdateAsync(ronda);
                await _unitOfWork.SaveChangesAsync();

                return Ok(rondaActualizada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ronda con Id {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/rondas/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var eliminado = await _unitOfWork.Rondas.DeleteAsync(id);

                if (!eliminado)
                    return NotFound($"Ronda con Id {id} no encontrada");

                await _unitOfWork.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar ronda con Id {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/rondas/usuario/5/ganadas
        [HttpGet("usuario/{usuarioId:int}/ganadas")]
        public async Task<ActionResult<IEnumerable<Ronda>>> GetRondasGanadas(int usuarioId)
        {
            try
            {
                var existe = await _unitOfWork.Usuarios.ExistsAsync(usuarioId);
                if (!existe)
                    return NotFound($"Usuario con Id {usuarioId} no encontrado");

                var rondas = await _unitOfWork.Rondas.GetRondasGanadasByUsuarioAsync(usuarioId);
                return Ok(rondas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rondas ganadas del usuario {UsuarioId}", usuarioId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/rondas/usuario/5/estadisticas-movimientos
        [HttpGet("usuario/{usuarioId:int}/estadisticas-movimientos")]
        public async Task<ActionResult> GetEstadisticasMovimientos(int usuarioId)
        {
            try
            {
                var existe = await _unitOfWork.Usuarios.ExistsAsync(usuarioId);
                if (!existe)
                    return NotFound($"Usuario con Id {usuarioId} no encontrado");

                var estadisticas = await _unitOfWork.Rondas.GetEstadisticasMovimientosByUsuarioAsync(usuarioId);
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de movimientos del usuario {UsuarioId}", usuarioId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/rondas/partida/5/empates
        [HttpGet("partida/{partidaId:int}/empates")]
        public async Task<ActionResult<IEnumerable<Ronda>>> GetEmpatesByPartida(int partidaId)
        {
            try
            {
                var existe = await _unitOfWork.Partidas.ExistsAsync(partidaId);
                if (!existe)
                    return NotFound($"Partida con Id {partidaId} no encontrada");

                var rondas = await _unitOfWork.Rondas.GetRondasByPartidaIdAsync(partidaId);
                var empates = rondas.Where(r => r.Resultado == "E");

                return Ok(empates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empates de la partida {PartidaId}", partidaId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Calcula el resultado de una ronda según los movimientos de ambos jugadores
        /// </summary>
        /// <param name="movimientoJ1">P (Piedra), A (pApel), T (Tijeras)</param>
        /// <param name="movimientoJ2">P (Piedra), A (pApel), T (Tijeras)</param>
        /// <returns>E (Empate), 1 (Gana J1), 2 (Gana J2)</returns>
        private string CalcularResultado(string movimientoJ1, string movimientoJ2)
        {
            // Si ambos movimientos son iguales, es empate
            if (movimientoJ1 == movimientoJ2)
                return "E";

            // Jugador 1 gana si:
            // - Piedra (P) vence Tijeras (T)
            // - pApel (A) vence Piedra (P)
            // - Tijeras (T) vence pApel (A)
            if ((movimientoJ1 == "P" && movimientoJ2 == "T") ||
                (movimientoJ1 == "A" && movimientoJ2 == "P") ||
                (movimientoJ1 == "T" && movimientoJ2 == "A"))
            {
                return "1";
            }

            // En cualquier otro caso, gana Jugador 2
            return "2";
        }
    }
}