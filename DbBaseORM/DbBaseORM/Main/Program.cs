using DbBaseORM.Controllers;
using DbBaseORM.Global;
using DbBaseORM.Models;
using System.Data.Common;

namespace DbBaseORM
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AppSettings.Instance.Startup();

            // === Testing === //
            DbUserController users = new(
                AppSettings.Instance.CurrentDbProviderName,
                AppSettings.Instance.GetConnectionStringByName("DbTypeSQL") // AppSettings.Instance.GetCurrentConnectionString()
                );

            // === Inserting === //
            DbUser d = new(0, 1, "Name 1", "Nick 1", "pas1", "Admin");
            DbUser d1 = new(0, 2, "Name 2", "Nick 2", "pas2", "Teacher");
            DbUser d2 = new(0, 3, "Name 3", "Nick 3", "pas3", "Student");
            DbUser d3 = new(0, 4, "Name 4", "Nick 4", "pas4", "Student");
            DbUser d4 = new(0, 5, "Name 5", "Nick 5", "pas5", "Student");

            users.Query_Insert(d, isChangeLocalData: true);
            users.Query_Insert(d1, isChangeLocalData: true);
            await users.Query_InsertAsync(d2, isChangeLocalData: true);

            // ATTENTION for parameters! isChangeLocalData: false
            users.Query_Insert(d3, isChangeLocalData: false);
            await users.Query_InsertAsync(d4, isChangeLocalData: false);

            Console.WriteLine("Testing Insert is Done!\nCheck DB ...");
            Print(users.Data);
            ConsolePause();

            // === Selecting All === //
            users.Query_SelectByLimit(1, 2, isChangeLocalData: true);
            Console.WriteLine("\nTesting Query_SelectByLimit (data inside): ");
            Print(users.Data);
            Console.WriteLine();

            await users.Query_SelectByLimitAsync(2, 1, isChangeLocalData: true);
            Console.WriteLine("\nTesting Query_SelectByLimit (data inside): ");
            Print(users.Data);
            Console.WriteLine();

            users.Query_SelectAll(isChangeLocalData: true);
            List<DbUser> users_data = users.Query_SelectAll();

            Console.WriteLine("Testing Query_SelectAll (data inside): ");
            Print(users.Data);

            Console.WriteLine("\nTesting Query_SelectAll (data outside): ");
            Print(users_data);
            Console.WriteLine();

            ConsolePause();

            await users.Query_SelectAllAsync();
            users_data = users.Query_SelectAllAsync().Result;

            Console.WriteLine("Testing Query_SelectAllAsync (data inside): ");
            Print(users.Data);

            Console.WriteLine("\nTesting Query_SelectAll (data outside): ");
            Print(users_data);
            ConsolePause();

            // === Updating === // 
            d.Id = users.Data[0].Id;
            d1.Id = users.Data[1].Id;
            d2.Id = users.Data[2].Id;
            d3.Id = users.Data[3].Id;

            d.TelegramId = 100;
            d.FullName = "NewName";
            d.NickName = "NewNick";
            d.Password = "NewPas";
            d.Role = "Student";

            d1.TelegramId = 101;
            d1.FullName = "NewName_1";
            d1.NickName = "NewNick_1";
            d1.Password = "NewPas_1";
            d1.Role = "Admin";

            d2.TelegramId = 102;
            d2.FullName = "NewName_2";
            d2.NickName = "NewNick_2";
            d2.Password = "NewPas_2";
            d2.Role = "Teacher";

            d3.TelegramId = 103;
            d3.FullName = "NewName_3";
            d3.NickName = "NewNick_3";
            d3.Password = "NewPas_3";
            d3.Role = "Teacher";

            users.Query_UpdateById(d, isChangeLocalData: true);
            await users.Query_UpdateByIdAsync(d1, isChangeLocalData: true);

            // ATTENTION for parameters! isChangeLocalData: false
            users.Query_UpdateById(d2, isChangeLocalData: false);
            await users.Query_UpdateByIdAsync(d3, isChangeLocalData: false);

            Console.WriteLine("Testing Update is Done!\nCheck DB ...");

            Print(users.Data);
            ConsolePause();
            Console.WriteLine();

            // === Deleting === //
            // ATTENTION for parameters! isChangeLocalData: false
            bool res_sync = users.Query_DeleteById(d.Id, isChangeLocalData: false);
            bool res_async = users.Query_DeleteByIdAsync(d1.Id, isChangeLocalData: false).Result;
            users.Query_DeleteById(d2.Id, isChangeLocalData: true);
            await users.Query_DeleteByIdAsync(d3.Id, isChangeLocalData: true);

            Console.WriteLine("Testing Delete is Done!\nCheck DB ...");
            Print(users.Data);
            ConsolePause();
            Console.WriteLine();

            // === Saving (Data Inside) === //
            users.Data.Clear();
            users.Data.Add(new(0, 1, "New My Name 1", "New My Nick 1", "NewMypas1", "NewRole1"));
            users.Data.Add(new(0, 2, "New My Name 2", "New My Nick 2", "NewMypas2", "NewRole2"));
            users.Data.Add(new(0, 3, "New My Name 3", "New My Nick 3", "NewMypas3", "NewRole3"));

            users.Query_Save();

            Console.WriteLine("Testing Save (Data Inside) is Done!\nCheck DB ...");
            Print(users.Data);
            ConsolePause();

            // === SavingAsync (Data Inside) === //
            users.Data.Clear();
            users.Data.Add(new(0, 1, "New My Name 1_0", "New My Nick 1_0", "NewMypas1_0", "NewRole1_0"));
            users.Data.Add(new(0, 2, "New My Name 2_0", "New My Nick 2_0", "NewMypas2_0", "NewRole2_0"));
            users.Data.Add(new(0, 3, "New My Name 3_0", "New My Nick 3_0", "NewMypas3_0", "NewRole3_0"));

            await users.Query_SaveAsync();

            Console.WriteLine("Testing SaveAsync (Data Inside) is Done!\nCheck DB ...");
            Print(users.Data);
            ConsolePause();

            // === Saving (Data Outside) === //
            users.Data.Clear();
            List<DbUser> data = new();
            data.Add(new(0, 1, "New My Name 1", "New My Nick 1", "NewMypas1", "NewRole1"));
            data.Add(new(0, 2, "New My Name 2", "New My Nick 2", "NewMypas2", "NewRole2"));
            data.Add(new(0, 3, "New My Name 3", "New My Nick 3", "NewMypas3", "NewRole3"));

            // ATTENTION for parameters! 
            users.Query_Save(ref data, isChangeLocalData: false);

            Console.WriteLine("Testing Save (isChangeLocalData: false) is Done!\nCheck DB ...\n" +
                "Data Outside:");
            Print(data);
            Console.WriteLine("\nData Inside:");
            Print(users.Data);
            ConsolePause();

            // === Saving (Data Outside) === //
            // ATTENTION for parameters! 
            users.Query_Save(ref data, isChangeLocalData: true);

            Console.WriteLine("Testing Save (isChangeLocalData: true) is Done!\nCheck DB ...\n" +
                "Data Outside:");
            Print(data);
            Console.WriteLine("\nData Inside:");
            Print(users.Data);
            ConsolePause();

            // === SavingAsync (Data Outside) === //
            users.Data.Clear();
            data.Clear();
            data.Add(new(0, 1, "New My Name 1_0", "New My Nick 1_0", "NewMypas1_0", "NewRole1_0"));
            data.Add(new(0, 2, "New My Name 2_0", "New My Nick 2_0", "NewMypas2_0", "NewRole2_0"));
            data.Add(new(0, 3, "New My Name 3_0", "New My Nick 3_0", "NewMypas3_0", "NewRole3_0"));

            // ATTENTION for parameters! 
            await users.Query_SaveAsync(data, isChangeLocalData: false);

            Console.WriteLine("Testing SaveAsync (isChangeLocalData: false) is Done!\nCheck DB ...\n" +
                "Data Outside:");
            Print(data);
            Console.WriteLine("\nData Inside:");
            Print(users.Data);
            ConsolePause();

            // === SavingAsync (Data Outside) === //
            // ATTENTION for parameters! 
            data = users.Query_SaveAsync(data, isChangeLocalData: true).Result;

            Console.WriteLine("Testing SaveAsync (isChangeLocalData: true) is Done!\nCheck DB ...\n" +
                "Data Outside:");
            Print(data);
            Console.WriteLine("\nData Inside:");
            Print(users.Data);
            ConsolePause();

            // === Clearning === //
            users.Query_ClearTable();

            Console.WriteLine("Testing Clear Table is Done!\nCheck DB ...");
            await users.Query_SelectAllAsync();
            Print(users.Data);
            ConsolePause();

            // === Clearning Async === //
            await users.Query_InsertAsync(d, isChangeLocalData: true);
            await users.Query_ClearTableAsync();

            Console.WriteLine("Testing ClearAsync Table is Done!\nCheck DB ...");
            await users.Query_SelectAllAsync();
            Print(users.Data);
            ConsolePause();
        }

        private static void Print(List<DbUser> data)
        {
            Console.WriteLine($"View data:");
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
