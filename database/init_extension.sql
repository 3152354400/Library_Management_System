-- ================================================================
-- 初始化脚本：执行所有扩展功能
-- 包含：表创建、字段扩展、存储过程、触发器、视图、索引
-- ================================================================

-- 1. 创建扩展表
@database/tables/RenewRecord.sql
@database/tables/Fine.sql
@database/tables/ReserveBook.sql
@database/tables/Notification.sql

-- 2. 扩展现有表字段
@database/tables/extend_tables.sql

-- 3. 创建索引
@database/index/extension_indexes.sql

-- 4. 创建存储过程
@database/procedures/reader/renew_book.sql
@database/procedures/reader/pay_fine.sql
@database/procedures/reader/get_reader_fines.sql
@database/procedures/reader/reserve_book.sql
@database/procedures/reader/recommend_purchase.sql
@database/procedures/notification/send_notification.sql

-- 5. 修复原有存储过程
@database/functions/reader/calculate_overduefine.sql
@database/procedures/reader/process_overdue.sql

-- 6. 创建触发器
@database/triggers/extension_triggers.sql

-- 7. 创建视图
@database/views/reader/reader_extensions_view.sql
@database/views/admin/admin_management_view.sql

COMMIT;
