using System;
using System.Collections.Generic;

namespace backend.DTOs.Reader
{
    // 用于表示单个座位及其状态
    public class SeatDto
    {
        public int SeatID { get; set; }
        public int BuildingID { get; set; }
        public int Floor { get; set; }
        public string SeatNumber { get; set; }
        public string Zone { get; set; }
        public string CurrentStatus { get; set; }
    }

    // 用于创建新的座位预约
    public class CreateSeatReservationDto
    {
        public int SeatID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    // 用于展示我的预约列表
    public class MyReservationDto
    {
        public int ReservationID { get; set; }
        public long ReaderID { get; set; } // <-- 新增这一行，并使用 long
        public int SeatID { get; set; }
        public string BuildingName { get; set; } = string.Empty; // 添加默认值
        public int Floor { get; set; }
        public string SeatNumber { get; set; } = string.Empty; // 添加默认值
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty; // 添加默认值
    }
}