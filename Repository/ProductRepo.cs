using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saleManagement.Repository
{
    public class ProductRepo
    {
        public string getAllProduct()
        {
            return "SELECT ProductID, ProductName, Quantity, CategoryID, SupplierID, Price, StockQuantity, EmployeeID FROM Product";
        }

        // crud
        // CREATE - Thêm sản phẩm mới
        public string InsertProduct()
        {
            return @"INSERT INTO Product (ProductID, ProductName, Quantity, Price, CategoryID, SupplierID, StockQuantity, EmployeeID)
                               VALUES (@ProductID, @ProductName, @Quantity, @Price, @CategoryID, @SupplierID, @StockQuantity, @EmployeeID)";
        }

        // READ - Lấy thông tin sản phẩm theo ID
        public string GetProductById()
        {
            return "SELECT * FROM Product WHERE ProductID = @ProductID";
        }

        // UPDATE - Cập nhật thông tin sản phẩm
        public string UpdateProduct()
        {
            return @"UPDATE Product SET 
                               ProductName = @ProductName,
                               Quantity = @Quantity,
                               CategoryID = @CategoryID,
                               SupplierID = @SupplierID,
                               EmployeeID = @EmployeeID,
                               StockQuantity = @StockQuantity,
                               Price = @Price
                               WHERE ProductID = @ProductID";
        }

        // DELETE - Xóa sản phẩm
        public string DeleteProduct()
        {
            return "DELETE FROM Product WHERE ProductID = @ProductID";
        }



    }
}
