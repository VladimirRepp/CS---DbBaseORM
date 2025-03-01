using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection.Metadata;
using MyORM.Models;

namespace MyORM.Controllers
{
    internal class DbBaseController<T> where T : IBaseModel, new()
    {
        #region === Protected fields ===
        protected List<T> _data = null;
        protected string? _tableName = null;
        protected string? _providerName = null;
        protected DbConnection? _dbConnection = null;
        protected DbProviderFactory? _dbProviderFactory = null;
        #endregion

        #region === Public properties ===
        public int Count => _data.Count;
        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }
        public List<T> Data => _data;
        public string? TableName => _tableName;
        public string? ProviderName => _providerName;
        public string? ConnectionString => _dbConnection.ConnectionString;
        public DbConnection? DbConnection => _dbConnection;
        public DbProviderFactory? DbProviderFactory => _dbProviderFactory;
        #endregion

        public DbBaseController(string providerName, string tableName = "")
        {
            _data = new List<T>();
            _tableName = tableName;
            _providerName = providerName;

            Startup();
        }

        #region === Privates Methods ===

        private void Startup()
        {
            _dbProviderFactory = DbProviderFactories.GetFactory(_providerName);
            _dbConnection = _dbProviderFactory.CreateConnection();
            _dbConnection.ConnectionString = GetConnectionStringByProviderName(_providerName);
        }

