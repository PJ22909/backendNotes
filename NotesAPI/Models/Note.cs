using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using NotesAPI.Models;

namespace NotesAPI.Models
{
    //[ModelBinder(BinderType = typeof(NoteBinder))]
    //[JsonConverter(typeof(JsonPathConverter))]
    
    public class Note
    {

        public Note()
        {
            this.Project = new Project();
            this.Attributes = new List<Attribute>();
        }
        public int? NoteId { get; set; }
        public DateTime? Created { get; set; }


        public string? NoteText { get; set; }
      
        public Project Project { get; set; }
        
        public List<Attribute> Attributes { get; set; }

    }
    //public override bool Equals(object? obj)
    //    {
    //        if (obj == null) return false;
    //        Note? objAsPart = obj as Note;
    //        if (objAsPart == null) return false;
    //        else return Equals(objAsPart);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return NoteId;
    //    }
    //    public bool Equals(Note? other)
    //    {
    //        if (other == null) return false;
    //        return NoteId.Equals(other.NoteId);
    //    }

    //}
}
