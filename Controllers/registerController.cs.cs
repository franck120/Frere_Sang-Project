using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
[ApiController]
[Route("[controller]")]
[Authorize]
public class registerController : ControllerBase
{
    private readonly string _connectionString;

    public registerController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("connectConnection");
    }

    [HttpPost("register")]
    public async Task<IActionResult> register([FromBody] register register)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("INSERT INTO enregistrement (nom, localisation, adresse, contact, chambre, type) VALUES (@nom, @localisation, @adresse, @contact, @chambre, @type)", connection);
                command.Parameters.AddWithValue("@nom", register.nom);
                command.Parameters.AddWithValue("@localisation", register.localisation);
                command.Parameters.AddWithValue("@adresse", register.adresse);
                command.Parameters.AddWithValue("@contact", register.contact);
                command.Parameters.AddWithValue("@chambre", register.chambre);
                command.Parameters.AddWithValue("@type", register.type);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return Ok();
                }
                return BadRequest("Erreur d'enregistrement");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
