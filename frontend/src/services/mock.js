/**
 * Mock 数据中心 —— 当 VITE_USE_MOCK=true 时启用
 * 这里只放 UI 演示所需的最小可信数据,不做后端未实现的业务
 */

const now = () => new Date().toISOString()
const daysFromNow = (n) => new Date(Date.now() + n * 86400000).toISOString()

// —— 通用假数据生成 ——
const sampleBooks = [
  { ISBN: '9787111213826', Title: '深入理解计算机系统', Author: 'Randal E. Bryant', Publisher: '机械工业出版社', PublishYear: 2011, Stock: 5 },
  { ISBN: '9787121362468', Title: '算法导论(原书第3版)', Author: 'Thomas H. Cormen', Publisher: '机械工业出版社', PublishYear: 2013, Stock: 3 },
  { ISBN: '9787111544517', Title: 'JavaScript高级程序设计', Author: 'Nicholas C. Zakas', Publisher: '人民邮电出版社', PublishYear: 2012, Stock: 4 },
  { ISBN: '9787508672069', Title: '人类简史', Author: '尤瓦尔·赫拉利', Publisher: '中信出版社', PublishYear: 2014, Stock: 6 },
  { ISBN: '9787544253994', Title: '百年孤独', Author: '加西亚·马尔克斯', Publisher: '南海出版公司', PublishYear: 2011, Stock: 4 },
  { ISBN: '9787020002207', Title: '红楼梦', Author: '曹雪芹', Publisher: '人民文学出版社', PublishYear: 1996, Stock: 8 },
  { ISBN: '9787508648286', Title: '三体', Author: '刘慈欣', Publisher: '重庆出版社', PublishYear: 2008, Stock: 5 },
  { ISBN: '9787508671536', Title: '未来简史', Author: '尤瓦尔·赫拉利', Publisher: '中信出版社', PublishYear: 2017, Stock: 4 }
]

const sampleCategories = [
  { CategoryID: 'C1', CategoryName: '计算机', ParentCategoryID: '' },
  { CategoryID: 'C2', CategoryName: '文学', ParentCategoryID: '' },
  { CategoryID: 'C3', CategoryName: '历史', ParentCategoryID: '' },
  { CategoryID: 'C11', CategoryName: '程序设计', ParentCategoryID: 'C1' },
  { CategoryID: 'C12', CategoryName: '算法', ParentCategoryID: 'C1' },
  { CategoryID: 'C13', CategoryName: '前端开发', ParentCategoryID: 'C1' },
  { CategoryID: 'C21', CategoryName: '外国文学', ParentCategoryID: 'C2' },
  { CategoryID: 'C22', CategoryName: '中国古典', ParentCategoryID: 'C2' }
]

const sampleAnnouncements = [
  {
    AnnouncementID: 1,
    LibrarianID: 'LIB001',
    Title: '关于2026年国庆期间开馆安排的通知',
    Content: '10月1日至7日,图书馆每日开放时间为 09:00-18:00;自习室照常预约。',
    CreateTime: daysFromNow(-3),
    TargetGroup: '所有人',
    Status: '发布中'
  },
  {
    AnnouncementID: 2,
    LibrarianID: 'LIB001',
    Title: '【紧急】系统维护通知',
    Content: '本周六凌晨02:00-04:00进行系统升级,请合理安排借还书。',
    CreateTime: daysFromNow(-1),
    TargetGroup: '所有人',
    Status: '发布中'
  },
  {
    AnnouncementID: 3,
    LibrarianID: 'LIB002',
    Title: '新书到馆:AI与机器学习专题',
    Content: '近期到馆100余册 AI/ML 新书,欢迎读者借阅。',
    CreateTime: daysFromNow(-7),
    TargetGroup: '读者',
    Status: '发布中'
  }
]

