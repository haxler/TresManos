using Microsoft.AspNetCore.Mvc;
using TresManos.Backend.Dtos;
using TresManos.Backend.Repositories.Interfaces;
using TresManos.Shared.Entities;

namespace TresManos.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartidasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PartidasController> _logger;

    public PartidasController(IUnitOfWork unitOfWork, ILogger<PartidasController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // GET: api/partidas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Partida>>> GetAll()
    {
        try
        {
            var partidas = await _unitOfWork.Partidas.GetAllAsync();
            return Ok(partidas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las partidas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/partidas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Partida>> GetById(int id)
    {
        try
        {
            var partida = await _unitOfWork.Partidas.GetByIdAsync(id);

            if (partida == null)
                return NotFound($"Partida con Id {id} no encontrada");

            return Ok(partida);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partida con Id {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/partidas/5/completa
    [HttpGet("{id:int}/completa")]
    public async Task<ActionResult<Partida>> GetByIdCompleta(int id)
    {
        try
        {
            var partida = await _unitOfWork.Partidas.GetByIdCompleteAsync(id);

            if (partida == null)
                return NotFound($"Partida con Id {id} no encontrada");

            return Ok(partida);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partida completa con Id {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/partidas
    // Acepta PartidaCreateDto y mapea a la entidad Partida
    [HttpPost]
    public async Task<ActionResult<Partida>> Create([FromBody] PartidaCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.UsuarioId_Jugador1 == dto.UsuarioId_Jugador2)
                return BadRequest("Los dos jugadores no pueden ser el mismo usuario");

            var jugador1Existe = await _unitOfWork.Usuarios.ExistsAsync(dto.UsuarioId_Jugador1);
            var jugador2Existe = await _unitOfWork.Usuarios.ExistsAsync(dto.UsuarioId_Jugador2);

            if (!jugador1Existe)
                return NotFound($"Jugador 1 con Id {dto.UsuarioId_Jugador1} no encontrado");

            if (!jugador2Existe)
                return NotFound($"Jugador 2 con Id {dto.UsuarioId_Jugador2} no encontrado");

            // Mapear DTO -> Entidad
            var partida = new Partida
            {
                UsuarioId_Jugador1 = dto.UsuarioId_Jugador1,
                UsuarioId_Jugador2 = dto.UsuarioId_Jugador2,
                FechaInicio = DateTime.Now,
                Estado = "EN_CURSO"
            };

            var nuevaPartida = await _unitOfWork.Partidas.CreateAsync(partida);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nuevaPartida.PartidaId }, nuevaPartida);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear partida");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/partidas/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Partida>> Update(int id, [FromBody] Partida partida)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != partida.PartidaId)
                return BadRequest("El Id de la URL no coincide con el Id de la partida");

            var existe = await _unitOfWork.Partidas.ExistsAsync(id);
            if (!existe)
                return NotFound($"Partida con Id {id} no encontrada");

            var partidaActualizada = await _unitOfWork.Partidas.UpdateAsync(partida);
            await _unitOfWork.SaveChangesAsync();

            return Ok(partidaActualizada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar partida con Id {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/partidas/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var eliminado = await _unitOfWork.Partidas.DeleteAsync(id);

            if (!eliminado)
                return NotFound($"Partida con Id {id} no encontrada");

            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar partida con Id {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/partidas/en-curso
    [HttpGet("en-curso")]
    public async Task<ActionResult<IEnumerable<Partida>>> GetEnCurso()
    {
        try
        {
            var partidas = await _unitOfWork.Partidas.GetPartidasEnCursoAsync();
            return Ok(partidas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas en curso");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/partidas/finalizadas
    [HttpGet("finalizadas")]
    public async Task<ActionResult<IEnumerable<Partida>>> GetFinalizadas()
    {
        try
        {
            var partidas = await _unitOfWork.Partidas.GetPartidasFinalizadasAsync();
            return Ok(partidas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas finalizadas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/partidas/usuario/5
    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<ActionResult<IEnumerable<Partida>>> GetByUsuario(int usuarioId)
    {
        try
        {
            var existe = await _unitOfWork.Usuarios.ExistsAsync(usuarioId);
            if (!existe)
                return NotFound($"Usuario con Id {usuarioId} no encontrado");

            var partidas = await _unitOfWork.Partidas.GetPartidasByUsuarioAsync(usuarioId);
            return Ok(partidas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas del usuario {UsuarioId}", usuarioId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/partidas/5/rondas
    [HttpGet("{id:int}/rondas")]
    public async Task<ActionResult<IEnumerable<Ronda>>> GetRondas(int id)
    {
        try
        {
            var existe = await _unitOfWork.Partidas.ExistsAsync(id);
            if (!existe)
                return NotFound($"Partida con Id {id} no encontrada");

            var rondas = await _unitOfWork.Rondas.GetRondasByPartidaIdAsync(id);
            return Ok(rondas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rondas de la partida {PartidaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/partidas/5/revanchas
    [HttpGet("{id:int}/revanchas")]
    public async Task<ActionResult<IEnumerable<Partida>>> GetRevanchas(int id)
    {
        try
        {
            var existe = await _unitOfWork.Partidas.ExistsAsync(id);
            if (!existe)
                return NotFound($"Partida con Id {id} no encontrada");

            var revanchas = await _unitOfWork.Partidas.GetRevanchasByPartidaOriginalAsync(id);
            return Ok(revanchas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener revanchas de la partida {PartidaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/partidas/5/revancha
    [HttpPost("{id:int}/revancha")]
    public async Task<ActionResult<Partida>> CrearRevancha(int id)
    {
        try
        {
            var partidaOriginal = await _unitOfWork.Partidas.GetByIdWithJugadoresAsync(id);

            if (partidaOriginal == null)
                return NotFound($"Partida con Id {id} no encontrada");

            if (partidaOriginal.Estado != "FINALIZADA")
                return BadRequest("Solo se puede crear revancha de una partida finalizada");

            var tieneRevanchaPendiente = await _unitOfWork.Partidas.TieneRevanchaPendienteAsync(id);
            if (tieneRevanchaPendiente)
                return BadRequest("Ya existe una revancha pendiente para esta partida");

            var revancha = new Partida
            {
                UsuarioId_Jugador1 = partidaOriginal.UsuarioId_Jugador1,
                UsuarioId_Jugador2 = partidaOriginal.UsuarioId_Jugador2,
                FechaInicio = DateTime.Now,
                Estado = "EN_CURSO",
                EsRevancha = true,
                PartidaOriginalId = id,
                RevanchaAceptadaPorPerdedor = null
            };

            var nuevaRevancha = await _unitOfWork.Partidas.CreateAsync(revancha);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nuevaRevancha.PartidaId }, nuevaRevancha);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear revancha de la partida {PartidaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}