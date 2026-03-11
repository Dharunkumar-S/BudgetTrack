# 🧪 NUnit & Moq (C#) — Complete Reference Guide

---

## Table of Contents

1. [Getting Started](#1-getting-started)
2. [Fundamentals of Unit Testing](#2-fundamentals-of-unit-testing)
3. [Core Unit Testing Techniques](#3-core-unit-testing-techniques)
4. [Breaking External Dependencies](#4-breaking-external-dependencies)

---

## 1. Getting Started

### 1.1 What is Automated Testing?

Automated testing is the practice of writing **code that tests your code**. Instead of manually running an app and clicking through it, you write test scripts that verify your application behaves correctly — automatically, every time.

- Tests run on every build or commit
- Catches bugs before they reach production
- Gives developers confidence to refactor and add features

---

### 1.2 Benefits of Automated Testing

| Benefit                  | Description                                                   |
|--------------------------|---------------------------------------------------------------|
| **Early bug detection**  | Catch issues immediately during development                   |
| **Regression safety**    | Ensure existing features don't break when you add new ones    |
| **Living documentation** | Tests describe *what* the code is supposed to do              |
| **Faster releases**      | Automated checks replace slow manual QA cycles                |
| **Better design**        | Writing testable code encourages cleaner architecture         |
| **Confidence to refactor**| Change internals freely without fear of breaking behavior    |

---

### 1.3 Types of Tests

| Type                | Scope                              | Speed    | Example                              |
|---------------------|------------------------------------|----------|--------------------------------------|
| **Unit Test**       | Single class or method             | Very fast| Test a `Calculate()` method in isolation |
| **Integration Test**| Multiple components together       | Medium   | Test a service + database interaction|
| **End-to-End (E2E)**| Full application flow              | Slow     | Simulate a user logging in via browser|

---

### 1.4 Test Pyramid

The test pyramid guides **how many tests of each type** you should write:

```
        /\
       /  \
      / E2E \        ← Few (slow, expensive)
     /--------\
    /Integration\    ← Some
   /------------\
  /  Unit Tests  \   ← Many (fast, cheap, isolated)
 /________________\
```

> 💡 Write **many unit tests**, a reasonable number of integration tests, and only a **few E2E tests**.

---

### 1.5 Popular Testing Frameworks (C#)

| Framework   | Purpose              | Notes                              |
|-------------|----------------------|------------------------------------|
| **NUnit**   | Unit testing         | Most popular, attribute-based      |
| **xUnit**   | Unit testing         | Modern, used by ASP.NET Core team  |
| **MSTest**  | Unit testing         | Microsoft's built-in framework     |
| **Moq**     | Mocking dependencies | Most widely used mocking library   |
| **FluentAssertions** | Assertions  | Readable, fluent assertion syntax  |

---

### 1.6 Using NUnit in Visual Studio

**Step 1: Install NuGet packages**
```bash
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
```

**Step 2: Write a test class**
```csharp
using NUnit.Framework;

[TestFixture]
public class CalculatorTests
{
    [Test]
    public void Add_TwoPositiveNumbers_ReturnsCorrectSum()
    {
        var calc = new Calculator();
        var result = calc.Add(2, 3);
        Assert.That(result, Is.EqualTo(5));
    }
}
```

**Step 3: Run tests**
- Visual Studio: `Test > Run All Tests` or use **Test Explorer**
- CLI: `dotnet test`

---

### 1.7 What is Test-Driven Development (TDD)?

TDD is a development approach where you write the **test before the production code**.

**The Red-Green-Refactor Cycle:**

```
1. RED    → Write a failing test (feature doesn't exist yet)
2. GREEN  → Write the minimum code to make the test pass
3. REFACTOR → Clean up the code, keep tests green
```

```csharp
// Step 1 - RED: Write test first (Calculator doesn't exist yet)
[Test]
public void Add_ReturnsSumOfTwoNumbers()
{
    var calc = new Calculator();
    Assert.That(calc.Add(2, 3), Is.EqualTo(5)); // Fails
}

// Step 2 - GREEN: Implement just enough
public class Calculator
{
    public int Add(int a, int b) => a + b;
}

// Step 3 - REFACTOR: Improve code quality if needed
```

> 💡 TDD forces you to think about **design and requirements** before writing implementation.

---

## 2. Fundamentals of Unit Testing

### 2.1 Characteristics of Good Unit Tests (FIRST)

| Principle   | Meaning                                                        |
|-------------|----------------------------------------------------------------|
| **Fast**    | Run in milliseconds — no DB, no network, no file I/O          |
| **Isolated**| Each test is independent; no shared state between tests        |
| **Repeatable** | Same result every time, regardless of environment           |
| **Self-validating** | Pass or fail automatically — no manual checking       |
| **Timely**  | Written at the same time as (or before) the production code    |

---

### 2.2 What to Test and What NOT to Test

**✅ Test:**
- Business logic and calculations
- Conditional branches (`if/else`, `switch`)
- Edge cases and boundary values
- Exception handling
- Return values and state changes

**❌ Don't Test:**
- Third-party library internals (trust them)
- Trivial code (auto-generated getters/setters)
- Private methods directly (test via public API)
- Infrastructure/framework code (e.g., EF Core itself)

---

### 2.3 Naming and Organizing Tests

**Naming Convention:** `MethodName_Scenario_ExpectedResult`

```csharp
// ✅ Good names
public void Divide_ByZero_ThrowsDivideByZeroException() { }
public void GetDiscount_PremiumCustomer_Returns20Percent() { }
public void IsValid_EmptyEmail_ReturnsFalse() { }

// ❌ Bad names
public void Test1() { }
public void DivideTest() { }
```

**Organizing Tests:**
```
Solution/
├── MyApp/
│   └── OrderService.cs
└── MyApp.Tests/
    └── OrderServiceTests.cs   ← Mirror your production structure
```

---

### 2.4 Black-box Testing

Test the **behavior** (inputs and outputs) without knowing the internal implementation.

```csharp
// We don't care HOW GetDiscount works internally
// We only care that it returns 20 for a premium customer
[Test]
public void GetDiscount_PremiumCustomer_Returns20()
{
    var service = new OrderService();
    var discount = service.GetDiscount("premium");
    Assert.That(discount, Is.EqualTo(20));
}
```

> 💡 Black-box tests are resilient — if you refactor internals, the tests still pass.

---

### 2.5 Set Up and Tear Down

Avoid repeating setup code across tests using NUnit lifecycle attributes:

```csharp
[TestFixture]
public class OrderServiceTests
{
    private OrderService _service;

    [SetUp]                          // Runs before EACH test
    public void SetUp()
    {
        _service = new OrderService();
    }

    [TearDown]                       // Runs after EACH test
    public void TearDown()
    {
        _service.Dispose();
    }

    [OneTimeSetUp]                   // Runs once before ALL tests in fixture
    public void Init() { }

    [OneTimeTearDown]                // Runs once after ALL tests in fixture
    public void Cleanup() { }

    [Test]
    public void SomeTest()
    {
        // _service is freshly created for every test
    }
}
```

---

### 2.6 Parameterized Tests

Run the same test logic with **multiple sets of inputs** using `[TestCase]` or `[TestCaseSource]`:

```csharp
[TestFixture]
public class MathTests
{
    // TestCase — inline parameters
    [TestCase(2, 3, 5)]
    [TestCase(0, 0, 0)]
    [TestCase(-1, 1, 0)]
    public void Add_VariousInputs_ReturnsExpectedSum(int a, int b, int expected)
    {
        var calc = new Calculator();
        Assert.That(calc.Add(a, b), Is.EqualTo(expected));
    }

    // TestCaseSource — data from a method or property
    private static IEnumerable<TestCaseData> DiscountCases()
    {
        yield return new TestCaseData("premium", 20);
        yield return new TestCaseData("standard", 10);
        yield return new TestCaseData("guest", 0);
    }

    [TestCaseSource(nameof(DiscountCases))]
    public void GetDiscount_ReturnsCorrectValue(string type, int expected)
    {
        var svc = new OrderService();
        Assert.That(svc.GetDiscount(type), Is.EqualTo(expected));
    }
}
```

---

### 2.7 Ignoring Tests

Temporarily skip a test without deleting it:

```csharp
[Test]
[Ignore("Bug #123 - fix pending")]
public void SomeTest_TemporarilySkipped()
{
    // Will not run — marked as skipped in Test Explorer
}
```

> ⚠️ Don't leave `[Ignore]` tests permanently. They hide problems. Fix or remove them.

---

### 2.8 Writing Trustworthy Tests

A trustworthy test **fails when the code is broken** and **passes when it's correct**.

```csharp
// ❌ Untrustworthy — always passes, tests nothing
[Test]
public void BadTest()
{
    var result = new Calculator().Add(2, 3);
    Assert.That(result, Is.Not.Null); // Trivial assertion
}

// ✅ Trustworthy — specific assertion that can actually fail
[Test]
public void Add_TwoNumbers_ReturnsExactSum()
{
    var result = new Calculator().Add(2, 3);
    Assert.That(result, Is.EqualTo(5));
}
```

**Rules for trustworthy tests:**
- One logical assertion per test (or one concept)
- Never write tests that always pass
- Verify your test actually FAILS before making it pass (TDD)
- Don't share mutable state between tests

---

## 3. Core Unit Testing Techniques

### 3.1 Testing Strings

```csharp
[Test]
public void GetFullName_ValidInput_ReturnsFormattedName()
{
    var user = new User { FirstName = "John", LastName = "Doe" };

    var result = user.GetFullName();

    Assert.That(result, Is.EqualTo("John Doe"));
    Assert.That(result, Does.StartWith("John"));
    Assert.That(result, Does.EndWith("Doe"));
    Assert.That(result, Does.Contain("Doe"));
    Assert.That(result, Is.Not.Null.Or.Empty);

    // Case-insensitive
    Assert.That(result, Is.EqualTo("john doe").IgnoreCase);
}
```

---

### 3.2 Testing Arrays and Collections

```csharp
[Test]
public void GetTopProducts_Returns3Products_SortedByPrice()
{
    var service = new ProductService();

    var result = service.GetTopProducts();

    Assert.That(result, Is.Not.Null);
    Assert.That(result, Has.Count.EqualTo(3));
    Assert.That(result, Does.Contain("Laptop"));
    Assert.That(result, Is.Ordered.Descending);
    Assert.That(result, Is.EquivalentTo(new[] { "Laptop", "Phone", "Tablet" }));
    Assert.That(result, Has.All.Not.Null);
}
```

---

### 3.3 Testing Return Type of Methods

```csharp
[Test]
public void CreateOrder_ValidInput_ReturnsOrderObject()
{
    var service = new OrderService();

    var result = service.CreateOrder(customerId: 1, amount: 99.99m);

    // Type check
    Assert.That(result, Is.TypeOf<Order>());
    Assert.That(result, Is.InstanceOf<IOrder>()); // Accepts subclasses too

    // Property assertions
    Assert.That(result.CustomerId, Is.EqualTo(1));
    Assert.That(result.Amount, Is.EqualTo(99.99m));
    Assert.That(result.Status, Is.EqualTo(OrderStatus.Pending));
}
```

---

### 3.4 Testing Void Methods

Void methods don't return a value — test the **state change** they cause:

```csharp
[Test]
public void AddItem_ValidItem_IncreasesCartCount()
{
    var cart = new ShoppingCart();

    cart.AddItem(new Item { Name = "Book", Price = 15m });

    // Assert the state changed
    Assert.That(cart.ItemCount, Is.EqualTo(1));
    Assert.That(cart.Total, Is.EqualTo(15m));
}

[Test]
public void ClearCart_NonEmptyCart_RemovesAllItems()
{
    var cart = new ShoppingCart();
    cart.AddItem(new Item { Name = "Book" });

    cart.Clear();

    Assert.That(cart.ItemCount, Is.EqualTo(0));
}
```

---

### 3.5 Testing Methods that Throw Exceptions

```csharp
[Test]
public void Divide_ByZero_ThrowsDivideByZeroException()
{
    var calc = new Calculator();

    // Using Assert.Throws
    Assert.Throws<DivideByZeroException>(() => calc.Divide(10, 0));
}

[Test]
public void CreateUser_NullEmail_ThrowsArgumentException()
{
    var service = new UserService();

    var ex = Assert.Throws<ArgumentException>(() => service.CreateUser(null));

    // Also verify the exception message
    Assert.That(ex.Message, Does.Contain("email"));
}

[Test]
public void ProcessOrder_InvalidAmount_ThrowsArgumentOutOfRange()
{
    var service = new OrderService();

    // Async version
    Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        async () => await service.ProcessOrderAsync(-1));
}
```

---

### 3.6 Testing Private Methods

> 💡 **Don't test private methods directly.** Private methods are implementation details.  
> Test them **indirectly through the public API** that calls them.

```csharp
// ❌ Don't do this — tests internal details
var method = typeof(OrderService)
    .GetMethod("CalculateTax", BindingFlags.NonPublic | BindingFlags.Instance);

// ✅ Do this — test the public method that uses CalculateTax internally
[Test]
public void PlaceOrder_IncludesTaxInTotal()
{
    var service = new OrderService();
    var order = service.PlaceOrder(amount: 100m); // CalculateTax called internally
    Assert.That(order.Total, Is.EqualTo(110m));   // 10% tax
}
```

If a private method is complex enough to need direct testing, consider moving it to its own class.

---

### 3.7 Code Coverage

**Code coverage** measures what percentage of your production code is executed by your tests.

```
Lines Covered / Total Lines × 100 = Coverage %
```

**Measuring coverage in Visual Studio:**
- `Test > Analyze Code Coverage for All Tests`

**Via CLI:**
```bash
dotnet add package coverlet.collector
dotnet test --collect:"XPlat Code Coverage"
```

| Coverage Level | Meaning                                |
|----------------|----------------------------------------|
| < 60%          | Risky — major gaps in testing          |
| 60–80%         | Acceptable for many projects           |
| 80–90%         | Good coverage                          |
| > 90%          | High coverage (diminishing returns)    |

> ⚠️ 100% coverage doesn't mean 0 bugs. Coverage shows *what was run*, not *what was verified correctly*.

---

## 4. Breaking External Dependencies

### 4.1 Loosely-coupled and Testable Code

**Tightly-coupled code** (hard to test):
```csharp
public class OrderService
{
    public void PlaceOrder(Order order)
    {
        var db = new SqlDatabase();        // ❌ creates dependency directly
        var emailer = new EmailService();  // ❌ hard to replace in tests
        db.Save(order);
        emailer.Send(order.CustomerEmail);
    }
}
```

**Loosely-coupled code** (testable):
```csharp
public class OrderService
{
    private readonly IDatabase _db;
    private readonly IEmailService _emailer;

    public OrderService(IDatabase db, IEmailService emailer)
    {
        _db = db;          // ✅ depends on abstractions
        _emailer = emailer;
    }
}
```

---

### 4.2 Refactoring Towards a Loosely-coupled Design

Steps to make tightly-coupled code testable:

1. **Extract an interface** from the dependency
2. **Inject the dependency** instead of instantiating it
3. **Replace real implementations** with fakes/mocks in tests

```csharp
// Step 1: Extract interface
public interface IEmailService
{
    void Send(string to, string body);
}

// Step 2: Real implementation (used in production)
public class EmailService : IEmailService
{
    public void Send(string to, string body) { /* SMTP logic */ }
}

// Step 3: Use interface in dependent class
public class OrderService
{
    private readonly IEmailService _emailService;
    public OrderService(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

---

### 4.3 Dependency Injection via Method Parameters

Pass the dependency directly into the method that needs it:

```csharp
public class OrderProcessor
{
    public void Process(Order order, IEmailService emailService)
    {
        // emailService injected per call
        emailService.Send(order.Email, "Your order is placed!");
    }
}

// In test:
[Test]
public void Process_ValidOrder_SendsEmail()
{
    var fakeEmail = new FakeEmailService();
    var processor = new OrderProcessor();
    processor.Process(new Order(), fakeEmail);
    Assert.That(fakeEmail.SentCount, Is.EqualTo(1));
}
```

**Best for:** Utility/stateless methods where dependency varies per call.

---

### 4.4 Dependency Injection via Properties

Set the dependency through a public property (optional dependency):

```csharp
public class OrderService
{
    public ILogger Logger { get; set; } = new NullLogger(); // default

    public void PlaceOrder(Order order)
    {
        Logger.Log("Order placed");
    }
}

// In test:
[Test]
public void PlaceOrder_LogsAction()
{
    var fakeLogger = new FakeLogger();
    var service = new OrderService { Logger = fakeLogger };
    service.PlaceOrder(new Order());
    Assert.That(fakeLogger.LastMessage, Does.Contain("Order placed"));
}
```

**Best for:** Optional dependencies with sensible defaults.

---

### 4.5 Dependency Injection via Constructor

The most common and recommended approach — inject all required dependencies through the constructor:

```csharp
public class OrderService
{
    private readonly IDatabase _db;
    private readonly IEmailService _emailService;

    public OrderService(IDatabase db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public void PlaceOrder(Order order)
    {
        _db.Save(order);
        _emailService.Send(order.Email, "Confirmed!");
    }
}

// In test:
[Test]
public void PlaceOrder_SavesOrderToDatabase()
{
    var fakeDb = new FakeDatabase();
    var fakeEmail = new FakeEmailService();
    var service = new OrderService(fakeDb, fakeEmail);

    service.PlaceOrder(new Order { Id = 1 });

    Assert.That(fakeDb.SavedOrders, Has.Count.EqualTo(1));
}
```

**Best for:** Required dependencies — makes dependencies explicit and mandatory.

---

### 4.6 Dependency Injection Frameworks

DI frameworks automatically create and wire up dependencies for you.

**In ASP.NET Core (built-in DI):**
```csharp
// Program.cs / Startup.cs
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDatabase, SqlDatabase>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Framework auto-injects into controller constructors:
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService; // Auto-injected
    }
}
```

**Popular third-party DI frameworks:**
- **Autofac** — feature-rich, widely used
- **Ninject** — lightweight
- **Castle Windsor** — enterprise-grade

---

### 4.7 Mocking Frameworks

Instead of writing manual fake classes, **mocking frameworks** generate fake implementations automatically.

**Install Moq:**
```bash
dotnet add package Moq
```

| Manual Fake                | Moq Mock                        |
|----------------------------|---------------------------------|
| Write a class by hand      | Generate at runtime             |
| More setup code            | Concise, readable syntax        |
| Hard to verify interactions| Built-in `Verify()` support     |

---

### 4.8 Creating Mock Objects Using Moq

```csharp
using Moq;
using NUnit.Framework;

[TestFixture]
public class OrderServiceTests
{
    [Test]
    public void PlaceOrder_ValidOrder_SavesAndSendsEmail()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        var mockEmail = new Mock<IEmailService>();

        // Setup mock behavior
        mockDb.Setup(db => db.Save(It.IsAny<Order>())).Returns(true);
        mockEmail.Setup(e => e.Send(It.IsAny<string>(), It.IsAny<string>()));

        var service = new OrderService(mockDb.Object, mockEmail.Object);

        // Act
        service.PlaceOrder(new Order { Id = 1, Email = "test@test.com" });

        // Assert — verify interactions
        mockDb.Verify(db => db.Save(It.IsAny<Order>()), Times.Once);
        mockEmail.Verify(e => e.Send("test@test.com", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void GetOrder_ExistingId_ReturnsOrder()
    {
        var mockDb = new Mock<IDatabase>();

        // Setup return value
        mockDb.Setup(db => db.GetById(1))
              .Returns(new Order { Id = 1, Amount = 99m });

        var service = new OrderService(mockDb.Object, Mock.Of<IEmailService>());

        var result = service.GetOrder(1);

        Assert.That(result.Amount, Is.EqualTo(99m));
    }
}
```

**Common Moq setups:**

```csharp
// Return a value
mock.Setup(x => x.GetUser(1)).Returns(new User());

// Throw an exception
mock.Setup(x => x.Save(null)).Throws<ArgumentNullException>();

// Match any argument
mock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>()));

// Match specific condition
mock.Setup(x => x.GetById(It.Is<int>(id => id > 0))).Returns(new Order());

// Async methods
mock.Setup(x => x.SaveAsync(It.IsAny<Order>())).ReturnsAsync(true);

// Verify call count
mock.Verify(x => x.Save(It.IsAny<Order>()), Times.Once);
mock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
mock.Verify(x => x.Log(It.IsAny<string>()), Times.Exactly(3));
```

---

### 4.9 State-based vs. Interaction Testing

#### State-based Testing
Verify the **state of an object** after an operation:

```csharp
[Test]
public void AddToCart_IncreasesItemCount()
{
    var cart = new ShoppingCart();

    cart.Add(new Item("Book", 15m));

    // Assert STATE
    Assert.That(cart.Count, Is.EqualTo(1));
    Assert.That(cart.Total, Is.EqualTo(15m));
}
```

#### Interaction Testing
Verify that the correct **methods were called** on a dependency:

```csharp
[Test]
public void PlaceOrder_CallsSaveOnDatabase()
{
    var mockDb = new Mock<IDatabase>();
    var service = new OrderService(mockDb.Object, Mock.Of<IEmailService>());

    service.PlaceOrder(new Order());

    // Assert INTERACTION
    mockDb.Verify(db => db.Save(It.IsAny<Order>()), Times.Once);
}
```

> 💡 **Prefer state-based testing** where possible — it tests *what* happened, not *how*. Use interaction testing when the key behavior *is* calling an external dependency (e.g., sending an email, logging).

---

### 4.10 Testing the Interaction Between Two Objects

Test that **Object A correctly calls Object B** using a mock for B:

```csharp
public interface INotificationService
{
    void Notify(string userId, string message);
}

public class OrderService
{
    private readonly INotificationService _notifier;
    public OrderService(INotificationService notifier) => _notifier = notifier;

    public void ShipOrder(Order order)
    {
        // ... shipping logic ...
        _notifier.Notify(order.UserId, $"Order {order.Id} has shipped!");
    }
}

[TestFixture]
public class OrderServiceInteractionTests
{
    [Test]
    public void ShipOrder_NotifiesCustomerWithCorrectMessage()
    {
        // Arrange
        var mockNotifier = new Mock<INotificationService>();
        var service = new OrderService(mockNotifier.Object);
        var order = new Order { Id = 42, UserId = "user-1" };

        // Act
        service.ShipOrder(order);

        // Assert interaction — was Notify called with the right args?
        mockNotifier.Verify(
            n => n.Notify("user-1", It.Is<string>(msg => msg.Contains("42"))),
            Times.Once
        );
    }

    [Test]
    public void ShipOrder_WhenNotificationFails_DoesNotThrow()
    {
        var mockNotifier = new Mock<INotificationService>();
        mockNotifier
            .Setup(n => n.Notify(It.IsAny<string>(), It.IsAny<string>()))
            .Throws<Exception>();

        var service = new OrderService(mockNotifier.Object);

        // Shipping should not crash if notification fails
        Assert.DoesNotThrow(() => service.ShipOrder(new Order { UserId = "u1" }));
    }
}
```

**Summary of when to use each:**

| Scenario                                   | Use                  |
|--------------------------------------------|----------------------|
| Verifying a computed result or state change| State-based testing  |
| Verifying an email was sent                | Interaction testing  |
| Verifying a DB save was called             | Interaction testing  |
| Verifying a return value                   | State-based testing  |
| Verifying exception is thrown              | State-based testing  |

---

*End of NUnit & Moq Complete Reference Guide*
