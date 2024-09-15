using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class connectController : ControllerBase
{
    private readonly string _connectionString;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public connectController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("connectConnection");
        _secretKey = configuration["Jwt:cle"];
        _issuer = configuration["Jwt:Issuer"];
        _audience = configuration["Jwt:Audience"];
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] connect connect)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM connexion WHERE nom = @nom AND mdp = @mdp", connection);
                command.Parameters.AddWithValue("@nom", connect.nom);
                command.Parameters.AddWithValue("@mdp", connect.mdp);

                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var token = GenerateJwtToken(connect.nom);
                    return Ok(new { token });
                }
                return Unauthorized();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private string GenerateJwtToken(string nom)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey); 
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, nom)
            }),
            Expires = DateTime.UtcNow.AddMinutes(2), 
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
