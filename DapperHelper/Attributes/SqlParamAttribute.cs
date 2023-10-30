using System.Data;
namespace DapperHelper.Attributes
{
    public class SqlParamAttribute : Attribute
    {
        //public string ParameterName { get; set; }
        //public string ParameterType { get; set; }
        //public object DefaultValue { get; set; }
        public int Size { get; set; }
        public ParameterDirection Direction { get; set; }
    }
}