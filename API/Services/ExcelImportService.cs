using System.Collections;
using API.Data.AppDbContext.DbElWagd;
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

            if (entityType.Namespace != null && entityType.Namespace.Contains("API.Data.Models.Entities.Sql2017"))
            {
                var options = new DbContextOptionsBuilder<Sql2017DbContext>();
                options.UseSqlServer(_serviceProvider.GetRequiredService<IConfiguration>()
                    .GetConnectionString("SQL2017Connection")?
                    .Replace("{DB_NAME}", dbName));
                return new Sql2017DbContext(options.Options);
            }
            else if (entityType.Namespace != null && entityType.Namespace.Contains("API.Data.Models.Entities.OneNineTwo"))
            {
                var options = new DbContextOptionsBuilder<OneNineTwoDbContext>();
                options.UseSqlServer(_serviceProvider.GetRequiredService<IConfiguration>()
                    .GetConnectionString("OneNineTwoConnection")?
                    .Replace("{DB_NAME}", dbName));
                return new OneNineTwoDbContext(options.Options);
            }
            else if (entityType.Namespace != null && entityType.Namespace.Contains("API.Data.Models.Entities.DbElWagd"))
            {
                var options = new DbContextOptionsBuilder<DbElWagdDbContext>();
                options.UseSqlServer(_serviceProvider.GetRequiredService<IConfiguration>()
                    .GetConnectionString("DbElWagd")?
                    .Replace("{DB_NAME}", dbName));
                return new DbElWagdDbContext(options.Options);
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
                // Use FormattableString to create a parameterized query
                string sql = $"DELETE FROM [{tableName}]";
                await context.Database.ExecuteSqlRawAsync(sql);

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
        public async Task<(int linesAdded, List<string> errorMessages)> ImportExcelData(MemoryStream stream, Type entityType, string dbName)
        {
            int linesAdded = 0;
            var errorMessages = new List<string>();

            try
            {
                var dataListType = typeof(List<>).MakeGenericType(entityType);
                var dataList = (IList?)Activator.CreateInstance(dataListType);

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    var headerRow = new List<string>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        headerRow.Add(worksheet.Cells[1, col].Text.Trim());
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var entityInstance = Activator.CreateInstance(entityType);
                        bool rowHasError = false;

                        for (int col = 1; col <= colCount; col++)
                        {
                            string columnName = headerRow[col - 1];
                            var property = entityType.GetProperty(columnName);

                            if (property != null)
                            {
                                var cellValue = worksheet.Cells[row, col].Text;
                                try
                                {
                                    // if (string.IsNullOrWhiteSpace(cellValue))
                                    // {
                                    //     if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                                    //     {
                                    //         property.SetValue(entityInstance, null);
                                    //     }
                                    //     else
                                    //     {
                                    //         property.SetValue(entityInstance, Activator.CreateInstance(property.PropertyType));
                                    //     }
                                    // }
                                    if (string.IsNullOrWhiteSpace(cellValue))
                                    {
                                        if (property.PropertyType == typeof(string))
                                        {
                                            property.SetValue(entityInstance, null); // or string.Empty if you prefer
                                        }
                                        else if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                                        {
                                            property.SetValue(entityInstance, null);
                                        }
                                        else
                                        {
                                            property.SetValue(entityInstance, Activator.CreateInstance(property.PropertyType));
                                        }
                                    }
                                    else
                                    {
                                        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

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
                                }
                                catch (Exception ex)
                                {
                                    errorMessages.Add($"Error in row {row}, column {columnName}, error: {ex.Message}");
                                    rowHasError = true;
                                }
                            }
                            else
                            {
                                errorMessages.Add($"No matching property found for column: {columnName} in row {row}");
                                rowHasError = true;
                            }
                        }

                        if (!rowHasError)
                        {
                            dataList?.Add(entityInstance);
                        }
                    }

                    var method = typeof(ExcelImportService)?
                        .GetMethod("InsertDataIntoDatabase")?
                        .MakeGenericMethod(entityType);

                    if (method != null && dataList != null)
                    {
                        // var result = method.Invoke(this, new object[] { dataList, dbName });
                        var result = method.Invoke(this, [dataList, dbName]);
                        if (result != null)
                        {
                            await (Task)result;
                        }
                        else
                        {
                            errorMessages.Add("Method invocation returned null.");
                        }
                    }
                    else
                    {
                        errorMessages.Add("Method or dataList is null.");
                    }

                    linesAdded = dataList?.Count ?? 0;
                }
            }
            catch (Exception ex)
            {
                errorMessages.Add($"General error during import: {ex.InnerException?.Message ?? ex.Message}");
            }

            return (linesAdded, errorMessages);
        }
    }
}
