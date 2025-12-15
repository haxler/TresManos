using Microsoft.AspNetCore.Mvc;
using TresManos.Backend.Dtos;
using TresManos.Backend.Repositories;
using TresManos.Backend.Repositories.Interfaces;
using TresManos.Shared.Entities;

namespace TresManos.Backend.Controllers;

/// <summary>
/// Controlador para gestionar operaciones CRUD de usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    /// <summary>
    /// Unidad de trabajo para coordinar transacciones entre repositorios.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Logger para registrar eventos y errores.
    /// </summary>
    private readonly ILogger<UsuariosController> _logger;

    /// <summary>
    /// Constructor del controlador.
    /// </summary>
    /// <param name="unitOfWork">Unidad de trabajo inyectada</param>
    /// <param name="logger">Logger inyectado</param>
    public UsuariosController(IUnitOfWork unitOfWork, ILogger<UsuariosController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados.
    /// </summary>
    /// <returns>Lista de usuarios</returns>
    // GET: api/usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetAll()
    {
        try
        {
            // Obtener todos los usuarios del repositorio
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();

            // Devolver respuesta HTTP 200 OK con la lista de usuarios
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            // Registrar el error en el log
            _logger.LogError(ex, "Error al obtener todos los usuarios");

            // Devolver respuesta HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un usuario específico por su ID.
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Usuario encontrado o NotFound</returns>
    // GET: api/usuarios/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> GetById(int id)
    {
        try
        {
            // Buscar el usuario por ID
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);

            // Si no existe, devolver HTTP 404 Not Found
            if (usuario == null)
                return NotFound($"Usuario con Id {id} no encontrado");

            // Devolver HTTP 200 OK con el usuario encontrado
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            // Registrar el error con el ID del usuario
            _logger.LogError(ex, "Error al obtener usuario con Id {Id}", id);

            // Devolver HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario.
    /// </summary>
    /// <param name="nombreUsuario">Nombre de usuario a buscar</param>
    /// <returns>Usuario encontrado o NotFound</returns>
    // GET: api/usuarios/nombre/juan123
    [HttpGet("nombre/{nombreUsuario}")]
    public async Task<ActionResult<Usuario>> GetByNombreUsuario(string nombreUsuario)
    {
        try
        {
            // Buscar el usuario por nombre de usuario
            var usuario = await _unitOfWork.Usuarios.GetByNombreUsuarioAsync(nombreUsuario);

            // Si no existe, devolver HTTP 404 Not Found
            if (usuario == null)
                return NotFound($"Usuario '{nombreUsuario}' no encontrado");

            // Devolver HTTP 200 OK con el usuario encontrado
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            // Registrar el error con el nombre de usuario
            _logger.LogError(ex, "Error al obtener usuario con nombre {NombreUsuario}", nombreUsuario);

            // Devolver HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="dto">DTO con los datos del usuario a crear</param>
    /// <returns>Usuario creado con HTTP 201 Created</returns>
    // POST: api/usuarios
    [HttpPost]
    public async Task<ActionResult<Usuario>> Create([FromBody] UsuarioCreateDto dto)
    {
        try
        {
            // Validar que el modelo sea válido según las Data Annotations
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que el nombre de usuario no esté vacío
            if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
                return BadRequest("El nombre de usuario es obligatorio.");

            // Normalizar el nombre (quitar espacios en blanco)
            var nombreNormalizado = dto.NombreUsuario.Trim();

            // Verificar si ya existe un usuario con ese nombre
            var existe = await _unitOfWork.Usuarios.ExistsByNombreUsuarioAsync(nombreNormalizado);
            if (existe)
                return Conflict($"Ya existe un usuario con el nombre '{nombreNormalizado}'");

            // Mapear el DTO a la entidad Usuario
            var usuario = new Usuario
            {
                NombreUsuario = nombreNormalizado,
                FechaRegistro = DateTime.Now
                // Las colecciones de navegación (Partidas, etc.) se inicializan automáticamente en la clase Usuario
            };

            // Crear el usuario en la base de datos
            var nuevoUsuario = await _unitOfWork.Usuarios.CreateAsync(usuario);

            // Guardar los cambios en la base de datos
            await _unitOfWork.SaveChangesAsync();

            // Devolver HTTP 201 Created con la ubicación del nuevo recurso
            return CreatedAtAction(
                nameof(GetById),                    // Nombre de la acción para obtener el usuario
                new { id = nuevoUsuario.UsuarioId }, // Parámetros de ruta
                nuevoUsuario                         // Cuerpo de la respuesta
            );
        }
        catch (Exception ex)
        {
            // Registrar el error
            _logger.LogError(ex, "Error al crear usuario");

            // Devolver HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    /// <param name="id">ID del usuario a actualizar</param>
    /// <param name="usuario">Datos actualizados del usuario</param>
    /// <returns>Usuario actualizado o error</returns>
    // PUT: api/usuarios/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Usuario>> Update(int id, [FromBody] Usuario usuario)
    {
        try
        {
            // Validar que el modelo sea válido
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que el ID de la URL coincida con el ID del usuario
            if (id != usuario.UsuarioId)
                return BadRequest("El Id de la URL no coincide con el Id del usuario");

            // Verificar que el usuario exista
            var existe = await _unitOfWork.Usuarios.ExistsAsync(id);
            if (!existe)
                return NotFound($"Usuario con Id {id} no encontrado");

            // Actualizar el usuario en la base de datos
            var usuarioActualizado = await _unitOfWork.Usuarios.UpdateAsync(usuario);

            // Guardar los cambios
            await _unitOfWork.SaveChangesAsync();

            // Devolver HTTP 200 OK con el usuario actualizado
            return Ok(usuarioActualizado);
        }
        catch (Exception ex)
        {
            // Registrar el error
            _logger.LogError(ex, "Error al actualizar usuario con Id {Id}", id);

            // Devolver HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Elimina un usuario del sistema.
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <returns>HTTP 204 No Content si se eliminó correctamente</returns>
    // DELETE: api/usuarios/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            // Intentar eliminar el usuario
            var eliminado = await _unitOfWork.Usuarios.DeleteAsync(id);

            // Si no se encontró el usuario, devolver HTTP 404 Not Found
            if (!eliminado)
                return NotFound($"Usuario con Id {id} no encontrado");

            // Guardar los cambios
            await _unitOfWork.SaveChangesAsync();

            // Devolver HTTP 204 No Content (eliminación exitosa)
            return NoContent();
        }
        catch (RepositoryException ex)
        {
            // Capturar excepciones específicas del repositorio (ej: usuario con partidas activas)
            _logger.LogWarning(ex, "No se pudo eliminar usuario con Id {Id}", id);

            // Devolver HTTP 400 Bad Request con el mensaje de error
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            // Capturar cualquier otro error
            _logger.LogError(ex, "Error al eliminar usuario con Id {Id}", id);

            // Devolver HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene todas las partidas en las que ha participado un usuario.
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Lista de partidas del usuario</returns>
    // GET: api/usuarios/5/partidas
    [HttpGet("{id:int}/partidas")]
    public async Task<ActionResult<IEnumerable<Partida>>> GetPartidas(int id)
    {
        try
        {
            // Verificar que el usuario exista
            var existe = await _unitOfWork.Usuarios.ExistsAsync(id);
            if (!existe)
                return NotFound($"Usuario con Id {id} no encontrado");

            // Obtener todas las partidas del usuario
            var partidas = await _unitOfWork.Usuarios.GetPartidasByUsuarioIdAsync(id);

            // Devolver HTTP 200 OK con la lista de partidas
            return Ok(partidas);
        }
        catch (Exception ex)
        {
            // Registrar el error
            _logger.LogError(ex, "Error al obtener partidas del usuario {Id}", id);

            // Devolver HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene las estadísticas de un usuario (partidas jugadas, ganadas, perdidas, etc.).
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Objeto con las estadísticas del usuario</returns>
    // GET: api/usuarios/5/estadisticas
    [HttpGet("{id:int}/estadisticas")]
    public async Task<ActionResult> GetEstadisticas(int id)
    {
        try
        {
            // Verificar que el usuario exista
            var existe = await _unitOfWork.Usuarios.ExistsAsync(id);
            if (!existe)
                return NotFound($"Usuario con Id {id} no encontrado");

            // Obtener el total de partidas jugadas
            var totalJugadas = await _unitOfWork.Usuarios.GetTotalPartidasJugadasAsync(id);

            // Obtener el total de partidas ganadas
            var totalGanadas = await _unitOfWork.Usuarios.GetTotalPartidasGanadasAsync(id);

            // Obtener el total de partidas perdidas
            var totalPerdidas = await _unitOfWork.Usuarios.GetTotalPartidasPerdidasAsync(id);

            // Calcular el porcentaje de victorias
            var porcentajeVictorias = await _unitOfWork.Usuarios.GetPorcentajeVictoriasAsync(id);

            // Crear un objeto anónimo con las estadísticas
            var estadisticas = new
            {
                UsuarioId = id,
                TotalPartidasJugadas = totalJugadas,
                TotalPartidasGanadas = totalGanadas,
                TotalPartidasPerdidas = totalPerdidas,
                PorcentajeVictorias = porcentajeVictorias
            };

            // Devolver HTTP 200 OK con las estadísticas
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            // Registrar el error
            _logger.LogError(ex, "Error al obtener estadísticas del usuario {Id}", id);

            // Devolver HTTP 500 Internal Server Error
            return StatusCode(500, "Error interno del servidor");
        }
    }
}