# CS---SimpleORM
A simple ORM based on C#, .Net9, ADO.Net and Provider factories

=== Release Notes: ===
======================
- v0.1
- IDE: MVS 2022
- C# / .Net9 / Console App
- ADO.Net
- Provider Factory
- Source project platform: Microsoft.NETCore.App (9.0.0)
- Instal packeges:
  * System.Configuration.ConfigurationManager (9.0.2)
  * Microsoft.Data.SqlClient (6.0.1)
  * System.Data.OleDb (9.0.2)

=== Description: ===
======================
- Registration of the provider's factory is required, depending on the chosen platform
- The provider is selected from the configuration file (App.config)
- The connection string is taken from the configuration file (App.config)
- When working with a Microsoft office package (for example, Access database), you must select the build size of the project in accordance with the office size (x64, x86)
- Default data types of fields in the database: id:int, other fields: varchar
- Otherwise, use the overloaded method with the List<DbParametr> args parameters for inserting and updating data

=== Description of architectural principles: ===
======================
- The abstract DbBaseController class implements the entire logic of actions with local data and databases
- All data models must be inherited from the IBaseModel interface, which contains common properties and methods

=== Example of work: ===
======================
- To implement the data model, you need to create a new class and inherit from the IBaseModel interface and implement all its actions
  * Example: class DbUser : IBaseModel { ... }

Ways to work with controllers: Each controller is associated with a single table!
- (I) To implement a controller for a specific model, you can create a new class and inherit from DbBaseController abstract class with the model template type. With this option, it is convenient to add and change new actions.
  * Example: class DbUserController : DbBaseController <DbUser> { ... }
- (II) You can immediately use an instance of the Db Base Controller class with the template type of the implemented model
  * Example:  DbBaseController <DbUser> users = new("NAME_YOUR_PROVIDER");

=== Actions: ===
======================
### **DbBaseController<T> Method Summary**

#### **Constructor**
- **DbBaseController(string providerName, string connectionString, string tableName = "")**  
  Initializes the controller with database provider, connection string, and optional table name. Sets up the database connection.

---

#### **Synchronous Methods**

##### **Connection Management**
- **OpenConnection()**  
  Opens the database connection if it's not already open.
- **CloseConnection()**  
  Closes the database connection if it's not already closed.

##### **Data Retrieval**
- **GetInedxById(int id)**  
  Returns the index of an item in `_data` by its ID.
- **GetById(int id)**  
  Retrieves an item from `_data` by its ID.
- **TryGetById(int id, out T? findemItem)**  
  Attempts to retrieve an item from `_data` by its ID; returns `true` if found.
- **Query_SelectAll(bool isChangeLocalData = false)**  
  Retrieves all records from the database table, optionally updating `_data`.
- **Query_SelectById(int id)**  
  Retrieves a single record from the database by ID.
- **Query_SelectByQuery(string query, params string[] args)**  
  Executes a custom query with parameters and returns a single record.
- **Query_SelectByQuery(string query, List<DbParameter> args)**  
  Executes a custom query with `DbParameter` parameters and returns a single record.
- **Query_SelectsByQuery(string query, params string[] args)**  
  Executes a custom query with parameters and returns multiple records.
- **Query_SelectsByQuery(string query, List<DbParameter> args)**  
  Executes a custom query with `DbParameter` parameters and returns multiple records.
- **Query_TrySelectById(int id, out T found)**  
  Attempts to retrieve a record by ID; returns `true` if found.

