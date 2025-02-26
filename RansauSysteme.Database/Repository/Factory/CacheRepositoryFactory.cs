using RansauSysteme.Database.Connection;

namespace RansauSysteme.Database.Repository.Factory
{
    public class CacheRepositoryFactory : IRepositoryFactory
    {
        private readonly Dictionary<Type, object> _repositories;
        private readonly IDatabaseConnection _databaseConnection;
        private readonly Dictionary<Type, TimeSpan?> _refreshIntervals;

        public CacheRepositoryFactory(IDatabaseConnection databaseService)
        {
            _repositories = new Dictionary<Type, object>();
            _refreshIntervals = new Dictionary<Type, TimeSpan?>();

            _databaseConnection = databaseService;
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
                repository = new CacheRepository<T>(_databaseConnection, _refreshIntervals.GetValueOrDefault(entityType));
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

        public bool AddRefreshInterval(Type type, TimeSpan? timeSpan) => _refreshIntervals.TryAdd(type, timeSpan);
    }
}