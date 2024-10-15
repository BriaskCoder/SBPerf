SELECT top 10 *
  FROM [dbo].[PerfThreadInfo]
  order by id desc

with lastrun
as
	(
		SELECT [Id],[NumberMessages],[NumberConcurrentCalls],[MinimumDuration],[Size],[Rate],[StartTime],[FinishTime],[Elapsed],
		[QueueName],[TopicName],[NumCreated],[ActualRate],[RunId],[ActualNumberMessages],[ASB_ConnectionString]
		FROM [dbo].[PerfThreadInfo] p
	)
select * from lastrun lr 
where lr.runid = (select top 1 id from Runs order by id desc)
order by lr.id desc



select top 1 * 
from Runs
order by id desc
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
