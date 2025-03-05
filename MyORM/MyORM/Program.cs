using MyORM.Controllers;
using MyORM.Global;
using MyORM.Models;
using System.Data.Common;

namespace MyORM
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AppSettings.GetInstance.Startup();

            // === Testing === //
            DbUserController users = new(
                AppSettings.GetInstance.DbProviderNameCurrent, 
                AppSettings.GetInstance.GetConnectionStringByName("DbTypeSQL")
                );

            // === Inserting === //
            DbUser d = new(0, 1, "Name 1", "Nick 1", "log1", "pas1", "Admin");
            DbUser d1 = new(0, 2, "Name 2", "Nick 2", "log2", "pas2", "Teacher");
            DbUser d2 = new(0, 3, "Name 3", "Nick 3", "log3", "pas3", "Student");

            users.Query_Insert(d);
            users.Query_Insert(d1);
            await users.Query_InsertAsync(d2);

            Console.WriteLine("Testing Insert is Done!\nCheck DB ...");
            ConsolePause();

            // === Selecting All === //
            users.Query_SelectAll();

            Console.WriteLine("Testing Query_SelectAll: ");
            Print(users.Data);
            Console.WriteLine();

            await users.Query_SelectAllAsync();

            Console.WriteLine("Testing Query_SelectAllAsync: ");
            Print(users.Data);
            ConsolePause();

            // Read Ids after load from DB
            d = users.Data[0];
            d1 = users.Data[1];
            d2 = users.Data[2];

            // === Updating === // 
            d.TelegramId = 100;
            d.FullName = "NewName";
            d.NickName = "NewNick";
            d.Login = "NewLog";
            d.Password = "NewPas";
            d.Role = "Teacher";

            d1.TelegramId = 101;
            d1.FullName = "NewName_1";
            d1.NickName = "NewNick_1";
            d1.Login = "NewLog_1";
            d1.Password = "NewPas_1";
            d1.Role = "Admin";

            users.Query_Update(d);
            await users.Query_UpdateAsync(d1);

            Console.WriteLine("Testing Update is Done!\nCheck DB ...");
           
            users.Query_SelectAll();
            Print(users.Data);
            ConsolePause();
            Console.WriteLine();

            // === Deleting === //
            bool res = users.Query_DeleteById(d2.Id);
            await users.Query_DeleteByIdAsync(d1.Id);

            Console.WriteLine("Testing Delete is Done!\nCheck DB ...");
            await users.Query_SelectAllAsync();
            Print(users.Data);
            ConsolePause();
            Console.WriteLine();

            // === Saving === //
            users.Data.Clear();
            users.Data.Add(new(0, 1, "New My Name 1", "New My Nick 1", "NewMylog1", "NewMypas1", "NewRole1"));
            users.Data.Add(new(0, 2, "New My Name 2", "New My Nick 2", "NewMylog2", "NewMypas2", "NewRole2"));
            users.Data.Add(new(0, 3, "New My Name 3", "New My Nick 3", "NewMylog3", "NewMypas3", "NewRole3"));

            users.Query_Save();

            Console.WriteLine("Testing Save Table is Done!\nCheck DB ...");
            users.Query_SelectAll();
            Print(users.Data);
            ConsolePause();

            // === Saving.Async === //
            users.Data.Clear();
            users.Data.Add(new(0, 1, "New My Name 1_0", "New My Nick 1_0", "NewMylog1_0", "NewMypas1_0", "NewRole1_0"));
            users.Data.Add(new(0, 2, "New My Name 2_0", "New My Nick 2_0", "NewMylog2_0", "NewMypas2_0", "NewRole2_0"));
            users.Data.Add(new(0, 3, "New My Name 3_0", "New My Nick 3_0", "NewMylog3_0", "NewMypas3_0", "NewRole3_0"));

            await users.Query_SaveAsync();

            Console.WriteLine("Testing SaveAsync Table is Done!\nCheck DB ...");
            await users.Query_SelectAllAsync();
            Print(users.Data);
            ConsolePause();

            // === Clearning === //
            await users.Query_ClearTableAsync();

            Console.WriteLine("Testing Clear Table is Done!\nCheck DB ...");
            await users.Query_SelectAllAsync();
            Print(users.Data);
            ConsolePause();
        }

        private static void Print(List<DbUser> data)
        {
            Console.WriteLine($"Local data:");
            foreach (var d in data)
            {
                string line = "";
                foreach (var s in d.ToArrayStr)
                    line += s + " | ";

                Console.WriteLine($"({d.Id}): {line}");
            }
        }

        private static void ConsolePause()
        {
            Console.WriteLine("Нажмите ВВОД для продолжения ...");
            Console.ReadLine();
        }

        private static void PrintRegisteredProviders()
        {
            var names = DbProviderFactories.GetProviderInvariantNames();
            foreach (var name in names)
            {
                Console.WriteLine(name);
            }
        }
    }
}