##### **Data Modification**
- **Query_Insert(T d, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Inserts a record into the database, optionally updating `_data` and handling ID generation.
- **Query_Insert(T d, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Inserts a record using `DbParameter` parameters, optionally updating `_data`.
- **Query_UpdateById(T d, bool isChangeLocalData = false)**  
  Updates a record in the database by ID, optionally updating `_data`.
- **Query_UpdateById(T d, List<DbParameter> args, bool isChangeLocalData = false)**  
  Updates a record using `DbParameter` parameters, optionally updating `_data`.
- **Query_DeleteById(int id, bool isChangeLocalData = false)**  
  Deletes a record by ID, optionally updating `_data`.
- **Query_ClearTable()**  
  Clears the database table and `_data`.
- **Query_Save(bool isParamQueryWithId = false)**  
  Saves `_data` to the database in a transaction (truncates table first).
- **Query_Save(List<DbParameter> args, bool isParamQueryWithId = false)**  
  Saves `_data` to the database using `DbParameter` parameters in a transaction.
- **Query_Save(ref List<T> data, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Saves an external list to the database in a transaction, optionally updating `_data`.
- **Query_Save(ref List<T> data, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Saves an external list using `DbParameter` parameters in a transaction, optionally updating `_data`.

##### **Utility Methods**
- **Query_Execute(string query)**  
  Executes a raw SQL query.
- **Query_Execute(string query, List<DbParameter> args)**  
  Executes a raw SQL query with `DbParameter` parameters.
- **Query_ExecuteReader(string query, string name_column)**  
  Executes a query and returns a single value from a specified column.
- **Query_ExecuteReaders(string query, string name_column)**  
  Executes a query and returns multiple values from a specified column.
- **Request_GetLastId()**  
  Retrieves the maximum ID from the database table.

---

#### **Asynchronous Methods**

##### **Connection Management**
- **OpenConnectionAsync()**  
  Asynchronously opens the database connection.
- **CloseConnectionAsync()**  
  Asynchronously closes the database connection.

##### **Data Retrieval**
- **Query_SelectAllAsync(bool isChangeLocalData = false)**  
  Asynchronously retrieves all records from the database table, optionally updating `_data`.
- **Query_SelectByIdAsync(int id)**  
  Asynchronously retrieves a record by ID.
- **Query_SelectByQueryAsync(string query, params string[] args)**  
  Asynchronously executes a custom query with parameters and returns a single record.
- **Query_SelectByQueryAsync(string query, List<DbParameter> args)**  
  Asynchronously executes a custom query with `DbParameter` parameters and returns a single record.
- **Query_SelectsByQueryAsync(string query, params string[] args)**  
  Asynchronously executes a custom query with parameters and returns multiple records.
- **Query_SelectsByQueryAsync(string query, List<DbParameter> args)**  
  Asynchronously executes a custom query with `DbParameter` parameters and returns multiple records.

##### **Data Modification**
- **Query_InsertAsync(T d, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Asynchronously inserts a record into the database, optionally updating `_data`.
- **Query_InsertAsync(T d, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Asynchronously inserts a record using `DbParameter` parameters, optionally updating `_data`.
- **Query_UpdateByIdAsync(T d, bool isChangeLocalData = false)**  
  Asynchronously updates a record by ID, optionally updating `_data`.
- **Query_UpdateByIdAsync(T d, List<DbParameter> args, bool isChangeLocalData = false)**  
  Asynchronously updates a record using `DbParameter` parameters, optionally updating `_data`.
- **Query_DeleteByIdAsync(int id, bool isChangeLocalData = false)**  
  Asynchronously deletes a record by ID, optionally updating `_data`.
- **Query_ClearTableAsync()**  
  Asynchronously clears the database table and `_data`.
- **Query_SaveAsync(bool isParamQueryWithId = false)**  
  Asynchronously saves `_data` to the database in a transaction.
- **Query_SaveAsync(List<DbParameter> args, bool isParamQueryWithId = false)**  
  Asynchronously saves `_data` using `DbParameter` parameters in a transaction.
- **Query_SaveAsync(List<T> data, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Asynchronously saves an external list to the database in a transaction, optionally updating `_data`.
- **Query_SaveAsync(List<T> data, List<DbParameter> args, bool isChangeLocalData = false, bool isParamQueryWithId = false)**  
  Asynchronously saves an external list using `DbParameter` parameters in a transaction, optionally updating `_data`.

##### **Utility Methods**
- **Query_ExecuteAsync(string query)**  
  Asynchronously executes a raw SQL query.
- **Query_ExecuteAsync(string query, List<DbParameter> args)**  
  Asynchronously executes a raw SQL query with `DbParameter` parameters.
- **Query_ExecuteReaderAsync(string query, string name_column)**  
  Asynchronously executes a query and returns a single value from a specified column.
- **Query_ExecuteReadersAsync(string query, string name_column)**  
  Asynchronously executes a query and returns multiple values from a specified column.
- **Request_GetLastIdAsync()**  
  Asynchronously retrieves the maximum ID from the database table.

---

### **Key Notes**
- **Synchronous vs. Asynchronous**: Most methods have both synchronous and asynchronous versions.
- **Local Data (`_data`)**: Many methods optionally sync changes with the internal `_data` list.
- **Transactions**: `Query_Save` methods use transactions for batch operations.
- **Parameterization**: Supports both inline parameters (`@param{i}`) and `DbParameter` objects for security.
  
=== Attention! ===
======================
Before using it, make sure that the code in your project is working correctly. Correct operation is NOT guaranteed, comprehensive testing is required within your system.

=== Health test №1: ===
======================
- IDE: MVS 2022
- C# / .Net9 / Console App
- SQL Server (2022) - local server
- Successfully tested methods
  - Query_Insert
  - Query_InsertAsync
  - Query_SelectAll
  - Query_SelectAllAsync
  - Query_Update
  - Query_UpdateAsync
  - Query_DeleteById
  - Query_DeleteByIdAsync
  - Query_Save
  - Query_SaveAsync
  - Query_ClearTableAsync
 - Output example:

![1](https://github.com/user-attachments/assets/bcc598f7-0fe8-413f-8da3-dedd96e1593d)

![1_1](https://github.com/user-attachments/assets/a1d49c8a-a2fb-40b1-95ff-75a009997ebf)

![2](https://github.com/user-attachments/assets/59a01924-ef07-4893-9814-de745d932c11)
   
![3](https://github.com/user-attachments/assets/785c2bca-3d0d-4970-8ae5-c206af7ada74)

![3_1](https://github.com/user-attachments/assets/e4a94e35-c7ab-4c3f-b5ff-509be4975af6)

![4](https://github.com/user-attachments/assets/1836eb39-e95c-44e4-98e1-2d50025a1f8a)

![4_1](https://github.com/user-attachments/assets/4a15c50a-0858-416f-87ff-d0a0a2709e2f)

![5](https://github.com/user-attachments/assets/2bbeb5f2-f2e1-409b-89c0-ade29cf2d8a0)

![5_1](https://github.com/user-attachments/assets/e499d78b-ce1c-400d-badc-d3b9cdc66bd3)

![6](https://github.com/user-attachments/assets/1f7db1bc-b35a-4682-9c62-d03ab76f06de)

![6_1](https://github.com/user-attachments/assets/8852899e-999c-4e62-ba03-88cdd4e410c7)

![7](https://github.com/user-attachments/assets/3c80562c-9437-47ec-a99e-45972ef08b67)

![7_1](https://github.com/user-attachments/assets/10e27a15-6d82-4e2a-b800-0883056d35d6)

=== TODO on next release: ===
======================
1. Add bool flag - isChanchedLockalData in all methods Qeury_
2. Update Insering - add load last id for inserting data