const sampleBorrowRecords = [
  {
    BorrowRecordID: 1001, BookID: 1, Barcode: 'BC001', ISBN: sampleBooks[0].ISBN,
    Title: sampleBooks[0].Title, Author: sampleBooks[0].Author,
    ReaderID: 1, Username: 'reader001', Fullname: '张小明',
    BorrowTime: daysFromNow(-10), DueTime: daysFromNow(20), ReturnTime: null,
    OverdueFine: 0, BorrowStatus: '未归还', BookStatus: '借出'
  },
  {
    BorrowRecordID: 1002, BookID: 2, Barcode: 'BC002', ISBN: sampleBooks[1].ISBN,
    Title: sampleBooks[1].Title, Author: sampleBooks[1].Author,
    ReaderID: 1, Username: 'reader001', Fullname: '张小明',
    BorrowTime: daysFromNow(-5), DueTime: daysFromNow(25), ReturnTime: null,
    OverdueFine: 0, BorrowStatus: '未归还', BookStatus: '借出'
  },
  {
    BorrowRecordID: 1003, BookID: 3, Barcode: 'BC003', ISBN: sampleBooks[2].ISBN,
    Title: sampleBooks[2].Title, Author: sampleBooks[2].Author,
    ReaderID: 1, Username: 'reader001', Fullname: '张小明',
    BorrowTime: daysFromNow(-30), DueTime: daysFromNow(-2), ReturnTime: daysFromNow(-2),
    OverdueFine: 2.5, BorrowStatus: '已归还', BookStatus: '正常'
  }
]

const sampleNotifications = [
  { NotificationId: 1, Title: '您的图书即将到期', Content: '《深入理解计算机系统》将于3天后到期,请及时归还或续借。', Type: 'borrow', Priority: 'high', CreateTime: daysFromNow(-1), IsRead: 'N', ReadTime: null, RelatedType: 'borrow', RelatedId: 1001 },
  { NotificationId: 2, Title: '预约成功', Content: '您已成功预约座位:主楼3层-A12号,有效期至今日22:00。', Type: 'seat', Priority: 'normal', CreateTime: daysFromNow(-2), IsRead: 'Y', ReadTime: daysFromNow(-1), RelatedType: 'seat', RelatedId: null },
  { NotificationId: 3, Title: '新公告:国庆开馆安排', Content: '请查看管理员发布的新公告。', Type: 'system', Priority: 'low', CreateTime: daysFromNow(-3), IsRead: 'N', ReadTime: null, RelatedType: 'announcement', RelatedId: 1 }
]

const sampleFines = [
  { FineID: 1, ReaderID: 1, ReaderName: 'reader001', Amount: 2.5, Reason: '逾期归还《JavaScript高级程序设计》', Status: '待缴纳', CreateTime: daysFromNow(-2), PayTime: null },
  { FineID: 2, ReaderID: 2, ReaderName: 'reader002', Amount: 5.0, Reason: '图书损坏赔偿', Status: '待缴纳', CreateTime: daysFromNow(-1), PayTime: null },
  { FineID: 3, ReaderID: 3, ReaderName: 'reader003', Amount: 1.0, Reason: '逾期1天', Status: '已缴纳', CreateTime: daysFromNow(-10), PayTime: daysFromNow(-9) }
]

const sampleReserves = [
  { ReservationID: 1, ISBN: sampleBooks[5].ISBN, Title: sampleBooks[5].Title, Author: sampleBooks[5].Author, ReserveTime: daysFromNow(-2), Status: '生效中' },
  { ReservationID: 2, ISBN: sampleBooks[6].ISBN, Title: sampleBooks[6].Title, Author: sampleBooks[6].Author, ReserveTime: daysFromNow(-5), Status: '已取消' }
]

