
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using NotesAPI.Models;
using Newtonsoft.Json;


namespace NotesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        //public IConfiguration Configuration { get; }
        //public IWebHostEnvironment Env { get; }
        public string connectionString = String.Empty;

        public NotesController(IConfiguration configuration, IWebHostEnvironment env)
        {
            //Configuration = configuration;
            connectionString = env.IsDevelopment() ? configuration.GetConnectionString("DevConnection") : configuration.GetConnectionString("ProdConnection");

        }
       


        /// <summary>
        ///  This will return a json formatted string of requested Note objects.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="attributeIds"></param>
        /// <returns></returns>
        [Route("GetNotes/{projectId}/")]
        [HttpPost]
       // public List<Note> GetNotes()
        public string GetNotes(int? projectId, [FromBody] List<int>? attributeIds)
        {
            List<Note> results = new();
            string resultsJSON = string.Empty; 

            string sql = @"WITH RankedItem AS
                            (SELECT NoteId,
                                        NoteAttributes.AttributeID,
                                        Attributes.Name,
                                        --NoteId,
			                            RowNumber = ROW_NUMBER() OVER(PARTITION BY NoteId ORDER BY NoteAttributes.AttributeId)

                                FROM NoteAttributes join Attributes on NoteAttributes.AttributeId = Attributes.AttributeId
    
                            ), Items AS
                            (
                                SELECT  notes.NoteID,
                                        notes.noteText,
                                        notes.creationtime,
                                        notes.projectId,
                                        projects.name,

			                            AttributeId1 = MIN(CASE WHEN RowNumber = 1 THEN r.AttributeId END),
			                            AttributeName1 = MIN(CASE WHEN RowNumber = 1 THEN r.Name END),
                                        
			                            AttributeId2 = MIN(CASE WHEN RowNumber = 2 THEN r.AttributeId END),
                                        AttributeName2 = MIN(CASE WHEN RowNumber = 2 THEN r.Name END),
			                            
			                            AttributeId3 = MIN(CASE WHEN RowNumber = 3 THEN r.AttributeId END),
                                        AttributeName3 = MIN(CASE WHEN RowNumber = 3 THEN r.Name END),
			                            
			                            AttributeId4 = MIN(CASE WHEN RowNumber = 4 THEN r.AttributeId END),
                                        AttributeName4 = MIN(CASE WHEN RowNumber = 4 THEN r.Name END)
                                        
                                FROM RankedItem r
                                right join Notes on r.NoteId = Notes.NoteId
                                left join projects on notes.projectid = projects.projectid
                                left join NoteAttributes on noteAttributes.noteId = notes.NoteId
                                left join Attributes a on NoteAttributes.attributeId = a.attributeId
                                GROUP BY notes.creationTime,  notes.notetext, notes.noteid, notes.projectid,  projects.name
                            )
                            select * from Items";


            if (projectId != null && projectId != 0)  // get notes with projectId
            {
                sql += " where projectid = " + projectId;
            }


            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();
                
                using (SqlCommand command = new(sql, connection))
                {
                    Note noteWithAttributes = new();
                    List<Models.Attribute> attributes = new();

                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Note note = new();
                        note.NoteId = reader.GetInt32(0);
                        note.NoteText = reader.GetString(1);
                        note.Created = reader.GetDateTime(2);
                        

                        // Assign Project 
                        if (!reader.IsDBNull(3))
                        {
                            note.Project.ProjectId = reader.GetInt32(3);
                            note.Project.Name = reader.GetString(4);
                        }

                        // Assign Attributes... Currently set to only have 3 attributes in the system
                        if (!reader.IsDBNull(5))
                        {
                            Models.Attribute firstAttribute = new();
                            firstAttribute.Id = reader.GetInt32(5);
                            firstAttribute.Name = reader.GetString(6);
                            note.Attributes.Add(firstAttribute);
                        }

                        if (!reader.IsDBNull(7))
                        {
                            Models.Attribute secondAttribute = new();
                            secondAttribute.Id = reader.GetInt32(7);
                            secondAttribute.Name = reader.GetString(8);
                            note.Attributes.Add(secondAttribute);
                        }
                        if (!reader.IsDBNull(9))
                        {
                            Models.Attribute thirdAttribute = new();
                            thirdAttribute.Id = reader.GetInt32(9);
                            thirdAttribute.Name = reader.GetString(10);
                            note.Attributes.Add(thirdAttribute);
                        }


                        results.Add(note);


                    }

                }



                // Filter results to find Notes with specified attributes if requested
                if (attributeIds != null && attributeIds[0] != 0)
                {
                    List<Note> filteredResults = new();
                    

                    foreach (Note n in results)
                    {
                        if (n.Attributes.Count != 0)
                        {
                            List<int> noteAttributeIds = new();

                            //super fancy way of seeing if the note has any of the attributes listed
                            if ( n.Attributes.Select(x => x.Id).Intersect(attributeIds).Any())
                            {
                                filteredResults.Add(n);
                            }

                            //This makes the note need all or nothing attributes... if looking for id=2... it will only return if note only has attribute=2
                            //Almost... we need to find if the note has any of the asked for attributes... we could use this for a different api call to specify wanting all attributes, or any attributes
                            //if (Enumerable.SequenceEqual(noteAttributeIds, attributeIds))
                            //{
                            //    filteredResults.Add(n);
                            //}
                        }
                    }
                    results = filteredResults;
                }

                //convert output to JSON
                foreach (Note note in results)
                {
                    resultsJSON += JsonConvert.SerializeObject(note, Formatting.Indented);
                }
                
                return resultsJSON;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                return e.ToString();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="newNote"></param>
        /// <returns></returns>

        [Route("NewNote/")]
        [HttpPost]
        public string NewNote([FromBody] Note newNote)
        {


            string sqlInsertIntoNotesTable = string.Empty;
            int noteId = 0;


            // Write sql to insert new note into notes table... doesn't require a projectId so two versions...
            if (newNote.Project.ProjectId != null)
            {
                sqlInsertIntoNotesTable = @"INSERT INTO NOTES (NoteText, ProjectId) OUTPUT INSERTED.NoteId values ('" + newNote.NoteText + "', " + 1 + ");"; //+ newNote.Project.ProjectId + ");";
            }
            else
            {
                sqlInsertIntoNotesTable = @"INSERT INTO NOTES (NoteText) OUTPUT INSERTED.NoteId values ('" + newNote.NoteText + "');";
            }


            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                using (SqlCommand command = new(sqlInsertIntoNotesTable, connection))
                {


                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        noteId = reader.GetInt32(0);
                        Console.WriteLine("success");

                    }

                }

                if (newNote.Attributes != null)
                {
                    string sqlInsertIntoNotesAttributesTable = String.Empty;
                    foreach (Models.Attribute a in newNote.Attributes)
                    {
                        sqlInsertIntoNotesAttributesTable += "Insert into NoteAttributes (NoteId, AttributeId) values (" + noteId + ", " + a.Id + ");";
                    }

                    using (SqlCommand command = new(sqlInsertIntoNotesAttributesTable, connection))
                    {
                        command.ExecuteReader();   
                    }
                }

                return "Success: New Note Id:" + noteId;

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                //return new string[] { sql, "error", e.ToString() };
                return e.ToString();
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[Route("DeleteNote/{id}")]
        [HttpDelete("{id}")]
        public string DeleteNote(int id)
        {
            string sql = @"DELETE FROM noteattributes WHERE noteid = " + id + "; " +
                           "DELETE FROM notes WHERE noteid = " + id + "; ";
                            
            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                using (SqlCommand command = new(sql, connection))
                {


                    using SqlDataReader reader = command.ExecuteReader();

                }

                return "success";
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                //return new string[] { sql, "error", e.ToString() };
                return e.ToString();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        [Route("UpdateNote/")]
        [HttpPost]
        public string UpdateNote([FromBody] Note note)
        {



            string sql = "Update notes SET noteText = '" + note.NoteText + "' WHERE noteId = " + note.NoteId;
            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                using (SqlCommand command = new(sql, connection))
                {


                    using SqlDataReader reader = command.ExecuteReader();

                }

                return "success";
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                //return new string[] { sql, "error", e.ToString() };
                return e.ToString();
            }

            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("GetProjectNoteCounts/")]
        [HttpGet]
        public List<NoteCounter> GetProjectNoteCounts()
        {
            string sql = @"select n.projectId, count(1), p.Name FROM notes n
                            left join Projects p on n.projectId = p.projectId
                            group by n.projectId, p.Name;";
 
            
            List<NoteCounter> counterList = new();
            

            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                using (SqlCommand command = new(sql, connection))
                {


                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        NoteCounter noteCounter = new();

                        if (reader.IsDBNull(0))
                        {
                            noteCounter.Project.Name = "No Project";
                            noteCounter.Project.ProjectId = 0;
                        } else
                        {
                            noteCounter.Project.ProjectId = reader.GetInt32(0);
                            noteCounter.Project.Name = reader.GetString(2);
                        }
                        
                        noteCounter.NoteCount = reader.GetInt32(1);
                        

                        counterList.Add(noteCounter);

                    }

                    return counterList;
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                //return new string[] { sql, "error", e.ToString() };
                //return e.ToString();
                return counterList;
            }
            
        }
    }
}


