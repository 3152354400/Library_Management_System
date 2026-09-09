-- ================================================================
-- 图书收藏表（BookFavorite）
-- 记录读者收藏具体图书的关系（与书单收藏区分）
-- ================================================================
CREATE TABLE BookFavorite (
    FavoriteID INT PRIMARY KEY,
    ReaderID INT NOT NULL,
    ISBN VARCHAR2(20) NOT NULL,
    FavoriteTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Notes VARCHAR2(500),
    FolderName VARCHAR2(50) DEFAULT '默认收藏夹',
    CONSTRAINT fk_fav_reader FOREIGN KEY (ReaderID) REFERENCES Reader(ReaderID),
    CONSTRAINT fk_fav_book FOREIGN KEY (ISBN) REFERENCES BookInfo(ISBN)
);

COMMENT ON TABLE BookFavorite IS '图书收藏表：记录读者收藏的具体图书';
COMMENT ON COLUMN BookFavorite.FavoriteID IS '收藏记录ID，主键';
COMMENT ON COLUMN BookFavorite.ReaderID IS '读者ID，外键';
COMMENT ON COLUMN BookFavorite.ISBN IS '图书ISBN，外键';
COMMENT ON COLUMN BookFavorite.FavoriteTime IS '收藏时间';
COMMENT ON COLUMN BookFavorite.Notes IS '收藏备注';
COMMENT ON COLUMN BookFavorite.FolderName IS '收藏夹名称';

-- 唯一约束：同一读者不能重复收藏同一本书
ALTER TABLE BookFavorite ADD CONSTRAINT uk_reader_isbn UNIQUE (ReaderID, ISBN);

-- 收藏ID序列
CREATE SEQUENCE seq_favorite_id START WITH 1 INCREMENT BY 1;

-- 收藏索引
CREATE INDEX idx_fav_reader ON BookFavorite(ReaderID);
CREATE INDEX idx_fav_folder ON BookFavorite(ReaderID, FolderName);
CREATE INDEX idx_fav_time ON BookFavorite(FavoriteTime DESC);
