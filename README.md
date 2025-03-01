# CS---SimpleORM
A simple CRM based on ADO.Net and Provider factories

Release Notes:
- v0.1
- Registration of the provider's factory is required, depending on the chosen platform
- When working with a Microsoft office package (for example, Access database), you must select the build size of the project in accordance with the office size
- Default data types of fields in the database: id:int, other fields: varchar
- Otherwise, use the overloaded method with the List<DbParametr> args parameters for inserting and updating data
