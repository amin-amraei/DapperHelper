using Dapper;
using DapperHelper.Attributes;
using DapperHelper.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;

namespace DapperHelper
{
    public class Dapper : IDapper
    {
        private readonly IConfiguration _config;
        private readonly string _connectionStringName;        
        public Dapper(IConfiguration config, string connectionStringName)
        {
            _config = config;
            _connectionStringName = connectionStringName;
        }

        public async Task<T> GetOne<T, TParam>(string query, TParam param)
        {
            var dynamicParameters = ConvertGenericClassToDynamicParameters(param);
            using (var connection = GetDbconnection(_connectionStringName))
            {
                connection.Open();
                return await connection.QueryFirstOrDefaultAsync<T>(query, dynamicParameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<List<TList>> GetAll<TList>(string query)
        {
            using (var connection = GetDbconnection(_connectionStringName))
            {
                connection.Open();
                return (await connection.QueryAsync<TList>(query)).ToList();
            }
        }

        public async Task<List<TList>> GetAllWithParams<TList, TParam>(string query, TParam param)
        {           
            var dynamicParameters = new DynamicParameters(param);
            using (var connection = GetDbconnection(_connectionStringName))
            {
                connection.Open();

                return (await connection.QueryAsync<TList>(query, dynamicParameters)).ToList();
            }
        }        
        
        public async Task<int> CudTransactional<TParam>(string query, TParam param)
        {
            int result = 0;
            var getDbConnection = GetDbconnection(_connectionStringName);
            var dynamicParameters = ConvertGenericClassToDynamicParameters(param);
            try
            {
                using (var connection = getDbConnection)
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    using var tran = connection.BeginTransaction();
                    try
                    {
                        result = (await connection.QueryFirstOrDefaultAsync<int>(query, dynamicParameters, commandType: CommandType.StoredProcedure, transaction: tran));
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                if (getDbConnection.State == ConnectionState.Open)
                    getDbConnection.Close();
            }
        }
        
        public async Task<int> Execute<TParam>(string query, TParam spParams)
        {
            var dynamicParameters = ConvertGenericClassToDynamicParameters(spParams);            
            using (var connection = GetDbconnection(_connectionStringName))
            {
                connection.Open();
                var result = await connection.ExecuteAsync(query, dynamicParameters.Item1, commandType: CommandType.StoredProcedure);
                if (dynamicParameters.Item2 != "")
                    return dynamicParameters.Item1.Get<int>(dynamicParameters.Item2);
                return result;
            }            
        }             

        private (DynamicParameters?,string) ConvertGenericClassToDynamicParameters<TParam> (TParam spParam)
        {
            var dynamicParameters = new DynamicParameters();
            string outPutParameterName = string.Empty;
            foreach (var val in spParam.GetType().GetProperties())
            {
                Type type = spParam.GetType();
                PropertyInfo propertyInfo = type.GetProperty(val.Name);
                var list = propertyInfo?.GetValue(spParam);                
                SqlParamAttribute[] sqlParamAttributes = propertyInfo.GetCustomAttributes(typeof(SqlParamAttribute), false).Cast<SqlParamAttribute>().ToArray();
                dynamicParameters.Add(val.Name, list, null, direction: sqlParamAttributes.Select(x => x.Direction).FirstOrDefault());
                if(sqlParamAttributes.Select(x => x.Direction).FirstOrDefault().ToString() == "Output")
                {
                    outPutParameterName = val.Name;
                }
            }
            return (dynamicParameters, outPutParameterName);
        }
         
        private DbConnection GetDbconnection(string connectionstring)
        {
            return new SqlConnection(_config.GetConnectionString(connectionstring));
        }
        public void Dispose()
        {

        }
    }
}