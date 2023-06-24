public class Order
{
    public int order_id { get; set; }
    public int customer_id { get; set; }
    public DateTime order_date { get; set; }
    public decimal total_amount { get; set; }
}