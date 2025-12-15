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

public class PartidaRepository : IPartidaRepository
{
    private readonly JuegoDbContext _context;
    private readonly ILogger<PartidaRepository> _logger;

    public PartidaRepository(JuegoDbContext context, ILogger<PartidaRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ============================================
    // OPERACIONES CRUD BÁSICAS
    // ============================================

    public async Task<Partida> GetByIdAsync(int partidaId)
    {
        try
        {
            return await _context.Partidas.FindAsync(partidaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partida con Id {PartidaId}", partidaId);
            throw new RepositoryException($"Error al obtener la Partida con Id {partidaId}.", ex);
        }
    }

    public async Task<Partida> GetByIdWithRondasAsync(int partidaId)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Rondas.OrderBy(r => r.NumeroRonda))
                .FirstOrDefaultAsync(p => p.PartidaId == partidaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partida con Rondas. Id {PartidaId}", partidaId);
            throw new RepositoryException($"Error al obtener la Partida {partidaId} con sus rondas.", ex);
        }
    }

    public async Task<Partida> GetByIdWithJugadoresAsync(int partidaId)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .FirstOrDefaultAsync(p => p.PartidaId == partidaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partida con Jugadores. Id {PartidaId}", partidaId);
            throw new RepositoryException($"Error al obtener la Partida {partidaId} con jugadores.", ex);
        }
    }

    public async Task<Partida> GetByIdCompleteAsync(int partidaId)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Include(p => p.Rondas.OrderBy(r => r.NumeroRonda))
                    .ThenInclude(r => r.GanadorUsuario)
                .Include(p => p.Rondas)
                    .ThenInclude(r => r.PerdedorUsuario)
                .Include(p => p.PartidaOriginal)
                .Include(p => p.Revanchas)
                .FirstOrDefaultAsync(p => p.PartidaId == partidaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partida completa. Id {PartidaId}", partidaId);
            throw new RepositoryException($"Error al obtener la Partida completa {partidaId}.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetAllAsync()
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las Partidas");
            throw new RepositoryException("Error al obtener todas las Partidas.", ex);
        }
    }

    public async Task<Partida> CreateAsync(Partida partida)
    {
        if (partida == null) throw new ArgumentNullException(nameof(partida));

        try
        {
            await _context.Partidas.AddAsync(partida);
            // NO se llama a SaveChanges; lo hace UnitOfWork
            return partida;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear Partida");
            throw new RepositoryException("Error al crear la Partida.", ex);
        }
    }

    public async Task<Partida> UpdateAsync(Partida partida)
    {
        if (partida == null) throw new ArgumentNullException(nameof(partida));

        try
        {
            _context.Partidas.Update(partida);
            return await Task.FromResult(partida);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar Partida con Id {PartidaId}", partida.PartidaId);
            throw new RepositoryException($"Error al actualizar la Partida {partida.PartidaId}.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int partidaId)
    {
        try
        {
            var partida = await _context.Partidas.FindAsync(partidaId);
            if (partida == null)
                return false;

            _context.Partidas.Remove(partida);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar Partida con Id {PartidaId}", partidaId);
            throw new RepositoryException($"Error al eliminar la Partida {partidaId}.", ex);
        }
    }

    public async Task<bool> ExistsAsync(int partidaId)
    {
        try
        {
            return await _context.Partidas.AnyAsync(p => p.PartidaId == partidaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia de Partida con Id {PartidaId}", partidaId);
            throw new RepositoryException($"Error al verificar existencia de la Partida {partidaId}.", ex);
        }
    }

    // ============================================
    // CONSULTAS ESPECÍFICAS DEL DOMINIO
    // ============================================

    public async Task<IEnumerable<Partida>> GetPartidasByEstadoAsync(string estado)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partidas por Estado {Estado}", estado);
            throw new RepositoryException($"Error al obtener partidas con estado '{estado}'.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasEnCursoAsync()
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Where(p => p.Estado == "EN_CURSO")
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partidas en curso");
            throw new RepositoryException("Error al obtener partidas en curso.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasFinalizadasAsync()
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p => p.Estado == "FINALIZADA")
                .OrderByDescending(p => p.FechaFin)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partidas finalizadas");
            throw new RepositoryException("Error al obtener partidas finalizadas.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasByUsuarioAsync(int usuarioId)
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
            _logger.LogError(ex, "Error al obtener Partidas del Usuario {UsuarioId}", usuarioId);
            throw new RepositoryException($"Error al obtener partidas del Usuario {usuarioId}.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasEntreUsuariosAsync(int usuarioId1, int usuarioId2)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p =>
                    (p.UsuarioId_Jugador1 == usuarioId1 && p.UsuarioId_Jugador2 == usuarioId2) ||
                    (p.UsuarioId_Jugador1 == usuarioId2 && p.UsuarioId_Jugador2 == usuarioId1))
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partidas entre Usuarios {UsuarioId1} y {UsuarioId2}", usuarioId1, usuarioId2);
            throw new RepositoryException($"Error al obtener partidas entre Usuarios {usuarioId1} y {usuarioId2}.", ex);
        }
    }

    public async Task<Partida> GetUltimaPartidaEntreUsuariosAsync(int usuarioId1, int usuarioId2)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p =>
                    (p.UsuarioId_Jugador1 == usuarioId1 && p.UsuarioId_Jugador2 == usuarioId2) ||
                    (p.UsuarioId_Jugador1 == usuarioId2 && p.UsuarioId_Jugador2 == usuarioId1))
                .OrderByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener última Partida entre Usuarios {UsuarioId1} y {UsuarioId2}", usuarioId1, usuarioId2);
            throw new RepositoryException($"Error al obtener última partida entre Usuarios {usuarioId1} y {usuarioId2}.", ex);
        }
    }

    // ============================================
    // REVANCHAS
    // ============================================

    public async Task<IEnumerable<Partida>> GetRevanchasByPartidaOriginalAsync(int partidaOriginalId)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p => p.PartidaOriginalId == partidaOriginalId && p.EsRevancha)
                .OrderBy(p => p.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Revanchas de la Partida {PartidaOriginalId}", partidaOriginalId);
            throw new RepositoryException($"Error al obtener revanchas de la Partida {partidaOriginalId}.", ex);
        }
    }

    public async Task<bool> TieneRevanchaPendienteAsync(int partidaId)
    {
        try
        {
            return await _context.Partidas
                .AnyAsync(p => p.PartidaOriginalId == partidaId &&
                               p.EsRevancha &&
                               p.RevanchaAceptadaPorPerdedor == null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si la Partida {PartidaId} tiene revancha pendiente", partidaId);
            throw new RepositoryException($"Error al verificar revancha pendiente de la Partida {partidaId}.", ex);
        }
    }

    public async Task<bool> TieneRevanchaAceptadaAsync(int partidaId)
    {
        try
        {
            return await _context.Partidas
                .AnyAsync(p => p.PartidaOriginalId == partidaId &&
                               p.EsRevancha &&
                               p.RevanchaAceptadaPorPerdedor == true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar si la Partida {PartidaId} tiene revancha aceptada", partidaId);
            throw new RepositoryException($"Error al verificar revancha aceptada de la Partida {partidaId}.", ex);
        }
    }

    public async Task<Partida> GetPartidaOriginalAsync(int partidaRevanchaId)
    {
        try
        {
            var partidaRevancha = await _context.Partidas
                .Include(p => p.PartidaOriginal)
                    .ThenInclude(po => po.Jugador1)
                .Include(p => p.PartidaOriginal)
                    .ThenInclude(po => po.Jugador2)
                .Include(p => p.PartidaOriginal)
                    .ThenInclude(po => po.Ganador)
                .Include(p => p.PartidaOriginal)
                    .ThenInclude(po => po.Perdedor)
                .FirstOrDefaultAsync(p => p.PartidaId == partidaRevanchaId && p.EsRevancha);

            return partidaRevancha?.PartidaOriginal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partida original de la revancha {PartidaRevanchaId}", partidaRevanchaId);
            throw new RepositoryException($"Error al obtener partida original de la revancha {partidaRevanchaId}.", ex);
        }
    }

    // ============================================
    // ESTADÍSTICAS
    // ============================================

    public async Task<int> GetTotalRondasByPartidaAsync(int partidaId)
    {
        try
        {
            return await _context.Rondas
                .CountAsync(r => r.PartidaId == partidaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar Rondas de la Partida {PartidaId}", partidaId);
            throw new RepositoryException($"Error al contar rondas de la Partida {partidaId}.", ex);
        }
    }

    public async Task<int> GetTotalEmpatesByPartidaAsync(int partidaId)
    {
        try
        {
            return await _context.Rondas
                .CountAsync(r => r.PartidaId == partidaId && r.Resultado == "E");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar empates de la Partida {PartidaId}", partidaId);
            throw new RepositoryException($"Error al contar empates de la Partida {partidaId}.", ex);
        }
    }

    public async Task<Partida> GetPartidaMasLargaAsync()
    {
        try
        {
            var partidaConMasRondas = await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Include(p => p.Rondas)
                .Where(p => p.Estado == "FINALIZADA")
                .OrderByDescending(p => p.Rondas.Count)
                .FirstOrDefaultAsync();

            return partidaConMasRondas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la Partida más larga");
            throw new RepositoryException("Error al obtener la partida más larga.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasPorFechaAsync(DateTime fecha)
    {
        try
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p => p.FechaInicio >= fechaInicio && p.FechaInicio < fechaFin)
                .OrderBy(p => p.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partidas por fecha {Fecha}", fecha);
            throw new RepositoryException($"Error al obtener partidas de la fecha {fecha:yyyy-MM-dd}.", ex);
        }
    }

    public async Task<IEnumerable<Partida>> GetPartidasEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            return await _context.Partidas
                .Include(p => p.Jugador1)
                .Include(p => p.Jugador2)
                .Include(p => p.Ganador)
                .Include(p => p.Perdedor)
                .Where(p => p.FechaInicio >= fechaInicio && p.FechaInicio <= fechaFin)
                .OrderBy(p => p.FechaInicio)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Partidas en rango de fechas {FechaInicio} - {FechaFin}", fechaInicio, fechaFin);
            throw new RepositoryException($"Error al obtener partidas entre {fechaInicio:yyyy-MM-dd} y {fechaFin:yyyy-MM-dd}.", ex);
        }
    }
}