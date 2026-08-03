---
description: 'Guidelines for SQL Server development'
applyTo: '**/*.sql'
---

# SQL Server Development Standards

## Database schema generation
- All table names should be in plural form
- All column names should be in singular form
- All tables should have a primary key column name that ends with Id or ID

## Database schema design
- All tables should have a primary key constraint
- All foreign key constraints should have a name
- All foreign key constraints should have `ON DELETE CASCADE` option
- All foreign key constraints should have `ON UPDATE CASCADE` option
- All foreign key constraints should reference the primary key of the parent table

## Data Types & Column Design
- Use `NVARCHAR` for text columns that may contain Unicode characters (names, addresses, descriptions)
- Use `VARCHAR` only when data is guaranteed to be ASCII-only (codes, slugs)
- Use `DATETIME2` instead of `DATETIME` for new columns (better precision and range)
- Use `DATE` for dates that do not require the time component (e.g. Date of birth)
- Use `DECIMAL` with explicit precision and scale for monetary values (e.g., `DECIMAL(18, 2)`)
- Use `INT` for primary keys unless there's a specific need for `BIGINT` or `UNIQUEIDENTIFIER`
- Avoid `VARCHAR(MAX)` and `NVARCHAR(MAX)` unless truly necessary; use specific length constraints when possible
- Use `BIT` for boolean flags
- Use `UNIQUEIDENTIFIER` (GUID) for distributed systems or when obfuscation is needed
- Always specify column length for `VARCHAR`/`NVARCHAR` (e.g., `NVARCHAR(100)`, not `NVARCHAR`)

## Constraint Naming Conventions
- Primary key constraints: `PK_TableName` (e.g., `PK_Customers`)
- Foreign key constraints: `FK_TableName_ReferencedTable_ColumnName` (e.g., `FK_Orders_Customers_CustomerId`)
- Unique constraints: `UQ_TableName_ColumnName` (e.g., `UQ_Users_Email`)
- Check constraints: `CK_TableName_ColumnName_Description` (e.g., `CK_Products_Price_Positive`)
- Default constraints: `DF_TableName_ColumnName` (e.g., `DF_Orders_OrderDate`)
- Indexes: `IX_TableName_ColumnName` for non-clustered, `CIX_TableName_ColumnName` for clustered (e.g., `IX_Orders_CustomerId`)

## SQL Coding Style
- Use uppercase for SQL keywords (`SELECT`, `FROM`, `WHERE`)
- Use consistent indentation for nested queries and conditions
- Include comments to explain complex logic
- Break long queries into multiple lines for readability
- Organize clauses consistently (`SELECT`, `FROM`, `JOIN`, `WHERE`, `GROUP BY`, `HAVING`, `ORDER BY`)
- Always qualify table and view references with their two-part schema name (e.g., dbo.ActivitiesDrAssets, not ActivitiesDrAssets); never use bare one-part object names in DML or DDL statements
- Qualify all database objects (tables, views, stored procedures, functions) with schema name in all DDL and DML

## SQL Query Structure
- Use explicit column names in `SELECT` statements instead of `SELECT *`
- Qualify column names with table name or alias when using multiple tables
- Limit the use of subqueries when joins can be used instead
- Include `TOP (n)` for simple limits, or `ORDER BY ... OFFSET ... FETCH` for pagination, to restrict result sets
- Use appropriate indexing for frequently queried columns
- Avoid using functions on indexed columns in `WHERE` clauses

## Indexing
- Create non-clustered indexes on foreign key columns for join performance
- Create covering indexes (`INCLUDE` clause) for frequently used query patterns
- Use filtered indexes for queries that target specific subsets of data (e.g., `WHERE IsDeleted = 0`)
- Avoid over-indexing; every index adds overhead to `INSERT`/`UPDATE`/`DELETE` operations
- Include `TenantId` as the first column in filtered indexes for multi-tenant tables
- Avoid indexes on columns with low cardinality (few distinct values) unless using filtered indexes
- Use index naming convention: `IX_TableName_ColumnName` or `IX_TableName_Column1_Column2` for composite indexes

