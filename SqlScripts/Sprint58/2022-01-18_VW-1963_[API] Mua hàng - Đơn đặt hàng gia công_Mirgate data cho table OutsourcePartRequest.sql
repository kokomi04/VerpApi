
use ManufacturingDB;

UPDATE r
SET r.ProductionOrderId = d.ProductionOrderId
from OutsourcePartRequest r 
left join ProductionOrderDetail d on r.ProductionOrderDetailId = d.ProductionOrderDetailId
where r.ProductionOrderDetailId is not null;
