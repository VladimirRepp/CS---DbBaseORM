using System.Data;
using System.Data.Common;
using DbBaseORM.Models;

namespace DbBaseORM.Controllers
{
    public class DbBaseController<T> where T : IBaseModel, new()
    {
        #region === Private fields ===
       
        private object _locker = new object();
        #endregion

        #region === Protected fields ===
       
        protected List<T> _data = null;
        protected string? _tableName = null;
        protected string? _connectionString = null;
        protected string? _providerName = null;
        protected DbConnection? _dbConnection = null;
        protected DbProviderFactory? _dbProviderFactory = null;
        #endregion

        #region === Public properties ===
       
        public int Count => _data.Count;
        public T this[int index]
        {
            get
            {
                lock (_locker)
                {
                    return _data[index];
                }
            }
            set
            {
                lock (_locker)
                {
                    _data[index] = value;
                }
            }
        }
        public List<T> Data
        {
            get
            {
                lock (_locker)
                {
                    return _data;
                }
            }
        }
        public string? TableName => _tableName;
        public string? ProviderName => _providerName;
        public string? ConnectionString => _dbConnection.ConnectionString;
        public DbConnection? DbConnection => _dbConnection;
        public DbProviderFactory? DbProviderFactory => _dbProviderFactory;
        #endregion

        /// <summary>
        /// Инициализирует контроллер с помощью поставщика базы данных, строки подключения и необязательного имени таблицы. Устанавливает подключение к базе данных.
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        public DbBaseController(string providerName, string connectionString, string tableName = "")
        {
            _data = new List<T>();
            _tableName = tableName;
            _connectionString = connectionString;
            _providerName = providerName;

            Startup();
        }

        #region === Privates Methods ===

        private void Startup()
        {
            _dbProviderFactory = DbProviderFactories.GetFactory(_providerName);
            _dbConnection = _dbProviderFactory.CreateConnection();
            _dbConnection.ConnectionString = _connectionString;
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
        /// Открывает подключение к базе данных, если оно еще не открыто.
        /// </summary>
        private void OpenConnection()
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
        }

        /// <summary>
        /// Закрывает соединение с базой данных, если оно еще не закрыто.
        /// </summary>
        private void CloseConnection()
        {
            if (_dbConnection.State != ConnectionState.Closed)
                _dbConnection.Close();
        }

        /// <summary>
        /// Возвращает индекс элемента в "_data" по его идентификатору.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetInedxById(int id)
        {
            return _data.FindIndex(x => x.Id == id);
        }

