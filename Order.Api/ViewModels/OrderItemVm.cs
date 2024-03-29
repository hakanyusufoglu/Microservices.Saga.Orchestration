namespace Order.Api.ViewModels
{
    public class OrderItemVm
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; } //Normalde veri tabanında olur. Ancak projenin orchestration mantığı için Price'ı kullanıcıdan alıyoruz.
    }
}
