--insert into report(commentid, readerid, REPORTREASON, REPORTTIME, STATUS, LIBRARIANID)
--values (6, 1, '测试', CURRENT_TIMESTAMP, '待处理', 8)

--ALTER TABLE report MODIFY STATUS varchar(12);

UPDATE report
SET STATUS = '处理完成'
WHERE reportid = 4;
