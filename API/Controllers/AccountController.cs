using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;
    private readonly TokenServices _tokenServices;
    public AccountController(DataContext context, ITokenServices tokenServices, TokenServices tokenServices1)
    {
        _context = context;
        _tokenServices = tokenServices1;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> RegisterUser(RegisterAccountDto registerAccountDto)
    {
        if (await UserExist(registerAccountDto.Username))
            return BadRequest("Username is taken, ble ble");
        
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            Username = registerAccountDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerAccountDto.Password)),
            PasswordSalt = hmac.Key
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return new UserDto
        {
            Username = user.Username,
            Token = _tokenServices.CreateToken(user)
        };
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == loginDto.Username);

        if (user == null)
            return Unauthorized("You have no rights here, get lost");

        using var hmac = new HMACSHA512(user.PasswordSalt);
            
        var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        var length = computeHash.Length;

        for (int i = 0; i < length; i++)
        {
            if (computeHash[i] != user.PasswordHash[i]) 
                return Unauthorized("Wrong password, use your brain to remember it");
        }

        return new UserDto
        {
            Username = user.Username,
            Token = _tokenServices.CreateToken(user)
        };
    }

    private async Task<bool> UserExist(string username)
    {
        return await _context.Users.AnyAsync(x => x.Username == username.ToLower());
    }
    
}