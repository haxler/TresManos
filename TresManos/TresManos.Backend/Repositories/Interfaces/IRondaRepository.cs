using TresManos.Shared.Entities;

namespace TresManos.Backend.Repositories.Interfaces;

public interface IRondaRepository
{
    // Operaciones CRUD básicas
    Task<Ronda> GetByIdAsync(int rondaId);
    Task<Ronda> GetByIdWithPartidaAsync(int rondaId);
    Task<Ronda> GetByIdCompleteAsync(int rondaId); // Con todas las relaciones
    Task<IEnumerable<Ronda>> GetAllAsync();
    Task<Ronda> CreateAsync(Ronda ronda);
    Task<Ronda> UpdateAsync(Ronda ronda);
    Task<bool> DeleteAsync(int rondaId);
    Task<bool> ExistsAsync(int rondaId);

    // Consultas específicas del dominio
    Task<IEnumerable<Ronda>> GetRondasByPartidaIdAsync(int partidaId);
    Task<Ronda> GetUltimaRondaByPartidaIdAsync(int partidaId);
    Task<int> GetNumeroUltimaRondaAsync(int partidaId);
    Task<int> GetTotalRondasByPartidaAsync(int partidaId);

    // Consultas por resultado
    Task<IEnumerable<Ronda>> GetRondasByResultadoAsync(string resultado); // E, 1, 2
    Task<IEnumerable<Ronda>> GetRondasEmpateByPartidaAsync(int partidaId);
    Task<int> GetTotalEmpatesByPartidaAsync(int partidaId);

    // Consultas por usuario
    Task<IEnumerable<Ronda>> GetRondasGanadasByUsuarioAsync(int usuarioId);
    Task<IEnumerable<Ronda>> GetRondasPerdidasByUsuarioAsync(int usuarioId);
    Task<int> GetTotalRondasGanadasByUsuarioAsync(int usuarioId);
    Task<int> GetTotalRondasPerdidasByUsuarioAsync(int usuarioId);

    // Consultas por movimiento
    Task<IEnumerable<Ronda>> GetRondasByMovimientoAsync(string movimiento); // P, A, T
    Task<Dictionary<string, int>> GetEstadisticasMovimientosByUsuarioAsync(int usuarioId);
    Task<string> GetMovimientoMasFrecuenteByUsuarioAsync(int usuarioId);

    // Consultas por fecha
    Task<IEnumerable<Ronda>> GetRondasPorFechaAsync(DateTime fecha);
    Task<IEnumerable<Ronda>> GetRondasEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
}
