using System.Linq.Expressions;

namespace WhiteLagoon.Application.Common.Interfaces;

/*
 * this Generic interface is used to implement the Repository pattern
 * 
 */
public interface IRepository<T> where T : class
{
    IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string includeProperties = null);
    T Get(Expression<Func<T, bool>> filter, string includeProperties = null);
    void Add(T entity);
    void Remove(T entity);
}
