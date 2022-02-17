use OrganizationDB;

Truncate table CountedSymbol;

INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription, IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(1,'Tr',N'Kí hiệu đi trễ', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(2,'Sm',N'Kí hiệu về sớm', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(3,'X',N'Kí hiệu đúng giờ', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(4,'+',N'Kí hiệu tăng ca', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(5,'KR',N'Kí hiệu thiếu giờ ra', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(6,'KV',N'Kí hiệu thiếu giờ vào', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(7,'V',N'Kí hiệu vắng', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(8,N'Đ',N'Kí hiệu đúng giờ ca có qua đêm', 0, 2, SYSDATETIME(),0);
INSERT INTO CountedSymbol(CountedSymbolType, SymbolCode, SymbolDescription,IsHide, UpdatedByUserId, UpdatedDatetimeUtc, IsDeleted) 
VALUES(9,'Off',N'Kí hiệu ngày không xếp ca', 0, 2, SYSDATETIME(),0);
