using System.Data.Common;

namespace DbBaseORM.Models
{
    /// <summary>
    /// Интерфейс для реализации моделей данных
    /// </summary>
    public interface IBaseModel
    {
        int Id { get; set; }

        string ToParamsInsertQueryWithID { get; }
        string ToParamsInsertQueryWithoutID { get; }
        string ToParamsUpdateQuery { get; }

        string[] ToArrayStr { get; }

        void SetData(ref DbDataReader reader);
    }
}
