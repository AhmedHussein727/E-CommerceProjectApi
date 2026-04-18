namespace ECommerce.Domain.Entities.OrderModule
{
    public class ProductItemOrdered
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;

        public string PictureUrl { get; set; } = default!;
    }
}