## Common Table Expressions (CTEs)
- Use CTEs to improve query readability and break down complex logic
- Prefer CTEs over subqueries when the same result set is referenced multiple times
- Use recursive CTEs for hierarchical data (organizational charts, category trees)
- Use temp tables instead of CTEs for large intermediate result sets that will be reused multiple times
- CTEs are not materialized; they are evaluated each time they are referenced

## NULL Handling
- Use `COALESCE` for multiple fallback values (e.g., `COALESCE(PreferredName, FirstName, 'Unknown')`)
- Use `ISNULL` for simple two-value scenarios with better performance (e.g., `ISNULL(Quantity, 0)`)
- Explicitly handle `NULL` in `WHERE` clause comparisons (remember: `NULL = NULL` is `FALSE`)
- Use `IS NULL` or `IS NOT NULL` for `NULL` checks, never `= NULL` or `<> NULL`
- Consider `NOT NULL` constraints on columns where `NULL` should never be allowed
- Provide `DEFAULT` values for non-nullable columns when appropriate

## Views
- Use PascalCase for view names
- Prefix views with descriptive category when helpful (e.g., `ActiveCustomers`, `SalesReport`)
- Qualify underlying table references with schema names in view definitions
- Avoid using `ORDER BY` in views (use in query that selects from view instead)
- Consider indexed views for complex aggregations with frequent reads and infrequent writes
- Document view purpose in header comment block

## User-Defined Functions
- Use PascalCase for function names
- Prefer table-valued functions over scalar functions for better performance
- Avoid using scalar functions in `WHERE` clauses on large tables (performance impact)
- Use inline table-valued functions when possible (better optimization than multi-statement)
- Place all functions in the `Fn` schema (e.g., `Fn.CalculateDiscount`, `Fn.GetCustomerOrders`)
- Function names should be descriptive and indicate their purpose using PascalCase
- Document function purpose, parameters, and return value in header comment

## Stored Procedure Naming Conventions
- Ensure that your stored procedures are owned by one of the specified schemas `CmdProcs`, `DelProcs`, `GetProcs`, `InsProcs`, `RptProcs` or `UpdProcs`
- Prefix stored procedure names with the verb portion of the schema that they belong to using camelCase (e.g., `GetProcs.getCustomer`); this intentional duplication of the verb improves discoverability and consistency
- Use camelCase for stored procedure names
- Use descriptive names that indicate purpose (e.g., `GetProcs.getCustomerOrders`)
- Include plural noun when returning multiple records (e.g., `GetProcs.getProducts`)
- Include singular noun when returning a single record (e.g., `GetProcs.getProduct`)

## Parameter Handling
- Prefix parameters with `@`
- Use PascalCase for parameter names
- Provide default values for optional parameters
- Validate parameter values before use
- Document parameters with comments
- Give parameters descriptive names
- Arrange parameters consistently (required first, optional later)

## Stored Procedure Structure
- Include `SET NOCOUNT ON` at the top of all stored procedures, regardless of whether they read or modify data
- Include header comment block with description, parameters, and return values
- Return standardized error codes/messages
- Return result sets with consistent column order
- Use `OUTPUT` parameters for returning status information
- Avoid returning multiple result sets when calling from Entity Framework Core (use `OUTPUT` parameters or separate procedures instead)

## Error Handling
- Wrap data modification operations in `TRY`/`CATCH` blocks where applicable
- Use `THROW` to re-raise errors in `CATCH` blocks (preserves original error information)
- Prefer `THROW` for raising custom error messages in new development
- Use `RAISERROR` only for legacy SQL Server versions where `THROW` is unavailable, and document the rationale when doing so
- Set `XACT_ABORT ON` for transactions to automatically rollback on errors
- Check `@@ERROR` immediately after statements when not using `TRY`/`CATCH`
- Log errors to audit/error tables when appropriate
- Return meaningful error codes via `OUTPUT` parameters or return values
- Avoid exposing system details in error messages returned to clients

