namespace GeoHelp.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CollectionNameAttribute : Attribute
    {
        public CollectionNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
