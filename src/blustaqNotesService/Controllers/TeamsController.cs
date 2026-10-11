using bluestaqNotesService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace bluestaqNotesService.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("api/teams")]
    public class TeamsController : ControllerBase
    {
        private readonly SqliteDatabase _db;

        public TeamsController(SqliteDatabase db)
        {
            _db = db;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        private bool TeamExists(int teamId)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            var checkTeamExists = connection.CreateCommand();
            checkTeamExists.CommandText = "SELECT COUNT(1) FROM Teams WHERE Id = @teamId;";
            checkTeamExists.Parameters.AddWithValue("@teamId", teamId);
            var teamExists = (long)(checkTeamExists.ExecuteScalar() ?? 0L) > 0;

            return teamExists;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        private string UserBelongsToTeam(int userId, int teamId)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            var checkUserCmd = connection.CreateCommand();
            checkUserCmd.CommandText = "SELECT Username FROM Users WHERE Id = @userId AND TeamId = @teamId;";
            checkUserCmd.Parameters.AddWithValue("@userId", userId);
            checkUserCmd.Parameters.AddWithValue("@teamId", teamId);

            using var userReader = checkUserCmd.ExecuteReader();
            if(!userReader.Read())
            {
                return string.Empty;
            }
            string username = userReader.GetString(0);
            userReader.Close();

            return username;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] string name)
        {
            // Establish DB conn
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();
            DateTime createdAt = DateTime.UtcNow; // record time once for both creation and return

            // SQL
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Teams (Name, Createdat) VALUES (@name, @createdAt); SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@createdAt", createdAt.ToString("0"));

            var teamId = (long)await command.ExecuteScalarAsync();

            // Build team model
            Team team = new Team
            {
                Id = (int)teamId,
                Name = name,
                CreatedAt = createdAt
            };

            // return success
            return Created($"/api/teams/{teamId}", team);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost("{teamId}/users")]
        public async Task<IActionResult> AddUser(int teamId, [FromBody] string userName)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            // Validation Short-circut
            if(!TeamExists(teamId))
            {
                return NotFound("Team not found.");
            }

            // Insert User
            var command = connection.CreateCommand();
            command.Parameters.AddWithValue("@teamdId", teamId);
            command.Parameters.AddWithValue("@username", userName);

            var userId = (long)await command.ExecuteScalarAsync();

            // Construct User and return success
            User user = new User
            {
                Id = (int)userId,
                TeamId = teamId,
                Username = userName
            };

            return Created($"/api/teams/{teamId}/users/{userId}", user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="createdById"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPost("{teamId}/notes")]
        public async Task<IActionResult> CreateNote(int teamId, [FromBody] int createdById, [FromBody] string title, [FromBody] string content)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            // Validation circutry
            if(!TeamExists(teamId))
            {
                return NotFound("Team not found.");
            }

            string username = UserBelongsToTeam(createdById, teamId);
            if (username == string.Empty)
            {
                return BadRequest("Invalid user or user does not belong to this team.");
            }

            // SQL
            DateTime now = DateTime.UtcNow;
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Notes (TeamId, CreatedById, Title, Content, CreatedAt, UpdatedAt)
                VALUES (@teamId, @createdById, @title, @content, @createdAt, @updatedAt);
                SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("@teamId", teamId);
            command.Parameters.AddWithValue("@createdById", createdById);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@createdAt", now.ToString("0"));
            command.Parameters.AddWithValue("@updatedAt", now.ToString("0"));

            // Note object
            var noteId = (long)await command.ExecuteScalarAsync();

            Note note = new Note
            {
                Id = (int)noteId,
                TeamId = teamId,
                CreatedById = createdById,
                Title = title,
                Content = content,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = new User
                {
                    Id = createdById,
                    TeamId = teamId,
                    Username = username
                }
            };

            return Created($"/api/notes/{noteId}", note)
        }

        [HttpGet("{teamId/notes")]
        public async Task<IActionResult> GetTeamNotes(int teamId)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            // Validation circut
            if(!TeamExists(teamId))
            {
                return NotFound("Team not found.");
            }

            // SQL
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT n.Id, n.TeamId, n.Title, n.Content, n.CreatedAt, n.UpdatedAt, u.Id as UserId, u.Username
                FROM Notes n
                JOIN Users u ON n.CreatedById = u.Id
                Where n.TeamId = @teamId";
            command.Parameters.AddWithValue("@teamId", teamId);


            // Get the notes
            List<Note> notes = new List<Note>();
            using var reader = await command.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                var note = new Note
                {
                    Id        = reader.GetInt32(0),
                    TeamId    = reader.GetInt32(1),
                    Title     = reader.GetString(2),
                    Content   = reader.GetString(3),
                    CreatedAt = DateTime.Parse(reader.GetString(4)),
                    UpdatedAt = DateTime.Parse(reader.GetString(5)),
                    CreatedBy = new User
                    {
                        Id       = reader.GetInt32(6),
                        TeamId   = reader.GetInt32(1),
                        Username = reader.GetString(7)
                    }
                };
                notes.Add(note);
            }

            return Ok(notes);
        }
    }
}
