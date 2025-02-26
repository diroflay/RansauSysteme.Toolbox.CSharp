using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Extensions.Logging;
using RansauSysteme.Database.Connection;
using RansauSysteme.Database.Exceptions;
using RansauSysteme.Utils;

namespace RansauSysteme.Database.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        public readonly IDatabaseConnection DatabaseConnection;

        public readonly string TableName;
        public readonly string KeyPropertyName;

        public readonly ImmutableArray<PropertyInfo> EntityProperties;
        public readonly PropertyInfo KeyProperty;
        public readonly ImmutableDictionary<string, PropertyInfo> PropertyMap;

        // Configurable batch size to prevent too large SQL queries
        public virtual int MaxBatchSize { get; set; }

        public virtual ILogger? Logger { get; set; }

        public BaseRepository(IDatabaseConnection databaseConnection, ILogger? logger = null)
        {
            DatabaseConnection = databaseConnection ?? throw new ArgumentNullException(nameof(databaseConnection));

            //Ensure Logs entity does not send log online
            Logger = null;

            MaxBatchSize = 1000;

            TableName = typeof(T).Name.ToSnakeCase();
            KeyPropertyName = GetKeyPropertyName();

            EntityProperties = typeof(T).GetProperties().ToImmutableArray();
            PropertyMap = EntityProperties.ToImmutableDictionary(p => p.Name, p => p);
            KeyProperty = PropertyMap[KeyPropertyName];
        }

        /********************************************
         *  SYNCHRONOUS METHODS
         */

        public virtual T? GetById(int id)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"SELECT * FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} = @Id";
                return connection.QueryFirstOrDefault<T>(query, new { Id = id });
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error retrieving entity with ID {Id} from {Table}", id, TableName);
                throw new RepositoryException($"Failed to retrieve entity from {TableName}", ex);
            }
        }

        public virtual IEnumerable<T?> GetById(IEnumerable<int> ids)
        {
            try
            {
                var idList = ids.ToList();
                if (!idList.Any())
                    return Enumerable.Empty<T>();

                using var connection = DatabaseConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Ids", idList);

                var results = connection.Query<T>(
                    $"SELECT * FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} IN @Ids",
                    parameters);

                // Return results in the same order as requested IDs
                return idList.Select(id =>
                    results.FirstOrDefault(r => GetKeyIntValue(r) == id));
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error retrieving multiple entities by IDs");
                throw new RepositoryException("Failed to retrieve multiple entities", ex);
            }
        }

        public virtual IEnumerable<T> GetAll()
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"SELECT * FROM {TableName}";
                return connection.Query<T>(query);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error retrieving all entities from {Table}", TableName);
                throw new RepositoryException($"Failed to retrieve entities from {TableName}", ex);
            }
        }

        public virtual int Add(T entity)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var properties = GetInsertableColumns(entity);
                var columns = string.Join(", ", properties.Keys);
                var values = string.Join(", ", properties.Values);
                var query = $"INSERT INTO {TableName} ({columns}) VALUES ({values}); SELECT LAST_INSERT_ID();";

                return connection.ExecuteScalar<int>(query, entity);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error adding entity to {Table}", TableName);
                throw new RepositoryException($"Failed to add entity to {TableName}", ex);
            }
        }

        public virtual bool Add(IEnumerable<T> entities)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var entity in entities)
                    {
                        var properties = GetInsertableColumns(entity);
                        var columns = string.Join(", ", properties.Keys);
                        var values = string.Join(", ", properties.Values);
                        var query = $"INSERT INTO {TableName} ({columns}) VALUES ({values})";

                        connection.Execute(query, entity, transaction);
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error adding multiple entities");
                throw new RepositoryException("Failed to add multiple entities", ex);
            }
        }

        public virtual bool Update(T entity)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var properties = GetUpdatableColumns(entity);
                var setClauses = properties.Values;
                var query = $"UPDATE {TableName} SET {string.Join(", ", setClauses)} WHERE {KeyPropertyName.ToSnakeCase()} = @{KeyPropertyName}";

                connection.Execute(query, entity);

                int rowsAffected = connection.Execute(query, entity);
                bool success = rowsAffected > 0;

                if (!success)
                {
                    Logger?.LogWarning("No entity was updated with ID {Id}", GetKeyIntValue(entity));
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating entity in {Table}", TableName);
                throw new RepositoryException($"Failed to update entity in {TableName}", ex);
            }
        }

        public virtual bool Update(IEnumerable<T> entities)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var entity in entities)
                    {
                        var properties = GetUpdatableColumns(entity);
                        var setClauses = properties.Values;
                        var query = $"UPDATE {TableName} SET {string.Join(", ", setClauses)} WHERE {KeyPropertyName.ToSnakeCase()} = @{KeyPropertyName}";

                        var rowsAffected = connection.Execute(query, entity, transaction);
                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating multiple entities");
                throw new RepositoryException("Failed to update multiple entities", ex);
            }
        }

        public virtual bool Delete(int id)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"DELETE FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} = @Id";

                var rowsAffected = connection.Execute(query, new { Id = id });

                if (rowsAffected == 0)
                {
                    Logger?.LogWarning("Attempted to delete non-existent entity with ID {Id} from {Table}",
                        id, TableName);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error deleting entity with ID {Id} from {Table}", id, TableName);
                throw new RepositoryException($"Failed to delete entity from {TableName}", ex);
            }
        }

        public virtual bool Delete(IEnumerable<int> ids)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Ids", ids);

                    var query = $"DELETE FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} IN @Ids";
                    var rowsAffected = connection.Execute(query, parameters, transaction);

                    if (rowsAffected != ids.Count())
                    {
                        transaction.Rollback();
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error deleting multiple entities");
                throw new RepositoryException("Failed to delete multiple entities", ex);
            }
        }

        public virtual bool Exists(int id)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"SELECT COUNT(1) FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} = @{KeyPropertyName}";
                return connection.ExecuteScalar<int>(query, new { Id = id }) > 0;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error checking existence of entity with ID {Id} in {Table}", id, TableName);
                throw new RepositoryException($"Failed to check entity existence in {TableName}", ex);
            }
        }

        /********************************************
         *  ASYNCHRONOUS METHODS
         */

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"SELECT * FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} = @{KeyPropertyName}";
                return await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error retrieving entity with ID {Id} from {Table}", id, TableName);
                throw new RepositoryException($"Failed to retrieve entity from {TableName}", ex);
            }
        }

        public virtual async Task<IEnumerable<T?>> GetByIdAsync(IEnumerable<int> ids)
        {
            try
            {
                var idList = ids.ToList();
                if (!idList.Any())
                    return Enumerable.Empty<T>();

                using var connection = DatabaseConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Ids", idList);

                // Get results from database
                var results = (await connection.QueryAsync<T>(
                    $"SELECT * FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} IN @Ids",
                    parameters)).ToArray();

                // Return results in the same order as requested IDs
                return idList.Select(id =>
                    Array.Find(results, r =>
                        Convert.ToInt32(GetKeyValue(r)) == id));
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error retrieving multiple entities by IDs");
                throw new RepositoryException("Failed to retrieve multiple entities", ex);
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"SELECT * FROM {TableName}";
                return await connection.QueryAsync<T>(query);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error retrieving all entities from {Table}", TableName);
                throw new RepositoryException($"Failed to retrieve entities from {TableName}", ex);
            }
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var properties = GetInsertableColumns(entity);
                var columns = string.Join(", ", properties.Keys);
                var values = string.Join(", ", properties.Values);
                var query = $"INSERT INTO {TableName} ({columns}) VALUES ({values}); SELECT LAST_INSERT_ID();";

                return await connection.ExecuteScalarAsync<int>(query, entity);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error adding entity to {Table}", TableName);
                throw new RepositoryException($"Failed to add entity to {TableName}", ex);
            }
        }

        public virtual async Task<bool> AddAsync(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();
            if (!entityList.Any()) return true;

            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Process entities in batches
                    for (int i = 0; i < entityList.Count; i += MaxBatchSize)
                    {
                        var batch = entityList.Skip(i).Take(MaxBatchSize);
                        var valuesList = new List<string>();
                        var parameters = new DynamicParameters();
                        var properties = GetInsertableColumns(entityList[0]);
                        var columns = string.Join(", ", properties.Keys);

                        foreach (var entity in batch)
                        {
                            var paramNames = new List<string>();
                            foreach (var prop in EntityProperties.Where(p => p != KeyProperty && p.CanWrite && p.CanRead))
                            {
                                var paramName = $"{prop.Name}_{Guid.NewGuid():N}";
                                var value = prop.GetValue(entity);
                                parameters.Add(paramName, value == null ? null : value);
                                paramNames.Add($"@{paramName}");
                            }
                            valuesList.Add($"({string.Join(", ", paramNames)})");
                        }

                        var query = $"INSERT INTO {TableName} ({columns}) VALUES {string.Join(", ", valuesList)}";
                        await connection.ExecuteAsync(query, parameters, transaction);
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error performing batch insert of {Count} entities", entityList.Count);
                throw new RepositoryException("Failed to perform batch insert", ex);
            }
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();

                // First check if entity exists
                if (!await ExistsAsync(GetKeyIntValue(entity)))
                {
                    Logger?.LogWarning("Attempted to update non-existent entity with ID {Id} in {Table}",
                        GetKeyIntValue(entity), TableName);
                    return false;
                }

                var properties = GetUpdatableColumns(entity);
                var setClauses = properties.Values;
                var query = $"UPDATE {TableName} SET {string.Join(", ", setClauses)} WHERE {KeyPropertyName.ToSnakeCase()} = @{KeyPropertyName}";

                var rowsAffected = await connection.ExecuteAsync(query, entity);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating entity in {Table}", TableName);
                throw new RepositoryException($"Failed to update entity in {TableName}", ex);
            }
        }

        public virtual async Task<bool> UpdateAsync(IEnumerable<T> entities)
        {
            var entityList = entities.ToList();
            if (!entityList.Any()) return true;

            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var batch in entityList.Chunk(MaxBatchSize))
                    {
                        var parameters = new DynamicParameters();
                        var updateCases = new List<string>();
                        var entityIds = new List<object>();

                        foreach (var entity in batch)
                        {
                            var id = GetKeyValue(entity);
                            entityIds.Add(id);

                            foreach (var prop in EntityProperties.Where(p => p != KeyProperty && p.CanWrite && p.CanRead))
                            {
                                var paramName = $"{prop.Name}_{Guid.NewGuid():N}";
                                var value = prop.GetValue(entity);
                                parameters.Add(paramName, value == null ? null : value);
                                updateCases.Add($"WHEN {KeyPropertyName.ToSnakeCase()} = {id} THEN @{paramName}");
                            }
                        }

                        var setClauses = EntityProperties
                            .Where(p => p != KeyProperty && p.CanWrite && p.CanRead)
                            .Select(prop => $"{prop.Name.ToSnakeCase()} = CASE {string.Join(" ", updateCases.Where(c => c.Contains(prop.Name)))} END");

                        var query = $@"
                    UPDATE {TableName}
                    SET {string.Join(",", setClauses)}
                    WHERE {KeyPropertyName.ToSnakeCase()} IN ({string.Join(",", entityIds)})";

                        var rowsAffected = await connection.ExecuteAsync(query, parameters, transaction);
                        if (rowsAffected != batch.Length)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error performing batch update of {Count} entities", entityList.Count);
                throw new RepositoryException("Failed to perform batch update", ex);
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"DELETE FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} = @{KeyPropertyName}";

                var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });

                if (rowsAffected == 0)
                {
                    Logger?.LogWarning("Attempted to delete non-existent entity with ID {Id} from {Table}",
                        id, TableName);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error deleting entity with ID {Id} from {Table}", id, TableName);
                throw new RepositoryException($"Failed to delete entity from {TableName}", ex);
            }
        }

        public virtual async Task<bool> DeleteAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any()) return true;

            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var batch in idList.Chunk(MaxBatchSize))
                    {
                        var parameters = new DynamicParameters();
                        var paramNames = batch.Select(id =>
                        {
                            var paramName = $"p_{Guid.NewGuid():N}";
                            parameters.Add(paramName, id);
                            return $"@{paramName}";
                        });

                        var query = $"DELETE FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} IN ({string.Join(",", paramNames)})";
                        var rowsAffected = await connection.ExecuteAsync(query, parameters, transaction);

                        if (rowsAffected != batch.Length)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error performing batch delete of {Count} entities", idList.Count);
                throw new RepositoryException("Failed to perform batch delete", ex);
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                var query = $"SELECT COUNT(1) FROM {TableName} WHERE {KeyPropertyName.ToSnakeCase()} = @Id";
                return await connection.ExecuteScalarAsync<int>(query, new { Id = id }) > 0;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error checking existence of entity with ID {Id} in {Table}", id, TableName);
                throw new RepositoryException($"Failed to check entity existence in {TableName}", ex);
            }
        }

        /********************************************
         *  ADDITIONAL SQL REPOSITORY METHODS
         */

        /// <summary>
        /// Executes a non-query SQL command and returns the number of affected rows
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="param">The parameters to use in the query (optional)</param>
        /// <param name="commandType">The type of command to execute (default: CommandType.Text)</param>
        /// <returns>Number of rows affected</returns>
        public virtual int Execute(string sql, object? param = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                return connection.Execute(sql, param, commandType: commandType);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error executing SQL command: {Sql}", sql);
                throw new RepositoryException("Failed to execute SQL command", ex);
            }
        }

        /// <summary>
        /// Executes a non-query SQL command asynchronously and returns the number of affected rows
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="param">The parameters to use in the query (optional)</param>
        /// <param name="commandType">The type of command to execute (default: CommandType.Text)</param>
        /// <param name="cancellationToken">Cancellation token (optional)</param>
        /// <returns>Number of rows affected</returns>
        public virtual async Task<int> ExecuteAsync(
            string sql,
            object? param = null,
            CommandType commandType = CommandType.Text,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                return await connection.ExecuteAsync(
                    new CommandDefinition(
                        sql,
                        param,
                        commandType: commandType,
                        cancellationToken: cancellationToken
                    )
                );
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error executing SQL command asynchronously: {Sql}", sql);
                throw new RepositoryException("Failed to execute SQL command asynchronously", ex);
            }
        }

        /// <summary>
        /// Executes a batch of SQL commands within a transaction
        /// </summary>
        /// <param name="commands">Collection of SQL commands and their parameters</param>
        /// <param name="commandType">The type of command to execute (default: CommandType.Text)</param>
        /// <returns>True if all commands executed successfully</returns>
        public virtual bool ExecuteBatch(
            IEnumerable<(string Sql, object? Parameters)> commands,
            CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var (sql, parameters) in commands)
                    {
                        connection.Execute(sql, parameters, transaction, commandType: commandType);
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error executing batch of SQL commands");
                throw new RepositoryException("Failed to execute batch of SQL commands", ex);
            }
        }

        /// <summary>
        /// Executes a batch of SQL commands asynchronously within a transaction
        /// </summary>
        /// <param name="commands">Collection of SQL commands and their parameters</param>
        /// <param name="commandType">The type of command to execute (default: CommandType.Text)</param>
        /// <param name="cancellationToken">Cancellation token (optional)</param>
        /// <returns>True if all commands executed successfully</returns>
        public virtual async Task<bool> ExecuteBatchAsync(
            IEnumerable<(string Sql, object? Parameters)> commands,
            CommandType commandType = CommandType.Text,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                using var transaction = connection.BeginTransaction();

                try
                {
                    foreach (var (sql, parameters) in commands)
                    {
                        await connection.ExecuteAsync(
                            new CommandDefinition(
                                sql,
                                parameters,
                                transaction,
                                commandType: commandType,
                                cancellationToken: cancellationToken
                            )
                        );
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error executing batch of SQL commands asynchronously");
                throw new RepositoryException("Failed to execute batch of SQL commands asynchronously", ex);
            }
        }

        /// <summary>
        /// Executes a query and returns a single result
        /// </summary>
        /// <typeparam name="TResult">The type of result to return</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="param">The parameters to use in the query (optional)</param>
        /// <param name="commandType">The type of command to execute (default: CommandType.Text)</param>
        /// <returns>The query result</returns>
        public virtual TResult? ExecuteScalar<TResult>(
            string sql,
            object? param = null,
            CommandType commandType = CommandType.Text)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                return connection.ExecuteScalar<TResult>(sql, param, commandType: commandType);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error executing scalar SQL query: {Sql}", sql);
                throw new RepositoryException("Failed to execute scalar SQL query", ex);
            }
        }

        /// <summary>
        /// Executes a query asynchronously and returns a single result
        /// </summary>
        /// <typeparam name="TResult">The type of result to return</typeparam>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="param">The parameters to use in the query (optional)</param>
        /// <param name="commandType">The type of command to execute (default: CommandType.Text)</param>
        /// <param name="cancellationToken">Cancellation token (optional)</param>
        /// <returns>The query result</returns>
        public virtual async Task<TResult?> ExecuteScalarAsync<TResult>(
            string sql,
            object? param = null,
            CommandType commandType = CommandType.Text,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = DatabaseConnection.CreateConnection();
                return await connection.ExecuteScalarAsync<TResult>(
                    new CommandDefinition(
                        sql,
                        param,
                        commandType: commandType,
                        cancellationToken: cancellationToken
                    )
                );
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error executing scalar SQL query asynchronously: {Sql}", sql);
                throw new RepositoryException("Failed to execute scalar SQL query asynchronously", ex);
            }
        }

        /********************************************
         *   UTILS METHODS
         */

        public string GetKeyPropertyName()
        {
            // Cache the properties array since reflection is expensive
            var properties = typeof(T).GetProperties();

            // Find property with [Key] attribute
            var keyProperty = Array.Find(properties,
                p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any());

            // If no [Key] attribute found, fallback to convention-based "Id" property
            if (keyProperty == null)
            {
                keyProperty = Array.Find(properties,
                    p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

                if (keyProperty == null)
                {
                    throw new InvalidOperationException(
                        $"No primary key found for entity {typeof(T).Name}. " +
                        "Please specify a primary key using the [Key] attribute or name the key property 'Id'.");
                }
            }

            return keyProperty.Name;
        }

        public virtual Dictionary<string, object> GetInsertableProperties(T entity)
        {
            return EntityProperties
                .Where(p => p != KeyProperty && p.CanWrite && p.CanRead)
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(entity) ?? DBNull.Value
                );
        }

        public virtual Dictionary<string, string> GetInsertableColumns(T entity)
        {
            return EntityProperties
                .Where(p => p != KeyProperty && p.CanWrite && p.CanRead)
                .ToDictionary(
                    prop => prop.Name.ToSnakeCase(),
                    prop => "@" + prop.Name
                );
        }

        public virtual Dictionary<string, object> GetUpdatableProperties(T entity)
        {
            return EntityProperties
                .Where(p => p.CanWrite && p.CanRead)
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.GetValue(entity) ?? DBNull.Value
                );
        }

        public virtual Dictionary<string, string> GetUpdatableColumns(T entity)
        {
            return EntityProperties
                .Where(p => p.CanWrite && p.CanRead)
                .ToDictionary(
                    prop => prop.Name.ToSnakeCase(),
                    prop => $"{prop.Name.ToSnakeCase()} = @{prop.Name}"
                );
        }

        public virtual object GetKeyValue(T entity)
        {
            return KeyProperty.GetValue(entity) ??
                throw new InvalidOperationException("Primary key value cannot be null");
        }

        public int GetKeyIntValue(T entity)
        {
            return Convert.ToInt32(GetKeyValue(entity));
        }
    }
}