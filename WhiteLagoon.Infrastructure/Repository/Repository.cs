using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    internal DbSet<T> dbSet;
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        dbSet = _context.Set<T>();
    }
    public void Add(T entity)
    {
        _context.Add(entity);
    }

    public T Get(Expression<Func<T, bool>> filter, string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrEmpty(includeProperties))
        {
            //case sensitive 
            foreach (var property in includeProperties
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }

        return query.FirstOrDefault();
    }

    public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string includeProperties = null)
    {
        IQueryable<T> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrEmpty(includeProperties))
        {
            //case sensitive 
            foreach (var property in includeProperties
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(property);
            }
        }

        return query.ToList();
    }

    public void Remove(T entity)
    {
        _context.Remove(entity);
    }
}
