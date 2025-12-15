using TresManos.Shared.Entities;

namespace TresManos.Backend.Repositories.Interfaces;

public interface IPartidaRepository
{
    // Operaciones CRUD básicas
    Task<Partida> GetByIdAsync(int partidaId);
    Task<Partida> GetByIdWithRondasAsync(int partidaId);
    Task<Partida> GetByIdWithJugadoresAsync(int partidaId);
    Task<Partida> GetByIdCompleteAsync(int partidaId); // Con todas las relaciones
    Task<IEnumerable<Partida>> GetAllAsync();
    Task<Partida> CreateAsync(Partida partida);
    Task<Partida> UpdateAsync(Partida partida);
    Task<bool> DeleteAsync(int partidaId);
    Task<bool> ExistsAsync(int partidaId);

    // Consultas específicas del dominio
    Task<IEnumerable<Partida>> GetPartidasByEstadoAsync(string estado);
    Task<IEnumerable<Partida>> GetPartidasEnCursoAsync();
    Task<IEnumerable<Partida>> GetPartidasFinalizadasAsync();
    Task<IEnumerable<Partida>> GetPartidasByUsuarioAsync(int usuarioId);
    Task<IEnumerable<Partida>> GetPartidasEntreUsuariosAsync(int usuarioId1, int usuarioId2);
    Task<Partida> GetUltimaPartidaEntreUsuariosAsync(int usuarioId1, int usuarioId2);

    // Revanchas
    Task<IEnumerable<Partida>> GetRevanchasByPartidaOriginalAsync(int partidaOriginalId);
    Task<bool> TieneRevanchaPendienteAsync(int partidaId);
    Task<bool> TieneRevanchaAceptadaAsync(int partidaId);
    Task<Partida> GetPartidaOriginalAsync(int partidaRevanchaId);

    // Estadísticas
    Task<int> GetTotalRondasByPartidaAsync(int partidaId);
    Task<int> GetTotalEmpatesByPartidaAsync(int partidaId);
    Task<Partida> GetPartidaMasLargaAsync(); // Partida con más rondas
    Task<IEnumerable<Partida>> GetPartidasPorFechaAsync(DateTime fecha);
    Task<IEnumerable<Partida>> GetPartidasEnRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
}