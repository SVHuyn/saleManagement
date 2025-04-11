using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace saleManagement
{
    public partial class ViewProduct: Form
    {
        SqlConnection connection;
        public ViewProduct(string username)
        {
            InitializeComponent();
            connection = new SqlConnection("Server = DESKTOP-MJ76D6E; Database = SalesManagement3; Integrated Security=True");
            lbUser.Text = "User: " + username;
        }
        private void ViewProduct_Load(object sender, EventArgs e)
        {
            FillData();
        }
        public void FillData()
        {
            try
            {
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    string query = "SELECT ProductID, ProductName, Quantity, CategoryID, SupplierID, Price, StockQuantity, EmployeeID FROM Product";
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

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to log out?", "Confirm logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Hide();
                login login = new login();
                login.ShowDialog();
                this.Dispose();
            }
        }
    }
}
