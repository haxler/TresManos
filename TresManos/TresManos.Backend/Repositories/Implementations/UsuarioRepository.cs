using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TresManos.Backend.Data;
using TresManos.Backend.Repositories.Interfaces;
using TresManos.Shared.Entities;

namespace TresManos.Backend.Repositories.Implementations;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly JuegoDbContext _context;
    private readonly ILogger<UsuarioRepository> _logger;

    public UsuarioRepository(JuegoDbContext context, ILogger<UsuarioRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ============================================
    // OPERACIONES CRUD BÁSICAS
    // ============================================

    public async Task<Usuario> GetByIdAsync(int usuarioId)
    {
        try
        {
            return await _context.Usuarios.FindAsync(usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Usuario con Id {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al obtener el Usuario con Id {usuarioId}.", ex);
        }
    }

    public async Task<Usuario> GetByNombreUsuarioAsync(string nombreUsuario)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(nombreUsuario));

            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
        }
        catch (ArgumentException)
        {
            throw; // Re-lanzar excepciones de validación
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Usuario por NombreUsuario {NombreUsuario}", nombreUsuario);
            throw new RepositoryException($"Error al obtener Usuario '{nombreUsuario}'.", ex);
        }
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        try
        {
            return await _context.Usuarios
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los Usuarios");
            throw new RepositoryException("Error al obtener todos los Usuarios.", ex);
        }
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        if (usuario == null) throw new ArgumentNullException(nameof(usuario));

        try
        {
            // Validar que el nombre de usuario no exista
            var existe = await ExistsByNombreUsuarioAsync(usuario.NombreUsuario);
            if (existe)
            {
                throw new RepositoryException($"Ya existe un usuario con el nombre '{usuario.NombreUsuario}'.");
            }

            await _context.Usuarios.AddAsync(usuario);
            // NO se llama a SaveChanges; lo hace UnitOfWork
            return usuario;
        }
        catch (RepositoryException)
        {
            throw; // Re-lanzar excepciones de negocio
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear Usuario con NombreUsuario {NombreUsuario}", usuario.NombreUsuario);
            throw new RepositoryException($"Error al crear el Usuario '{usuario.NombreUsuario}'.", ex);
        }
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {
        if (usuario == null) throw new ArgumentNullException(nameof(usuario));

        try
        {
            // Verificar que el usuario existe
            var existe = await ExistsAsync(usuario.UsuarioId);
            if (!existe)
            {
                throw new RepositoryException($"No existe un usuario con Id {usuario.UsuarioId}.");
            }

            // Verificar que el nuevo nombre de usuario no esté en uso por otro usuario
            var usuarioConMismoNombre = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == usuario.NombreUsuario && u.UsuarioId != usuario.UsuarioId);

            if (usuarioConMismoNombre != null)
            {
                throw new RepositoryException($"Ya existe otro usuario con el nombre '{usuario.NombreUsuario}'.");
            }

            _context.Usuarios.Update(usuario);
            return await Task.FromResult(usuario);
        }
        catch (RepositoryException)
        {
            throw; // Re-lanzar excepciones de negocio
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar Usuario con Id {UsuarioId}", usuario.UsuarioId);
            throw new RepositoryException($"Error al actualizar el Usuario con Id {usuario.UsuarioId}.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int usuarioId)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                return false;

            // Verificar si el usuario tiene partidas asociadas
            var tienePartidas = await _context.Partidas
                .AnyAsync(p => p.UsuarioId_Jugador1 == usuarioId || p.UsuarioId_Jugador2 == usuarioId);

            if (tienePartidas)
            {
                throw new RepositoryException(
                    $"No se puede eliminar el Usuario {usuarioId} porque tiene partidas asociadas.");
            }

            _context.Usuarios.Remove(usuario);
            return true;
        }
        catch (RepositoryException)
        {
            throw; // Re-lanzar excepciones de negocio
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar Usuario con Id {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al eliminar el Usuario con Id {usuarioId}.", ex);
        }
    }

    public async Task<bool> ExistsAsync(int usuarioId)
    {
        try
        {
            return await _context.Usuarios.AnyAsync(u => u.UsuarioId == usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de Usuario con Id {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al verificar existencia del Usuario con Id {usuarioId}.", ex);
        }
    }

    public async Task<bool> ExistsByNombreUsuarioAsync(string nombreUsuario)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return false;

            return await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de Usuario con NombreUsuario {NombreUsuario}", nombreUsuario);
            throw new RepositoryException($"Error al verificar existencia del Usuario '{nombreUsuario}'.", ex);
        }
    }

    // ============================================
    // CONSULTAS ESPECÍFICAS DEL DOMINIO
    // ============================================

    public async Task<IEnumerable<Partida>> GetPartidasByUsuarioIdAsync(int usuarioId)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p => p.UsuarioId_Jugador1 == usuarioId || p.UsuarioId_Jugador2 == usuarioId)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas del Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al obtener partidas del Usuario {usuarioId}.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasGanadasAsync(int usuarioId)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p => p.GanadorUsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaFin)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas ganadas por el Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al obtener partidas ganadas por el Usuario {usuarioId}.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasPerdidasAsync(int usuarioId)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p => p.PerdedorUsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaFin)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener partidas perdidas por el Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al obtener partidas perdidas por el Usuario {usuarioId}.", ex);
        }
    }

    public async Task<int> GetTotalPartidasJugadasAsync(int usuarioId)
    {
        try
        {
            return await _context.Partidas
                .CountAsync(p => p.UsuarioId_Jugador1 == usuarioId || p.UsuarioId_Jugador2 == usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar partidas jugadas por el Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al contar partidas jugadas por el Usuario {usuarioId}.", ex);
        }
    }

    public async Task<int> GetTotalPartidasGanadasAsync(int usuarioId)
    {
        try
        {
            return await _context.Partidas
                .CountAsync(p => p.GanadorUsuarioId == usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar partidas ganadas por el Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al contar partidas ganadas por el Usuario {usuarioId}.", ex);
        }
    }

    public async Task<int> GetTotalPartidasPerdidasAsync(int usuarioId)
    {
        try
        {
            return await _context.Partidas
                .CountAsync(p => p.PerdedorUsuarioId == usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar partidas perdidas por el Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al contar partidas perdidas por el Usuario {usuarioId}.", ex);
        }
    }

    public async Task<decimal> GetPorcentajeVictoriasAsync(int usuarioId)
    {
        try
        {
            var totalJugadas = await GetTotalPartidasJugadasAsync(usuarioId);

            if (totalJugadas == 0)
                return 0m;

            var totalGanadas = await GetTotalPartidasGanadasAsync(usuarioId);

            // Calcular porcentaje con 2 decimales
            var porcentaje = Math.Round((decimal)totalGanadas / totalJugadas * 100, 2);

            return porcentaje;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular porcentaje de victorias del Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al calcular porcentaje de victorias del Usuario {usuarioId}.", ex);
        }
    }
}