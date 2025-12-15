using TresManos.Shared.Entities;

namespace TresManos.Backend.Repositories.Interfaces;

public interface IUsuarioRepository
{
    // Operaciones CRUD básicas
    Task<Usuario> GetByIdAsync(int usuarioId);
    Task<Usuario> GetByNombreUsuarioAsync(string nombreUsuario);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<Usuario> CreateAsync(Usuario usuario);
    Task<Usuario> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(int usuarioId);
    Task<bool> ExistsAsync(int usuarioId);
    Task<bool> ExistsByNombreUsuarioAsync(string nombreUsuario);

    // Consultas específicas del dominio
    Task<IEnumerable<Partida>> GetPartidasByUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Partida>> GetPartidasGanadasAsync(int usuarioId);
    Task<IEnumerable<Partida>> GetPartidasPerdidasAsync(int usuarioId);
    Task<int> GetTotalPartidasJugadasAsync(int usuarioId);
    Task<int> GetTotalPartidasGanadasAsync(int usuarioId);
    Task<int> GetTotalPartidasPerdidasAsync(int usuarioId);
    Task<decimal> GetPorcentajeVictoriasAsync(int usuarioId);
}