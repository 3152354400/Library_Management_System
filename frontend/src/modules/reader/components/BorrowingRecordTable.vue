<template>
  <div class="records-container">
    <!-- 排序方式 -->
    <div class="sort-container" v-if="records.length">
      <label>排序方式：</label>
      <select v-model="sortOrder">
        <option value="desc">借出时间降序</option>
        <option value="asc">借出时间升序</option>
      </select>
      <span class="hint">📌 点击"续借"按钮可延长借期</span>
    </div>

    <!-- 表格 -->
    <table v-if="records.length" class="styled-table">
      <thead>
        <tr>
          <th>ISBN</th>
          <th>书名</th>
          <th>作者</th>
          <th>借出时间</th>
          <th>应还时间</th>
          <th>归还时间</th>
          <th>状态</th>
          <th>逾期罚金</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(record, index) in paginatedRecords" :key="record.id || index">
          <td>{{ record.ISBN || '' }}</td>
          <td>{{ record.BookTitle || '' }}</td>
          <td>{{ record.BookAuthor || '' }}</td>
          <td>{{ record.BorrowTime ? formatDate(record.BorrowTime) : '' }}</td>
          <td>{{ record.DueTime ? formatDate(record.DueTime) : '-' }}</td>
          <td>
            <span v-if="record.ReturnTime">{{ formatDate(record.ReturnTime) }}</span>
            <span v-else class="text-gray-400">未归还</span>
          </td>
          <td :class="getStatusClass(record)">
            {{ getBorrowStatus(record) }}
          </td>
          <td :class="{ overdue: getOverdueFine(record) > 0 }">
            {{ record.ISBN ? getOverdueFine(record).toFixed(2) : '' }}
          </td>
          <td>
            <button
              v-if="canRenew(record)"
              @click="$emit('renew', record)"
              class="action-btn renew-btn"
              :disabled="renewingId === record.BorrowRecordID"
            >
              {{ renewingId === record.BorrowRecordID ? '续借中' : '续借' }}
            </button>
            <span v-else class="text-gray-400 text-sm">-</span>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- 没有记录时显示 -->
    <p v-else class="no-data">暂无记录</p>

    <!-- 分页控制 -->
    <div class="pagination" v-if="totalPages >= 1">
      <button @click="prevPage" :disabled="currentPage === 1">上一页</button>
      <span>第 {{ currentPage }} 页 / 共 {{ totalPages }} 页</span>
      <button @click="nextPage" :disabled="currentPage === totalPages">下一页</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';

const props = defineProps({
  records: {
    type: Array,
    default: () => []
  },
  pageSize: {
    type: Number,
    default: 10
  },
  renewingId: {
    type: [Number, String],
    default: null
  }
});

defineEmits(['renew']);

const currentPage = ref(1);
const sortOrder = ref("desc");
const fixedRows = 7;

// 总页数
const totalPages = computed(() => {
  return Math.ceil(props.records.length / props.pageSize);
});

// 当前页数据
const paginatedRecords = computed(() => {
  const sorted = [...props.records].sort((a, b) => {
    const timeA = new Date(a.BorrowTime).getTime();
    const timeB = new Date(b.BorrowTime).getTime();
    return sortOrder.value === "desc" ? timeB - timeA : timeA - timeB;
  });
  const start = (currentPage.value - 1) * props.pageSize;
  const pageData = sorted.slice(start, start + props.pageSize);

  const filled = [...pageData];
  while (filled.length < fixedRows) {
    filled.push({});
  }
  return filled;
});

// 分页
const prevPage = () => {
  if (currentPage.value > 1) currentPage.value--;
};
const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++;
};

// 重置分页
watch(() => props.records, () => {
  currentPage.value = 1;
});

// 时间格式化
function formatDate(date) {
  if (!date) return '-';
  return new Date(date).toLocaleString();
}

