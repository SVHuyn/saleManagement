using saleManagement.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;



namespace saleManagement
{

    public partial class SaleManagement: Form
    {
        SqlConnection connection;
        DataTable orderDetailsTable;
        string connectionString = "Server=DESKTOP-MJ76D6E; Database=SalesManagement3; Integrated Security=True";
        DataTable dtOrderDetailManage;

        private static int saleDetailIdCounter = 1;

        public SaleManagement(string username)
        {
            InitializeComponent();
            connection = new SqlConnection("Server = DESKTOP-MJ76D6E; Database = SalesManagement3; Integrated Security=True");
        }

        private void SaleManagement_Load(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Successful connection!", "Result", MessageBoxButtons.OK, MessageBoxIcon.None);
            //
            FillData();
            //
            FillDataProduct();

            //
            LoadProducts();
            LoadCustomers();
            InitializeOrderDetailsTable();
            dtpOrderDate.Value = DateTime.Now;
            lblTotalAmount.Text = "0 VND";

            LoadStatistics();

            // Manage OrderDetail
            LoadOrderDetailData();
        }

        public void FillData()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT CustomerID, CustomerName, PhoneNumber, Address, Email FROM Customer";
                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(dt);
                dgvCustomer.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customer list: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private void ClearInput()
        {
            txtCustomerID.Clear();
            txtCustomerName.Clear();
            txtPhoneNumber.Clear();
            txtAddress.Clear();
            txtEmail.Clear();

            txtProductID.Text = "";
            txtProductName.Text = "";
            txtQuantity.Text = "";
            txtCategoryID.Text = "";
            txtSupplierID.Text = "";
            txtPrice.Text = "";
            txtStockQuantity.Text = "";
            txtEmployeeID.Text = "";
        }

