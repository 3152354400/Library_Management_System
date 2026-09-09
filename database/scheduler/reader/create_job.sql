-- 创建定时任务：每天凌晨2点执行逾期处理存储过程
BEGIN
    DBMS_SCHEDULER.CREATE_JOB (
        job_name           => 'LIBRARY_OVERDUE_JOB',  -- 任务名称（大写）
        job_type           => 'STORED_PROCEDURE',     -- 任务类型：存储过程
        job_action         => 'ProcessOverdueRecords', -- 执行的存储过程
        start_date         => SYSTIMESTAMP,            -- 立即开始
        repeat_interval    => 'FREQ=DAILY;BYHOUR=23;BYMINUTE=36',  -- 每天凌晨4点执行
        end_date           => NULL,                    -- 无结束时间
        enabled            => TRUE,                    -- 创建后立即启用
        comments           => '每日自动处理图书逾期记录'
    );
END;
/