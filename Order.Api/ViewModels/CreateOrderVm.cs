namespace Order.Api.ViewModels
{
    public class CreateOrderVm
    {
        public int BuyerId { get; set; }
        public List<OrderItemVm> OrderItems { get; set; }
    }
}
