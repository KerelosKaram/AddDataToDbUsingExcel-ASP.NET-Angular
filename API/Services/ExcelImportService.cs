using System.Collections;
using API.Data.AppDbContext.OneNineTwo;
using API.Data.AppDbContext.Sql2017DbContext;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

public class ExcelImportService
{

    private readonly Sql2017DbContext _sql2017Context;
    private readonly IServiceProvider _serviceProvider;
    private readonly OneNineTwoDbContext _oneNineTwoContext;

    public ExcelImportService(IServiceProvider serviceProvider, Sql2017DbContext sql2017Context, OneNineTwoDbContext oneNineTwoContext)
    {
            _oneNineTwoContext = oneNineTwoContext;
            _serviceProvider = serviceProvider;
            _sql2017Context = sql2017Context;

    }

    private DbContext GetDbContext(Type entityType, string dbName)
    {
        // Console.WriteLine("Entity Namespace: " + entityType.Namespace);
        // Console.WriteLine(entityType.Namespace.Contains("API.Data.Models.Entities.OneNineTwo"));
        // Console.WriteLine(entityType.Namespace.Contains("API.Data.Models.Entities.Sql2017"));

        if (entityType.Namespace.Contains("API.Data.Models.Entities.Sql2017"))
        {
            var options = new DbContextOptionsBuilder<Sql2017DbContext>();
            options.UseSqlServer(_serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("SQL2017Connection")
                .Replace("{DB_NAME}", dbName));
            return new Sql2017DbContext(options.Options);
        }
        else if (entityType.Namespace.Contains("API.Data.Models.Entities.OneNineTwo"))
        {
            var options = new DbContextOptionsBuilder<OneNineTwoDbContext>();
            options.UseSqlServer(_serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("OneNineTwoConnection")
                .Replace("{DB_NAME}", dbName));
            return new OneNineTwoDbContext(options.Options);
        }

        throw new ArgumentException($"Entity type {entityType.Name} does not belong to a recognized namespace.");
    }
    
    // Generic method to insert data into the correct DbSet
    public async Task InsertDataIntoDatabase<T>(List<T> data, string dbName) where T : class
    {
        // System.Console.WriteLine(dbName);
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
            Console.WriteLine($"Error inserting data: {ex.Message}");
            throw;
        }
    }

    // Method to import data from Excel and insert into the database
    public async Task ImportExcelData(MemoryStream stream, Type entityType, string dbName)
    {
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
                                // Log each cell value being mapped
                                // Console.WriteLine($"Row {row}, Column {col} ({columnName}): {cellValue}");

                                if (string.IsNullOrWhiteSpace(cellValue))
                                {
                                    if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                                    {
                                        property.SetValue(entityInstance, null);  // Set to null for nullable types
                                    }
                                    else
                                    {
                                        property.SetValue(entityInstance, Activator.CreateInstance(property.PropertyType));
                                    }
                                }
                                else
                                {
                                    if (property.PropertyType == typeof(int))
                                    {
                                        property.SetValue(entityInstance, int.TryParse(cellValue, out int intValue) ? intValue : default);
                                    }
                                    else if (property.PropertyType == typeof(DateTime))
                                    {
                                        property.SetValue(entityInstance, DateTime.TryParse(cellValue, out DateTime dateTimeValue) ? dateTimeValue : default);
                                    }
                                    else if (property.PropertyType == typeof(decimal))
                                    {
                                        property.SetValue(entityInstance, decimal.TryParse(cellValue, out decimal decimalValue) ? decimalValue : default);
                                    }
                                    else if (property.PropertyType == typeof(bool))
                                    {
                                        property.SetValue(entityInstance, bool.TryParse(cellValue, out bool boolValue) ? boolValue : default);
                                    }
                                    else if (property.PropertyType == typeof(string))
                                    {
                                        property.SetValue(entityInstance, cellValue);
                                    }
                                    else
                                    {
                                        property.SetValue(entityInstance, Convert.ChangeType(cellValue, property.PropertyType));
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
                    dataList.Add(entityInstance);
                }

                // Explicitly pass the correct type and dbName to InsertDataIntoDatabase
                var method = typeof(ExcelImportService)
                    .GetMethod("InsertDataIntoDatabase")
                    .MakeGenericMethod(entityType);

                await (Task)method.Invoke(this, new object[] { dataList, dbName });  // Pass dbName here
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing Excel data: {ex.Message}");
            throw;
        }
    }
}
