using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EmployeeContext>
    (options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello Employees!");

app.MapGet("/employees", async (EmployeeContext _context) =>
    await _context.Employees.ToListAsync());

app.MapGet("/employees/{id}", async (Guid id, EmployeeContext _context) =>
    await _context.Employees.FindAsync(id)
        is Employee employee
            ? Results.Ok(employee)
            : Results.NotFound());

app.MapPost("/employees", async (Employee employee, EmployeeContext _context) =>
{
    _context.Employees.Add(employee);
    await _context.SaveChangesAsync();

    return Results.Created($"/employees/{employee.Id}", employee);
});

app.MapPut("/employees/{id}", async (Guid id, Employee editEmployee, EmployeeContext _context) =>
{
    if (id != editEmployee.Id) return Results.BadRequest();

    var employee = await _context.Employees.FindAsync(id);

    if (employee is null) return Results.NotFound();

    employee.FirstName = editEmployee.FirstName;
    employee.LastName = editEmployee.LastName;
    employee.Age = editEmployee.Age;
    employee.Gender = editEmployee.Gender;
    employee.Email = editEmployee.Email;
    employee.EmployedSince = editEmployee.EmployedSince;

    await _context.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/employees/{id}", async (Guid id, EmployeeContext _context) =>
{
    if (await _context.Employees.FindAsync(id) is Employee employee)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return Results.Ok(employee);
    }

    return Results.NotFound();
});

app.Run();

class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(25)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string LastName { get; set; }

    public int Age { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; }

    [MaxLength(50)]
    public string Email { get; set; }

    public DateTime EmployedSince { get; set; }
}

class EmployeeContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }

    public EmployeeContext(DbContextOptions options)
        : base(options) { }
}