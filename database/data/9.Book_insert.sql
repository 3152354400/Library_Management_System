-- bookinfo与bookshelf数据插入后，按每个书架10本书插入book表

INSERT INTO Book (Barcode, Status, ShelfID, ISBN)
WITH shelves AS (
  -- 给每个书架编号：决定分配顺序（可按需调整 ORDER BY）
  SELECT
    ShelfID,
    ROW_NUMBER() OVER (ORDER BY BuildingID, Floor, ShelfCode, ShelfID) AS rn
  FROM Bookshelf
),
expanded AS (
  -- 按 Stock 将每个 ISBN 展开为逐本记录，并给所有“册”排全局序
  SELECT
    t.ISBN,
    ROW_NUMBER() OVER (ORDER BY t.ISBN) AS global_rn   -- 想按书名可改为 ORDER BY t.Title, t.ISBN
  FROM (
    SELECT bi.ISBN
    FROM BookInfo bi
    CONNECT BY PRIOR bi.ISBN = bi.ISBN
           AND PRIOR SYS_GUID() IS NOT NULL
           AND LEVEL <= bi.Stock
  ) t
),
pairing AS (
  -- 每 10 本映射到一个书架：1~10 → rn=1；11~20 → rn=2；以此类推
  SELECT e.ISBN, s.ShelfID
  FROM expanded e
  JOIN shelves s
    ON CEIL(e.global_rn / 10) = s.rn
)
SELECT
  NULL           AS Barcode,         -- 交给触发器自动生成；若无触发器，可改成你需要的条码
  '正常'         AS Status,
  p.ShelfID,
  p.ISBN
FROM pairing p;
