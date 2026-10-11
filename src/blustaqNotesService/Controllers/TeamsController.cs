using bluestaqNotesService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace bluestaqNotesService.Controllers
{
    [ApiController]
    [Route("api/teams")]
    public class TeamsController : ControllerBase
    {
        private readonly SqliteDatabase _db;

        public TeamsController(SqliteDatabase db)
        {
            _db = db;
        }

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
    }
}
