﻿using Dapper;
using Newtonsoft.Json;
using NFC.Application.Shared;
using NFC.Common.Constants;
using NFC.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace NFC.Infrastructure.SharedKernel
{
    /// <summary>
    /// Provides the common methods for all repositories.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="NFC.Infrastructure.SharedKernel.IGenericRepository{TKey, TEntity}" />
    public abstract class GenericRepositoryBase<TKey, TEntity> : IDisposable, IGenericRepository<TKey, TEntity>
        where TKey : IComparable
        where TEntity : class, IDomainEntity<TKey>
    {
        #region Variables

        /// <summary>
        /// The repository
        /// </summary>
        protected readonly IRepository repository;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The identifier col name
        /// </summary>
        protected readonly string identifierColName = string.Empty;

        /// <summary>
        /// The table name
        /// </summary>
        private readonly string tblName;

        /// <summary>
        /// The table attribute.
        /// </summary>
        private const string TableAttribute = "TableAttribute";

        /// <summary>
        /// The skip properties
        /// </summary>
        private readonly IEnumerable<string> ignoreProps;

        #endregion

        #region Properties

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepositoryBase{TKey, TEntity}"/> class.
        /// </summary>
        /// <param name="repository">The data access object.</param>
        public GenericRepositoryBase(IRepository repository)
        {
            this.tblName = this.GetTableName(typeof(TEntity));
            this.ignoreProps = typeof(TEntity).GetProperties().Where(n => n.PropertyType.GetProperty(n.Name).GetAccessors()[0].IsVirtual)?.Select(n => n.Name);
            this.repository = repository;
        }

        #endregion

        #region Implementation of IDao<TKey, TEntity>

        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public virtual TKey Add(TEntity model)
        {
            return this.repository.SelectSingleOrDefault<TKey>($"Create{this.tblName}", this.BuildParmas4Adding(model));
        }

        /// <summary>
        /// Gets the single by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual TEntity GetSingleById(TKey id)
        {
            return this.repository.SelectSingleOrDefault<TEntity>($"Get{this.tblName}ById", this.BuildParmas(id));
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> GetAll()
        {
            return this.repository.Select<TEntity>($"GetAll{this.tblName}");
        }

        /// <summary>
        /// Gets all by paging.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="getLatest">if set to <c>true</c> [get latest].</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAllByPaging(int pageNumber = 1, int pageSize = 30, bool getLatest = false)
        {
            return this.repository.Select<TEntity>($"GetAll{this.tblName}ByPaging", this.BuildPagingParams(pageNumber, pageSize, getLatest));
        }

        /// <summary>
        /// Gets all by paging.
        /// </summary>
        /// <param name="storeName">Name of the store.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="getLatest">if set to <c>true</c> [get latest].</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAllByPaging(string storeName, int pageNumber = 1, int pageSize = 30, bool getLatest = false)
        {
            return this.repository.Select<TEntity>(storeName, this.BuildPagingParams(pageNumber, pageSize, getLatest));
        }

        /// <summary>
        /// Gets all by paging.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storeName">Name of the store.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="getLatest">if set to <c>true</c> [get latest].</param>
        /// <returns></returns>
        public IEnumerable<T> GetAllByPaging<T>(string storeName, int pageNumber = 1, int pageSize = 30, bool getLatest = false) where T : class
        {
            return this.repository.Select<T>(storeName, this.BuildPagingParams(pageNumber, pageSize, getLatest));
        }

        /// <summary>
        /// Gets the by by paging search.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="getLatest">if set to <c>true</c> [get latest].</param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetByByPagingSearch(string name, int pageNumber = 1, int pageSize = 30, bool getLatest = false)
        {
            return this.Select<TEntity>($"GetAll{this.tblName}ByPagingSearch", this.BuildPagingSearchParams(name, pageNumber, pageSize, getLatest));
        }



        /// <summary>
        /// Gets the by by paging search.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storeName">Name of the store.</param>
        /// <param name="name">The name.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="getLatest">if set to <c>true</c> [get latest].</param>
        /// <returns></returns>
        public IEnumerable<T> GetByByPagingSearch<T>(string storeName, string name, int pageNumber = 1, int pageSize = 30, bool getLatest = false) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find by condition.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, object>> expression)
        {
            return this.repository.Query<TEntity>(this.BuildSqlQuery(expression));
        }

        /// <summary>
        /// Find by condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public IEnumerable<T> Find<T>(Expression<Func<T, object>> expression) where T : class
        {
            return this.repository.Query<T>(this.BuildSqlQuery(expression));
        }

        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="model">The model.</param>
        public virtual void Update(TKey id, TEntity model)
        {
            this.repository.Execute<int>($"[Update{this.tblName}]", this.BuildParmas(model));
        }

        /// <summary>
        /// Remove an instance.
        /// </summary>
        /// <param name="key">The key.</param>
        public virtual void Remove(TKey key)
        {
            this.repository.Execute<int>($"Delete{this.tblName}ById", this.BuildParmas(key));
        }

        /// <summary>
        /// Selects the specified instances.
        /// </summary>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> Select(string storedName, IDictionary<string, object> parmas = null)
        {
            return this.repository.Select<TEntity>(storedName, parmas);
        }

        /// <summary>
        /// Selects the specified instances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public IEnumerable<T> Select<T>(string storedName, IDictionary<string, object> parmas = null) where T : class
        {
            return this.repository.Select<T>(storedName, parmas);
        }

        /// <summary>
        /// Selects the single or default.
        /// </summary>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public TEntity SelectSingleOrDefault(string storedName, IDictionary<string, object> parmas = null)
        {
            return this.repository.SelectSingleOrDefault<TEntity>(storedName, parmas);
        }

        /// <summary>
        /// Selects the single or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public T SelectSingleOrDefault<T>(string storedName, IDictionary<string, object> parmas = null) where T : class
        {
            return this.repository.SelectSingleOrDefault<T>(storedName, parmas);
        }

        /// <summary>
        /// Executes the specified stored procedure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public IEnumerable<T> Execute<T>(string storedName, IDictionary<string, object> parmas = null) where T : class
        {
            return this.repository.Execute<T>(storedName, parmas);
        }

        /// <summary>
        /// Executes the specified stored procedure.
        /// </summary>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public IEnumerable<TEntity> Execute(string storedName, IDictionary<string, object> parmas = null)
        {
            return this.repository.Execute<TEntity>(storedName, parmas);
        }

        #endregion

        #region Implementation of IDisposal

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                return;
            }
            this.disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GenericRepositoryBase{TKey, TEntity}"/> class.
        /// </summary>
        ~GenericRepositoryBase()
        {
            this.Dispose(false);
        }
        #endregion

        #region Internal methods

        /// <summary>
        /// Builds the parmas for adding.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> BuildParmas4Adding(TEntity model)
        {
            var parmas = this.BuildParmas(model);
            parmas.Remove(Const.Id);
            return parmas;
        }

        /// <summary>
        /// Builds the parmas.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> BuildParmas(TEntity model)
        {
            var json = JsonConvert.SerializeObject(model);
            var dicMapping = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return this.RemoveNotMappingProps(dicMapping);
        }

        protected virtual IDictionary<string, object> BuildParmas<T>(params T[] input)
        {
            var json = JsonConvert.SerializeObject(input);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }

        /// <summary>
        /// Builds the parmas.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> BuildParmas(TKey id)
        {
            var dic = new Dictionary<string, object>
            {
                { Const.Id, id }
            };
            return dic;
        }

        /// <summary>
        /// Removes the not mapping props.
        /// </summary>
        /// <param name="dicMapping">The dic mapping.</param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> RemoveNotMappingProps(IDictionary<string, object> dicMapping)
        {
            if (this.ignoreProps != null)
            {
                foreach (var prop in this.ignoreProps)
                {
                    dicMapping.Remove(prop);
                }
            }

            return dicMapping;
        }

       

        /// <summary>
        /// Gets table name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private string GetTableName(Type type)
        {
            var name = type.Name;
            return name;
        }

        /// <summary>
        /// Builds the paging parameters.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="getLatest">if set to <c>true</c> [get latest].</param>
        /// <returns></returns>
        protected IDictionary<string, object> BuildPagingParams(int pageNumber, int pageSize, bool getLatest)
        {
            return new Dictionary<string, object>
            {
                [Const.PageNumber] = pageNumber,
                [Const.PageSize] = pageSize,
                [Const.GetLatest] = getLatest
            };
        }

        /// <summary>
        /// Builds the paging search parameters.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="getLatest">if set to <c>true</c> [get latest].</param>
        /// <returns></returns>
        protected virtual IDictionary<string, object> BuildPagingSearchParams(string name, int pageNumber, int pageSize, bool getLatest)
        {
            return new Dictionary<string, object>
            {
                [Const.Name] = name,
                [Const.PageNumber] = pageNumber,
                [Const.PageSize] = pageSize,
                [Const.GetLatest] = getLatest
            };
        }

        /// <summary>
        /// Build sql query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        protected virtual string BuildSqlQuery<T>(Expression<Func<T, object>> expression)
        {
            var binaryExpression = (BinaryExpression)((UnaryExpression)expression.Body).Operand;
            var left = Expression.Lambda<Func<T, object>>(Expression.Convert(binaryExpression.Left, typeof(object)), expression.Parameters[0]);
            var right = binaryExpression.Right.GetType().GetProperty("Value").GetValue(binaryExpression.Right);
            var theOperator = this.DetermineOperator(binaryExpression);
            return $"SELECT * FROM {this.tblName} WHERE {left} {theOperator} {right}";
        }

        /// <summary>
        /// Determine operator.
        /// </summary>
        /// <param name="binaryExpression">The binary expression.</param>
        /// <returns></returns>
        protected virtual string DetermineOperator(BinaryExpression binaryExpression)
        {
            return binaryExpression.NodeType switch
            {
                ExpressionType.Equal => SqlOperator.Equal.GetDescription(),
                ExpressionType.GreaterThan => SqlOperator.GeaterThan.GetDescription(),
                ExpressionType.GreaterThanOrEqual => SqlOperator.GearterThanOrEqual.GetDescription(),
                ExpressionType.LessThan => SqlOperator.LessThan.GetDescription(),
                ExpressionType.LessThanOrEqual => SqlOperator.LessThanOrEqual.GetDescription(),
                _ => SqlOperator.Equal.GetDescription(),
            };
        }

        #endregion
    }

    /// <summary>
    /// Provides the common methods to interact with database through dapper framework
    /// </summary>
    /// <remarks>The wraper dapper framework methods.</remarks>
    /// <seealso cref="System.IDisposable" />
    /// <seealso cref="NFC.Infrastructure.SharedKernel.IRepository" />
    public class Repository : IRepository, IDisposable
    {
        #region Variables

        /// <summary>
        /// The database factory
        /// </summary>
        private readonly IDbFactory dbFactory;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="dbFactory">The database factory.</param>
        public Repository(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        #endregion

        #region Implementation of IDao

        /// <summary>
        /// Executes the specified stored name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public IEnumerable<T> Execute<T>(string storedName, IDictionary<string, object> parmas)
        {
            return this.OnDbExecute(this.GetMany<T>, storedName, parmas);
        }

        /// <summary>
        /// Selects the specified stored name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedName">Name of the stored.</param>
        /// <returns></returns>
        public IEnumerable<T> Select<T>(string storedName)
        {
            return this.OnDbExecute(this.GetMany<T>, storedName);
        }

        /// <summary>
        /// Selects the specified stored name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public IEnumerable<T> Select<T>(string storedName, IDictionary<string, object> parmas)
        {
            return this.OnDbExecute(this.GetMany<T>, storedName, parmas);
        }

        /// <summary>
        /// Selects the single or default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        public T SelectSingleOrDefault<T>(string storedName, IDictionary<string, object> parmas)
        {
            return this.OnDbExecute(this.GetSingle<T>, storedName, parmas);
        }

        /// <summary>
        /// Queries the sql.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">The sql.</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql)
        {
            using (var cnn = this.dbFactory.CreateConnection())
            {
                cnn.Open();
                var result = cnn.Query<T>(sql);
                cnn.Close();
                return result;
            }
        }

        #endregion

        #region Implementation of IDisposal

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                return;
            }
            this.disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Repository"/> class.
        /// </summary>
        ~Repository()
        {
            this.Dispose(false);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Builds the parmas.
        /// </summary>
        /// <param name="dp">The dp.</param>
        /// <param name="parmas">The parmas.</param>
        private void BuildParmas(DynamicParameters dp, IDictionary<string, object> parmas)
        {

            foreach (var item in parmas)
            {
                var key = !item.Key.Contains("@") ? $"@{item.Key}" : item.Key;
                dp.Add(key, item.Value);
            }
        }

        /// <summary>
        /// Called when [database execute].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function.</param>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        private T OnDbExecute<T>(Func<IDbConnection, string, IDictionary<string, object>, T> func, string storedName, IDictionary<string, object> parmas = null)
        {
            using (var cnn = this.dbFactory.CreateConnection())
            {
                cnn.Open();
                var result = func.Invoke(cnn, storedName, parmas);
                cnn.Close();
                return result;
            }
        }

        /// <summary>
        /// Gets the many.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn">The CNN.</param>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        private IEnumerable<T> GetMany<T>(IDbConnection cnn, string storedName, IDictionary<string, object> parmas)
        {
            return this.GetDbInfo<T>(cnn, storedName, parmas) as IEnumerable<T>;
        }

        /// <summary>
        /// Gets the single.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn">The CNN.</param>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        private T GetSingle<T>(IDbConnection cnn, string storedName, IDictionary<string, object> parmas)
        {
            return (this.GetDbInfo<T>(cnn, storedName, parmas) as IEnumerable<T>).SingleOrDefault();
        }

        /// <summary>
        /// Gets the database information.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cnn">The connection.</param>
        /// <param name="storedName">Name of the stored.</param>
        /// <param name="parmas">The parmas.</param>
        /// <returns></returns>
        private object GetDbInfo<T>(IDbConnection cnn, string storedName, IDictionary<string, object> parmas)
        {
            if (!parmas.IsNullOrEmpty())
            {
                var dp = new DynamicParameters();
                this.BuildParmas(dp, parmas);
                var data = cnn.Query<T>(storedName, dp, commandType: CommandType.StoredProcedure);
                return data;
            }
            else
            {
                var data = cnn.Query<T>(storedName, commandType: CommandType.StoredProcedure);
                return data;
            }
        }

        #endregion
    }
}
