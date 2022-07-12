using System;
namespace NotesAPI.Models
{
    
    public class NoteCounter
    {

        public NoteCounter()

        {
            
            this.Project = new();
            this.Attribute = new();

        }

        public int NoteCount { get; set; }
        public Project Project { get; set; }
        public Attribute Attribute { get; set; }
    }
    
    
}

