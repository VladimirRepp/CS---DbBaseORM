using System.Data.Common;

namespace MyORM.Models
{
    internal class DbUser : IBaseModel
    {
        private int _id;
        public int Id { get => _id; set => _id = value; }
        public long TelegramId { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public string ToParamsInsertQueryWithID => $"([Id], [TelegramId], [FullName], [NickName], [Login], [Password], [Role]) VALUES " +
            $"(@id, @param0, @param1, @param2, @param3, @param4, @param5)";
        public string ToParamsInsertQueryWithoutID => $"([TelegramId], [FullName], [NickName], [Login], [Password], [Role]) VALUES " +
            $"(@param0, @param1, @param2, @param3, @param4, @param5)";

        public string ToParamsUpdateQuery => $"[TelegramId] = @param0, [FullName] = @param1, " +
            $"[NickName] = @param2, [Login] = @param3, [Password] = @param4, [Role] = @param5";

        public string[] ToArrayStr => new string[] { TelegramId.ToString(), FullName, NickName, Login, Password, Role };

        public DbUser() { }
        public DbUser(int id, long telegramId, string fullname, string nickname, string login, string password, string role) 
        { 
            _id = id;
            TelegramId = telegramId;
            FullName = fullname;
            NickName = nickname;
            Login = login;
            Password = password;
            Role = role;
        }
        public DbUser(ref DbDataReader reader) 
        {
            SetData(ref reader);
        }

        public void SetData(ref DbDataReader reader)
        {
            _id = Convert.ToInt32(reader["Id"]);

            if (reader["TelegramId"] != DBNull.Value)
                TelegramId = Convert.ToInt32(reader["TelegramId"]);

            if (reader["FullName"] != DBNull.Value)
                FullName = reader["FullName"].ToString();

            if (reader["NickName"] != DBNull.Value)
                NickName = reader["NickName"].ToString();

            if (reader["Login"] != DBNull.Value)
                Login = reader["Login"].ToString();

            if (reader["Password"] != DBNull.Value)
                Password = reader["Password"].ToString();

            if (reader["Role"] != DBNull.Value)
                Role = reader["Role"].ToString();
        }
    }
}
