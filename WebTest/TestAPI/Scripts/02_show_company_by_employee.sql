IF OBJECT_ID('GetCompanyByEmployeeId', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE GetCompanyByEmployeeId;
END;
GO

CREATE PROCEDURE GetCompanyByEmployeeId
    @id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Id, c.Name, c.Address, c.Country
    FROM Companies c
    RIGHT JOIN Employees e ON c.Id = e.CompanyId
    WHERE e.Id = @id;
END;
GO
