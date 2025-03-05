using System.Configuration;
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
        /// Получить строку подключения из ConfigurationManager.ConnectionStrings по имени провайдера 
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public string GetConnectionStringByProviderName(string providerName)
        {
            string found_value = null;

            ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;

            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.ProviderName == providerName)
                    {
                        found_value = cs.ConnectionString;
                        break;
                    }
                }
            }

            return found_value;
        }

        /// <summary>
        /// Получить строку подключения из ConfigurationManager.ConnectionStrings по названию 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetConnectionStringByName(string name)
        {
            string found_value = null;

            ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;

            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.Name == name)
                    {
                        found_value = cs.ConnectionString;
                        break;
                    }
                }
            }

            return found_value;
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