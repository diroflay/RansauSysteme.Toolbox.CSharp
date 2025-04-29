namespace RansauSysteme.Database.Repository.Factory
{
    public interface IRepositoryFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
    }
}