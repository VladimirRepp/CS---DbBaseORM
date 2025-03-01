using MyORM.Models;

namespace MyORM.Controllers
{
    internal class DbUserController : DbBaseController<DbUser>
    {
        public DbUserController(string provider_name) : base(provider_name) 
        {
            _tableName = "Users";
        }
    }
}