const sampleRecommends = [
  { RecommendId: 1, ISBN: '9787111111111', Title: '代码大全(第2版)', Author: 'Steve McConnell', Publisher: '机械工业出版社', PublishYear: 2008, Reason: '软件工程经典', RecommendTime: daysFromNow(-3), Status: '待审核' },
  { RecommendId: 2, ISBN: '9787111111112', Title: '计算机网络:自顶向下方法', Author: 'James F. Kurose', Publisher: '机械工业出版社', PublishYear: 2009, Reason: '网络教材', RecommendTime: daysFromNow(-7), Status: '已采纳' }
]

const sampleFavorites = [
  { FavoriteID: 1, ISBN: sampleBooks[0].ISBN, Title: sampleBooks[0].Title, Author: sampleBooks[0].Author, Notes: '重要参考书', FolderName: '技术书', FavoriteTime: daysFromNow(-15), TotalStock: 5, AvailableStock: 3 },
  { FavoriteID: 2, ISBN: sampleBooks[3].ISBN, Title: sampleBooks[3].Title, Author: sampleBooks[3].Author, Notes: '想再读一遍', FolderName: '课外阅读', FavoriteTime: daysFromNow(-10), TotalStock: 6, AvailableStock: 4 }
]

const sampleBooklists = [
  { BooklistID: 1, BooklistName: '前端必读Top10', BooklistIntroduction: '前端工程师的经典书目清单', CreatorID: 1, CreatorName: 'reader001', BookCount: 5, CollectorCount: 23, CreatedTime: daysFromNow(-30) },
  { BooklistID: 2, BooklistName: 'AI入门书单', BooklistIntroduction: '从入门到进阶,系统学习AI', CreatorID: 2, CreatorName: 'reader002', BookCount: 8, CollectorCount: 47, CreatedTime: daysFromNow(-20) }
]

const sampleComments = [
  { CommentID: 1, ReaderID: 1, ISBN: sampleBooks[0].ISBN, Rating: 5, ReviewContent: '神书,反复读了三遍仍能学到新东西。', CreateTime: daysFromNow(-7), Status: '正常', NickName: '张小明' },
  { CommentID: 2, ReaderID: 2, ISBN: sampleBooks[0].ISBN, Rating: 4, ReviewContent: '内容深入,适合有一定基础的读者。', CreateTime: daysFromNow(-3), Status: '正常', NickName: '李雷' }
]

const sampleReports = [
  { ReportID: 1, CommentID: 99, ReportReason: '人身攻击', ReportTime: daysFromNow(-2), ReportStatus: '待处理', ReviewContent: '辱骂其他读者', Rating: 1, CommentTime: daysFromNow(-3), CommentStatus: '正常', ReporterID: 3, ReporterUsername: 'reader003', CommenterID: 4, CommenterNickname: '韩梅梅', ISBN: sampleBooks[0].ISBN, BookTitle: sampleBooks[0].Title }
]

const sampleBookLoss = [
  { ReportID: 1, ReaderID: 1, ReaderName: 'reader001', BookID: 1, ISBN: sampleBooks[0].ISBN, BookTitle: sampleBooks[0].Title, ReportType: '遗失', ReportTime: daysFromNow(-5), Description: '在校车上遗失', EstimatedValue: 80, CompensationAmount: 0, Status: '待处理', HandleTime: null, HandleResult: '', PaymentStatus: '未缴纳' },
  { ReportID: 2, ReaderID: 2, ReaderName: 'reader002', BookID: 2, ISBN: sampleBooks[1].ISBN, BookTitle: sampleBooks[1].Title, ReportType: '损坏', ReportTime: daysFromNow(-10), Description: '封面严重撕裂', EstimatedValue: 100, CompensationAmount: 50, Status: '已确认', HandleTime: daysFromNow(-8), HandleResult: '按定价50%赔偿', PaymentStatus: '未缴纳' }
]

const sampleSeats = Array.from({ length: 24 }, (_, i) => ({
  SeatID: i + 1, BuildingID: 1, Floor: 3,
  SeatNumber: `${String.fromCharCode(65 + Math.floor(i / 6))}${(i % 6) + 1}`,
  Zone: '自习区', CurrentStatus: i % 5 === 0 ? '已预约' : '空闲'
}))

