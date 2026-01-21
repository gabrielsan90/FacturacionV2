---
name: dotnet-testing
description: Create automated tests with xUnit, Moq, and FluentAssertions. Use when writing unit tests for repositories and controllers, integration tests for API endpoints, DTO validation tests, or mocking dependencies.
---

# .NET QA Engineer / Testing Specialist

Implement automated tests with xUnit, Moq and FluentAssertions.

## Test Project Structure

```
[ProjectName].Tests/
├── [ProjectName].UnitTests/
│   ├── Repositories/
│   ├── Services/
│   └── Controllers/
├── [ProjectName].IntegrationTests/
│   ├── Api/
│   └── Database/
└── [ProjectName].E2ETests/
    └── Scenarios/
```

## NuGet Packages

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" />
<PackageReference Include="xunit" />
<PackageReference Include="xunit.runner.visualstudio" />
<PackageReference Include="Moq" />
<PackageReference Include="FluentAssertions" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" />
```

## Unit Test for Repository

```csharp
public class PackageRepositoryTests : IDisposable
{
    private readonly DataContext _context;
    private readonly PackageRepository _repository;

    public PackageRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new DataContext(options);
        _repository = new PackageRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPackages()
    {
        // Arrange
        await _context.Packages.AddRangeAsync(
            new Package { Id = 1, TrackingNumber = "T001", StatusId = 1, UserId = "user1" },
            new Package { Id = 2, TrackingNumber = "T002", StatusId = 1, UserId = "user1" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.WasSuccess.Should().BeTrue();
        result.Result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsPackage()
    {
        // Arrange
        await _context.Packages.AddAsync(
            new Package { Id = 1, TrackingNumber = "T001", StatusId = 1, UserId = "user1" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAsync(1);

        // Assert
        result.WasSuccess.Should().BeTrue();
        result.Result!.TrackingNumber.Should().Be("T001");
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ReturnsFailure()
    {
        // Act
        var result = await _repository.GetAsync(999);

        // Assert
        result.WasSuccess.Should().BeFalse();
        result.Message.Should().Contain("found");
    }

    [Fact]
    public async Task AddAsync_ValidPackage_ReturnsSuccess()
    {
        // Arrange
        var package = new Package { TrackingNumber = "T001", StatusId = 1, UserId = "user1" };

        // Act
        var result = await _repository.AddAsync(package);

        // Assert
        result.WasSuccess.Should().BeTrue();
        result.Result!.Id.Should().BeGreaterThan(0);
        _context.Packages.Should().HaveCount(1);
    }

    public void Dispose() => _context.Dispose();
}
```

## Unit Test for Controller with Mocks

```csharp
public class PackageControllerTests
{
    private readonly Mock<IPackageUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<PackageController>> _mockLogger;
    private readonly PackageController _controller;

    public PackageControllerTests()
    {
        _mockUnitOfWork = new Mock<IPackageUnitOfWork>();
        _mockLogger = new Mock<ILogger<PackageController>>();
        _controller = new PackageController(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithData()
    {
        // Arrange
        var packages = new List<Package>
        {
            new() { Id = 1, TrackingNumber = "T001" },
            new() { Id = 2, TrackingNumber = "T002" }
        };
        _mockUnitOfWork.Setup(u => u.GetAllAsync())
            .ReturnsAsync(ActionResponse<IEnumerable<Package>>.Success(packages));

        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeAssignableTo<IEnumerable<Package>>().Subject;
        data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOk()
    {
        // Arrange
        var package = new Package { Id = 1, TrackingNumber = "T001" };
        _mockUnitOfWork.Setup(u => u.GetAsync(1))
            .ReturnsAsync(ActionResponse<Package>.Success(package));

        // Act
        var result = await _controller.GetAsync(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value.Should().BeOfType<Package>().Subject;
        data.TrackingNumber.Should().Be("T001");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.GetAsync(999))
            .ReturnsAsync(ActionResponse<Package>.Failure("Not found"));

        // Act
        var result = await _controller.GetAsync(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Post_ValidDto_ReturnsOk()
    {
        // Arrange
        var dto = new PackageCreateDto { TrackingNumber = "T001", StatusId = 1 };
        var package = new Package { Id = 1, TrackingNumber = "T001", StatusId = 1 };
        _mockUnitOfWork.Setup(u => u.AddAsync(It.IsAny<Package>()))
            .ReturnsAsync(ActionResponse<Package>.Success(package));

        // Act
        var result = await _controller.PostAsync(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
```

## Integration Test for API

```csharp
public class PackageApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PackageApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DataContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<DataContext>(opt =>
                    opt.UseInMemoryDatabase("TestDb"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/package");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithAuth_ReturnsOk()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/package");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task AuthenticateAsync()
    {
        var login = new LoginDto { Email = "test@test.com", Password = "Test123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);
        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.Token);
    }
}
```

## DTO Validation Test

```csharp
public class PackageCreateDtoValidationTests
{
    [Fact]
    public void Validate_ValidDto_NoErrors()
    {
        var dto = new PackageCreateDto { TrackingNumber = "T00001", StatusId = 1 };
        var results = ValidateModel(dto);
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyTrackingNumber_ReturnsError()
    {
        var dto = new PackageCreateDto { TrackingNumber = "", StatusId = 1 };
        var results = ValidateModel(dto);
        results.Should().ContainSingle().Which.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void Validate_TrackingNumberTooShort_ReturnsError()
    {
        var dto = new PackageCreateDto { TrackingNumber = "T1", StatusId = 1 };
        var results = ValidateModel(dto);
        results.Should().ContainSingle().Which.ErrorMessage.Should().Contain("5 and 50");
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }
}
```

## Unbreakable Rules

1. **ALWAYS** follow AAA pattern (Arrange, Act, Assert)
2. **ALWAYS** use descriptive names: Method_Scenario_ExpectedBehavior
3. **ALWAYS** isolate tests (no dependencies between them)
4. **ALWAYS** use mocks for external dependencies
5. **ALWAYS** test positive and negative cases
6. **ALWAYS** test DTO validations
7. **ALWAYS** test authorization in API tests
8. **NEVER** use production data in tests
9. **ALWAYS** clean state between tests
10. **ALWAYS** aim for >80% code coverage

## Testing Commands

```bash
# Run all tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific tests
dotnet test --filter "FullyQualifiedName~PackageRepositoryTests"

# Verbose
dotnet test -v detailed
```

## Test Naming Convention

```
{Method}_{Scenario}_{ExpectedResult}

GetAsync_WithValidId_ReturnsPackage
GetAsync_WithInvalidId_ReturnsFailure
AddAsync_ValidPackage_ReturnsSuccess
Post_WithoutAuth_ReturnsUnauthorized
```
