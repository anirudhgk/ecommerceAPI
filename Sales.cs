// Temporary model
// Model: OrderItem
public class OrderItem
{
    public int product_id { get; set; }
    public int quantity { get; set; }
    public decimal price { get; set; }
}

// Request Model: SalesOrderRequest
public class SalesOrderRequest
{
    public int customer_id { get; set; }
    public decimal total_amount { get; set; }
    public string shipping_address { get; set; }
    public string shipping_city { get; set; }
    public string shipping_state { get; set; }
    public string shipping_country { get; set; }
    public string shipping_postal_code { get; set; }
    public List<OrderItem> Order_Items { get; set; }
    public int order_id { get; set; }
}