        //Add Customer
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"INSERT INTO Customer (CustomerID, CustomerName, PhoneNumber, Address, Email)
                                 VALUES (@CustomerID, @CustomerName, @PhoneNumber, @Address, @Email)";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", Convert.ToInt32(txtCustomerID.Text));
                    cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
                    cmd.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer added successfully!");
                    FillData();
                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding customer: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Update Customer
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerID.Text))
            {
                MessageBox.Show("Please select a customer to update!");
                return;
            }

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"UPDATE Customer SET 
                                 CustomerName = @CustomerName,
                                 PhoneNumber = @PhoneNumber,
                                 Address = @Address,
                                 Email = @Email
                                 WHERE CustomerID = @CustomerID";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", Convert.ToInt32(txtCustomerID.Text));
                    cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
                    cmd.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text);
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer updated successfully!");
                    FillData();
                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating customer: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Delete Customer
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerID.Text))
            {
                MessageBox.Show("Please select a customer to delete!");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this customer?",
                                                  "Confirmation",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "DELETE FROM Customer WHERE CustomerID = @CustomerID";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", Convert.ToInt32(txtCustomerID.Text));
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer deleted successfully!");
                    FillData();
                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting customer: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Clear Customer
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInput();
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // tránh chọn header
            {
                DataGridViewRow row = dgvCustomer.Rows[e.RowIndex];
                txtCustomerID.Text = row.Cells["CustomerID"].Value.ToString();
                txtCustomerName.Text = row.Cells["CustomerName"].Value.ToString();
                txtPhoneNumber.Text = row.Cells["PhoneNumber"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string searchQuery = @"SELECT CustomerID, CustomerName, PhoneNumber, Address, Email 
                               FROM Customer
                               WHERE CustomerName LIKE @keyword OR PhoneNumber LIKE @keyword";

                using (SqlCommand cmd = new SqlCommand(searchQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@keyword", "%" + txtSearch.Text.Trim() + "%");

                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    dgvCustomer.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when searching for customers: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Manage Product
        public void FillDataProduct()
        {
            try
            {
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    ProductRepo productrepo = new ProductRepo();
                    string query = productrepo.getAllProduct();
                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.Fill(dt);

                    dgvProduct.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading product list: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

        }

        // Add Product
        private void btnAdd1_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"INSERT INTO Product (ProductID, ProductName, Quantity, Price, CategoryID, SupplierID, StockQuantity, EmployeeID)
                               VALUES (@ProductID, @ProductName, @Quantity, @Price, @CategoryID, @SupplierID, @StockQuantity, @EmployeeID)";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", txtProductID.Text);
                    cmd.Parameters.AddWithValue("@ProductName", txtProductName.Text);
                    cmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(txtQuantity.Text));
                    cmd.Parameters.AddWithValue("@CategoryID", Convert.ToInt32(txtCategoryID.Text));
                    cmd.Parameters.AddWithValue("@SupplierID", Convert.ToInt32(txtSupplierID.Text));
                    cmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@StockQuantity", Convert.ToInt32(txtStockQuantity.Text));
                    cmd.Parameters.AddWithValue("@EmployeeID", Convert.ToInt32(txtEmployeeID.Text));

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product added successfully!");
                    FillData();
                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding product: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Update Product
        private void btnUpdate1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductID.Text))
            {
                MessageBox.Show("Please select the product you want to update!");
                return;
            }
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"UPDATE Product SET 
                               ProductName = @ProductName,
                               Quantity = @Quantity,
                               CategoryID = @CategoryID,
                               SupplierID = @SupplierID,
                               EmployeeID = @EmployeeID,
                               StockQuantity = @StockQuantity,
                               Price = @Price
                               WHERE ProductID = @ProductID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(txtProductID.Text));
                    cmd.Parameters.AddWithValue("@ProductName", txtProductName.Text);
                    cmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(txtQuantity.Text));
                    cmd.Parameters.AddWithValue("@CategoryID", Convert.ToInt32(txtCategoryID.Text));
                    cmd.Parameters.AddWithValue("@SupplierID", Convert.ToInt32(txtSupplierID.Text));
                    cmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(txtPrice.Text));
                    cmd.Parameters.AddWithValue("@StockQuantity", Convert.ToInt32(txtStockQuantity.Text));
                    cmd.Parameters.AddWithValue("@EmployeeID", Convert.ToInt32(txtEmployeeID.Text));

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product update successful!");
                    FillData();
                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating product: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Delete Product
        private void btnDelete1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductID.Text))
            {
                MessageBox.Show("Please select a product to delete!");
                return;
            }
            DialogResult result = MessageBox.Show("Are you sure you want to delete this product??",
                                              "Confirm",
                                              MessageBoxButtons.YesNo,
                                              MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "DELETE FROM Product WHERE ProductID = @ProductID";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(txtProductID.Text));
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Product deleted successfully!");
                    FillData();
                    ClearInput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting product: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Cancel Product
        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClearInput();
        }

        //dgv product
        private void dgvProduct_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProduct.Rows[e.RowIndex];

                txtProductID.Text = row.Cells["ProductID"].Value.ToString();
                txtProductName.Text = row.Cells["ProductName"].Value.ToString();
                txtQuantity.Text = row.Cells["Quantity"].Value.ToString();
                txtCategoryID.Text = row.Cells["CategoryID"].Value.ToString();
                txtSupplierID.Text = row.Cells["SupplierID"].Value.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
                txtStockQuantity.Text = row.Cells["StockQuantity"].Value.ToString();
                txtEmployeeID.Text = row.Cells["EmployeeID"].Value.ToString();

            }
        }

        // Make Order

        // Load customer list into ComboBox
        private void LoadCustomers()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT CustomerID, CustomerName FROM Customer";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cbCustomer.DataSource = dt;
                cbCustomer.DisplayMember = "CustomerName";
                cbCustomer.ValueMember = "CustomerID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading client: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Load product list into DataGridView
        private void LoadProducts()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT ProductID, ProductName, Price, StockQuantity FROM Product";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dgvProducts.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading product list: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        // Initialize DataTable for Order Details
        private void InitializeOrderDetailsTable()
        {
            orderDetailsTable = new DataTable();
            orderDetailsTable.Columns.Add("ProductID", typeof(int));
            orderDetailsTable.Columns.Add("ProductName", typeof(string));
            orderDetailsTable.Columns.Add("Price", typeof(decimal));
            orderDetailsTable.Columns.Add("Quantity", typeof(int));
            orderDetailsTable.Columns.Add("Subtotal", typeof(decimal), "Price * Quantity");

            dgvOrderDetails.DataSource = orderDetailsTable;
        }

        private void btnAddToOrder_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please select product!");
                return;
            }

            // Get selected product data
            DataGridViewRow selectedRow = dgvProducts.SelectedRows[0];
            int productId = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);
            string productName = selectedRow.Cells["ProductName"].Value.ToString();
            decimal price = Convert.ToDecimal(selectedRow.Cells["Price"].Value);
            int quantity = (int)nudQuantity.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("Please select quantity greater than 0!");
                return;
            }

            // Check inventory (if needed)
            int stockQuantity = Convert.ToInt32(selectedRow.Cells["StockQuantity"].Value);
            if (quantity > stockQuantity)
            {
                MessageBox.Show("Quantity Exceeds Stock!");
                return;
            }

            // Add product to OrderDetails (if product is already in order, increase quantity)
            DataRow[] existingRows = orderDetailsTable.Select("ProductID = " + productId);
            if (existingRows.Length > 0)
            {
                existingRows[0]["Quantity"] = Convert.ToInt32(existingRows[0]["Quantity"]) + quantity;
            }
            else
            {
                DataRow row = orderDetailsTable.NewRow();
                row["ProductID"] = productId;
                row["ProductName"] = productName;
                row["Price"] = price;
                row["Quantity"] = quantity;
                orderDetailsTable.Rows.Add(row);
            }

            UpdateTotalAmount();
        }

        // Update order total
        private void UpdateTotalAmount()
        {
            decimal total = 0;
            foreach (DataRow row in orderDetailsTable.Rows)
            {
                total += Convert.ToDecimal(row["Subtotal"]);
            }
            lblTotalAmount.Text = total.ToString("N2") + " VND";
        }

        // Place order
        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if the order has products or not
                if (orderDetailsTable.Rows.Count == 0)
                {
                    MessageBox.Show("Empty order!");
                    return;
                }

                // Check if the customer is selected
                if (cbCustomer.SelectedValue == null)
                {
                    MessageBox.Show("Please select customer!");
                    return;
                }
                if (!int.TryParse(cbCustomer.SelectedValue.ToString(), out int customerId))
                {
                    MessageBox.Show("Error: Invalid customer code!");
                    return;
                }

                DateTime orderDate = dtpOrderDate.Value;
                decimal totalAmount = 0;
                foreach (DataRow row in orderDetailsTable.Rows)
                {
                    if (!decimal.TryParse(row["Subtotal"].ToString(), out decimal subtotal))
                    {
                        MessageBox.Show("Error: Invalid Subtotal value!");
                        return;
                    }
                    totalAmount += subtotal;
                }
                // Suppose the logged in EmployeeID is 1
                int employeeId = 1;
                // Payment method (eg "Cash")
                string paymentMethod = "Cash";

                // Use a new connection to make the transaction
                using (SqlConnection conn = new SqlConnection("Server=DESKTOP-MJ76D6E; Database=SalesManagement3; Integrated Security=True"))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Insert into the [Order] table
                            string insertOrderQuery = @"INSERT INTO [Order] (OrderID, CustomerID, OrderDate, TotalAmount, EmployeeID)
                                                VALUES (@OrderID, @CustomerID, @OrderDate, @TotalAmount, @EmployeeID)";
                            int orderId = GenerateSaleDetailID(conn, transaction);
                            using (SqlCommand cmd = new SqlCommand(insertOrderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                                cmd.Parameters.AddWithValue("@OrderDate", orderDate);
                                cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                                cmd.ExecuteNonQuery();
                            }

                            // 2. Insert into Sale table (use OrderID as SaleID)
                            string insertSaleQuery = @"INSERT INTO Sale (SaleID, OrderID, SaleDate, TotalAmount, PaymentMethod, EmployeeID)
                                               VALUES (@SaleID, @OrderID, @SaleDate, @TotalAmount, @PaymentMethod, @EmployeeID)";
                            using (SqlCommand saleCmd = new SqlCommand(insertSaleQuery, conn, transaction))
                            {
                                saleCmd.Parameters.AddWithValue("@SaleID", orderId); // Sử dụng OrderID làm SaleID
                                saleCmd.Parameters.AddWithValue("@OrderID", orderId);
                                saleCmd.Parameters.AddWithValue("@SaleDate", orderDate);
                                saleCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                                saleCmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                                saleCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                                saleCmd.ExecuteNonQuery();
                            }

                            // 3.Insert each row into the SalesDetails table
                            string insertDetailQuery = @"INSERT INTO SalesDetails (SaleDetailID, SaleID, ProductID, Quantity, Subtotal)
                                                 VALUES (@SaleDetailID, @SaleID, @ProductID, @Quantity, @Subtotal)";
                            int saleDetailId = GenerateSaleDetailID(conn, transaction);
                            foreach (DataRow row in orderDetailsTable.Rows)
                            {
                                if (!int.TryParse(row["ProductID"].ToString(), out int productId) ||
                                    !int.TryParse(row["Quantity"].ToString(), out int quantity) ||
                                    !decimal.TryParse(row["Subtotal"].ToString(), out decimal rowSubtotal))
                                {
                                    MessageBox.Show("Error: Invalid product data!");
                                    transaction.Rollback();
                                    return;
                                }
                                using (SqlCommand detailCmd = new SqlCommand(insertDetailQuery, conn, transaction))
                                {
                                    detailCmd.Parameters.AddWithValue("@SaleDetailID", saleDetailId);
                                    detailCmd.Parameters.AddWithValue("@SaleID", orderId);
                                    detailCmd.Parameters.AddWithValue("@ProductID", productId);
                                    detailCmd.Parameters.AddWithValue("@Quantity", quantity);
                                    detailCmd.Parameters.AddWithValue("@Subtotal", rowSubtotal);
                                    detailCmd.ExecuteNonQuery();
                                }
                                saleDetailId++;
                            }

                            transaction.Commit();
                            MessageBox.Show("The order has been placed successfully and payment has been recorded.!");
                            orderDetailsTable.Rows.Clear();
                            UpdateTotalAmount();
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { }
                            MessageBox.Show("Error placing order: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // OrderID generator function (simple example, should be replaced with appropriate mechanism)
        private int GenerateSaleDetailID(SqlConnection conn, SqlTransaction transaction)
        {
            int id = 1;
            string query = "SELECT ISNULL(MAX(SaleDetailID), 0) + 1 FROM SalesDetails";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return id;
        }

        // Delete temporary order
        private void btnClearOrder_Click(object sender, EventArgs e)
        {
            orderDetailsTable.Rows.Clear();
            UpdateTotalAmount();
        }

        // Statistics
        private void LoadStatistics()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Calculate total number of orders
                using (SqlCommand cmdOrders = new SqlCommand("SELECT COUNT(*) FROM [Order]", conn))
                {
                    int totalOrders = (int)cmdOrders.ExecuteScalar();
                    txtTotalOrders.Text = totalOrders.ToString();
                }

                // 2. Calculate total revenue (from [Order] table)
                using (SqlCommand cmdRevenue = new SqlCommand("SELECT ISNULL(SUM(TotalAmount),0) FROM [Order]", conn))
                {
                    object result = cmdRevenue.ExecuteScalar();
                    decimal totalRevenue = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    txtTotalRevenue.Text = totalRevenue.ToString("N0") + " đ";
                }

                // 3. Get Top 5 best selling products
                // (Based on SalesDetails table; group by ProductName)

                string topProductsQuery = @"
                    SELECT TOP 5 P.ProductName, SUM(SD.Quantity) AS TotalSold
                    FROM SalesDetails SD
                    JOIN Product P ON SD.ProductID = P.ProductID
                    GROUP BY P.ProductName
                    ORDER BY TotalSold DESC";
                DataTable dtTopProducts = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(topProductsQuery, conn))
                {
                    adapter.Fill(dtTopProducts);
                }
                dgvTopProducts.DataSource = dtTopProducts;

                // 4. (Optional) If using Chart, you can plot revenue by month:
                LoadSalesChart(conn);
            }
        }

        private void LoadSalesChart(SqlConnection conn)
        {

            string query = @"
                SELECT MONTH(SaleDate) AS [Month], ISNULL(SUM(TotalAmount),0) AS [MonthlyRevenue]
                FROM Sale
                WHERE YEAR(SaleDate) = @Year
                GROUP BY MONTH(SaleDate)
                ORDER BY [Month]";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Year", DateTime.Now.Year);
                DataTable dtChart = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dtChart);
                }

                chartSales.Series.Clear();
                Series series = new Series("Revenue")
                {
                    ChartType = SeriesChartType.Column
                };

                foreach (DataRow row in dtChart.Rows)
                {
                    int month = Convert.ToInt32(row["Month"]);
                    decimal monthlyRevenue = Convert.ToDecimal(row["MonthlyRevenue"]);
                    series.Points.AddXY("Month " + month, monthlyRevenue);
                }
                chartSales.Series.Add(series);
                chartSales.Titles.Clear();
                chartSales.Titles.Add("Monthly Revenue (" + DateTime.Now.Year + ")");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        // Manage OrderDetail
        private void LoadOrderDetailData()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT SaleDetailID, SaleID, ProductID, Quantity, Subtotal FROM SalesDetails";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                dtOrderDetailManage = new DataTable();
                adapter.Fill(dtOrderDetailManage);

                dgvOrderDetailManage.DataSource = dtOrderDetailManage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading OrderDetail: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void dgvOrderDetailManage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvOrderDetailManage.Rows[e.RowIndex];
                txtSaleDetailID.Text = row.Cells["SaleDetailID"].Value.ToString();
                txtSaleID.Text = row.Cells["SaleID"].Value.ToString();
                txtProductID_OD.Text = row.Cells["ProductID"].Value.ToString();
                txtQuantity_OD.Text = row.Cells["Quantity"].Value.ToString();
                txtSubtotal_OD.Text = row.Cells["Subtotal"].Value.ToString();
            }
        }

        private void btnClearOD_Click(object sender, EventArgs e)
        {
            txtSaleDetailID.Clear();
            txtSaleID.Clear();
            txtProductID_OD.Clear();
            txtQuantity_OD.Clear();
            txtSubtotal_OD.Clear();
        }

        private void btnUpdateOD_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSaleDetailID.Text))
            {
                MessageBox.Show("Please select or enter the SaleDetailID to edit!");
                return;
            }

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string updateQuery = @"UPDATE SalesDetails
                               SET SaleID = @SaleID,
                                   ProductID = @ProductID,
                                   Quantity = @Quantity,
                                   Subtotal = @Subtotal
                               WHERE SaleDetailID = @SaleDetailID";
                using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@SaleDetailID", Convert.ToInt32(txtSaleDetailID.Text));
                    cmd.Parameters.AddWithValue("@SaleID", Convert.ToInt32(txtSaleID.Text));
                    cmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(txtProductID_OD.Text));
                    cmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(txtQuantity_OD.Text));
                    cmd.Parameters.AddWithValue("@Subtotal", Convert.ToDecimal(txtSubtotal_OD.Text));

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("OrderDetail updated successfully!");
                LoadOrderDetailData();
                btnClearOD_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating OrderDetail: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void btnDeleteOD_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSaleDetailID.Text))
            {
                MessageBox.Show("Please select the SaleDetailID to delete!");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this line??",
                                                  "Confirm",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);
            if (result == DialogResult.No) return;

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string deleteQuery = "DELETE FROM SalesDetails WHERE SaleDetailID = @SaleDetailID";
                using (SqlCommand cmd = new SqlCommand(deleteQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@SaleDetailID", Convert.ToInt32(txtSaleDetailID.Text));
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Delete OrderDetail successfully!");
                LoadOrderDetailData();
                btnClearOD_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting OrderDetail: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to log out?", "Confirm logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                login loginForm = new login();
                loginForm.ShowDialog();
                this.Dispose();
            }
        }
    }
}