const sampleMyReservations = [
  { ReservationID: 1, ReaderID: 1, SeatID: 5, BuildingName: '主图书馆', Floor: 3, SeatNumber: 'B2', StartTime: now(), EndTime: daysFromNow(1), Status: '未完成' },
  { ReservationID: 2, ReaderID: 1, SeatID: 8, BuildingName: '主图书馆', Floor: 3, SeatNumber: 'C3', StartTime: daysFromNow(-2), EndTime: daysFromNow(-1), Status: '已完成' }
]

const samplePurchaseAnalysis = {
  TopByBorrowCount: sampleBooks.slice(0, 5).map((b, i) => ({ ISBN: b.ISBN, Title: b.Title, Author: b.Author, BorrowCount: 200 - i * 20 })),
  TopByBorrowDuration: sampleBooks.slice(0, 5).map((b, i) => ({ ISBN: b.ISBN, Title: b.Title, Author: b.Author, TotalDays: 120 - i * 15 })),
  TopByInstanceBorrow: sampleBooks.slice(0, 5).map((b, i) => ({ ISBN: b.ISBN, Title: b.Title, Author: b.Author, UsageRate: 0.95 - i * 0.1 }))
}

const samplePurchaseLogs = [
  { LogID: 1, LogText: '本月采购人工智能类图书30册', CreateTime: daysFromNow(-7) },
  { LogID: 2, LogText: '完成2026年Q3季度图书采购评估', CreateTime: daysFromNow(-15) }
]

const sampleDashboardStats = {
  totalBooks: 12453,
  totalReaders: 3287,
  todayBorrows: 142,
  todayReturns: 89,
  overdueCount: 23,
  pendingFines: 1245.5,
  seatAvailable: 78,
  newBooksThisWeek: 32
}

