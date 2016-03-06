using System;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;

namespace DutyBot
{
    public class Repository<T> : IDisposable
        where T : DbContext, new()
    {
        private T _context;

        public virtual T DataContext
        {
            get { return _context ?? (_context = new T()); }
        }

        public virtual TEntity Get<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            predicate.CheckNotNull("Predicate value must be passed to Get<TResult>.");
            return DataContext.Set<TEntity>().Where(predicate).SingleOrDefault();
        }

        public virtual IQueryable<TEntity> GetList<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            predicate.CheckNotNull("Predicate value must be passed to GetList<TResult>.");
            return DataContext.Set<TEntity>().Where(predicate);
        }

        public virtual IQueryable<TEntity> GetList<TEntity, TKey>(Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TKey>> orderBy) where TEntity : class
        {
            return GetList(predicate).OrderBy(orderBy);
        }

        public virtual IQueryable<TEntity> GetList<TEntity, TKey>(Expression<Func<TEntity, TKey>> orderBy)
            where TEntity : class
        {
            return GetList<TEntity>().OrderBy(orderBy);
        }

        public virtual IQueryable<TEntity> GetList<TEntity>() where TEntity : class
        {
            return DataContext.Set<TEntity>();
        }

        public virtual bool Update()
        {
            return DataContext.SaveChanges() > 0;
        }

        public virtual bool Create<TEntity>(TEntity entity, params string[] propsToUpdate) where TEntity : class
        {

            ObjectSet<TEntity> objectSet =
                ((IObjectContextAdapter) DataContext).ObjectContext.CreateObjectSet<TEntity>();
            objectSet.AddObject(entity);

            return DataContext.SaveChanges() > 0;
        }

        public virtual bool Delete<TEntity>(TEntity entity) where TEntity : class
        {
            //проверка, чтобы нельзя юбыло удалить запись из таблицы Log
            NotDelete ra = (NotDelete) Attribute.GetCustomAttribute(typeof (TEntity), typeof (NotDelete));
            //провреяю, что объект не иммет атрибут notDelete 
            if (ra != null)
            {
                if (ra.notDelete) //или что он не true
                {
                    throw new Exception("Объект " + typeof (TEntity) + " не может быть удалён из БД");
                }
            }
        ObjectSet<TEntity> objectSet =
                ((IObjectContextAdapter) DataContext).ObjectContext.CreateObjectSet<TEntity>();
            objectSet.Attach(entity);
            objectSet.DeleteObject(entity);
            return DataContext.SaveChanges() > 0;
        }
        public void Dispose()
        {
            if (DataContext != null) DataContext.Dispose();
        }
    }

   public static class ObjectExtensions
    {
        public static void CheckNotNull(this object value, string error)
        {
            if (value == null)
                throw new Exception(error);
        }
    }
}
