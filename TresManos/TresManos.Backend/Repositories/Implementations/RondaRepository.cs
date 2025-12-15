using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TresManos.Backend.Data;
using TresManos.Backend.Repositories.Interfaces;
using TresManos.Shared.Entities;

namespace TresManos.Backend.Repositories.Implementations
{
    public class RondaRepository : IRondaRepository
    {
        private readonly JuegoDbContext _context;
        private readonly ILogger<RondaRepository> _logger;

        public RondaRepository(JuegoDbContext context, ILogger<RondaRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================
        // OPERACIONES CRUD BÁSICAS
        // ============================================

        public async Task<Ronda> GetByIdAsync(int rondaId)
        {
            try
            {
                return await _context.Rondas.FindAsync(rondaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Ronda con Id {RondaId}", rondaId);
                throw new RepositoryException($"Error al obtener la Ronda con Id {rondaId}.", ex);
            }
        }

        public async Task<Ronda> GetByIdWithPartidaAsync(int rondaId)
        {
            try
            {
                return await _context.Rondas
                    .Include(r => r.Partida)
                    .FirstOrDefaultAsync(r => r.RondaId == rondaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Ronda con Partida. Id {RondaId}", rondaId);
                throw new RepositoryException($"Error al obtener la Ronda {rondaId} con Partida.", ex);
            }
        }

        public async Task<Ronda> GetByIdCompleteAsync(int rondaId)
        {
            try
            {
                return await _context.Rondas
                    .Include(r => r.Partida)
                        .ThenInclude(p => p.Jugador1)
                    .Include(r => r.Partida)
                        .ThenInclude(p => p.Jugador2)
                    .Include(r => r.GanadorUsuario)
                    .Include(r => r.PerdedorUsuario)
                    .FirstOrDefaultAsync(r => r.RondaId == rondaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Ronda completa. Id {RondaId}", rondaId);
                throw new RepositoryException($"Error al obtener la Ronda completa {rondaId}.", ex);
            }
        }

        public async Task<IEnumerable<Ronda>> GetAllAsync()
        {
            try
            {
                return await _context.Rondas.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las Rondas");
                throw new RepositoryException("Error al obtener todas las Rondas.", ex);
            }
        }

        public async Task<Ronda> CreateAsync(Ronda ronda)
        {
            if (ronda == null) throw new ArgumentNullException(nameof(ronda));

            try
            {
                await _context.Rondas.AddAsync(ronda);
                // NO se llama a SaveChanges; lo hace UnitOfWork
                return ronda;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear Ronda");
                throw new RepositoryException("Error al crear la Ronda.", ex);
            }
        }

        public async Task<Ronda> UpdateAsync(Ronda ronda)
        {
            if (ronda == null) throw new ArgumentNullException(nameof(ronda));

            try
            {
                _context.Rondas.Update(ronda);
                return await Task.FromResult(ronda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar Ronda con Id {RondaId}", ronda.RondaId);
                throw new RepositoryException($"Error al actualizar la Ronda {ronda.RondaId}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int rondaId)
        {
            try
            {
                var ronda = await _context.Rondas.FindAsync(rondaId);
                if (ronda == null)
                    return false;

                _context.Rondas.Remove(ronda);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar Ronda con Id {RondaId}", rondaId);
                throw new RepositoryException($"Error al eliminar la Ronda {rondaId}.", ex);
            }
        }

        public async Task<bool> ExistsAsync(int rondaId)
        {
            try
            {
                return await _context.Rondas.AnyAsync(r => r.RondaId == rondaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de Ronda con Id {RondaId}", rondaId);
                throw new RepositoryException($"Error al verificar existencia de la Ronda {rondaId}.", ex);
            }
        }

        // ============================================
        // CONSULTAS ESPECÍFICAS DEL DOMINIO
        // ============================================

        public async Task<IEnumerable<Ronda>> GetRondasByPartidaIdAsync(int partidaId)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.PartidaId == partidaId)
                    .OrderBy(r => r.NumeroRonda)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas de la Partida {PartidaId}", partidaId);
                throw new RepositoryException($"Error al obtener las rondas de la Partida {partidaId}.", ex);
            }
        }

        public async Task<Ronda> GetUltimaRondaByPartidaIdAsync(int partidaId)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.PartidaId == partidaId)
                    .OrderByDescending(r => r.NumeroRonda)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la última Ronda de la Partida {PartidaId}", partidaId);
                throw new RepositoryException($"Error al obtener la última ronda de la Partida {partidaId}.", ex);
            }
        }

        public async Task<int> GetNumeroUltimaRondaAsync(int partidaId)
        {
            try
            {
                var numero = await _context.Rondas
                    .Where(r => r.PartidaId == partidaId)
                    .MaxAsync(r => (int?)r.NumeroRonda);

                return numero ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el número de la última Ronda de la Partida {PartidaId}", partidaId);
                throw new RepositoryException($"Error al obtener el número de la última ronda de la Partida {partidaId}.", ex);
            }
        }

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
                throw new RepositoryException($"Error al contar las rondas de la Partida {partidaId}.", ex);
            }
        }

        // ============================================
        // CONSULTAS POR RESULTADO
        // ============================================

        public async Task<IEnumerable<Ronda>> GetRondasByResultadoAsync(string resultado)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.Resultado == resultado)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas por Resultado {Resultado}", resultado);
                throw new RepositoryException($"Error al obtener rondas con resultado '{resultado}'.", ex);
            }
        }

        public async Task<IEnumerable<Ronda>> GetRondasEmpateByPartidaAsync(int partidaId)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.PartidaId == partidaId && r.Resultado == "E")
                    .OrderBy(r => r.NumeroRonda)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas empatadas de la Partida {PartidaId}", partidaId);
                throw new RepositoryException($"Error al obtener rondas empatadas de la Partida {partidaId}.", ex);
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

        // ============================================
        // CONSULTAS POR USUARIO
        // ============================================

        public async Task<IEnumerable<Ronda>> GetRondasGanadasByUsuarioAsync(int usuarioId)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.GanadorUsuarioId == usuarioId)
                    .OrderByDescending(r => r.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas ganadas por Usuario {UsuarioId}", usuarioId);
                throw new RepositoryException($"Error al obtener rondas ganadas por el Usuario {usuarioId}.", ex);
            }
        }

        public async Task<IEnumerable<Ronda>> GetRondasPerdidasByUsuarioAsync(int usuarioId)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.PerdedorUsuarioId == usuarioId)
                    .OrderByDescending(r => r.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas perdidas por Usuario {UsuarioId}", usuarioId);
                throw new RepositoryException($"Error al obtener rondas perdidas por el Usuario {usuarioId}.", ex);
            }
        }

        public async Task<int> GetTotalRondasGanadasByUsuarioAsync(int usuarioId)
        {
            try
            {
                return await _context.Rondas
                    .CountAsync(r => r.GanadorUsuarioId == usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar Rondas ganadas por Usuario {UsuarioId}", usuarioId);
                throw new RepositoryException($"Error al contar rondas ganadas por el Usuario {usuarioId}.", ex);
            }
        }

        public async Task<int> GetTotalRondasPerdidasByUsuarioAsync(int usuarioId)
        {
            try
            {
                return await _context.Rondas
                    .CountAsync(r => r.PerdedorUsuarioId == usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar Rondas perdidas por Usuario {UsuarioId}", usuarioId);
                throw new RepositoryException($"Error al contar rondas perdidas por el Usuario {usuarioId}.", ex);
            }
        }

        // ============================================
        // CONSULTAS POR MOVIMIENTO
        // ============================================

        public async Task<IEnumerable<Ronda>> GetRondasByMovimientoAsync(string movimiento)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.MovimientoJugador1 == movimiento || r.MovimientoJugador2 == movimiento)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas por Movimiento {Movimiento}", movimiento);
                throw new RepositoryException($"Error al obtener rondas con movimiento '{movimiento}'.", ex);
            }
        }

        public async Task<Dictionary<string, int>> GetEstadisticasMovimientosByUsuarioAsync(int usuarioId)
        {
            try
            {
                // Obtener todas las partidas donde el usuario participó
                var partidasIds = await _context.Partidas
                    .Where(p => p.UsuarioId_Jugador1 == usuarioId || p.UsuarioId_Jugador2 == usuarioId)
                    .Select(p => new { p.PartidaId, p.UsuarioId_Jugador1, p.UsuarioId_Jugador2 })
                    .ToListAsync();

                var rondas = await _context.Rondas
                    .Where(r => partidasIds.Select(p => p.PartidaId).Contains(r.PartidaId))
                    .Include(r => r.Partida)
                    .ToListAsync();

                var estadisticas = new Dictionary<string, int>
                {
                    { "P", 0 }, // Piedra
                    { "A", 0 }, // Papel (hoja)
                    { "T", 0 }  // Tijeras
                };

                foreach (var ronda in rondas)
                {
                    // Determinar qué movimiento hizo el usuario
                    string movimiento = null;
                    if (ronda.Partida.UsuarioId_Jugador1 == usuarioId)
                        movimiento = ronda.MovimientoJugador1;
                    else if (ronda.Partida.UsuarioId_Jugador2 == usuarioId)
                        movimiento = ronda.MovimientoJugador2;

                    if (movimiento != null && estadisticas.ContainsKey(movimiento))
                        estadisticas[movimiento]++;
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de movimientos del Usuario {UsuarioId}", usuarioId);
                throw new RepositoryException($"Error al obtener estadísticas de movimientos del Usuario {usuarioId}.", ex);
            }
        }

        public async Task<string> GetMovimientoMasFrecuenteByUsuarioAsync(int usuarioId)
        {
            try
            {
                var estadisticas = await GetEstadisticasMovimientosByUsuarioAsync(usuarioId);

                if (estadisticas == null || !estadisticas.Any() || estadisticas.Values.Sum() == 0)
                    return null;

                return estadisticas.OrderByDescending(e => e.Value).First().Key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener movimiento más frecuente del Usuario {UsuarioId}", usuarioId);
                throw new RepositoryException($"Error al obtener movimiento más frecuente del Usuario {usuarioId}.", ex);
            }
        }

        // ============================================
        // CONSULTAS POR FECHA
        // ============================================

        public async Task<IEnumerable<Ronda>> GetRondasPorFechaAsync(DateTime fecha)
        {
            try
            {
                var fechaInicio = fecha.Date;
                var fechaFin = fechaInicio.AddDays(1);

                return await _context.Rondas
                    .Where(r => r.FechaCreacion >= fechaInicio && r.FechaCreacion < fechaFin)
                    .OrderBy(r => r.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas por fecha {Fecha}", fecha);
                throw new RepositoryException($"Error al obtener rondas de la fecha {fecha:yyyy-MM-dd}.", ex);
            }
        }

        public async Task<IEnumerable<Ronda>> GetRondasEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return await _context.Rondas
                    .Where(r => r.FechaCreacion >= fechaInicio && r.FechaCreacion <= fechaFin)
                    .OrderBy(r => r.FechaCreacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Rondas en rango de fechas {FechaInicio} - {FechaFin}", fechaInicio, fechaFin);
                throw new RepositoryException($"Error al obtener rondas entre {fechaInicio:yyyy-MM-dd} y {fechaFin:yyyy-MM-dd}.", ex);
            }
        }
    }
}