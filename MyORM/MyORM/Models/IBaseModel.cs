using System.Data.Common;

namespace MyORM.Models
{
    internal interface IBaseModel
    {
        int Id { get; set; }

        string ToParamsInsertQueryWithID { get; }
        string ToParamsInsertQueryWithoutID { get; }
        string ToParamsUpdateQuery { get; }

        string[] ToArrayStr { get; }

        void SetData(ref DbDataReader reader);
    }
}
