IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Categories_Id')
    BEGIN
        ALTER TABLE Categories
            ADD CONSTRAINT DF_Categories_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Products_Id')
    BEGIN
        ALTER TABLE Products
            ADD CONSTRAINT DF_Products_Id DEFAULT NEWID() FOR Id;
    END    
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Suppliers_Id')
    BEGIN
        ALTER TABLE Suppliers
            ADD CONSTRAINT DF_Suppliers_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Supplies_Id')
    BEGIN
        ALTER TABLE Supplies
            ADD CONSTRAINT DF_Supplies_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_SupplyRows_Id')
    BEGIN
        ALTER TABLE SupplyRows
            ADD CONSTRAINT DF_SupplyRows_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_SalePlatforms_Id')
    BEGIN
        ALTER TABLE SalePlatforms
            ADD CONSTRAINT DF_SalePlatforms_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Orders_Id')
    BEGIN
        ALTER TABLE Orders
            ADD CONSTRAINT DF_Orders_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Items_Id')
    BEGIN
        ALTER TABLE Items
            ADD CONSTRAINT DF_Items_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Checks_Id')
    BEGIN
        ALTER TABLE Checks
            ADD CONSTRAINT DF_Checks_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_CommissionCategories_Id')
    BEGIN
        ALTER TABLE CommissionCategories
            ADD CONSTRAINT DF_CommissionCategories_Id DEFAULT NEWID() FOR Id;
    END
IF NOT EXISTS(SELECT * FROM sys.default_constraints WHERE name = 'DF_Commissions_Id')
    BEGIN
        ALTER TABLE Commissions
            ADD CONSTRAINT DF_Commissions_Id DEFAULT NEWID() FOR Id;
    END
    