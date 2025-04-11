using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saleManagement.Repository
{
    public class CustomerRepo
    {
        // READ - Lấy tất cả khách hàng
        public string GetAllCustomer()
        {
            return @"SELECT CustomerID, CustomerName, PhoneNumber, Address, Email FROM Customer";
        }

        // CREATE - Thêm khách hàng mới
        public string InsertCustomer()
        {
            return @"INSERT INTO Customer (CustomerID, CustomerName, PhoneNumber, Address, Email)
                                 VALUES (@CustomerID, @CustomerName, @PhoneNumber, @Address, @Email)";
        }

        // READ - Lấy thông tin khách hàng theo ID
        public string GetCustomerById()
        {
            return "SELECT * FROM Customer WHERE CustomerID = @CustomerID";
        }

        // UPDATE - Cập nhật thông tin khách hàng
        public string UpdateCustomer()
        {
            return @"
            UPDATE Customer
            SET CustomerName = @CustomerName,
                PhoneNumber = @PhoneNumber,
                Address = @Address,
                Email = @Email
            WHERE CustomerID = @CustomerID";
        }

        // DELETE - Xóa khách hàng
        public string DeleteCustomer()
        {
            return "DELETE FROM Customer WHERE CustomerID = @CustomerID";
        }
    }
}
