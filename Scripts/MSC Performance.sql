SELECT top 300 *
  FROM [dbo].[PerfThreadInfo]
  order by id desc

select * from [ConsumerInstances]
	  	   	   	  	
with lastrun
as
	(
		SELECT [Id], [NumberThreads],[Sessions],[NumberMessages],[NumberConcurrentCalls],[MinimumDuration],[Size],[Rate],[StartTime],[FinishTime],[Elapsed],
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


USE [MSCResults]
GO

INSERT INTO [dbo].[ConsumerInstances]
([Subscription],[InUse])
     VALUES ('subscription0', 0), 
			('subscription1', 0),
			('subscription2', 0),
			('subscription3', 0),
			('subscription4', 0),
			('subscription5', 0),
			('subscription6', 0),
			('subscription7', 0),
			('subscription8', 0),
			('subscription9', 0),
			('subscription10', 0),
			('subscription11', 0),
			('subscription12', 0),
			('subscription13', 0),
			('subscription14', 0),
			('subscription15', 0),
			('subscription16', 0),
			('subscription17', 0),
			('subscription18', 0),
			('subscription19', 0),
			('subscription20', 0),
			('subscription21', 0),
			('subscription22', 0),
			('subscription23', 0),
			('subscription24', 0),
			('subscription25', 0),
			('subscription26', 0),
			('subscription27', 0),
			('subscription28', 0),
			('subscription29', 0),
			('subscription30', 0),
			('subscription31', 0),
			('subscription32', 0),
			('subscription33', 0),
			('subscription34', 0),
			('subscription35', 0),
			('subscription36', 0),
			('subscription37', 0),
			('subscription38', 0),
			('subscription39', 0),
			('subscription40', 0),
			('subscription41', 0),
			('subscription42', 0),
			('subscription43', 0),
			('subscription44', 0),
			('subscription45', 0),
			('subscription46', 0),
			('subscription47', 0),
			('subscription48', 0),
			('subscription49', 0)--,
--			('subscription50', 0)
GO
truncate table [ConsumerInstances] 
go
select * from [ConsumerInstances]

