using RansauSysteme.Database.Connection;

namespace RansauSysteme.Database.Repository.Factory
{
    public class BaseRepositoryFactory : IRepositoryFactory
    {
        private readonly Dictionary<Type, object> _repositories;
        private readonly IDatabaseConnection _databaseConnection;

        public BaseRepositoryFactory(IDatabaseConnection databaseConnection)
        {
            _repositories = new Dictionary<Type, object>();
            _databaseConnection = databaseConnection;
        }

        public IRepository<T> CreateRepository<T>() where T : class
        {
            var entityType = typeof(T);

            if (_repositories.TryGetValue(entityType, out var existingRepo))
            {
                return (IRepository<T>)existingRepo;
            }

            IRepository<T> repository;

            try
            {
                var repositoryType = GetRepositoryTypeForEntity<T>();

                // We first apptemting to find the repository as '[entityName]Repository'
                // And if it not exists we create a BaseRepository

                repository = (IRepository<T>)Activator.CreateInstance(
                repositoryType,
                _databaseConnection)!;
            }
            catch
            {
                repository = new BaseRepository<T>(_databaseConnection);
            }

            _repositories[entityType] = repository;

            return repository;
        }

        private Type GetRepositoryTypeForEntity<T>() where T : class
        {
            // Convention: EntityName + "Repository"
            var repositoryName = $"{typeof(T).Name}Repository";
            var repositoryType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == repositoryName &&
                                   !t.IsAbstract &&
                                   t.IsClass &&
                                   t.IsAssignableTo(typeof(IRepository<T>)));

            if (repositoryType == null)
            {
                throw new InvalidOperationException(
                    $"No repository implementation found for entity {typeof(T).Name}");
            }

            return repositoryType;
        }
    }
}