        /// <summary>
        /// Извлекает элемент из "_data" по его идентификатору.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T? GetById(int id)
        {
            return _data.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Пытается извлечь элемент из "_data" по его идентификатору; возвращает значение "true", если он найден.
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
        /// Запрос возвращающий общее количество строк в таблице, включая или исключая NULL строки 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int Query_GetCount(bool withoutNullRows = false)
        {
            int count = 0;

            try
            {
                DbCommand command = _dbConnection.CreateCommand();
                
                if(!withoutNullRows)
                    command.CommandText = $"SELECT COUNT(*) AS TotalCount FROM {_tableName}";
                else
                    command.CommandText = $"SELECT COUNT(Id) AS TotalCount FROM  {_tableName}";

                OpenConnection();
                count = (int)command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectAll(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return count;
        }

        /// <summary>
        /// Извлекает все записи из базы данных, при необходимости обновляя `_data`.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<T> Query_SelectAll(bool isChangeLocalData = false)
        {
            DbDataReader reader = null;
            List<T> data = new();

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

                        data.Add(new_data);
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

            if (isChangeLocalData)
                _data = data;

            return data;
        }

        /// <summary>
        /// Извлекает все записи из базы данных в диапазоне от `offset` до `limit`, при необходимости обновляя `_data`. Нумерация с 0.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<T> Query_SelectByLimit(int offset, int limit, bool isChangeLocalData = false)
        {
            DbDataReader reader = null;
            List<T> data = new();

            try
            {
                DbCommand command = _dbConnection.CreateCommand();
                command.CommandText = $"Select * From {_tableName} \r\n" +
                    $"ORDER BY Id \r\n" +
                    $"OFFSET @p_offset ROWS \r\n" +
                    $"FETCH NEXT @p_limit ROWS ONLY;";

                DbParameter parameter_limit = command.CreateParameter();
                parameter_limit.ParameterName = $"@p_limit";
                parameter_limit.Value = limit;

                DbParameter parameter_offset = command.CreateParameter();
                parameter_offset.ParameterName = $"@p_offset";
                parameter_offset.Value = offset;

                command.Parameters.Add(parameter_limit);
                command.Parameters.Add(parameter_offset);

                OpenConnection();
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        T new_data = new();
                        new_data.SetData(ref reader);

                        data.Add(new_data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectByLimit(Exception): {ex.Message}");
            }
            finally
            {
                reader?.Close();
                CloseConnection();
            }

            if (isChangeLocalData)
                _data = data;

            return data;
        }

        /// <summary>
        /// Извлекает одну запись из базы данных по идентификатору.
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
        /// Выполняет пользовательский запрос с параметрами и возвращает одну запись.
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
        /// Выполняет пользовательский запрос с параметрами `DbParameter` и возвращает одну запись. 
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
        /// Выполняет пользовательский запрос с параметрами и возвращает несколько записей.
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
        /// Выполняет пользовательский запрос с параметрами `DbParameter` и возвращает несколько записей.
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
        /// Пытается получить запись по идентификатору; возвращает `true`, если она найдена.
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
        /// Вставляет запись в базу данных, при необходимости обновляя `_data` и обрабатывая генерацию идентификатора.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="isParamQueryWithId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Query_Insert(T d, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";

                if (isParamQueryWithId)
                {
                    DbParameter parameter_ID = command.CreateParameter();
                    parameter_ID.ParameterName = $"@id";
                    parameter_ID.Value = d.Id;
                    command.Parameters.Add(parameter_ID);
                }
                else
                {
                    command.CommandText += " SELECT SCOPE_IDENTITY();";
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
                if (isParamQueryWithId)
                {
                    if (command.ExecuteNonQuery() <= 0)
                        throw new Exception("ExecuteNonQuery <= 0");
                }
                else
                {
                    d.Id = Convert.ToInt32(command.ExecuteScalar());
                }

                if (isChangeLocalData)
                    _data.Add(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Insert(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return d;
        }

        /// <summary>
        /// Вставляет запись, используя параметры `DbParameter`, при необходимости обновляя `_data`.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <param name="isParamQueryWithId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Query_Insert(T d, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";

                if (!isParamQueryWithId)
                {
                    command.CommandText += " SELECT SCOPE_IDENTITY();";
                }

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                OpenConnection();

                if (isParamQueryWithId)
                {
                    if (command.ExecuteNonQuery() <= 0)
                        throw new Exception("ExecuteNonQuery <= 0");
                }
                else
                {
                    d.Id = Convert.ToInt32(command.ExecuteScalar());
                }

                if (isChangeLocalData)
                    _data.Add(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_Insert(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return d;
        }

        /// <summary>
        /// Обновляет запись в базе данных по идентификатору, при необходимости обновляя "_data".
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_UpdateById(T d, bool isChangeLocalData = false)
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

                if (command.ExecuteNonQuery() <= 0)
                {
                    throw new Exception("ExecuteNonQuery <= 0");
                }

                if (isChangeLocalData)
                    isDone = UpdateById(d);
                else
                    isDone = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_UpdateById(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Обновляет запись, используя параметры `DbParameter`, при необходимости обновляя `_data`.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_UpdateById(T d, List<DbParameter> args, bool isChangeLocalData = false)
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

                if (command.ExecuteNonQuery() <= 0)
                {
                    throw new Exception("ExecuteNonQuery <= 0");
                }

                if (isChangeLocalData)
                    isDone = UpdateById(d);
                else
                    isDone = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_UpdateById(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return isDone;
        }

        /// <summary>
        /// Удаляет запись по идентификатору, при необходимости обновляя "_data".
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_DeleteById(int id, bool isChangeLocalData = false)
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

                if (command.ExecuteNonQuery() <= 0)
                {
                    throw new Exception("ExecuteNonQuery <= 0");
                }

                if (isChangeLocalData)
                    isDone = RemoveById(id);
                else
                    isDone = true;
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
        /// Очищает таблицу базы данных и при необходимости `_data`.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_ClearTable(bool isChangeLocalData = false)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"TRUNCATE TABLE {_tableName}";

                OpenConnection();
                command.ExecuteNonQuery();

                if (isChangeLocalData)
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
        /// Извлекает максимальный идентификатор из таблицы базы данных.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int Query_GetLastId()
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
                        if (reader["IdValue"] != DBNull.Value)
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
        /// Сохраняет "_data` в базе данных в транзакции (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void Query_Save(bool isParamQueryWithId = false)
        {
            DbTransaction transaction = null;

            try
            {
                OpenConnection();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                command.ExecuteNonQuery();

                int index = 0;
                foreach (var d in _data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();

                    if (isParamQueryWithId)
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

                    if (!isParamQueryWithId)
                    {
                        _data[index].Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        if (command.ExecuteNonQuery() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQuery <= 0 with index: {index}");
                    }

                    index++;
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
        }

        /// <summary>
        /// Cохраняет "_data" в базе данных, используя параметры "DbParameter" в транзакции (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_Save(List<DbParameter> args, bool isParamQueryWithId = false)
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
                command.ExecuteNonQuery();

                int index = 0;
                foreach (var d in _data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();
                    foreach (var arg in args)
                    {
                        command.Parameters.Add(arg);
                    }

                    if (!isParamQueryWithId)
                    {
                        _data[index].Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        if (command.ExecuteNonQuery() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQuery <= 0 with index: {index}");
                    }

                    index++;
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
        /// Сохраняет внешний список в базе данных в транзакции, при необходимости обновляя `_data` (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void Query_Save(ref List<T> data, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            DbTransaction transaction = null;

            try
            {
                OpenConnection();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                command.ExecuteNonQuery();

                int index = 0;
                foreach (var d in data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();

                    if (isParamQueryWithId)
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

                    if (!isParamQueryWithId)
                    {
                        data[index].Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        if (command.ExecuteNonQuery() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQuery <= 0 with index: {index}");
                    }

                    index++;
                }

                transaction.Commit();

                if (isChangeLocalData)
                {
                    _data.Clear();
                    data.ForEach((item) =>
                    {
                        _data.Add(item);
                    });
                }
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
        }

        /// <summary>
        ///  Сохраняет внешний список, используя параметры "DbParameter" в транзакции, при необходимости обновляя "_data` (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void Query_Save(ref List<T> data, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            DbTransaction transaction = null;

            try
            {
                OpenConnection();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                command.ExecuteNonQuery();

                int index = 0;
                foreach (var d in data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();
                    foreach (var arg in args)
                    {
                        command.Parameters.Add(arg);
                    }

                    if (!isParamQueryWithId)
                    {
                        data[index].Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        if (command.ExecuteNonQuery() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQuery <= 0 with index: {index}");
                    }

                    index++;
                }

                transaction.Commit();

                if (isChangeLocalData)
                {
                    _data.Clear();
                    data.ForEach((item) =>
                    {
                        _data.Add(item);
                    });
                }
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
        }

        /// <summary>
        /// Выполняет необработанный запрос.
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
        /// Выполняет необработанный запрос с параметрами `DbParameter`.
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
        /// Выполняет запрос и возвращает единственное значение из указанного столбца. 
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
                    while (reader.Read())
                    {
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
        /// Выполняет запрос и возвращает несколько значений из указанного столбца.
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
        /// Асинхронно открывает соединение с базой данных.
        /// </summary>
        /// <returns></returns>
        private async Task OpenConnectionAsync()
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();
        }

        /// <summary>
        ///   Асинхронно закрывает соединение с базой данных.
        /// </summary>
        /// <returns></returns>
        private async Task CloseConnectionAsync()
        {
            if (_dbConnection.State != ConnectionState.Closed)
                await _dbConnection.CloseAsync();
        }

        /// <summary>
        /// Асинхронный запрос возвращающий общее количество строк в таблице, включая или исключая NULL строки 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> Query_GetCountAsync(bool withoutNullRows = false)
        {
            int count = 0;

            try
            {
                DbCommand command = _dbConnection.CreateCommand();

                if (!withoutNullRows)
                    command.CommandText = $"SELECT COUNT(*) AS TotalCount FROM {_tableName}";
                else
                    command.CommandText = $"SELECT COUNT(Id) AS TotalCount FROM  {_tableName}";

                await OpenConnectionAsync();
                count = (int)command.ExecuteScalarAsync().Result;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectAll(Exception): {ex.Message}");
            }
            finally
            {
                CloseConnectionAsync();
            }

            return count;
        }

        /// <summary>
        /// Асинхронно извлекает все записи из таблицы базы данных, при необходимости обновляя `_data`.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<T>> Query_SelectAllAsync(bool isChangeLocalData = false)
        {
            DbDataReader reader = null;
            List<T> data = new();

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

                        data.Add(new_data);
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

            if (isChangeLocalData)
                _data = data;

            return data;
        }

        /// <summary>
        /// Асинхронно извлекает все записи из базы данных в диапазоне от `offset` до `limit`, при необходимости обновляя `_data`. Нумерация с 0.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<T>> Query_SelectByLimitAsync(int offset, int limit, bool isChangeLocalData = false)
        {
            DbDataReader reader = null;
            List<T> data = new();

            try
            {
                DbCommand command = _dbConnection.CreateCommand();
                command.CommandText = $"Select * From {_tableName} \r\n" +
                    $"ORDER BY Id \r\n" +
                    $"OFFSET @p_offset ROWS \r\n" +
                    $"FETCH NEXT @p_limit ROWS ONLY;";

                DbParameter parameter_limit = command.CreateParameter();
                parameter_limit.ParameterName = $"@p_limit";
                parameter_limit.Value = limit;

                DbParameter parameter_offset = command.CreateParameter();
                parameter_offset.ParameterName = $"@p_offset";
                parameter_offset.Value = offset;

                command.Parameters.Add(parameter_limit);
                command.Parameters.Add(parameter_offset);

                await OpenConnectionAsync();
                reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        T new_data = new();
                        new_data.SetData(ref reader);

                        data.Add(new_data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_SelectByLimitAsync(Exception): {ex.Message}");
            }
            finally
            {
                reader?.CloseAsync();
                await CloseConnectionAsync();
            }

            if (isChangeLocalData)
                _data = data;

            return data;
        }

        /// <summary>
        /// Асинхронно извлекает запись по идентификатору. 
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
        /// Асинхронно выполняет пользовательский запрос с параметрами и возвращает одну запись.
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
        /// Асинхронно выполняет пользовательский запрос с параметрами `DbParameter` и возвращает одну запись.
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
        /// Асинхронно выполняет пользовательский запрос с параметрами и возвращает несколько записей.
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
        /// Асинхронно выполняет пользовательский запрос с параметрами `DbParameter` и возвращает несколько записей. 
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
        /// Асинхронно вставляет запись в базу данных, при необходимости обновляя `_data`.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="isParamQueryWithId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> Query_InsertAsync(T d, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";

                if (isParamQueryWithId)
                {
                    DbParameter parameter_ID = command.CreateParameter();
                    parameter_ID.ParameterName = $"@id";
                    parameter_ID.Value = d.Id;
                    command.Parameters.Add(parameter_ID);
                }
                else
                {
                    command.CommandText += " SELECT SCOPE_IDENTITY();";
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
                if (isParamQueryWithId)
                {
                    if (await command.ExecuteNonQueryAsync() <= 0)
                        throw new Exception("ExecuteNonQueryAsync <= 0");
                }
                else
                {
                    d.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                if (isChangeLocalData)
                    _data.Add(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_InsertAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return d;
        }

        /// <summary>
        /// Асинхронно вставляет запись, используя параметры `DbParameter`, при необходимости обновляя `_data`.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <param name="isParamQueryWithId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> Query_InsertAsync(T d, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            try
            {
                var command = _dbConnection.CreateCommand();
                string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";

                if (!isParamQueryWithId)
                {
                    command.CommandText += " SELECT SCOPE_IDENTITY();";
                }

                foreach (var arg in args)
                {
                    command.Parameters.Add(arg);
                }

                await OpenConnectionAsync();

                if (isParamQueryWithId)
                {
                    if (await command.ExecuteNonQueryAsync() <= 0)
                        throw new Exception("ExecuteNonQueryAsync <= 0");
                }
                else
                {
                    d.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                if (isChangeLocalData)
                    _data.Add(d);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_InsertAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return d;
        }

        /// <summary>
        /// Асинхронно обновляет запись по идентификатору, при необходимости обновляя `_data`.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_UpdateByIdAsync(T d, bool isChangeLocalData = false)
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

                if (await command.ExecuteNonQueryAsync() <= 0)
                {
                    throw new Exception("ExecuteNonQueryAsync <= 0");
                }

                if (isChangeLocalData)
                    isDone = UpdateById(d);
                else
                    isDone = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_UpdateByIdAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронно обновляет запись, используя параметры "DbParameter", при необходимости обновляя "_data".
        /// </summary>
        /// <param name="d"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_UpdateByIdAsync(T d, List<DbParameter> args, bool isChangeLocalData = false)
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

                if (await command.ExecuteNonQueryAsync() <= 0)
                {
                    throw new Exception("ExecuteNonQueryAsync <= 0");
                }

                if (isChangeLocalData)
                    isDone = UpdateById(d);
                else
                    isDone = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"DbBaseController.Query_UpdateByIdAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }

            return isDone;
        }

        /// <summary>
        /// Асинхронно удаляет запись по идентификатору, при необходимости обновляя "_data".
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_DeleteByIdAsync(int id, bool isChangeLocalData = false)
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

                if (await command.ExecuteNonQueryAsync() <= 0)
                {
                    throw new Exception("ExecuteNonQueryAsync <= 0");
                }

                if (isChangeLocalData)
                    isDone = RemoveById(id);
                else
                    isDone = true;
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
        /// Асинхронно очищает таблицу базы данных и при необходимости `_data`.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> Query_ClearTableAsync(bool isChangeLocalData = false)
        {
            bool isDone = false;

            try
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = $"TRUNCATE TABLE {_tableName}";

                await OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                if (isChangeLocalData)
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
        /// Асинхронно извлекает максимальный идентификатор из таблицы базы данных.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> Query_GetLastIdAsync()
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
                        if (reader["IdValue"] != DBNull.Value)
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
        /// Асинхронно сохраняет `_data` в базе данных в транзакции (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Query_SaveAsync(bool isParamQueryWithId = false)
        {
            DbTransaction transaction = null;

            try
            {
                await OpenConnectionAsync();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                await command.ExecuteNonQueryAsync();

                int index = 0;
                foreach (var d in _data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();

                    if (isParamQueryWithId)
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

                    if (!isParamQueryWithId)
                    {
                        _data[index].Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                    else
                    {
                        if (await command.ExecuteNonQueryAsync() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQueryAsync <= 0 with index: {index}");
                    }

                    index++;
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw new Exception($"DbBaseController.Query_SaveAsync(Exception): {ex.Message}");
            }
            finally
            {
                await CloseConnectionAsync();
            }
        }

        /// <summary>
        /// Асинхронно сохраняет "_data", используя параметры `DbParameter` в транзакции (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Query_SaveAsync(List<DbParameter> args, bool isParamQueryWithId = false)
        {
            DbTransaction transaction = null;

            try
            {
                await OpenConnectionAsync();

                var command = _dbConnection.CreateCommand();
                transaction = await _dbConnection.BeginTransactionAsync();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                await command.ExecuteNonQueryAsync();

                int index = 0;
                foreach (var d in _data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();
                    foreach (var arg in args)
                    {
                        command.Parameters.Add(arg);
                    }

                    if (!isParamQueryWithId)
                    {
                        _data[index].Id = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        if (await command.ExecuteNonQueryAsync() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQuery <= 0 with index: {index}");
                    }

                    index++;
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
        }

        /// <summary>
        /// Асинхронно сохраняет внешний список в базе данных в транзакции, при необходимости обновляя "_data` (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<T>> Query_SaveAsync(List<T> data, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            DbTransaction transaction = null;

            try
            {
                await OpenConnectionAsync();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                await command.ExecuteNonQueryAsync();

                int index = 0;
                foreach (var d in data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();

                    if (isParamQueryWithId)
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

                    if (!isParamQueryWithId)
                    {
                        data[index].Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                    else
                    {
                        if (await command.ExecuteNonQueryAsync() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQueryAsync <= 0 with index: {index}");
                    }

                    index++;
                }

                await transaction.CommitAsync();

                if (isChangeLocalData)
                {
                    _data.Clear();
                    data.ForEach((item) =>
                    {
                        _data.Add(item);
                    });
                }
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

            return data;
        }

        /// <summary>
        /// Асинхронно сохраняет внешний список, используя параметры "DbParameter" в транзакции, при необходимости обновляя "_data" (сначала обрезает таблицу).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<T>> Query_SaveAsync(List<T> data, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)
        {
            DbTransaction transaction = null;

            try
            {
                await OpenConnectionAsync();

                var command = _dbConnection.CreateCommand();
                transaction = _dbConnection.BeginTransaction();

                command.Transaction = transaction;

                command.CommandText = $"TRUNCATE TABLE {_tableName}";
                await command.ExecuteNonQueryAsync();

                int index = 0;
                foreach (var d in data)
                {
                    string toParamsQuery = isParamQueryWithId ? d.ToParamsInsertQueryWithID : d.ToParamsInsertQueryWithoutID;
                    command.CommandText = $"INSERT INTO {_tableName} {toParamsQuery};";
                    if (!isParamQueryWithId)
                        command.CommandText += " SELECT SCOPE_IDENTITY();";

                    command.Parameters.Clear();
                    foreach (var arg in args)
                    {
                        command.Parameters.Add(arg);
                    }

                    if (!isParamQueryWithId)
                    {
                        data[index].Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                    else
                    {
                        if (await command.ExecuteNonQueryAsync() <= 0)
                            throw new Exception($"INSERT INTO - ExecuteNonQueryAsync <= 0 with index: {index}");
                    }

                    index++;
                }

                await transaction.CommitAsync();

                if (isChangeLocalData)
                {
                    _data.Clear();
                    data.ForEach((item) =>
                    {
                        _data.Add(item);
                    });
                }
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

            return data;
        }

        /// <summary>
        /// Асинхронно выполняет необработанный запрос.
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
        /// Асинхронно выполняет необработанный запрос с параметрами `DbParameter`.
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
        /// Асинхронно выполняет запрос и возвращает единственное значение из указанного столбца.
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
        /// Асинхронно выполняет запрос и возвращает несколько значений из указанного столбца. 
        /// </summary>a
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
