## 教程：如何向 19. Book_Classify 表 (图书-分类关联表) 插入对应数据

### 0. 说明
- 7月29日 各图书数据已插入完成，category 表未建立，无法向 Book_Classify 导入数据。
- 图书信息使用真实图书信息，大批量导入，为方便category组建立Book_Classify，把图书-分类关系保存在isbn_subject.csv中，以下教程为如何使用这个文件为Book_Classify插入数据
- 为Book_Classify插入数据前请先按照isbn_subject中需要的分类建立分类树，给category插入数据。

### 1. 步骤
```
/* 1. 一次性建临时映射表 */
CREATE TABLE ISBN_SUB_RAW (
    ISBN        VARCHAR2(20),
    SubjectName VARCHAR2(100)
);

/* 2. SQL Developer 导入数据向导
   Tools → Import Data → 选中 isbn_subject.csv → 指到 ISBN_SUB_RAW */

/* 3. 批量写入 Book_Classify，自动跳过重复值 */
INSERT /*+ IGNORE_ROW_ON_DUPKEY_INDEX(Book_Classify, PK_BOOK_CLASSIFY) */
INTO Book_Classify (ISBN, CategoryID)
SELECT r.ISBN,
       c.CategoryID
FROM   ISBN_SUB_RAW r
JOIN   Category c
  ON   TRIM(UPPER(c.CategoryName)) = TRIM(UPPER(r.SubjectName));

COMMIT;

/* 4. （可选）清空或保留临时表 */
TRUNCATE TABLE ISBN_SUB_RAW;
```

### 2.注意
isbn_subject中isbn数比实际插入数据库的isbn多，数据库在插入数据时会自动跳过多余的isbn并弹出提醒