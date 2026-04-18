namespace ECommerce.Shared.DTOs.OrderDTOs
{
    public record OrderItemDTO
    {
        public string ProductName { get; init; } = default!;

        public string PictureUrl { get; init; } = default!;

        public decimal Price { get; init; }

        public int Quantity { get; init; }
    }
}
