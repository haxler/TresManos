using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TresManos.Backend.Data;
using TresManos.Backend.Repositories.Interfaces;

namespace TresManos.Backend.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly JuegoDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    private IDbContextTransaction _currentTransaction;

    public IUsuarioRepository Usuarios { get; }
    public IPartidaRepository Partidas { get; }
    public IRondaRepository Rondas { get; }

    public UnitOfWork(
        JuegoDbContext context,
        ILogger<UnitOfWork> logger,
        IUsuarioRepository usuarioRepository,
        IPartidaRepository partidaRepository,
        IRondaRepository rondaRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Usuarios = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        Partidas = partidaRepository ?? throw new ArgumentNullException(nameof(partidaRepository));
        Rondas = rondaRepository ?? throw new ArgumentNullException(nameof(rondaRepository));
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error de concurrencia al guardar cambios en la base de datos.");
            throw new RepositoryException(
                "Se produjo un conflicto de concurrencia al guardar los cambios.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de actualización (DbUpdateException) al guardar cambios.");
            throw new RepositoryException(
                "Error al guardar los cambios en la base de datos.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al guardar cambios en la base de datos.");
            throw new RepositoryException(
                "Error inesperado al guardar los cambios en la base de datos.", ex);
        }
    }

    // Si quieres manejar transacciones explícitas:
    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            return;

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
            return;

        try
        {
            await SaveChangesAsync();
            await _currentTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar la transacción. Se realizará Rollback.");
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
            return;

        try
        {
            await _currentTransaction.RollbackAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al hacer Rollback de la transacción.");
            throw new RepositoryException(
                "Error al revertir la transacción.", ex);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context?.Dispose();
    }
}