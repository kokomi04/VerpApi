
/*
SELECT 
	po.ProductionOrderId,
	po.ProductionOrderCode,
	p.MonthPlanId,
	p.MonthPlanName,
	w1.WeekPlanId,
	w1.WeekPlanName,
	w2.WeekPlanId,
	w2.WeekPlanName
FROM dbo.ProductionOrder po 
 JOIN dbo.MonthPlan p ON po.StartDate>= p.StartDate AND po.PlanEndDate <= p.EndDate
 OUTER APPLY (
 SELECT TOP(1) w.WeekPlanId, w.WeekPlanName FROM dbo.WeekPlan w WHERE w.IsDeleted = 0 AND p.MonthPlanId = w.MonthPlanId AND po.StartDate>= w.StartDate ORDER BY w.StartDate
 ) w1

 OUTER APPLY (
 SELECT TOP(1) w.WeekPlanId, w.WeekPlanName FROM dbo.WeekPlan w WHERE w.IsDeleted = 0 AND p.MonthPlanId = w.MonthPlanId AND po.PlanEndDate<= w.EndDate ORDER BY w.StartDate ASC
 ) w2
 WHERE p.IsDeleted = 0 AND po.IsDeleted = 0

 */

 USE ManufacturingDB
 GO
UPDATE po
SET po.MonthPlanId = p.MonthPlanId,
	po.FromWeekPlanId = w1.WeekPlanId,
	po.ToWeekPlanId = w2.WeekPlanId
FROM dbo.ProductionOrder po 
 JOIN dbo.MonthPlan p ON po.StartDate>= p.StartDate AND po.PlanEndDate <= p.EndDate
 OUTER APPLY (
 SELECT TOP(1) w.WeekPlanId, w.WeekPlanName FROM dbo.WeekPlan w WHERE w.IsDeleted = 0 AND p.MonthPlanId = w.MonthPlanId AND po.StartDate>= w.StartDate ORDER BY w.StartDate
 ) w1

 OUTER APPLY (
 SELECT TOP(1) w.WeekPlanId, w.WeekPlanName FROM dbo.WeekPlan w WHERE w.IsDeleted = 0 AND p.MonthPlanId = w.MonthPlanId AND po.PlanEndDate<= w.EndDate ORDER BY w.StartDate ASC
 ) w2
 WHERE p.IsDeleted = 0 AND po.IsDeleted = 0 