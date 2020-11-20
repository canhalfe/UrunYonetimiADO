Use master;

If(DB_ID('UrunYonetimiAdoDB') is null)
	Create Database UrunYonetimiAdoDb;

GO
Use UrunYonetimiAdoDb;

If OBJECT_ID(N'dbo.Urunler', N'U') is null
Create Table Urunler
(
	Id int primary key identity,
	UrunAd Nvarchar(100) not null,
	BirimFiyat Decimal(18,2) not null
);