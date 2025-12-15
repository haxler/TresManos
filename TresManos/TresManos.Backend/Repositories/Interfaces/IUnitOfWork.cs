namespace TresManos.Backend.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUsuarioRepository Usuarios { get; }
    IPartidaRepository Partidas { get; }
    IRondaRepository Rondas { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}