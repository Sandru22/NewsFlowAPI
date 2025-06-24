using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NewsFlowAPI.Models;
using NewsFlowAPI.Auth;
using System.Net;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using NewsFlowAPI.Dto;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController( UserManager<User> userManager, SignInManager<User> signInManager,IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = new User { FullName = model.UserName , Email = model.Email , UserName= model.UserName };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("User created");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest("User not found");
        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return Ok("Password reset successfully");
    }


    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordModel model,
    [FromServices] EmailService emailService)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Ok("Dacă adresa există, un cod a fost trimis."); 

        var code = new Random().Next(100000, 999999).ToString();

        var expires = DateTime.UtcNow.AddMinutes(15);
        var tokenValue = $"{code}|{expires:o}";

        await _userManager.SetAuthenticationTokenAsync(user, "Default", "PasswordReset", tokenValue);


        await emailService.SendEmailAsync(
            model.Email,
            "Codul tău de resetare a parolei",
            $"<h3>Salut, {user.FullName}!</h3>" +
            $"<p>Codul tău de resetare a parolei este:</p>" +
            $"<h2>{code}</h2>" +
            $"<p><small>Codul expiră în 15 minute.</small></p>");

        return Ok("Verifică emailul pentru codul de resetare.");
    }


    [HttpPost("validate-reset-code")]
    public async Task<IActionResult> ValidateResetCode([FromBody] ValidateResetCodeModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest("Cont invalid.");

        var savedToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "PasswordReset");

        if (string.IsNullOrEmpty(savedToken) || !savedToken.Contains("|"))
            return BadRequest("Codul a expirat sau nu există.");

        var parts = savedToken.Split('|');
        var code = parts[0];
        var expiration = DateTime.Parse(parts[1]);

        if (DateTime.UtcNow > expiration)
            return BadRequest("Codul a expirat.");

        if (code != model.Code)
            return BadRequest("Cod incorect.");


        await _userManager.RemoveAuthenticationTokenAsync(
            user,
            "Default",
            "PasswordReset");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return Ok(new { Token = token });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid credentials");

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }



    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email)
    };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}