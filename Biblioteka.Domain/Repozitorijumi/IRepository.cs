using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Biblioteka.Domain.Repozitorijumi
{
    public interface IRepository<T> where T: class
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        T? GetById(params object[] keyValues);
        void Add(T entity);
        void Remove(T entity);
        void Update(T entity);
    }
}
