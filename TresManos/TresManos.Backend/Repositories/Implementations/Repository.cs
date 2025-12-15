using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TresManos.Backend.Data;
using TresManos.Backend.Repositories.Interfaces;

namespace TresManos.Backend.Repositories.Implementations;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly JuegoDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<Repository<T>> _logger;

    public Repository(JuegoDbContext context, ILogger<Repository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener entidad {Entity} por Id {Id}",
                typeof(T).Name, id);
            throw new RepositoryException(
                $"Error al obtener {typeof(T).Name} con Id {id}.", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al obtener todas las entidades de tipo {Entity}",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al obtener la lista de {typeof(T).Name}.", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al buscar entidades de tipo {Entity} con un predicado",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al buscar {typeof(T).Name}.", ex);
        }
    }

    public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "Se encontró más de una entidad de tipo {Entity} que cumple el predicado",
                typeof(T).Name);
            throw new RepositoryException(
                $"Se encontró más de un {typeof(T).Name} que cumple la condición.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al buscar una única entidad de tipo {Entity}",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al buscar {typeof(T).Name}.", ex);
        }
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        try
        {
            await _dbSet.AddAsync(entity);
            // NO se llama a SaveChanges aquí; lo hace UnitOfWork.
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al agregar entidad de tipo {Entity}",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al agregar {typeof(T).Name}.", ex);
        }
    }

    public virtual Task<T> UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al actualizar entidad de tipo {Entity}",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al actualizar {typeof(T).Name}.", ex);
        }
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al eliminar entidad de tipo {Entity} con Id {Id}",
                typeof(T).Name, id);
            throw new RepositoryException(
                $"Error al eliminar {typeof(T).Name} con Id {id}.", ex);
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        try
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al verificar existencia de {Entity} con Id {Id}",
                typeof(T).Name, id);
            throw new RepositoryException(
                $"Error al verificar existencia de {typeof(T).Name}.", ex);
        }
    }

    public virtual async Task<int> CountAsync()
    {
        try
        {
            return await _dbSet.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al contar entidades de tipo {Entity}",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al contar {typeof(T).Name}.", ex);
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.CountAsync(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al contar entidades de tipo {Entity} con un predicado",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al contar {typeof(T).Name}.", ex);
        }
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.AnyAsync(predicate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al verificar existencia (Any) de entidades de tipo {Entity}",
                typeof(T).Name);
            throw new RepositoryException(
                $"Error al verificar existencia de {typeof(T).Name}.", ex);
        }
    }
}