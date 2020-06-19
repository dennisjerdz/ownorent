delete from Payments;
delete from Transactions;
delete from TransactionGroupPaymentAttempts;
delete from TransactionGroups;
Update Products SET ProductStatus = 0

select * from Payments;
select * from Transactions;
select * from TransactionGroupPaymentAttempts;
select * from TransactionGroups;
select * from Products;

select MobileNumber,* from ApplicationUsers

select * from settings
update settings SET Value = '1' where code = 'PLATFORM_TAX_CASHOUT'