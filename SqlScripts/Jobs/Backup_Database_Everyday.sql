USE [msdb]
GO

/****** Object:  Job [Backup_Database_Everyday]    Script Date: 12/15/2021 4:36:27 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 12/15/2021 4:36:27 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Backup_Database_Everyday', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Uncategorized (Local)]', 
		--@owner_login_name=N'WIN-1IM5PTUG78E\Administrator', 
		@job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [execute_backup]    Script Date: 12/15/2021 4:36:27 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'execute_backup', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'DECLARE @DbName NVARCHAR(100) ,
    @path NVARCHAR(100) = ''F:\backup\Verp\'' ,
    @DayName NVARCHAR(5) ;
SELECT  @DayName = ''T'' + CAST(DATEPART(dw, GETDATE()) AS NVARCHAR(5)) ;
SET @path = @path+@DayName +''\'';
EXEC master.dbo.xp_create_subdir @path
DECLARE database_cursor CURSOR LOCAL
FOR
    SELECT  name
    FROM    sys.sysdatabases
    WHERE   name IN (''MasterDB'',''StockDB'',''StockDB'',''OrganizationDB'',''AccountancyDB'',''PurchaseOrderDB'',''ReportConfigDB'')

		
OPEN database_cursor ;
FETCH NEXT FROM database_cursor INTO @DbName ;
WHILE @@FETCH_STATUS = 0 
BEGIN
		

        DECLARE @Sql NVARCHAR(200) = ''BACKUP DATABASE ['' + @DbName
            + ''] TO DISK = '''''' + @path + @DayName + ''_'' + @DbName + ''.bak''
            + '''''' WITH INIT'' ;
        EXEC(@Sql) ;
        FETCH NEXT FROM database_cursor INTO @DbName ;
END
CLOSE database_cursor ;
DEALLOCATE database_cursor ;', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'every23pm', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20200827, 
		@active_end_date=99991231, 
		@active_start_time=230000, 
		@active_end_time=235959, 
		@schedule_uid=N'f64b1fd7-0a57-44d8-b3f2-8b96c16bd8f8'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO


