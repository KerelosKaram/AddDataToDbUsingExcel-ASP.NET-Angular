using System.Collections;
using API.Data.AppDbContext.OneNineTwo;
using API.Data.AppDbContext.Sql2017DbContext;
using API.Data.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace API.Services
{
    public class ExcelImportService
    {

        private readonly IServiceProvider _serviceProvider;

        public ExcelImportService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

        }
        
        private DbContext GetDbContext(Type entityType, string dbName)
        {

            if (entityType.Namespace.Contains("API.Data.Models.Entities.Sql2017"))
            {
                var options = new DbContextOptionsBuilder<Sql2017DbContext>();
                options.UseSqlServer(_serviceProvider.GetRequiredService<IConfiguration>()
                    .GetConnectionString("SQL2017Connection")?
                    .Replace("{DB_NAME}", dbName));
                return new Sql2017DbContext(options.Options);
            }
            else if (entityType.Namespace.Contains("API.Data.Models.Entities.OneNineTwo"))
            {
                var options = new DbContextOptionsBuilder<OneNineTwoDbContext>();
                options.UseSqlServer(_serviceProvider.GetRequiredService<IConfiguration>()
                    .GetConnectionString("OneNineTwoConnection")?
                    .Replace("{DB_NAME}", dbName));
                return new OneNineTwoDbContext(options.Options);
            }

            throw new ArgumentException($"Entity type {entityType.Name} does not belong to a recognized namespace.");
        }

        public async Task DeleteDataFromDatabase<T>(string dbName) where T : BaseEntity
        {
            try
            {
                // Get the DbContext dynamically
                using var context = GetDbContext(typeof(T), dbName);
                var entityType = context.Model.FindEntityType(typeof(T));

                if (entityType == null)
                {
                    throw new InvalidOperationException("Entity type not found in DbContext.");
                }

                // Safely get the table name from the EF model
                var tableName = entityType.GetTableName();
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new Exception("Table name could not be determined.");
                }

                // Use a parameterized raw SQL query to avoid injection risks
                await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}]");

                Console.WriteLine($"All records in table '{tableName}' have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting data from table {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }

        // Generic method to insert data into the correct DbSet
        public async Task InsertDataIntoDatabase<T>(List<T> data, string dbName) where T : class
        {
            try
            {
                using var context = GetDbContext(typeof(T), dbName);  // Pass dbName to GetDbContext
                var dbSet = context.Set<T>();  // Get the DbSet of the specific entity type
                dbSet.AddRange(data);         // Add the records to the DbSet

                // Save the changes to the database
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Method to import data from Excel and insert into the database
        public async Task ImportExcelData(MemoryStream stream, Type entityType, string dbName)
        {

            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // For non-commercial use
            try
            {
                // Create a list to hold the data with the correct entity type
                var dataListType = typeof(List<>).MakeGenericType(entityType);
                var dataList = (IList)Activator.CreateInstance(dataListType);

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];  // Assuming data is in the first worksheet
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    // Read the header row to map the Excel columns to entity properties
                    var headerRow = new List<string>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        headerRow.Add(worksheet.Cells[1, col].Text.Trim()); // Trim to avoid spaces in headers
                    }

                    // Read each subsequent row and map the values to the entity
                    for (int row = 2; row <= rowCount; row++)  // Starting from row 2 (data rows)
                    {
                        var entityInstance = Activator.CreateInstance(entityType);

                        for (int col = 1; col <= colCount; col++)
                        {
                            string columnName = headerRow[col - 1];
                            var property = entityType.GetProperty(columnName);

                            if (property != null)
                            {
                                var cellValue = worksheet.Cells[row, col].Text;
                                try
                                {
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                                        {
                                            // Set to null for nullable types
                                            property.SetValue(entityInstance, null);
                                        }
                                        else
                                        {
                                            // Set default value for non-nullable types
                                            property.SetValue(entityInstance, Activator.CreateInstance(property.PropertyType));
                                        }
                                    }
                                    else
                                    {
                                        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                                        try
                                        {
                                            if (propertyType == typeof(int))
                                            {
                                                property.SetValue(entityInstance, int.TryParse(cellValue, out int intValue) ? intValue : default(int?));
                                            }
                                            else if (propertyType == typeof(DateTime))
                                            {
                                                property.SetValue(entityInstance, DateTime.TryParse(cellValue, out DateTime dateTimeValue) ? dateTimeValue : default(DateTime?));
                                            }
                                            else if (propertyType == typeof(decimal))
                                            {
                                                property.SetValue(entityInstance, decimal.TryParse(cellValue, out decimal decimalValue) ? decimalValue : default(decimal?));
                                            }
                                            else if (propertyType == typeof(bool))
                                            {
                                                property.SetValue(entityInstance, bool.TryParse(cellValue, out bool boolValue) ? boolValue : default(bool?));
                                            }
                                            else if (propertyType == typeof(string))
                                            {
                                                property.SetValue(entityInstance, cellValue);
                                            }
                                            else
                                            {
                                                property.SetValue(entityInstance, Convert.ChangeType(cellValue, propertyType));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error mapping column {columnName} with value '{cellValue}': {ex.Message}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error mapping column {columnName} with value '{cellValue}': {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"No matching property found for column: {columnName}");
                            }
                        }

                        // Add the created instance of the entity to the list
                        dataList?.Add(entityInstance);
                    }

                    // Explicitly pass the correct type and dbName to InsertDataIntoDatabase
                    var method = typeof(ExcelImportService)?
                        .GetMethod("InsertDataIntoDatabase")?
                        .MakeGenericMethod(entityType);

                    await (Task)method.Invoke(this, new object[] { dataList, dbName });  // Pass dbName here
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing Excel data: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack Trace: {ex.InnerException.StackTrace}");
                }

                throw;
            }
        }
    }
}
