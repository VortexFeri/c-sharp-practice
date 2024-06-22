using System.Data;
using Dapper;
using TestAPI.Context;
using TestAPI.Contracts;
using TestAPI.DTO;
using TestAPI.Entities;

namespace TestAPI.Repository;

public class CompanyRepository(DapperContext context) : ICompanyRepository
{
    private readonly DapperContext _context = context;

    public async Task<IEnumerable<Company>> GetCompanies()
    {
        const string query = "SELECT * FROM Companies";

        using (var connection = _context.CreateConnection())
        {
            var companies = await connection.QueryAsync<Company>(query);
            return companies.ToList();
        }
    }

    public async Task<Company?> GetCompanyById(int id)
    {
        const string query = "SELECT * FROM Companies WHERE Id = @Id";
        using (var connection = _context.CreateConnection())
        {
            var company = await connection.QuerySingleOrDefaultAsync<Company>(query, new { id });
            return company;
        }
    }

    public async Task<Company> CreateCompany(CompanyForCreationDto company)
    {
        const string query = """
                             INSERT INTO Companies (Name, Address, Country)
                             VALUES (@Name, @Address, @Country)
                             SELECT CAST(SCOPE_IDENTITY() AS int)
                             """;
        var parameters = new DynamicParameters();
        parameters.Add("Name", company.Name, DbType.String);
        parameters.Add("Address", company.Address, DbType.String);
        parameters.Add("Country", company.Country, DbType.String);

        using (var connection = _context.CreateConnection())
        {
            var id = await connection.QuerySingleAsync<int>(query, parameters);
            var createdCompany = new Company
            {
                Id = id,
                Name = company.Name,
                Address = company.Address,
                Country = company.Country
            };
            return createdCompany;
        }
    }

    public async Task UpdateCompany(int id, CompanyForUpdateDto company)
    {
        const string query = """
                             UPDATE Companies
                             SET Name = @Name, Address = @Address, Country = @Country
                             WHERE Id = @Id"
                             """;
        var parameters = new DynamicParameters();
        parameters.Add("Id", id, DbType.Int32);
        parameters.Add("Name", company.Name, DbType.String);
        parameters.Add("Address", company.Address, DbType.String);
        parameters.Add("Country", company.Country, DbType.String);

        using (var connection = _context.CreateConnection())
        {
            await connection.ExecuteAsync(query, parameters);
        }
    }

    public async Task DeleteCompany(int id)
    {
        const string query = "DELETE FROM Companies WHERE Id = @Id";
        using (var connection = _context.CreateConnection())
        {
            await connection.ExecuteAsync(query, new { id });
        }
    }

    public async Task<Company?> GetCompanyByEmployeeId(int id)
    {
        const string procedureName = "GetCompanyByEmployeeId";
        var parameters = new DynamicParameters();
        parameters.Add("id", id, DbType.Int32, ParameterDirection.Input);

        using (var connection = _context.CreateConnection())
        {
            var company = await connection.QueryFirstOrDefaultAsync<Company>(procedureName, parameters,
                commandType: CommandType.StoredProcedure);

            return company;
        }
    }

    public async Task<Company?> GetMultipleResults(int id)
    {
        const string query = """
                             SELECT * FROM Companies WHERE id = @id
                             SELECT * FROM Employees WHERE CompanyId = @id
                             """;

        using (var connection = _context.CreateConnection())
        await using (var multi = await connection.QueryMultipleAsync(query, new { id }))
        {
            var company = await multi.ReadSingleOrDefaultAsync<Company>();
            if (company is not null) company.Employees = (await multi.ReadAsync<Employee>()).ToList();

            return company;
        }
    }

    public async Task<List<Company>> GetMultipleMapping()
    {
        const string query = """
                             SELECT c.*, e.* 
                             FROM Companies c 
                             JOIN Employees e ON c.Id = e.CompanyId
                             """;

        using (var connection = _context.CreateConnection())
        {
            var companyDict = new Dictionary<int, Company>();
            var companies = await connection.QueryAsync<Company, Employee, Company>(
                query, (company, employee) =>
                {
                    if (!companyDict.TryGetValue(company.Id, out var currentCompany))
                    {
                        currentCompany = company;
                        currentCompany.Employees = new List<Employee>();
                        companyDict.Add(currentCompany.Id, currentCompany);
                    }

                    currentCompany.Employees.Add(employee);

                    return currentCompany;
                }
            );
            return companies.Distinct().ToList();
        }
    }
}