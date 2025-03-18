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
That implement a query to the database and, if successful, make changes to the local data:
- Synchronous:
  * Query_SelectAll: Requesting a selection of all data from a database table to a local list Data(List<T>)
  * Query_SelectById: Requesting data selection by ID from a database table to a local variable
  * Query_SelectByQuery (2 overloads): Fetching data by query string to a local variable
  * Query_SelectsByQuery (2 overloads): Fetching data by query string to a local list
  * Query_TrySelectById: An attempt to get data by ID from a database table to a local variable
  * Query_Insert (2 overloads): Inserting data into Data (List<T>) and requesting an insert into the database
  * Query_Update (2 overloads): Updating data in Data (List<T>) and requesting an update in the database
  * Query_DeleteById: Deleting data by ID in Data (List<T>) and requesting deletion in the database
  * Query_ClearTable: Clearing Data (List<T>) and requesting a table cleanup in the database
  * Request_GetLastId: Return the last largest ID to the database tables
  * Query_Save (2 overloads): Saving local data in the database. A transaction is being used
  * Query_Execute (2 overloads): Make a query to the database
  * Query_ExecuteReader: Run a query to the database with the returned result in a local variable
  * Query_ExecuteReaders: Run a query to the database with the returned result in a local list
- Asynchronous analogues:
  * Query_SelectAllAsync: Asynchronous request to fetch all data to a local list Data (List<T>)
  * Query_SelectByIdAsync: Asynchronous selection request by ID to a local variable
  * Query_SelectByQueryAsync (2 overloads): Asynchronous fetching of data by query string to a local variable
  * Query_SelectsByQueryAsync (2 overloads): Asynchronous fetching of data by query string to a local list
  * Query_InsertAsync (2 overloads): Asynchronous insertion of data into Data (List<T>) and query for insertion into the database
  * Query_UpdateAsync (2 overloads): Asynchronous updating of data in Data (List<T>) and requesting an update in the database
  * Query_DeleteByIdAsync: Asynchronous deletion of data from Data (List<T>) and request deletion from the database
  * Query_ClearTableAsync: Asynchronous Data cleanup (List<T>) and a table cleanup request in the database
  * Request_GetLastIdAsync: Asynchronous request to the database for the return of the last largest ID
  * Query_SaveAsync (2 overloads): Asynchronous storage of local data in the database. A transaction is being used
  * Query_ExecuteAsync (2 overloads): Make an asynchronous database request
  * Query_ExecuteReaderAsync: Execute an asynchronous query to the database with the result returned to a local variable
  * Query_ExecuteReadersAsync: Execute an asynchronous query to the database with the result returned to a local list

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
