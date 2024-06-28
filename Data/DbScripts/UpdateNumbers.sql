with cte as (
    select id, SalePlatformId, row_number() OVER (PARTITION BY SalePlatformId ORDER BY Date) as rownum from Orders
)
update Orders set Number = (select rownum from cte where cte.id = Orders.id);


with cte as (
    select id, SupplierId, row_number() OVER (PARTITION BY SupplierId ORDER BY Date) as rownum from Supplies
)
update Supplies set Number = (select rownum from cte where cte.id = Supplies.id);