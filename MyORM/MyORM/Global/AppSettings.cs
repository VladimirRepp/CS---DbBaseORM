using System.Data.Common;

namespace MyORM.Global
{
    public static class AppSettings
    {
        public static string DB_PROVIDER_NAME = "Microsoft.Data.SqlClient";

        /// <summary>
        /// Установка начальных настроек приложения: 
        /// - регистрация фабрик провайдеров; 
        /// - добавить при необходимости ...
        /// </summary>
        public static void Startup()
        {
            DbProviderFactories.RegisterFactory("System.Data.OleDb", System.Data.OleDb.OleDbFactory.Instance);
            DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
        }
    }
}
