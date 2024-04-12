namespace Customers.Contracts;

public record CustomerCreated
(
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail
);
