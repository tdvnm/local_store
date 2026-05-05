namespace SocietyCommerce.Contracts.Requests;

public record RegisterBuyerRequest(string Phone, string Name, string FlatNumber, string? Block);
