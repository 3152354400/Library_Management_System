-- =================================================================
-- 图书分类关联相关触发器和约束
-- =================================================================

-- 1. 创建检查叶子节点的函数
CREATE OR REPLACE FUNCTION is_leaf_category(p_category_id VARCHAR2)
RETURN NUMBER
IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM Category
    WHERE ParentCategoryID = p_category_id;
    
    RETURN CASE WHEN v_count = 0 THEN 1 ELSE 0 END;
END;
/

-- 2. 创建获取分类路径的函数
CREATE OR REPLACE FUNCTION get_category_path(p_category_id VARCHAR2)
RETURN VARCHAR2
IS
    v_path VARCHAR2(500);
    v_current_id VARCHAR2(20);
    v_category_name VARCHAR2(40);
    v_parent_id VARCHAR2(20);
BEGIN
    v_path := '';
    v_current_id := p_category_id;
    
    WHILE v_current_id IS NOT NULL LOOP
        SELECT CategoryName, ParentCategoryID
        INTO v_category_name, v_parent_id
        FROM Category
        WHERE CategoryID = v_current_id;
        
        IF v_path IS NULL OR v_path = '' THEN
            v_path := v_category_name;
        ELSE
            v_path := v_category_name || ' / ' || v_path;
        END IF;
        
        v_current_id := v_parent_id;
    END LOOP;
    
    RETURN v_path;
END;
/

-- 3. 创建图书分类关联触发器：确保只能绑定到叶子节点
CREATE OR REPLACE TRIGGER trg_book_classify_leaf_check
BEFORE INSERT OR UPDATE ON Book_Classify
FOR EACH ROW
DECLARE
    v_is_leaf NUMBER;
BEGIN
    -- 检查目标分类是否为叶子节点
    v_is_leaf := is_leaf_category(:NEW.CategoryID);
    
    IF v_is_leaf = 0 THEN
        RAISE_APPLICATION_ERROR(-20001, '只能将图书绑定到叶子节点分类');
    END IF;
END;
/

-- 4. 创建视图：图书分类关联详情（不使用函数，直接查询）
CREATE OR REPLACE VIEW book_category_detail_view AS
SELECT 
    bc.ISBN,
    bi.Title as BookTitle,
    bi.Author as BookAuthor,
    bc.CategoryID,
    c.CategoryName,
    c.CategoryName as CategoryPath,
    bc.RelationNote
FROM Book_Classify bc
JOIN BookInfo bi ON bc.ISBN = bi.ISBN
JOIN Category c ON bc.CategoryID = c.CategoryID;

-- 5. 创建视图：叶子节点分类列表（不使用函数）
CREATE OR REPLACE VIEW leaf_categories_view AS
SELECT 
    c.CategoryID,
    c.CategoryName,
    c.CategoryName as CategoryPath
FROM Category c
WHERE NOT EXISTS (
    SELECT 1 FROM Category child 
    WHERE child.ParentCategoryID = c.CategoryID
)
ORDER BY c.CategoryName;

-- 6. 创建视图：分类图书统计（不使用函数）
CREATE OR REPLACE VIEW category_book_stats_view AS
SELECT 
    c.CategoryID,
    c.CategoryName,
    c.CategoryName as CategoryPath,
    COUNT(bc.ISBN) as BookCount
FROM Category c
LEFT JOIN Book_Classify bc ON c.CategoryID = bc.CategoryID
GROUP BY c.CategoryID, c.CategoryName
ORDER BY BookCount DESC, c.CategoryName;
