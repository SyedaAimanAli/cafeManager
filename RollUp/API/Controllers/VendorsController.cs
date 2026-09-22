using RollUp.Application.DTOs;
using RollUp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RollUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendors;

    public VendorsController(IVendorService vendors) => _vendors = vendors;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _vendors.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vendor = await _vendors.GetByIdAsync(id);
        return vendor is null ? NotFound() : Ok(vendor);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
    {
        var created = await _vendors.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateVendorDto dto)
    {
        var updated = await _vendors.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _vendors.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
