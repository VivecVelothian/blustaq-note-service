using bluestaqNotesService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

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

        [HttpPost("{teamId}/users")]
        public async Task<IActionResult> AddUser(int teamId, [FromBody] string userName)
        {
            using var connection = _db.CreateConnection();
            await connection.OpenAsync();

            // Validation
            var checkTeamExists = connection.CreateCommand();
            checkTeamExists.CommandText = "SELECT COUNT(1) FROM Teams WHERE Id = @teamId;";
            checkTeamExists.Parameters.AddWithValue("@teamId", teamId);
            var teamExists = (long)(await checkTeamExists.ExecuteScalarAsync() ?? 0L) > 0;

            // Short-circut
            if(!teamExists)
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
    }
}
