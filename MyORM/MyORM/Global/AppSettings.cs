using System.Data.Common;

namespace MyORM.Global
{
    /// <summary>
    /// Одиночка. Настройки приложенния. 
    /// </summary>
    public class AppSettings
    {
        private static AppSettings INSTANCE;

        public string DbProviderNameCurrent = "Microsoft.Data.SqlClient";

        private AppSettings()
        {
            Startup();
        }

        public static AppSettings GetInstance
        {
            get
            {
                if (INSTANCE == null)
                {
                    INSTANCE = new AppSettings();
                }

                return INSTANCE;
            }
        }

        /// <summary>
        /// Вызывается в конструкторе. При необходимости можно вызвать из вне. 
        /// Установка начальных настроек приложения:
        /// - регистрация фабрик провайдеров; 
        /// - добавить при необходимости ...
        /// </summary>
        public void Startup()
        {
            // DbProviderFactories.RegisterFactory("System.Data.OleDb", System.Data.OleDb.OleDbFactory.Instance);
            DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
        }
    }
}
