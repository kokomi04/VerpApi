using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;

namespace SynTool
{
    public class DbHelper
    {
        #region properties
        private readonly string _connectionString;
        #endregion

        #region init
        public DbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }
        #endregion

        #region open/close connection
        private SqlConnection OpenSqlConnection()
        {
            try
            {
                var sqlConnection = new SqlConnection(_connectionString);
                sqlConnection.Open();
                return sqlConnection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenSqlConnection():Error={ex.Message}");
                throw;
            }
        }

        private void CloseSqlConnection(ref SqlConnection sqlConnection)
        {
            if (sqlConnection == null)
                return;
            try
            {
                sqlConnection.Close();
                sqlConnection.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CloseSqlConnection(sqlConnection.State={sqlConnection.State}):Error={ex.StackTrace}");
            }
        }
        #endregion

        #region Get SqlDataReader
        public SqlDataReader GetDataReader(SqlCommand sqlCommand, params SqlParameter[] parameters)
        {
            var sqlConnection = OpenSqlConnection();
            try
            {
                if (parameters != null)
                {
                    foreach (var sqlParameter in parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();
                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }
                if (sqlCommand.Parameters != null)
                {
                    foreach (SqlParameter sqlParameter in sqlCommand.Parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();
                    }
                }
                sqlCommand.Connection = sqlConnection;
                return sqlCommand.ExecuteReader();
            }
            finally
            {
                CloseSqlConnection(ref sqlConnection);
            }
        }

        public SqlDataReader GetDataReader(string strSql, params SqlParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(strSql);
            return GetDataReader(sqlCommand, parameters);
        }

        #endregion

        #region ExecuteScalar

        public object ExecuteScalar(SqlCommand sqlCommand, params SqlParameter[] parameters)
        {
            var sqlConnection = OpenSqlConnection();
            try
            {
                if (parameters != null)
                {
                    foreach (var sqlParameter in parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();
                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }
                if (sqlCommand.Parameters != null)
                {
                    foreach (SqlParameter sqlParameter in sqlCommand.Parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();
                    }
                }
                sqlCommand.Connection = sqlConnection;
                return sqlCommand.ExecuteScalar();
            }
            finally
            {
                CloseSqlConnection(ref sqlConnection);
            }
        }

        public object ExecuteScalar(string strSql, params SqlParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(strSql);
            return ExecuteScalar(sqlCommand, parameters);
        }

        public object ExecuteScalarSp(string spName, params SqlParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(spName) { CommandType = CommandType.StoredProcedure };
            return ExecuteScalar(sqlCommand, parameters);
        }
        #endregion

        # region ExecuteNonQuery
        public int ExecuteNonQuery(SqlCommand sqlCommand, params SqlParameter[] parameters)
        {
            var sqlConnection = OpenSqlConnection();
            try
            {
                if (parameters != null)
                {
                    foreach (var sqlParameter in parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();
                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }
                if (sqlCommand.Parameters != null)
                {
                    foreach (SqlParameter sqlParameter in sqlCommand.Parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();                      
                    }
                }
               
                sqlCommand.Connection = sqlConnection;
                return sqlCommand.ExecuteNonQuery();
            }
            finally
            {
                CloseSqlConnection(ref sqlConnection);
            }
        }

        public int ExecuteNonQuery(string strSql, params SqlParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(strSql);
            return ExecuteNonQuery(sqlCommand, parameters);
        }

        public int ExecuteNonQuerySp(string spName, params SqlParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(spName) { CommandType = CommandType.StoredProcedure };
            return ExecuteNonQuery(sqlCommand, parameters);
        }

        #endregion

        #region Get List<T>
        public List<T> GetList<T>(SqlCommand sqlCommand, params SqlParameter[] parameters)
        {
            var sqlConnection = OpenSqlConnection();
            try
            {

                if (parameters != null)
                {
                    foreach (var sqlParameter in parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();
                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }
                sqlCommand.Connection = sqlConnection;
                var sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.FieldCount == 0)
                    return new List<T>();

                var mList = new List<T>();

                var builder = DynamicBuilder<T>.CreateBuilder(sqlDataReader);

                while (sqlDataReader.Read())
                {
                    var r = builder.Build(sqlDataReader);
                    mList.Add(r);
                }
                sqlDataReader.Close();
                return mList;
            }
            finally
            {
                CloseSqlConnection(ref sqlConnection);
            }
        }

        public List<T> GetList<T>(string strSql)
        {
            var sqlCommand = new SqlCommand(strSql);
            return GetList<T>(sqlCommand);
        }
        #endregion

        #region Get DataTable
        public DataTable GetDataTable(SqlCommand sqlCommand, params SqlParameter[] parameters)
        {
            var sqlConnection = OpenSqlConnection();
            try
            {
                if (parameters != null)
                {
                    foreach (var sqlParameter in parameters)
                    {
                        if (sqlParameter.SqlDbType == SqlDbType.NVarChar)
                            sqlParameter.Value = (sqlParameter.Value ?? "").ToString().Trim();
                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }
                sqlCommand.Connection = sqlConnection;
                var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                var dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);

                return dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
            }
            finally
            {
                CloseSqlConnection(ref sqlConnection);
            }
        }

        public DataTable GetDataTable(string strSql, params SqlParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(strSql);
            return GetDataTable(sqlCommand, parameters);
        }

        public DataTable GetDataTableSp(string spName, params SqlParameter[] parameters)
        {
            var sqlCommand = new SqlCommand(spName) { CommandType = CommandType.StoredProcedure };
            return GetDataTable(sqlCommand, parameters);
        }
        #endregion

    }

    public class DynamicBuilder<T>
    {
        private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new[] { typeof(int) });

        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new[] { typeof(int) });

        private Load _handler;

        private DynamicBuilder()
        {
        }

        public T Build(IDataRecord dataRecord)
        {
            return _handler(dataRecord);
        }

        public static DynamicBuilder<T> CreateBuilder(IDataRecord dataRecord)
        {
            var dynamicBuilder = new DynamicBuilder<T>();

            var method = new DynamicMethod("DynamicCreate", typeof(T), new[] { typeof(IDataRecord) }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(dataRecord.GetName(i));
                Label endIfLabel = generator.DefineLabel();

                if (propertyInfo == null || propertyInfo.GetSetMethod() == null) continue;
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                generator.Emit(OpCodes.Brtrue, endIfLabel);

                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, getValueMethod);
                generator.Emit(OpCodes.Unbox_Any, dataRecord.GetFieldType(i));
                generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());

                generator.MarkLabel(endIfLabel);
            }

            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            dynamicBuilder._handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }

        #region Nested type: Load

        private delegate T Load(IDataRecord dataRecord);

        #endregion
    }
}