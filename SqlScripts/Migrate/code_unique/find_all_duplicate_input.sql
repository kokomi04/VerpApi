--select all duplicate so_ct
SELECT * FROM
(
	SELECT 
	
	InputBill_F_ID, 
	ct.so_ct, 
	ROW_NUMBER() OVER(PARTITION BY ct.so_ct ORDER BY InputBill_F_Id DESC)  rNumber

	FROM
	(
		SELECT 
		v.InputBill_F_Id, 
		MAX(v.so_ct)  so_ct
		FROM  dbo.InputValueRow v 
		WHERE v.IsDeleted=0
		GROUP BY v.InputBill_F_Id
	) ct 
) v
WHERE v.rNumber>1