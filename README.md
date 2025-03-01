# CS---SimpleORM
A simple ORM based on C#, .Net9, ADO.Net and Provider factories

=== Release Notes: ===
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
- Registration of the provider's factory is required, depending on the chosen platform
- The provider is selected from the configuration file (App.config)
- When working with a Microsoft office package (for example, Access database), you must select the build size of the project in accordance with the office size
- Default data types of fields in the database: id:int, other fields: varchar
- Otherwise, use the overloaded method with the List<DbParametr> args parameters for inserting and updating data

===  Actions: ===
That implement a query to the database and, if successful, make changes to the local data:
    Synchronous:
    - Query_SelectAll
    - Query_SelectById
    - Query_SelectByQuery (2 overloads)
    - Query_SelectsByQuery (2 overloads)
    - Query_TrySelectById
    - Query_Insert (2 overloads)
    - Query_Update (2 overloads)
    - Query_DeleteById
    - Query_ClearTable
    - Request_GetLastId
    - Query_Save (2 overloads)
    - Query_Execute (2 overloads)
    - Query_ExecuteReader
    - Query_ExecuteReaders
    Asynchronous analogues:
    - Query_SelectAllAsync
    - Query_SelectByIdAsync
    - Query_SelectByQueryAsync (2 overloads)
    - Query_SelectsByQueryAsync (2 overloads)
    - Query_TrySelectByIdAsync
    - Query_InsertAsync (2 overloads)
    - Query_UpdateAsync (2 overloads)
    - Query_DeleteByIdAsync
    - Query_ClearTableAsync
    - Request_GetLastIdAsync
    - Query_SaveAsync (2 overloads)
    - Query_ExecuteAsync (2 overloads)
    - Query_ExecuteReaderAsync
    - Query_ExecuteReadersAsync

=== Attention! ===
Before using it, make sure that the code in your project is working correctly. Correct operation is NOT guaranteed, comprehensive testing is required within your system.

=== Health test №1: ===
- IDE: MVS 2022
- C# / .Net9 / Console App
- SQL Server (2022)
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