// —— Handlers ——
// 每个 handler 接收 {method,url,params,data} 返回模拟响应
export const mockHandlers = {
  // 登录
  'auth.login': () => 'mock-token-' + Date.now(),
  'auth.profile': () => ({ userName: 'reader001', fullName: '张小明', nickName: '小明', avatar: 'system_0.png', creditScore: 98, accountStatus: '正常', permission: '普通' }),

  // 图书
  'book.search': ({ params }) => {
    const kw = (params?.keyword || '').toLowerCase()
    if (!kw) return sampleBooks
    return sampleBooks.filter((b) => b.Title.toLowerCase().includes(kw) || b.Author.toLowerCase().includes(kw))
  },
  'book.byId': ({ params }) => ({
    ...sampleBooks[0],
    BookID: params?.id || 1,
    Barcode: 'BC001',
    Status: '正常',
    ShelfID: 1,
    PublishYear: sampleBooks[0].PublishYear,
    Publisher: sampleBooks[0].Publisher
  }),

  // 分类
  'category.tree': () => sampleCategories.filter((c) => !c.ParentCategoryID).map((root) => ({
    ...root,
    Children: sampleCategories.filter((c) => c.ParentCategoryID === root.CategoryID)
  })),

  // 公告
  'announcement.public': () => ({
    Urgent: sampleAnnouncements.filter((a) => a.Title.includes('紧急')),
    Regular: sampleAnnouncements.filter((a) => !a.Title.includes('紧急'))
  }),
  'announcement.all': () => sampleAnnouncements,
  'announcement.create': ({ data }) => {
    const newOne = { AnnouncementID: sampleAnnouncements.length + 1, LibrarianID: 'LIB001', CreateTime: now(), Status: '发布中', ...data }
    sampleAnnouncements.unshift(newOne)
    return newOne
  },
  'announcement.takedown': () => true,

  // 借阅
  'borrowing.myPaged': () => ({
    Items: sampleBorrowRecords,
    TotalCount: sampleBorrowRecords.length,
    PageSize: 10,
    PageNum: 1
  }),
  'borrowing.all': () => sampleBorrowRecords,
  'borrowing.borrow': () => ({ Success: true, Message: '借阅成功', DueTime: daysFromNow(30) }),
  'borrowing.return': () => ({ message: '还书成功' }),

  // 评论
  'comment.byISBN': () => sampleComments,
  'comment.add': () => ({ Message: '评论已提交' }),

  // 通知
  'notification.list': () => ({ Notifications: sampleNotifications, TotalCount: sampleNotifications.length, UnreadCount: 2 }),
  'notification.unread': () => ({ unreadCount: 2 }),
  'notification.markRead': () => ({ success: true }),
  'notification.markAllRead': () => ({ markedCount: 2 }),

  // 罚款
  'fine.myList': () => sampleFines.filter((f) => f.Status === '待缴纳'),
  'fine.mySummary': () => ({ TotalAmount: 2.5, PendingCount: 1, PaidCount: 0 }),
  'fine.pendingAll': () => sampleFines.filter((f) => f.Status === '待缴纳'),
  'fine.pay': () => ({ message: '罚款已缴纳' }),
  'fine.waive': () => ({ success: true }),

  // 预约
  'reserve.list': () => sampleReserves,
  'reserve.create': () => ({ Success: true, Message: '预约成功' }),
  'reserve.cancel': () => ({ message: '已取消' }),

  // 荐购
  'recommend.myList': () => sampleRecommends,
  'recommend.create': () => ({ Success: true, Message: '已提交荐购', RecommendId: 99 }),
  'recommend.pending': () => sampleRecommends,
  'recommend.handle': () => ({ message: '已处理' }),

  // 收藏
  'favorite.list': () => sampleFavorites,
  'favorite.folders': () => [
    { FolderName: '默认收藏夹', Count: 3 },
    { FolderName: '技术书', Count: 5 },
    { FolderName: '课外阅读', Count: 2 }
  ],
  'favorite.check': () => ({ isFavorite: false }),
  'favorite.add': () => ({ message: '已收藏', favoriteId: 99 }),

  // 书单
  'booklist.list': () => sampleBooklists,
  'booklist.byReader': () => sampleBooklists,
  'booklist.create': () => ({ Success: true, BooklistID: 99 }),
  'booklist.recommend': () => sampleBooklists.slice(1),

  // 读者管理
  'reader.list': () => [
    { ReaderID: 1, Name: '张小明', ReaderType: '学生', AccountStatus: '正常', CreditScore: 98, Permission: '普通' },
    { ReaderID: 2, Name: '李雷', ReaderType: '学生', AccountStatus: '正常', CreditScore: 100, Permission: '普通' },
    { ReaderID: 3, Name: '韩梅梅', ReaderType: '教师', AccountStatus: '正常', CreditScore: 95, Permission: '高级' }
  ],

  // 举报
  'report.pending': () => sampleReports,
  'report.handle': () => null,

  // 丢书
  'bookLoss.list': () => sampleBookLoss,
  'bookLoss.report': () => ({ message: '已上报', reportId: 99 }),

  // 采购分析
  'purchaseAnalysis.summary': () => samplePurchaseAnalysis,
  'purchaseAnalysis.logs': () => samplePurchaseLogs,

  // 空间服务
  'space.seats': () => sampleSeats,
  'space.myReservations': () => sampleMyReservations,
  'space.createReservation': () => ({ message: '预约成功' }),
  'space.cancel': () => null,

  // 仪表盘
  'admin.dashboard': () => sampleDashboardStats,
  'reader.dashboard': () => ({
    unreturnedCount: 2,
    overdueCount: 0,
    pendingFineAmount: 0,
    unreadNotification: 2,
    activeReserves: 1,
    favoriteCount: 2,
    creditScore: 98
  })
}