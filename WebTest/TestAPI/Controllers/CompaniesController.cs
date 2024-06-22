using Microsoft.AspNetCore.Mvc;
using TestAPI.Contracts;
using TestAPI.DTO;

namespace TestAPI.Controllers;

[Route("api/companies")]
[ApiController]
public class CompaniesController(ICompanyRepository companyRepository) : ControllerBase
{
    private readonly ICompanyRepository _companyRepository = companyRepository;

    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _companyRepository.GetCompanies();

        return Ok(companies);
    }

    [HttpGet("{id:int}", Name = "CompanyById")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _companyRepository.GetCompanyById(id);
        if (company is null)
            return NotFound();
        return Ok(company);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
    {
        var createdCompany = await _companyRepository.CreateCompany(company);
        return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyForUpdateDto company)
    {
        var dbCompany = await _companyRepository.GetCompanyById(id);
        if (dbCompany is null) return NotFound();
        await _companyRepository.UpdateCompany(id, company);
        return Ok(dbCompany);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var dbCompany = await _companyRepository.GetCompanyById(id);
        if (dbCompany is null) return NotFound();
        await _companyRepository.DeleteCompany(id);
        return Ok(dbCompany);
    }

    [HttpGet("ByEmployeeId/{id:int}")]
    public async Task<IActionResult> GetCompanyForEmployee(int id)
    {
        var company = await _companyRepository.GetCompanyByEmployeeId(id);
        if (company is null) return NotFound();
        return Ok(company);
    }

    [HttpGet("{id:int}/MultipleResult")]
    public async Task<IActionResult> GetMultipleResults(int id)
    {
        var company = await _companyRepository.GetMultipleResults(id);
        if (company is null) return NotFound();
        return Ok(company);
    }

    [HttpGet("MultipleMapping")]
    public async Task<IActionResult> GetMultipleMapping()
    {
        var companies = await _companyRepository.GetMultipleMapping();
        return Ok(companies);
    }
}
