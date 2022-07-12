using System.Runtime.Serialization;

namespace NotesAPI.Models
{
    
    public class Attribute : IEquatable<Attribute>
    {
        
        public int Id { get; set; }
        
        public string? Name { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            Attribute? objAsPart = obj as Attribute;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return Id;
        }
        public bool Equals(Attribute? other)
        {
            if (other == null) return false;
            return Id.Equals(other.Id);
        }
    }
}