## SQL Security Best Practices
- Parameterize all queries to prevent SQL injection
- Use prepared statements when executing dynamic SQL (`sp_executesql`)
- Avoid embedding credentials in SQL scripts
- Implement proper error handling without exposing system details
- Use dynamic SQL sparingly; prefer parameterized stored procedures
- When dynamic SQL is necessary, use `sp_executesql` with parameters (never concatenate user input)
- Validate and sanitize parameter values before use in dynamic SQL
- Use `QUOTENAME()` for dynamic object names to prevent SQL injection

## Transaction Management
- Explicitly begin and commit transactions
- Use appropriate isolation levels based on requirements
- Avoid long-running transactions that lock tables
- Use batch processing for large data operations
- Set `XACT_ABORT ON` at the beginning of transactions to ensure automatic rollback on errors
- Always pair `BEGIN TRANSACTION` with `COMMIT TRANSACTION` or `ROLLBACK TRANSACTION` in `TRY`/`CATCH` blocks
- Check `@@TRANCOUNT` before committing or rolling back to avoid errors
- Use `SAVE TRANSACTION` for nested transaction scenarios
- Minimize transaction scope to only necessary operations

## Temporal Tables (System-Versioned Tables)
- Use temporal tables for automatic audit trail and historical tracking
- Entities must implement `ITemporalEntity` in the application layer
- SQL Server maintains history automatically in the associated history table
- Never directly modify the history table
- Query historical data using `FOR SYSTEM_TIME` clause (`AS OF`, `BETWEEN`, `FROM TO`, `CONTAINED IN`)
- History tables inherit schema changes from main table automatically
- Consider partitioning history tables for long-term data retention

## Multi-Tenancy Data Patterns
- Include `TenantId` column (`INT` or `UNIQUEIDENTIFIER`) in all tenant-scoped tables
- Add indexes with `TenantId` as the leading key (or composite indexes starting with `TenantId`) for query performance; use filtered indexes only with constant predicates (e.g., `WHERE IsDeleted = 0`)
- Never expose data across tenants; always filter by `TenantId` in queries
- Ensure all foreign key relationships respect tenant boundaries
- Add `TenantId` to composite unique constraints where appropriate
- Test multi-tenant queries to prevent tenant data leakage

## Performance Optimization
- Use set-based operations instead of cursors or `WHILE` loops whenever possible
- Avoid using `SELECT *` in production code; specify only needed columns
- Use `EXISTS` instead of `IN` for subquery checks when possible (better performance)
- Use `UNION ALL` instead of `UNION` when duplicates are not a concern (avoids distinct sort)
- Avoid user-defined functions in `WHERE` clauses on large tables
- Avoid functions on indexed columns in `WHERE` clauses (prevents index usage)
- Use `WITH (NOLOCK)` hint carefully and only for reports where dirty reads are acceptable
- Batch large operations (`INSERT`/`UPDATE`/`DELETE`) to avoid excessive locking
- Use `OFFSET`/`FETCH` for pagination instead of `TOP` with dynamic logic
- Analyze query execution plans to identify table scans, missing indexes, and costly operations
- Avoid parameter sniffing issues by using `OPTION (RECOMPILE)` or local variables when appropriate

## String Operations
- Use `CONCAT()` function instead of `+` operator (handles `NULL` values better)
- Use `STRING_AGG()` for concatenating multiple rows into delimited string (SQL Server 2017+)
- Be aware of trailing space behavior with `VARCHAR` (automatically trimmed on comparison)
- Use `TRIM()`, `LTRIM()`, `RTRIM()` to handle whitespace explicitly
- Use `UPPER()` or `LOWER()` for case-insensitive comparisons when collation doesn't handle it