        private string GetConnectionStringByProviderName(string providerName)
        {
            string found_value = null;

            ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;

            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.ProviderName == providerName)
                    {
                        found_value = cs.ConnectionString;
                        break;
                    }
                }
            }

            return found_value;
        }

        #endregion

        #region === Protected methods ===

        protected bool UpdateById(T new_data)
        {
            int index = _data.FindIndex(x => x.Id == new_data.Id);

            if (index == -1)
                return false;

            _data[index] = new_data;
            return true;
        }

        protected bool RemoveById(int id)
        {
            return _data.RemoveAll(x => x.Id == id) == 1;
        }

        #endregion

        #region === Synchronous actions with local Data and Query in DB === 

        /// <summary>
        /// Открытие соединения к БД
        /// </summary>
        private void OpenConnection()
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
        }

        /// <summary>
        /// Закрытие соединения к БД
        /// </summary>
        private void CloseConnection()
        {
            if (_dbConnection.State != ConnectionState.Closed)
                _dbConnection.Close();
        }

        /// <summary>
        /// Возвращение индекса из Data (List<T>) по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetInedxById(int id)
        {
            return _data.FindIndex(x => x.Id == id);
        }

        /// <summary>
        /// Вернуть найденный элемент из Data (List<T>) по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T? GetById(int id)
        {
            return _data.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Попытка вернуть найденный элемент из Data (List<T>) по ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="findemItem"></param>
        /// <returns>true - наден, false - иначе</returns>
        public bool TryGetById(int id, out T? findemItem)
        {
            findemItem = _data.FirstOrDefault(x => x.Id == id);
            return findemItem != null;
        }

        /// <summary>
        /// Запрос выбороки всех данных из таблицы БД
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_SelectAll()
        {
            DbDataReader reader = null;
            _data.Clear();

            try
            {
                DbCommand command = _dbConnection.CreateCommand();
                command.CommandText = $"Select * From {_tableName}";

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        T new_data = new();
                        new_data.SetData(ref reader);

                        _data.Add(new_data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectAll(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return _data.Count > 0;
        }

        /// <summary>
        /// Запрос выборки данных по ID из таблицы БД
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Query_SelectById(int id)
        {
            DbDataReader reader = null;
            T? found = default;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"Select * From {_tableName} Where Id = @id";

                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = $"@id";
                parameter.Value = id;

                command.Parameters.Add(parameter);

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        found = new();
                        found.SetData(ref reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectById(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return found;
        }

        /// <summary>
        /// Выборка данных по строке запроса
        /// </summary>
        /// <param name="query"> - строка запроса, требования к именам аргументов запросов @param+индекс</param>
        /// <param name="args"> - аргументы запрса</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Query_SelectByQuery(string query, params string[] args)
        {
            DbDataReader reader = null;
            T found = default;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                int i = 0;
                foreach (var d in args)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = d;

                    command.Parameters.Add(parameter);
                }

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        found = new T();
                        found.SetData(ref reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectByQuery(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return found;
        }

        /// <summary>
        /// Выборка данных по строке запроса с указанными параметрами 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Query_SelectByQuery(string query, List<DbParameter> args)
        {
            DbDataReader reader = null;
            T found = default;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        found = new T();
                        found.SetData(ref reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectByQuery(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return found;
        }

        /// <summary>
        /// Выборка данных по строке запроса
        /// </summary>
        /// <param name="query"> - строка запроса, требования к именам аргументов запросов @param+индекс</param>
        /// <param name="args"> - аргументы запрса</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<T> Query_SelectsByQuery(string query, params string[] args)
        {
            DbDataReader reader = null;
            List<T> founds = new List<T>();

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                int i = 0;
                foreach (var d in args)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = d;

                    command.Parameters.Add(parameter);
                }

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        T d = new();
                        d.SetData(ref reader);

                        founds.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectsByQuery(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return founds;
        }

        /// <summary>
        /// Выборка данных по строке запроса с указанными параметрами 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<T> Query_SelectsByQuery(string query, List<DbParameter> args)
        {
            DbDataReader reader = null;
            List<T> founds = new List<T>();

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        T d = new();
                        d.SetData(ref reader);

                        founds.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectsByQuery(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return founds;
        }

        /// <summary>
        /// Попытка получить данные по ID из таблицы БД
        /// </summary>
        /// <param name="id"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_TrySelectById(int id, out T found)
        {
            try
            {
                found = Query_SelectById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_TrySelectById(Exception): {ex.Message}");
            }

            return found != null;
        }

        /// <summary>
        /// Вставка данных в Data (List<T>) и запрос встаки в БД
        /// </summary>
        /// <param name="d"></param>
        /// <param name="is_param_query_with_id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Insert(T d, bool is_param_query_with_id = false)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";

                if (is_param_query_with_id)
                {
                    DbParameter parameter_ID = command.CreateParameter();
                    parameter_ID.ParameterName = $"@id";
                    parameter_ID.Value = d.Id;
                    command.Parameters.Add(parameter_ID);
                }

                int i = 0;
                foreach (string s in d.ToArrayStr)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = s;

                    command.Parameters.Add(parameter);
                }

                OpenConnection();
                if (command.ExecuteNonQuery() > 0)
                {
                    isDone = true;
                    _data.Add(d);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Insert(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Вставка данных в Data (List<T>) и запрос встаки в БД
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <param name="is_param_query_with_id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Insert(T d, List<DbParameter> args,  bool is_param_query_with_id = false)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                OpenConnection();
                if (command.ExecuteNonQuery() > 0)
                {
                    isDone = true;
                    _data.Add(d);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Insert(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Обновление данных в Data (List<T>) и запрос обновления в БД
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Update(T d)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"UPDATE {_tableName} SET {d.ToParamsUpdateQuery} WHERE Id = @id";

                DbParameter parameter_id = command.CreateParameter();
                parameter_id.ParameterName = "@id";
                parameter_id.Value = d.Id;

                command.Parameters.Add(parameter_id);

                int i = 0;
                foreach (string s in d.ToArrayStr)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = s;

                    command.Parameters.Add(parameter);
                }

                OpenConnection();
                if (command.ExecuteNonQuery() > 0)
                    isDone = UpdateById(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Update(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Обновление данных в Data (List<T>) и запрос обновления в БД
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Update(T d, List<DbParameter> args)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"UPDATE {_tableName} SET {d.ToParamsUpdateQuery} WHERE Id = @id";

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                OpenConnection();
                if (command.ExecuteNonQuery() > 0)
                    isDone = UpdateById(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Update(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Удаление данных по ID в Data (List<T>) и запрос удаления в БД
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_DeleteById(int id)
        {
            bool isDone = false;
            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"DELETE FROM {_tableName} WHERE Id = @id";

                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = id;

                command.Parameters.Add(parameter);

                OpenConnection();
                if(command.ExecuteNonQuery() > 0)
                    isDone = RemoveById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_DeleteById(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Очистка Data (List<T>) и запрос очистки таблицы в БД
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_ClearTable()
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"TRUNCATE TABLE {_tableName}";

                OpenConnection();
                command.ExecuteNonQuery();
                _data.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_ClearTable(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Вернуть последний наибольший ID в таблицы БД
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int Request_GetLastId()
        {
            DbDataReader reader = null;
            int last_id = -1;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"SELECT MAX(Id) AS IdValue FROM {_tableName}";

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if(reader["IdValue"] != DBNull.Value)
                            last_id = Convert.ToInt32(reader["IdValue"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Request_GetLastId(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return last_id;
        }

        /// <summary>
        /// Сохранение локальных данных в БД. Используется транзакция. 
        /// Алгоритм: 
        /// 1. Очистить таблицу 
        /// 2. Добавить данные в таблицу
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Save(bool is_param_query_with_id = false)
        {
            bool isDone = false;
            DbTransaction transaction = null;

            try
            {
                OpenConnection();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                isDone = command.ExecuteNonQuery() > 0;

                foreach (var d in _data)
                {
                    string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";
                    command.Parameters.Clear();

                    if (is_param_query_with_id)
                    {
                        DbParameter parameter_ID = command.CreateParameter();
                        parameter_ID.ParameterName = $"@id";
                        parameter_ID.Value = d.Id;
                        command.Parameters.Add(parameter_ID);
                    }

                    int i = 0;
                    foreach (string s in d.ToArrayStr)
                    {
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = $"@param{i++}";
                        parameter.Value = s;

                        command.Parameters.Add(parameter);
                    }

                    isDone = command.ExecuteNonQuery() > 0;
                }

                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction?.Rollback();
                throw new Exception($"DbBaseController.Query_Save(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Сохранение локальных данных в БД. Используется транзакция.
        /// Алгоритм: 
        /// 1. Очистить таблицу 
        /// 2. Добавить данные в таблицу
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Save(List<DbParameter> args, bool is_param_query_with_id = false)
        {
            bool isDone = false;
            DbTransaction transaction = null;

            try
            {
                OpenConnection();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                isDone = command.ExecuteNonQuery() > 0;

                foreach (var d in _data)
                {
                    string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";
                    command.Parameters.Clear();

                    foreach (var arg in args)
                    {
                        command.Parameters.Add(arg);
                    }

                    isDone = command.ExecuteNonQuery() > 0;
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw new Exception($"DbBaseController.Query_Save(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Выполнить запрос к БД
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Execute(string query)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                OpenConnection();

                isDone = command.ExecuteNonQuery() > 0;
            }
            catch(Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Execute(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Выполнить запрос к БД
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Execute(string query, List<DbParameter> args)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                foreach (var arg in args)
                    command.Parameters.Add(arg);

                OpenConnection();

                isDone = command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Execute(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Выполнить запрос к БД с возвращаемым результатом
        /// </summary>
        /// <param name="query"></param>
        /// <param name="name_column"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object Query_ExecuteReader(string query, string name_column)
        {
            object result = null;
            DbDataReader reader = null;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                OpenConnection();

                reader = command.ExecuteReader();

                if (reader.HasRows) 
                {
                    while (reader.Read()) {
                        result = reader.GetValue(name_column);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_ExecuteReaders(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return result;
        }

        /// <summary>
        /// Выполнить запрос к БД с возвращаемым результатом
        /// </summary>
        /// <param name="query"></param>
        /// <param name="name_column"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<object> Query_ExecuteReaders(string query, string name_column)
        {
            List<object> results = null;
            DbDataReader reader = null;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                OpenConnection();

                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        results.Add(reader.GetValue(name_column));
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_ExecuteReaders(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            return results;
        }
        #endregion

        #region === Asynchronous actions with local Data and Query in DB === 
        /// <summary>
        /// Асинхронное открытие соединения к БД
        /// </summary>
        /// <returns></returns>
        private async Task OpenConnectionAsync()
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();
        }

        /// <summary>
        /// Асинхронное закрытие соединения к БД
        /// </summary>
        /// <returns></returns>
        private async Task CloseConnectionAsync()
        {
            if (_dbConnection.State != ConnectionState.Closed)
                await _dbConnection.CloseAsync();
        }

        /// <summary>
        /// Асинхронный запрос выборки всех данных
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_SelectAllAsync()
        {
            DbDataReader reader = null;
            _data.Clear();

            try
            {
                DbCommand command = _dbConnection.CreateCommand();
                command.CommandText = $"Select * From {_tableName}";

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        T new_data = new();
                        new_data.SetData(ref reader);

                        _data.Add(new_data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectAllAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return _data.Count > 0;
        }

        /// <summary>
        /// Асинхронный запрос выборки по ID 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> Query_SelectByIdAsync(int id)
        {
            DbDataReader reader = null;
            T? found = default;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"Select * From {_tableName} Where Id = @id";

                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = $"@id";
                parameter.Value = id;

                command.Parameters.Add(parameter);

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        found = new();
                        found.SetData(ref reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectByIdAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return found;
        }

        /// <summary>
        /// Выборка данных по строке запроса
        /// </summary>
        /// <param name="query"> - строка запроса, требования к именам аргументов запросов @param+индекс</param>
        /// <param name="args"> - аргументы запрса</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> Query_SelectByQueryAsync(string query, params string[] args)
        {
            DbDataReader reader = null;
            T found = default;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                int i = 0;
                foreach (var d in args)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = d;

                    command.Parameters.Add(parameter);
                }

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        found = new T();
                        found.SetData(ref reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectByQueryAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return found;
        }

        /// <summary>
        /// Выборка данных по строке запроса с указанными параметрами 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> Query_SelectByQueryAsync(string query, List<DbParameter> args)
        {
            DbDataReader reader = null;
            T found = default;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        found = new T();
                        found.SetData(ref reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectByQueryAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return found;
        }

        /// <summary>
        /// Выборка данных по строке запроса
        /// </summary>
        /// <param name="query"> - строка запроса, требования к именам аргументов запросов @param+индекс</param>
        /// <param name="args"> - аргументы запрса</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<T>> Query_SelectsByQueryAsync(string query, params string[] args)
        {
            DbDataReader reader = null;
            List<T> founds = new List<T>();

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                int i = 0;
                foreach (var d in args)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = d;

                    command.Parameters.Add(parameter);
                }

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        T d = new();
                        d.SetData(ref reader);

                        founds.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectsByQueryAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                await CloseConnectionAsync();
            }

            return founds;
        }

        /// <summary>
        /// Выборка данных по строке запроса с указанными параметрами 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<T>> Query_SelectsByQueryAsync(string query, List<DbParameter> args)
        {
            DbDataReader reader = null;
            List<T> founds = new List<T>();

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        T d = new();
                        d.SetData(ref reader);

                        founds.Add(d);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectsByQueryAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return founds;
        }

        /// <summary>
        /// Асинхронная вставка данных в Data (List<T>) и запрос вставки в БД
        /// </summary>
        /// <param name="d"></param>
        /// <param name="is_param_query_with_id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_InsertAsync(T d, bool is_param_query_with_id = false)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";

                if (is_param_query_with_id)
                {
                    DbParameter parameter_ID = command.CreateParameter();
                    parameter_ID.ParameterName = $"@id";
                    parameter_ID.Value = d.Id;
                    command.Parameters.Add(parameter_ID);
                }

                int i = 0;
                foreach (string s in d.ToArrayStr)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = s;

                    command.Parameters.Add(parameter);
                }

                await OpenConnectionAsync();
                if (await command.ExecuteNonQueryAsync() == 1)
                {
                    isDone = true;
                    _data.Add(d);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_InsertAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронная вставка данных в Data (List<T>) и запрос вставки в БД
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <param name="is_param_query_with_id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_InsertAsync(T d, List<DbParameter> args, bool is_param_query_with_id = false)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                await OpenConnectionAsync();
                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    isDone = true;
                    _data.Add(d);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_InsertAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронное обновление данных в Data (List<T>) и запрос обновления в БД
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_UpdateAsync(T d)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"UPDATE {_tableName} SET {d.ToParamsUpdateQuery} WHERE Id = @id";

                DbParameter parameter_id = command.CreateParameter();
                parameter_id.ParameterName = $"@id";
                parameter_id.Value = d.Id;

                command.Parameters.Add(parameter_id);

                int i = 0;
                foreach (string s in d.ToArrayStr)
                {
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = $"@param{i++}";
                    parameter.Value = s;

                    command.Parameters.Add(parameter);
                }

                await OpenConnectionAsync();
                if(await command.ExecuteNonQueryAsync() > 0)
                    isDone = UpdateById(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_UpdateAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронное обновление данных в Data (List<T>) и запрос обновления в БД
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_UpdateAsync(T d, List<DbParameter> args)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"UPDATE {_tableName} SET {d.ToParamsUpdateQuery} WHERE Id = @id";

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                await OpenConnectionAsync();
                if (await command.ExecuteNonQueryAsync() > 0)
                    isDone = UpdateById(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_UpdateAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронное удаление данных из Data (List<T>) и запрос удаления из БД
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_DeleteByIdAsync(int id)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"DELETE FROM {_tableName} WHERE Id = @id";

                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = $"@id";
                parameter.Value = id;

                command.Parameters.Add(parameter);

                await OpenConnectionAsync();
                if(await command.ExecuteNonQueryAsync() > 0)
                    isDone = RemoveById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_DeleteByIdAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронная очиста Data (List<T>) и запрос очистки таблицы в БД
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_ClearTableAsync()
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"TRUNCATE TABLE {_tableName}";

                await OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                _data.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_ClearTableAsync(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронный запрос к БД по возвращению последнего наибольшего ID
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> Request_GetLastIdAsync()
        {
            DbDataReader reader = null;
            int last_id = -1;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"SELECT MAX(Id) AS IdValue FROM {_tableName}";

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        if(reader["IdValue"] != DBNull.Value)
                            last_id = Convert.ToInt32(reader["IdValue"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Request_GetLastId(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return last_id;
        }

        /// <summary>
        /// Асинхронное сохранение локальных данных в БД. Используется транзакция. 
        /// Алгоритм: 
        /// 1. Очистить таблицу 
        /// 2. Добавить данные в таблицу
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_SaveAsync(bool is_param_query_with_id = false)
        {
            bool isDone = false;
            DbTransaction transaction = null;

            try
            {
                await OpenConnectionAsync();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                isDone = await command.ExecuteNonQueryAsync() > 0;

                foreach (var d in _data)
                {
                    string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";
                    command.Parameters.Clear();

                    if (is_param_query_with_id)
                    {
                        DbParameter parameter_ID = command.CreateParameter();
                        parameter_ID.ParameterName = $"@id";
                        parameter_ID.Value = d.Id;
                        command.Parameters.Add(parameter_ID);
                    }

                    int i = 0;
                    foreach (string s in d.ToArrayStr)
                    {
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = $"@param{i++}";
                        parameter.Value = s;

                        command.Parameters.Add(parameter);
                    }

                    isDone = await command.ExecuteNonQueryAsync() > 0;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw new Exception($"DbBaseController.Query_Save(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронное сохранение локальных данных в БД. Используется транзакция. 
        /// Алгоритм: 
        /// 1. Очистить таблицу 
        /// 2. Добавить данные в таблицу
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_SaveAsync(List<DbParameter> args, bool is_param_query_with_id = false)
        {
            bool isDone = false;
            DbTransaction transaction = null;

            try
            {
                await OpenConnectionAsync();

                var command = _dbConnection.CreateCommand();
                transaction = await _dbConnection.BeginTransactionAsync();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                isDone = await command.ExecuteNonQueryAsync() > 0;

                foreach (var d in _data)
                {
                    string toParamsQuery = is_param_query_with_id ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery}";
                    command.Parameters.Clear();

                    foreach (var arg in args)
                    {
                        command.Parameters.Add(arg);
                    }

                    isDone = await command.ExecuteNonQueryAsync() > 0;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                transaction?.RollbackAsync();
                throw new Exception($"DbBaseController.Query_SaveAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Выполнить асинхронный запрос к БД
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_ExecuteAsync(string query)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                await OpenConnectionAsync();

                isDone = await command.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_ExecuteAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Выполнить асинхронный запрос к БД
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_ExecuteAsync(string query, List<DbParameter> args)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                foreach (var arg in args)
                    command.Parameters.Add(arg);

                await OpenConnectionAsync();

                isDone = await command.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Execute(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Выполнить асинхронный запрос к БД с возвращаемым результатом
        /// </summary>
        /// <param name="query"></param>
        /// <param name="name_column"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<object> Query_ExecuteReaderAsync(string query, string name_column)
        {
            object result = null;
            DbDataReader reader = null;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                await OpenConnectionAsync();

                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        result = reader.GetValue(name_column);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_ExecuteReaderAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return result;
        }

        /// <summary>
        /// Выполнить асинхронный запрос к БД с возвращаемым результатом
        /// </summary>
        /// <param name="query"></param>
        /// <param name="name_column"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<object>> Query_ExecuteReadersAsync(string query, string name_column)
        {
            List<object> results = new List<object>();
            DbDataReader reader = null;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = query;

                await OpenConnectionAsync();

                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(reader.GetValue(name_column));
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_ExecuteReadersAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            return results;
        }
        #endregion
    }
}
