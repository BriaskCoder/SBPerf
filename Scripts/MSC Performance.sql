SELECT top 10 *
  FROM [dbo].[PerfThreadInfo]
  order by id desc

  SELECT *
  FROM [dbo].[PerfThreadInfo]
  order by id desc
go

/*get latest run id*/
select top 1 * 
from Runs
order by id desc

SELECT *
FROM [dbo].[PerfThreadInfo]
where RunId = 101
order by id desc

select p.RunId,  sum(numcreated) as Created, sum(p.ActualRate) as Rate
FROM [dbo].[PerfThreadInfo] p
where RunId = 97 or RunId = 105 or RunId = 106
group by p.RunId
