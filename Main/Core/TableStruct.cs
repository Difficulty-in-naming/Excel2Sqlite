namespace DreamExcel.Core
{
    public class TableStruct
    {
        public string Name;
        public string Type;

        public bool IsArray
        {
            get { return Type.EndsWith("[]"); }
        }
        public TableStruct(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
