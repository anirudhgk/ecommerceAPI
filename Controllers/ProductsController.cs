using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CustomersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET Customers
        [HttpGet("customers")]
        public ActionResult<IEnumerable<Customer>> Get()
        {
            var customers = new List<Customer>();
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var query = "SELECT * FROM Customers";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var customer = new Customer
                            {
                                customer_id = reader.GetInt32("customer_id"),
                                name = reader.GetString("name"),
                                email = reader.GetString("email"),
                                address = reader.GetString("address"),
                                phone = reader.GetString("phone"),
                                city = reader.GetString("city")
                            };
                            customers.Add(customer);
                        }
                    }
                }
            }
            return Ok(customers);
        }

        // GET customer from id
        [HttpGet("customer/{id}")]
        public ActionResult<Customer> GetById(int id)
        {
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var query = "SELECT * FROM customers WHERE customer_id = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var customer = new Customer
                            {
                                customer_id = reader.GetInt32("customer_id"),
                                name = reader.GetString("name"),
                                email = reader.GetString("email"),
                                address = reader.GetString("address"),
                                phone = reader.GetString("phone"),
                                city = reader.GetString("city")
                            };
                            return Ok(customer);
                        }
                    }
                }
            }
            return NotFound();
        }

        // GET Products
        [HttpGet("products")]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            var products = new List<Product>();
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var query = "SELECT * FROM Products";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                product_id = reader.GetInt32("product_id"),
                                name = reader.GetString("name"),
                                description = reader.GetString("description"),
                                price = reader.GetDecimal("price"),
                                quantity_available = reader.GetInt32("quantity_available")
                            };
                            products.Add(product);
                        }
                    }
                }
            }
            return Ok(products);
        }

        // POST Sales
        [HttpPost("sales-orders")]
        public ActionResult CreateSalesOrder(SalesOrderRequest salesOrder)
        {
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Save the sales order to the Orders table
                var orderQuery = "INSERT INTO Orders (customer_id, order_date, total_amount) VALUES (@customer_id, @order_date, @total_amount); SELECT LAST_INSERT_ID();";
                using (var command = new MySqlCommand(orderQuery, connection))
                {
                    command.Parameters.AddWithValue("@customer_id", salesOrder.customer_id);
                    command.Parameters.AddWithValue("@order_date", DateTime.UtcNow.Date);
                    command.Parameters.AddWithValue("@total_amount", salesOrder.total_amount);
                    var orderId = command.ExecuteScalar();
                    salesOrder.order_id = Convert.ToInt32(orderId);
                }

                // Save the order items to the Order_Items table
                foreach (var orderItem in salesOrder.Order_Items)
                {
                    var orderItemQuery = "INSERT INTO Order_Items (order_id, product_id, quantity, price) VALUES (@order_id, @product_id, @quantity, @price)";
                    using (var itemCommand = new MySqlCommand(orderItemQuery, connection))
                    {
                        itemCommand.Parameters.AddWithValue("@order_id", salesOrder.order_id);
                        itemCommand.Parameters.AddWithValue("@product_id", orderItem.product_id);
                        itemCommand.Parameters.AddWithValue("@quantity", orderItem.quantity);
                        itemCommand.Parameters.AddWithValue("@price", orderItem.price);
                        itemCommand.ExecuteNonQuery();
                    }
                }

                // Save the shipping information to the Shipping table
                var shippingQuery = "INSERT INTO Shipping (order_id, address, city, state, country, postal_code) VALUES (@order_id, @address, @city, @state, @country, @postalCode)";
                using (var shippingCommand = new MySqlCommand(shippingQuery, connection))
                {
                    shippingCommand.Parameters.AddWithValue("@order_id", salesOrder.order_id);
                    shippingCommand.Parameters.AddWithValue("@address", salesOrder.shipping_address);
                    shippingCommand.Parameters.AddWithValue("@city", salesOrder.shipping_city);
                    shippingCommand.Parameters.AddWithValue("@state", salesOrder.shipping_state);
                    shippingCommand.Parameters.AddWithValue("@country", salesOrder.shipping_country);
                    shippingCommand.Parameters.AddWithValue("@postalCode", salesOrder.shipping_postal_code);
                    shippingCommand.ExecuteNonQuery();
                }

                // Return a success response
                return Ok("Record inserted successfully");
            }
        }

        // GET customer orders by customer id
        [HttpGet("customer/{id}/orders")]
        public ActionResult<IEnumerable<Order>> GetCustomerOrders(int id)
        {
            var orders = new List<Order>();
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                var query = "SELECT * FROM Orders WHERE customer_id = @id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order
                            {
                                order_id = reader.GetInt32("order_id"),
                                customer_id = reader.GetInt32("customer_id"),
                                order_date = reader.GetDateTime("order_date"),
                                total_amount = reader.GetDecimal("total_amount")
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return Ok(orders);
        }

        // GET item info by item id (product info from product id)
        [HttpGet("products/{product_id}")]
        public ActionResult<Product> GetItemInfo(int product_id)
        {
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var query = "SELECT * FROM Products WHERE product_id = @product_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@product_id", product_id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var product = new Product
                            {
                                product_id = reader.GetInt32("product_id"),
                                name = reader.GetString("name"),
                                description = reader.GetString("description"),
                                price = reader.GetDecimal("price"),
                                quantity_available = reader.GetInt32("quantity_available")
                            };

                            return Ok(product);
                        }
                    }
                }
            }
            return NotFound();
        }

        // POST Products
        [HttpPost("products")]
        public ActionResult<Product> CreateProduct([FromBody] Product product)
        {
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var query = "INSERT INTO Products (name, description, price, quantity_available) VALUES (@name, @description, @price, @quantity)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", product.name);
                    command.Parameters.AddWithValue("@description", product.description);
                    command.Parameters.AddWithValue("@price", product.price);
                    command.Parameters.AddWithValue("@quantity", product.quantity_available);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // Retrieve the newly created product's ID
                        long product_id = command.LastInsertedId;

                        product.product_id = (int)product_id;
                        return Ok(product);
                    }
                }
            }
            return BadRequest();
        }

        // UPDATE an existing project
        [HttpPut("products/{product_id}")]
        public ActionResult<Product> UpdateProduct(int product_id, [FromBody] Product product)
        {
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var query = "UPDATE Products SET name = @name, description = @description, price = @price, quantity_available = @quantity WHERE product_id = @product_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", product.name);
                    command.Parameters.AddWithValue("@description", product.description);
                    command.Parameters.AddWithValue("@price", product.price);
                    command.Parameters.AddWithValue("@quantity", product.quantity_available);
                    command.Parameters.AddWithValue("@product_id", product_id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        product.product_id = product_id;
                        return Ok(product);
                    }
                }
            }
            return NotFound();
        }

        // DELETE Product
        [HttpDelete("products/{product_id}")]
        public ActionResult DeleteProduct(int product_id)
        {
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var query = "DELETE FROM Products WHERE product_id = @product_id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@product_id", product_id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Ok("Product deleted");
                    }
                }
            }
            return NotFound();
        }

        // GET customer name who ordered a particular product to a particular city
        [HttpGet("customername")]
        public ActionResult<IEnumerable<string>> GetCustomerByProductAndCity(string product_name, string city)
        {
            var customer_names = new List<string>();
            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var query = @"SELECT c.name 
                      FROM Customers c
                      JOIN Orders o ON c.customer_id = o.customer_id
                      JOIN Order_Items oi ON o.order_id = oi.order_id
                      JOIN Products p ON oi.product_id = p.product_id
                      JOIN Shipping s ON o.order_id = s.order_id
                      WHERE p.name = @product_name AND s.city = @city";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@product_name", product_name);
                    command.Parameters.AddWithValue("@city", city);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var customer_name = reader.GetString("name");
                            customer_names.Add(customer_name);
                        }
                    }
                }
            }
            return Ok(customer_names);
        }
    }
}
