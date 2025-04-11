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
    public partial class login: Form
    {
        SqlConnection connection;
        public login()
        {
            InitializeComponent();
            connection = new SqlConnection("Server = DESKTOP-MJ76D6E; Database = SalesManagement3; Integrated Security=True");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string query = "SELECT u_role FROM Dbo.Account WHERE username = @username AND u_password = @u_password";

            connection.Open();
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", SqlDbType.VarChar);
            cmd.Parameters["@username"].Value = username;
            cmd.Parameters.AddWithValue("@u_password", SqlDbType.VarChar);
            cmd.Parameters["@u_password"].Value = password;
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string role = reader["u_role"].ToString();
                if (role.Equals("admin"))
                {
                    MessageBox.Show(this, "Login Successful! ", "Result", MessageBoxButtons.OK, MessageBoxIcon.None);
                    this.Hide();
                    adminForm p = new adminForm(username);
                    p.ShowDialog();
                    this.Dispose();
                }
                else if (role.Equals("user"))
                {
                    MessageBox.Show(this, "Login Successful! ", "Result", MessageBoxButtons.OK, MessageBoxIcon.None);
                    this.Hide();
                    ViewProduct vp = new ViewProduct(username);
                    vp.ShowDialog();
                    this.Dispose();
                }
                else if (role.Equals("sale"))
                {
                    MessageBox.Show(this, "Login Successful! ", "Result", MessageBoxButtons.OK, MessageBoxIcon.None);
                    this.Hide();
                    SaleManagement vp = new SaleManagement(username);
                    vp.ShowDialog();
                    this.Dispose();
                }
                else if (role.Equals("warehouse"))
                {
                    MessageBox.Show(this, "Login Successful! ", "Result", MessageBoxButtons.OK, MessageBoxIcon.None);
                    this.Hide();
                    Warehouse vp = new Warehouse(username);
                    vp.ShowDialog();
                    this.Dispose();
                }

                else
                {
                    lblError.Text = "Error";
                }
            }
            else
            {
                lblError.Text = "Wrong username or password";
                reader.Close();
                connection.Close();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Do you want to exit?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
