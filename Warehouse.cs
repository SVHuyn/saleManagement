using ClosedXML.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Excel = Microsoft.Office.Interop.Excel;

namespace saleManagement
{

    public partial class Warehouse: Form
    {
        // Kết nối CSDL
        SqlConnection connection;

        public Warehouse(string username)
        {
            InitializeComponent();
            connection = new SqlConnection("Server=DESKTOP-MJ76D6E; Database=SalesManagement3; Integrated Security=True");
        }

        private void Warehouse_Load(object sender, EventArgs e)
        {
            // inventory
            LoadInventoryData();

            // export
            LoadExportData();

            // import
            InitializeImportDetailsTable();
            LoadImportProducts();
        }

        private void LoadInventoryData()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT ProductID, ProductName, Price, StockQuantity FROM Product";
                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(dt);
                dgvInventory.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading inventory data: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void btnUpdateInventory_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtProductID.Text))
                {
                    MessageBox.Show("Please select the product you want to update!");
                    return;
                }

                int productId = Convert.ToInt32(txtProductID.Text);
                int newStockQuantity = Convert.ToInt32(txtStockQuantity.Text);

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "UPDATE Product SET StockQuantity = @StockQuantity WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@StockQuantity", newStockQuantity);
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Inventory has been updated!");
                LoadInventoryData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Inventory update error: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }


        private void dgvInventory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvInventory.Rows[e.RowIndex];
                txtProductID.Text = row.Cells["ProductID"].Value.ToString();
                txtProductName.Text = row.Cells["ProductName"].Value.ToString();
                txtStockQuantity.Text = row.Cells["StockQuantity"].Value.ToString();
                txtPrice.Text = row.Cells["Price"].Value.ToString();
            }
        }

        private void btnSearchInventory_Click(object sender, EventArgs e)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT ProductID, ProductName, Price, StockQuantity FROM Product " +
                               "WHERE ProductName LIKE @keyword OR CAST(ProductID AS VARCHAR) LIKE @keyword";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@keyword", "%" + txtSearchInventory.Text.Trim() + "%");
                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                    dgvInventory.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }


        private void btnClearInputs_Click(object sender, EventArgs e)
        {
            txtProductID.Clear();
            txtProductName.Clear();
            txtStockQuantity.Clear();
            txtPrice.Clear();
            txtSearchInventory.Clear();
        }

        private void btnRefreshInventory_Click(object sender, EventArgs e)
        {
            LoadInventoryData();
            txtSearchInventory.Text = "";
        }

        // Export
        private void ExportDataGridViewToCSV(DataGridView dgv, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            string[] columnNames = dgv.Columns.Cast<DataGridViewColumn>()
                                   .Select(col => col.HeaderText)
                                   .ToArray();
            sb.AppendLine(string.Join(",", columnNames));

            // Get data row by row
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (!row.IsNewRow)
                {
                    string[] cells = row.Cells.Cast<DataGridViewCell>()
                                      .Select(cell =>
                                          cell.Value != null ? cell.Value.ToString().Replace(",", " ") : "")
                                      .ToArray();
                    sb.AppendLine(string.Join(",", cells));
                }
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        private void btnExportToCSV_Click(object sender, EventArgs e)
        {
            // Open SaveFileDialog to select the path to save the CSV file
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            sfd.FileName = "ExportedData.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportDataGridViewToCSV(dgvExport, sfd.FileName);
                    MessageBox.Show("Export success!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while exporting: " + ex.Message);
                }
            }
        }

        private void LoadExportData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection("Server=DESKTOP-MJ76D6E; Database=SalesManagement3; Integrated Security=True"))
                {
                    string query = "SELECT ProductID, ProductName, Price, StockQuantity FROM Product";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvExport.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading export data: " + ex.Message);
            }
        }

        // Export to excel
        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "Excel Workbook|*.xlsx",
                Title = "Save as Excel File"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Export Data");

                        // Thêm tiêu đề cột
                        for (int i = 1; i <= dgvExport.Columns.Count; i++)
                        {
                            worksheet.Cell(1, i).Value = dgvExport.Columns[i - 1].HeaderText;
                        }

                        // Thêm dữ liệu dòng
                        for (int i = 0; i < dgvExport.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgvExport.Columns.Count; j++)
                            {
                                worksheet.Cell(i + 2, j + 1).Value = dgvExport.Rows[i].Cells[j].Value?.ToString();
                            }
                        }

                        try
                        {
                            workbook.SaveAs(sfd.FileName);
                            MessageBox.Show("Exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("File is currently open. Please close it and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        // Import
        // DataTable is used to store imported products (temporary import vouchers)
        DataTable importDetailsTable = new DataTable();

        private void InitializeImportDetailsTable()
        {
            importDetailsTable.Columns.Add("ProductID", typeof(int));
            importDetailsTable.Columns.Add("ProductName", typeof(string));
            importDetailsTable.Columns.Add("Quantity", typeof(int));

            dgvImportOrderDetails.DataSource = importDetailsTable;
        }

        // Load product list (for dgvImportProducts)
        private void LoadImportProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection("Server=DESKTOP-MJ76D6E; Database=SalesManagement3; Integrated Security=True"))
                {
                    string query = "SELECT ProductID, ProductName, Price, StockQuantity FROM Product";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvImportProducts.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading product list: " + ex.Message);
            }
        }

        private void btnAddToImportOrder_Click(object sender, EventArgs e)
        {
            if (dgvImportProducts.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please select the product you want to import!");
                return;
            }

            DataGridViewRow selectedRow = dgvImportProducts.SelectedRows[0];
            int productId = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);
            string productName = selectedRow.Cells["ProductName"].Value.ToString();
            int quantity = (int)nudImportQuantity.Value;

            if (quantity <= 0)
            {
                MessageBox.Show("The quantity entered must be greater than 0!");
                return;
            }

            int stockQuantity = Convert.ToInt32(selectedRow.Cells["StockQuantity"].Value);

            // Check if the product is already in the import form
            DataRow[] existingRows = importDetailsTable.Select("ProductID = " + productId);
            if (existingRows.Length > 0)
            {
                int currentQty = Convert.ToInt32(existingRows[0]["Quantity"]);
                existingRows[0]["Quantity"] = currentQty + quantity;
            }
            else
            {
                DataRow row = importDetailsTable.NewRow();
                row["ProductID"] = productId;
                row["ProductName"] = productName;
                row["Quantity"] = quantity;
                importDetailsTable.Rows.Add(row);
            }
        }

        private void btnCreateImportOrder_Click(object sender, EventArgs e)
        {
            if (importDetailsTable.Rows.Count == 0)
            {
                MessageBox.Show("The delivery note is empty, please add products!");
                return;
            }

            using (SqlConnection conn = new SqlConnection("Server=DESKTOP-MJ76D6E; Database=SalesManagement3; Integrated Security=True"))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // For each product in the receipt, update the StockQuantity in the Product table
                        foreach (DataRow row in importDetailsTable.Rows)
                        {
                            int productId = Convert.ToInt32(row["ProductID"]);
                            int importQty = Convert.ToInt32(row["Quantity"]);

                            // Update inventory: increase quantity

                            string updateQuery = "UPDATE Product SET StockQuantity = StockQuantity + @ImportQty WHERE ProductID = @ProductID";
                            using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ImportQty", importQty);
                                cmd.Parameters.AddWithValue("@ProductID", productId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("The goods receipt has been processed successfully.!");
                        importDetailsTable.Clear();
                        // Refresh product list to show new inventory
                        LoadImportProducts();
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        MessageBox.Show("Error when processing goods receipt: " + ex.Message);
                    }
                }
            }
        }


        private void btnClearImportOrder_Click(object sender, EventArgs e)
        {
            importDetailsTable.Clear();
        }


        private void btnExportImportOrder_Click(object sender, EventArgs e)
        {
            if (dgvImportOrderDetails.Rows.Count == 0)
            {
                MessageBox.Show("No data to export!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Excel.Application excelApp = new Excel.Application();
                Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = workbook.ActiveSheet;
                worksheet.Name = "ImportOrderDetails";

                // Output column names
                for (int i = 0; i < dgvImportOrderDetails.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = dgvImportOrderDetails.Columns[i].HeaderText;
                }

                // Export data
                for (int i = 0; i < dgvImportOrderDetails.Rows.Count; i++)
                {
                    for (int j = 0; j < dgvImportOrderDetails.Columns.Count; j++)
                    {
                        object cellValue = dgvImportOrderDetails.Rows[i].Cells[j].Value;
                        worksheet.Cells[i + 2, j + 1] = cellValue?.ToString() ?? "";
                    }
                }

                // Display Excel for user to save file
                excelApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Reporting

        private void btnFilterReport_Click(object sender, EventArgs e)
        {
            DateTime fromDate = dtpFrom.Value.Date;
            DateTime toDate = dtpTo.Value.Date;

            string query = @"
                SELECT 'Import' AS Type, SaleDate AS Date, TotalAmount FROM Sale
                WHERE SaleDate BETWEEN @from AND @to
                UNION
                SELECT 'Export' AS Type, OrderDate AS Date, TotalAmount FROM [Order]
                WHERE OrderDate BETWEEN @from AND @to";

            SqlDataAdapter da = new SqlDataAdapter(query, connection);
            da.SelectCommand.Parameters.AddWithValue("@from", fromDate);
            da.SelectCommand.Parameters.AddWithValue("@to", toDate);

            DataTable dt = new DataTable();
            da.Fill(dt);
            dgvReportDetails.DataSource = dt;

            LoadChart(dt);
        }

        private void LoadChart(DataTable dt)
        {
            chartReport.Series.Clear();
            Series seriesImport = new Series("Import goods");
            Series seriesExport = new Series("Product delivery");

            seriesImport.ChartType = SeriesChartType.Column;
            seriesExport.ChartType = SeriesChartType.Column;

            foreach (DataRow row in dt.Rows)
            {
                string type = row["Type"].ToString();
                DateTime date = Convert.ToDateTime(row["Date"]);
                decimal amount = Convert.ToDecimal(row["TotalAmount"]);

                if (type == "Import")
                    seriesImport.Points.AddXY(date.ToShortDateString(), amount);
                else if (type == "Export")
                    seriesExport.Points.AddXY(date.ToShortDateString(), amount);
            }

            chartReport.Series.Add(seriesImport);
            chartReport.Series.Add(seriesExport);
        }

        private void btnExportReportToExcel_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.csv)|*.csv",
                FileName = "BaoCaoNhapXuat.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    // Header
                    for (int i = 0; i < dgvReportDetails.Columns.Count; i++)
                    {
                        sw.Write(dgvReportDetails.Columns[i].HeaderText);
                        if (i < dgvReportDetails.Columns.Count - 1) sw.Write(",");
                    }
                    sw.WriteLine();

                    // Rows
                    foreach (DataGridViewRow row in dgvReportDetails.Rows)
                    {
                        for (int i = 0; i < dgvReportDetails.Columns.Count; i++)
                        {
                            sw.Write(row.Cells[i].Value);
                            if (i < dgvReportDetails.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine();
                    }
                }

                MessageBox.Show("Excel export successful!");
            }
        }

        private void btnPrintReport_Click(object sender, EventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == DialogResult.OK)
            {
                // Print chartReport content to printer
                chartReport.Printing.PrintDocument.Print();
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