// 借阅状态
function getBorrowStatus(record) {
  if (!record || !record.ISBN) return '';
  if (record.ReturnTime) {
    // 已归还，判断是否逾期归还
    if (record.DueTime && new Date(record.ReturnTime) > new Date(record.DueTime)) {
      return '逾期归还';
    }
    return '已归还';
  }
  // 未归还
  if (record.DueTime && new Date(record.DueTime) < Date.now()) {
    return '已逾期';
  }
  return '借阅中';
}

// 状态样式
function getStatusClass(record) {
  const status = getBorrowStatus(record);
  if (status === '已逾期') return 'status-overdue';
  if (status === '逾期归还') return 'status-overdue';
  if (status === '已归还') return 'status-returned';
  if (status === '借阅中') return 'status-borrowing';
  return '';
}

// 是否可续借
function canRenew(record) {
  if (!record || !record.ISBN) return false;
  // 已归还的不可续借
  if (record.ReturnTime) return false;
  // 逾期的不能续借（需先归还）
  if (record.DueTime && new Date(record.DueTime) < Date.now()) return false;
  return true;
}

// 计算逾期罚金
function getOverdueFine(record) {
  if (!record || !record.BorrowTime) return 0;

  const borrowTime = new Date(record.BorrowTime).getTime();
  const endTime = record.ReturnTime ? new Date(record.ReturnTime).getTime() : Date.now();
  const dueTime = record.DueTime ? new Date(record.DueTime).getTime() : borrowTime + 30 * 24 * 60 * 60 * 1000;

  if (endTime <= dueTime) return 0;

  const overdueDays = Math.ceil((endTime - dueTime) / (1000 * 60 * 60 * 24));
  return overdueDays * 0.5;
}
</script>

<style scoped>
.records-container {
  padding: 24px;
  background-color: #f9fbfd;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.sort-container {
  margin-bottom: 14px;
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 16px;
}

.sort-container select {
  font-size: 16px;
  padding: 4px 8px;
}

.hint {
  margin-left: auto;
  font-size: 13px;
  color: #6b7280;
}

.styled-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 16px;
  border-radius: 10px;
  overflow: hidden;
}

.styled-table th {
  background-color: #4da6ff;
  color: white;
  text-align: center;
  padding: 14px 8px;
  font-size: 15px;
}

.styled-table td {
  padding: 14px 8px;
  border-bottom: 1px solid #ddd;
  text-align: center;
  height: 58px;
  font-size: 14px;
}

.styled-table tbody tr:nth-child(even) {
  background-color: #f2f8ff;
}

.styled-table tbody tr:hover {
  background-color: #e6f3ff;
}

.no-data {
  text-align: center;
  color: #999;
  padding: 24px;
  font-size: 16px;
}

.pagination {
  margin-top: 18px;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 14px;
  font-size: 16px;
}

.pagination button {
  padding: 8px 14px;
  background-color: #4da6ff;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  font-size: 16px;
}

.pagination button:hover:not(:disabled) {
  background-color: #3399ff;
}

.pagination button:disabled {
  background-color: #b3d9ff;
  cursor: not-allowed;
}

.overdue {
  color: #e57373;
  font-weight: bold;
  font-size: 15px;
}

.status-borrowing {
  color: #2563eb;
  font-weight: bold;
}
.status-returned {
  color: #059669;
  font-weight: bold;
}
.status-overdue {
  color: #dc2626;
  font-weight: bold;
}

.action-btn {
  padding: 4px 10px;
  border-radius: 4px;
  border: none;
  cursor: pointer;
  font-size: 13px;
  font-weight: 500;
  transition: all 0.2s;
}

.renew-btn {
  background-color: #2563eb;
  color: white;
}

.renew-btn:hover:not(:disabled) {
  background-color: #1d4ed8;
}

.renew-btn:disabled {
  background-color: #9ca3af;
  cursor: not-allowed;
}

.text-gray-400 {
  color: #9ca3af;
}

.text-sm {
  font-size: 13px;
}
</style>
