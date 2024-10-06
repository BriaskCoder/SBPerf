SELECT top 1 *
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

select p.RunId,  sum(numcreated) as Created, sum(p.ActualRate) as Rate
FROM [dbo].[PerfThreadInfo] p
where RunId = 97 or RunId = 101
group by p.RunId
