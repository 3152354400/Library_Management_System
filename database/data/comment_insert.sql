INSERT INTO COMMENT_TABLE (READERID, ISBN, REVIEWCONTENT, CREATETIME, STATUS)
VALUES (
--    1,                          -- COMMENTID (主键，唯一)
    1,                   -- READERID (读者ID)
   '978-7-04-039663-8',       -- ISBN (书籍ISBN号)
--    5,                       -- RATING (评分，假设是1-5分)
    '这本书内容非常精彩，强烈推荐！作者的观点独到，论述清晰。', -- REVIEWCONTENT (评论内容)
    CURRENT_TIMESTAMP,         -- CREATETIME (使用当前时间戳)
    '正常'                     -- STATUS (评论状态)
);

--INSERT INTO BOOKINFO (ISBN, title, author, stock) VALUES ('978-7-04-039663-8', '高等数学', '同济大学数学系', 1)
--INSERT INTO reader (USERNAME, PASSWORD, FULLNAME, NICKNAME, AVATAR)
--VALUES ('zhangsan', '123456789', '张三', '小三', '/192.168.1.1/zhangsan');

--select * from reader