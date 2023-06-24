INSERT INTO Customers (name, email, address, phone, city)
VALUES ('anirudh', 'anirudh@xyz.com', '123 apt xyz', '123-456-7890', 'Charlotte');

INSERT INTO Products (name, description, price, quantity_available)
VALUES ('Pen', 'A pen that writes fast and glows in the dark', 10.99, 100),
       ('Phone', 'A smartphone made by Apple that combines a computer, iPod, digital camera and cellular phone into one device with a touchscreen interface.', 699.99, 50);

INSERT INTO Orders (customer_id, order_date, total_amount)
VALUES (1, '2023-06-23', 30.99);

INSERT INTO Order_Items (order_id, product_id, quantity, price)
VALUES (1, 1, 2, 21.98),
       (1, 2, 1, 699.99);

INSERT INTO Shipping (order_id, address, city, state, country, postal_code)
VALUES (1, '456 abc street', 'Charlotte', 'NC', 'USA', '28262');