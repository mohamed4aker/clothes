using MAS.Application.DTOs;
using MAS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MAS.API.Controllers;

// ============================================================
// AuthController - تسجيل الدخول
// ============================================================
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(IAuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة" });

        // توليد JWT Token
        response.Token = GenerateJwtToken(response);
        return Ok(response);
    }

    private string GenerateJwtToken(LoginResponse user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "MAS-Default-Secret-Key-Change-In-Production-12345678";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("BrandId", user.BrandId.ToString()),
            new Claim("BrandName", user.BrandName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "MAS",
            audience: _config["Jwt:Audience"] ?? "MAS-Users",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ============================================================
// ProductsController - المنتجات
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    private int GetBrandId() => int.Parse(User.FindFirst("BrandId")!.Value);
    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetProductsAsync(GetBrandId());
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id, GetBrandId());
        return product == null ? NotFound() : Ok(product);
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<IActionResult> GetByBarcode(string barcode)
    {
        var product = await _productService.GetByBarcodeAsync(barcode, GetBrandId());
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var id = await _productService.CreateProductAsync(GetBrandId(), GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id }, new { productId = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductRequest request)
    {
        var success = await _productService.UpdateProductAsync(id, GetBrandId(), request);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.DeleteProductAsync(id, GetBrandId());
        return success ? Ok() : NotFound();
    }
}

// ============================================================
// SalesController - المبيعات (POS)
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    private int GetBrandId() => int.Parse(User.FindFirst("BrandId")!.Value);
    private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
    {
        try
        {
            var sale = await _salesService.CreateSaleAsync(GetBrandId(), GetUserId(), request);
            return Ok(sale);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("recent/{branchId}")]
    public async Task<IActionResult> GetRecent(int branchId, [FromQuery] int count = 20)
    {
        var sales = await _salesService.GetRecentSalesAsync(GetBrandId(), branchId, count);
        return Ok(sales);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sale = await _salesService.GetSaleByIdAsync(id, GetBrandId());
        return sale == null ? NotFound() : Ok(sale);
    }
}

// ============================================================
// DashboardController - لوحة التحكم
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private int GetBrandId() => int.Parse(User.FindFirst("BrandId")!.Value);

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] int? branchId = null)
    {
        var stats = await _dashboardService.GetStatsAsync(GetBrandId(), branchId);
        return Ok(stats);
    }
}

// ============================================================
// BranchesController - الفروع
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    private int GetBrandId() => int.Parse(User.FindFirst("BrandId")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _branchService.GetBranchesAsync(GetBrandId());
        return Ok(branches);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request)
    {
        var id = await _branchService.CreateBranchAsync(GetBrandId(), request);
        return Ok(new { branchId = id });
    }
}

// ============================================================
// UsersController - المستخدمين
// ============================================================
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    private int GetBrandId() => int.Parse(User.FindFirst("BrandId")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetUsersAsync(GetBrandId());
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var id = await _userService.CreateUserAsync(GetBrandId(), request);
            return Ok(new { userId = id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("can-add")]
    public async Task<IActionResult> CanAddUser()
    {
        var canAdd = await _userService.CheckUserLimitAsync(GetBrandId());
        return Ok(new { canAdd });
    }
}
