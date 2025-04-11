namespace RansauSysteme.Database.Repository
{
    /// <summary>
    /// Defines the contract for a generic repository pattern implementation
    /// that provides data access operations for entity types.
    /// </summary>
    /// <typeparam name="T">The entity type this repository manages. Must be a class.</typeparam>
    public interface IRepository<T> where T : class
    {
        // Synchronous methods

        /// <summary>
        /// Retrieves an entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the retrieval operation.</exception>
        public T? GetById(int id);

        /// <summary>
        /// Retrieves multiple entities by their identifiers.
        /// </summary>
        /// <param name="ids">The collection of unique identifiers to retrieve.</param>
        /// <returns>A collection of entities matching the provided identifiers. Positions without a match will contain null.</returns>
        /// <remarks>
        /// The returned collection preserves the order of the input identifiers.
        /// If an entity with a specified ID doesn't exist, the corresponding position in the result will be null.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the retrieval operation.</exception>
        public IEnumerable<T?> GetById(IEnumerable<int> ids);

        /// <summary>
        /// Retrieves all entities from the repository.
        /// </summary>
        /// <returns>A collection of all entities in the repository.</returns>
        /// <remarks>
        /// This method may return a large result set depending on the size of the database table.
        /// Consider using paging for large data sets.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the retrieval operation.</exception>
        public IEnumerable<T> GetAll();

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The identifier of the newly added entity.</returns>
        /// <remarks>
        /// The entity's ID property will not be automatically updated with the new ID.
        /// If you need to access the newly assigned ID, use the returned value.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the add operation.</exception>
        public int Add(T entity);

        /// <summary>
        /// Adds multiple entities to the repository as a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        /// <returns>True if all entities were added successfully; otherwise, false.</returns>
        /// <remarks>
        /// This operation is transactional. If any entity fails to be added,
        /// the entire operation will be rolled back.
        /// The entities' ID properties will not be automatically updated with their new IDs.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the add operation.</exception>
        public bool Add(IEnumerable<T> entities);

        /// <summary>
        /// Updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>True if the entity was updated successfully; otherwise, false.</returns>
        /// <remarks>
        /// Returns false if no entity with the matching ID exists in the repository.
        /// All properties of the entity will be updated in the database.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the update operation.</exception>
        public bool Update(T entity);

        /// <summary>
        /// Updates multiple entities in the repository as a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <returns>True if all entities were updated successfully; otherwise, false.</returns>
        /// <remarks>
        /// This operation is transactional. If any entity fails to be updated,
        /// the entire operation will be rolled back.
        /// Returns false if any of the entities do not exist in the repository.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the update operation.</exception>
        public bool Update(IEnumerable<T> entities);

        /// <summary>
        /// Deletes an entity from the repository by its identifier.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>True if the entity was deleted successfully; otherwise, false.</returns>
        /// <remarks>
        /// Returns false if no entity with the specified ID exists in the repository.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the delete operation.</exception>
        public bool Delete(T entity);

        /// <summary>
        /// Deletes multiple entities from the repository by their identifiers as a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to delete.</param>
        /// <returns>True if all entities were deleted successfully; otherwise, false.</returns>
        /// <remarks>
        /// This operation is transactional. If any entity fails to be deleted,
        /// the entire operation will be rolled back.
        /// Returns false if any of the specified IDs do not exist in the repository.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the delete operation.</exception>
        public bool Delete(IEnumerable<T> entities);

        /// <summary>
        /// Determines whether an entity with the specified identifier exists in the repository.
        /// </summary>
        /// <param name="id">The unique identifier to check.</param>
        /// <returns>True if an entity with the specified identifier exists; otherwise, false.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the existence check.</exception>
        public bool Exists(int id);

        // Asynchronous methods

        /// <summary>
        /// Asynchronously retrieves an entity by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the retrieval operation.</exception>
        public Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Asynchronously retrieves multiple entities by their identifiers.
        /// </summary>
        /// <param name="ids">The collection of unique identifiers to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a collection of entities matching the provided identifiers. Positions without a match will contain null.</returns>
        /// <remarks>
        /// The returned collection preserves the order of the input identifiers.
        /// If an entity with a specified ID doesn't exist, the corresponding position in the result will be null.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the retrieval operation.</exception>
        public Task<IEnumerable<T?>> GetByIdAsync(IEnumerable<int> ids);

        /// <summary>
        /// Asynchronously retrieves all entities from the repository.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains a collection of all entities in the repository.</returns>
        /// <remarks>
        /// This method may return a large result set depending on the size of the database table.
        /// Consider using paging for large data sets.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the retrieval operation.</exception>
        public Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Asynchronously adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the identifier of the newly added entity.</returns>
        /// <remarks>
        /// The entity's ID property will not be automatically updated with the new ID.
        /// If you need to access the newly assigned ID, use the returned value.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the add operation.</exception>
        public Task<int> AddAsync(T entity);

        /// <summary>
        /// Asynchronously adds multiple entities to the repository as a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        /// <returns>A task representing the asynchronous operation. The task result indicates whether all entities were added successfully.</returns>
        /// <remarks>
        /// This operation is transactional. If any entity fails to be added,
        /// the entire operation will be rolled back.
        /// The entities' ID properties will not be automatically updated with their new IDs.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the add operation.</exception>
        public Task<bool> AddAsync(IEnumerable<T> entities);

        /// <summary>
        /// Asynchronously updates an existing entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>A task representing the asynchronous operation. The task result indicates whether the entity was updated successfully.</returns>
        /// <remarks>
        /// Returns false if no entity with the matching ID exists in the repository.
        /// All properties of the entity will be updated in the database.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the update operation.</exception>
        public Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// Asynchronously updates multiple entities in the repository as a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <returns>A task representing the asynchronous operation. The task result indicates whether all entities were updated successfully.</returns>
        /// <remarks>
        /// This operation is transactional. If any entity fails to be updated,
        /// the entire operation will be rolled back.
        /// Returns false if any of the entities do not exist in the repository.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the update operation.</exception>
        public Task<bool> UpdateAsync(IEnumerable<T> entities);

        /// <summary>
        /// Asynchronously deletes an entity from the repository by its identifier.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A task representing the asynchronous operation. The task result indicates whether the entity was deleted successfully.</returns>
        /// <remarks>
        /// Returns false if no entity with the specified ID exists in the repository.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the delete operation.</exception>
        public Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Asynchronously deletes multiple entities from the repository by their identifiers as a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to delete.</param>
        /// <returns>A task representing the asynchronous operation. The task result indicates whether all entities were deleted successfully.</returns>
        /// <remarks>
        /// This operation is transactional. If any entity fails to be deleted,
        /// the entire operation will be rolled back.
        /// Returns false if any of the specified IDs do not exist in the repository.
        /// </remarks>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the delete operation.</exception>
        public Task<bool> DeleteAsync(IEnumerable<T> entities);

        /// <summary>
        /// Asynchronously determines whether an entity with the specified identifier exists in the repository.
        /// </summary>
        /// <param name="id">The unique identifier to check.</param>
        /// <returns>A task representing the asynchronous operation. The task result indicates whether an entity with the specified identifier exists.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs during the existence check.</exception>
        public Task<bool> ExistsAsync(int id);
    }
}