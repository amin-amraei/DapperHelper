namespace DapperHelper.Interfaces
{
    public interface IDapper : IDisposable
    {        
        Task<int> CudTransactional<TParam>(string query, TParam param);
        Task<int> Execute<TParam>(string query, TParam spParams);
        Task<T> GetOne<T, TParam>(string query, TParam param);
        Task<List<TList>> GetAll<TList>(string query);       
        Task<List<TList>> GetAllWithParams<TList, TParam>(string query, TParam param);
    }
}