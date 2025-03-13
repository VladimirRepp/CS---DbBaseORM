using System.Data.Common;

namespace MyORM.Models
{
    public class DbUser : IBaseModel
    {
        private int _id;
        public int Id { get => _id; set => _id = value; }
        public long TelegramId { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public string ToParamsInsertQueryWithID => $"([Id], [TelegramId], [FullName], [NickName], [Password], [Role]) VALUES " +
            $"(@id, @param0, @param1, @param2, @param3, @param4)";
        public string ToParamsInsertQueryWithoutID => $"([TelegramId], [FullName], [NickName],  [Password], [Role]) VALUES " +
            $"(@param0, @param1, @param2, @param3, @param4)";

        public string ToParamsUpdateQuery => $"[TelegramId] = @param0, [FullName] = @param1, " +
            $"[NickName] = @param2, [Password] = @param3, [Role] = @param4";

        public string[] ToArrayStr => new string[] { TelegramId.ToString(), FullName, NickName, Password, Role };

        public DbUser() { }
        public DbUser(int id, long TelegramId, string FullName, string NickName, string Password, string Role) 
        { 
            _id = id;
            this.TelegramId = TelegramId;
            this.FullName = FullName;
            this.NickName = NickName;
            this.Password = Password;
            this. Role = Role;
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

            if (reader["Password"] != DBNull.Value)
                Password = reader["Password"].ToString();

            if (reader["Role"] != DBNull.Value)
                Role = reader["Role"].ToString();
        }
    }
}
