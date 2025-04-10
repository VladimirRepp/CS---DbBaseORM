using DbBaseORM.Models;

namespace DbBaseORM.Controllers
{
    internal class DbUserController : DbBaseController<DbUser>
    {
        public DbUserController(string provider_name, string connection_string) : 
            base(provider_name, connection_string) 
        {
            _tableName = "Users";
        }

        /// <summary>
        /// Попробовать получить пользователя по TelegramId 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="found_user"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_TrySelectByTelegramId(long t_id, out DbUser found_user)
        {
            try
            {
                string query = $"select * from {_tableName} where TelegramId = @param0";
                found_user = Query_SelectByQuery(query, t_id.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"DbUserController.Query_TrySelectByTelegramID(Exception): {ex.Message}");
            }

            return found_user != null;
        }

        /// <summary>
        /// Попробовать авторизироваться 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="pass"></param>
        /// <param name="login_user"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Query_TryLogin(long telegramId, string pass, out DbUser login_user)
        {
            try
            {
                string query = $"select * from {_tableName} where TelegramId = @param0 and Password = @param1";
                login_user = Query_SelectByQuery(query, telegramId.ToString(), pass);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbUserController.Query_TryLogin(Exception): {ex.Message}");
            }

            return login_user != null;
        }

        /// <summary>
        /// Зарегистрировать пользователя и вернуть его данные с 
        /// аткуальными зачениями Id
        /// </summary>
        /// <param name="new_user"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DbUser Query_Registration(ref DbUser new_user)
        {
            try
            {
                new_user = Query_Insert(new_user);
            }
            catch (Exception ex)
            {
                throw new Exception($"DbUserController.Query_Registration(Exception): {ex.Message}");
            }

            return new_user;
        }
    }
}
