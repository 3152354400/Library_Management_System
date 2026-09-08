<template>
  <div class="container">
    <h1>借阅记录管理系统</h1>
    
    <!-- API基础URL设置 -->
    <div class="form-group">
      <label for="baseUrl">API基础URL:</label>
      <input 
        type="text" 
        id="baseUrl" 
        v-model="baseUrl" 
        placeholder="例如：http://localhost:5000/api"
      />
    </div>
    
    <!-- 功能选项卡（包含借阅图书选项） -->
    <div class="tabs">
      <button 
        @click="activeTab = 'getAll'" 
        :class="{ active: activeTab === 'getAll' }"
      >
        查询所有记录
      </button>
      <button 
        @click="activeTab = 'getById'" 
        :class="{ active: activeTab === 'getById' }"
      >
        按ID查询
      </button>
      <button 
        @click="activeTab = 'getByReader'" 
        :class="{ active: activeTab === 'getByReader' }"
      >
        按读者ID查询
      </button>
      <button 
        @click="activeTab = 'getByBook'" 
        :class="{ active: activeTab === 'getByBook' }"
      >
        按图书ID查询
      </button>
      <button 
        @click="activeTab = 'add'" 
        :class="{ active: activeTab === 'add' }"
      >
        添加记录
      </button>
      <button 
        @click="activeTab = 'update'" 
        :class="{ active: activeTab === 'update' }"
      >
        更新记录
      </button>
      <button 
        @click="activeTab = 'delete'" 
        :class="{ active: activeTab === 'delete' }"
      >
        删除记录
      </button>
      <button 
        @click="activeTab = 'return'" 
        :class="{ active: activeTab === 'return' }"
      >
        归还图书
      </button>
      <button 
        @click="activeTab = 'borrow'" 
        :class="{ active: activeTab === 'borrow' }"
      >
        借阅图书
      </button>
      <button 
        @click="activeTab = 'getByRecentReader'" 
        :class="{ active: activeTab === 'getByRecentReader' }"
      >
        查询当前读者借阅记录
      </button>
    </div>
    
    <!-- 功能内容区域（新增借阅图书标签页） -->
    <div class="tab-content">
      <!-- 查询所有借阅记录 -->
      <div v-show="activeTab === 'getAll'" class="tab-pane">
        <button @click="getAllBorrowRecords" class="btn-primary">
          查询所有记录
        </button>
      </div>
      
      <!-- 按ID查询借阅记录 -->
      <div v-show="activeTab === 'getById'" class="tab-pane">
        <div class="form-group">
          <label for="queryById">借阅记录ID:</label>
          <input 
            type="number" 
            id="queryById" 
            v-model="queryById" 
            placeholder="请输入借阅记录ID"
          />
        </div>
        <button @click="getBorrowRecordById" class="btn-primary">
          查询
        </button>
      </div>
      
      <!-- 按读者ID查询 -->
      <div v-show="activeTab === 'getByReader'" class="tab-pane">
        <div class="form-group">
          <label for="queryByReader">读者ID:</label>
          <input 
            type="text" 
            id="queryByReader" 
            v-model="queryByReader" 
            placeholder="请输入读者ID"
          />
        </div>
        <button @click="getBorrowRecordsByReader" class="btn-primary">
          查询
        </button>
      </div>
      
      <!-- 按图书ID查询 -->
      <div v-show="activeTab === 'getByBook'" class="tab-pane">
        <div class="form-group">
          <label for="queryByBook">图书ID:</label>
          <input 
            type="text" 
            id="queryByBook" 
            v-model="queryByBook" 
            placeholder="请输入图书ID"
          />
        </div>
        <button @click="getBorrowRecordsByBook" class="btn-primary">
          查询
        </button>
      </div>
      
      <!-- 添加借阅记录 -->
      <div v-show="activeTab === 'add'" class="tab-pane">
        <div class="form-group">
          <label for="addBookId">图书ID:</label>
          <input 
            type="text" 
            id="addBookId" 
            v-model="newRecord.bookId" 
            placeholder="请输入图书ID"
          />
        </div>
        <div class="form-group">
          <label for="addReaderId">读者ID:</label>
          <input 
            type="text" 
            id="addReaderId" 
            v-model="newRecord.readerId" 
            placeholder="请输入读者ID"
          />
        </div>
        <div class="form-group">
          <label for="addBorrowTime">借阅时间:</label>
          <input 
            type="datetime-local" 
            id="addBorrowTime" 
            v-model="newRecord.borrowTime"
          />
        </div>
        <div class="form-group">
          <label for="addReturnTime">归还时间:</label>
          <input 
            type="datetime-local" 
            id="addReturnTime" 
            v-model="newRecord.returnTime"
          />
        </div>
        <div class="form-group">
          <label for="addOverdueFine">逾期费用:</label>
          <input 
            type="number" 
            id="addOverdueFine" 
            v-model="newRecord.overdueFine" 
            placeholder="请输入逾期费用"
          />
        </div>
        <button @click="addBorrowRecord" class="btn-primary">
          添加记录
        </button>
      </div>
      
      <!-- 更新借阅记录 -->
      <div v-show="activeTab === 'update'" class="tab-pane">
        <div class="form-group">
          <label for="updateId">记录ID:</label>
          <input 
            type="number" 
            id="updateId" 
            v-model="updateRecord.borrowRecordId" 
            placeholder="请输入要更新的记录ID"
          />
        </div>
        <div class="form-group">
          <label for="updateBookId">图书ID:</label>
          <input 
            type="text" 
            id="updateBookId" 
            v-model="updateRecord.bookId" 
            placeholder="请输入图书ID"
          />
        </div>
        <div class="form-group">
          <label for="updateReaderId">读者ID:</label>
          <input 
            type="text" 
            id="updateReaderId" 
            v-model="updateRecord.readerId" 
            placeholder="请输入读者ID"
          />
        </div>
        <div class="form-group">
          <label for="updateBorrowTime">借阅时间:</label>
          <input 
            type="datetime-local" 
            id="updateBorrowTime" 
            v-model="updateRecord.borrowTime"
          />
        </div>
        <div class="form-group">
          <label for="updateReturnTime">归还时间:</label>
          <input 
            type="datetime-local" 
            id="updateReturnTime" 
            v-model="updateRecord.returnTime"
          />
        </div>
        <div class="form-group">
          <label for="updateOverdueFine">逾期费用:</label>
          <input 
            type="number" 
            id="updateOverdueFine" 
            v-model="updateRecord.overdueFine" 
            placeholder="请输入逾期费用"
          />
        </div>
        <button @click="updateBorrowRecord" class="btn-primary">
          更新记录
        </button>
      </div>
      
      <!-- 删除借阅记录 -->
      <div v-show="activeTab === 'delete'" class="tab-pane">
        <div class="form-group">
          <label for="deleteId">记录ID:</label>
          <input 
            type="number" 
            id="deleteId" 
            v-model="deleteId" 
            placeholder="请输入要删除的记录ID"
          />
        </div>
        <button @click="deleteBorrowRecord" class="btn-danger">
          删除记录
        </button>
      </div>

      <!-- 归还图书标签页 -->
      <div v-show="activeTab === 'return'" class="tab-pane">
        <div class="form-group">
          <label for="returnReaderId">读者ID:</label>
          <input 
            type="text" 
            id="returnReaderId" 
            v-model="returnInfo.readerId" 
            placeholder="请输入读者ID"
          />
        </div>
        <div class="form-group">
          <label for="returnBookId">图书ID:</label>
          <input 
            type="text" 
            id="returnBookId" 
            v-model="returnInfo.bookId" 
            placeholder="请输入图书ID"
          />
        </div>
        <button @click="returnBook" class="btn-primary">
          确认归还
        </button>
      </div>

      <!-- 新增：借阅图书标签页 -->
      <div v-show="activeTab === 'borrow'" class="tab-pane">
        <div class="form-group">
          <label for="borrowReaderId">读者ID:</label>
          <input 
            type="text" 
            id="borrowReaderId" 
            v-model="borrowInfo.readerId" 
            placeholder="请输入读者ID"
          />
        </div>
        <div class="form-group">
          <label for="borrowBookId">图书ID:</label>
          <input 
            type="text" 
            id="borrowBookId" 
            v-model="borrowInfo.bookId" 
            placeholder="请输入图书ID"
          />
        </div>
        <div class="form-group">
          <label>当前系统时间:</label>
          <p class="system-time">{{ currentTime }}</p>
          <small>（将自动作为借阅时间）</small>
        </div>
        <button @click="borrowBook" class="btn-primary">
          确认借阅
        </button>
      </div>  

        <!-- 新增：查询当前读者借阅记录标签页 -->
      <div v-show="activeTab === 'getByRecentReader'" class="tab-pane">
        <div class="form-group">
          <label for="borrowReaderId">读者ID:</label>
          <input 
            type="text" 
            id="queryByRecentReaderId" 
            v-model="queryByRecentReader" 
            placeholder="请输入读者ID"
          />
        </div>
        <button @click="getMyBorrowRecordsByReader" class="btn-primary">
          开始查询
        </button>

        <!-- 新增：查询结果表格 -->
        <div class="result-table" v-if="responseData && activeTab === 'getByRecentReader'">
          <table>
            <thead>
              <tr>
                <th>图书ISBN</th>
                <th>图书名字</th>
                <th>借阅时间</th>
                <th>归还时间</th>
                <th>逾期费用</th>
              </tr>
            </thead>
            <tbody>
              <tr 
                v-for="(item, index) in responseData" 
                :key="index"
              >
                <td>{{ item.isbn }}</td>
                <td>{{ item.title }}</td>
                <td>{{ formatDate(item.borrowTime) }}</td>
                <td>{{ item.returnTime ? formatDate(item.returnTime) : '未归还' }}</td>
                <td>{{ item.overdueFine }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        
      </div>
    </div>
    
    <!-- 结果展示区域 -->
    <div class="result-container">
      <h3>响应结果:</h3>
      
      <div class="status-info">
        <p v-if="responseStatus">
          状态码: <span class="status-code">{{ responseStatus.code }}</span>
          | 状态文本: {{ responseStatus.text }}
          | 请求时间: {{ responseStatus.time }}
        </p>
        <p v-else>暂无请求</p>
      </div>
      
      <div class="response-data">
        <pre v-if="responseData">{{ formattedResponse }}</pre>
        <div v-else>暂无数据</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, toRaw, onMounted } from 'vue';
import axios from 'axios';

// 基础URL
const baseUrl = ref(import.meta.env.VITE_API_BASE_URL);

// 激活的标签页
const activeTab = ref('getAll');

// 查询参数
const queryById = ref('');
const queryByReader = ref('');
const queryByBook = ref('');
const deleteId = ref('');
const queryByRecentReader = ref('');

// 新增记录数据
const newRecord = reactive({
  bookId: '',
  readerId: '',
  borrowTime: new Date().toISOString().slice(0, 16),
  returnTime: null,
  overdueFine: 0
});

// 更新记录数据
const updateRecord = reactive({
  borrowRecordId: '',
  bookId: '',
  readerId: '',
  borrowTime: null,
  returnTime: null,
  overdueFine: 0
});

// 归还图书数据
const returnInfo = reactive({
  readerId: '',
  bookId: ''
});

// 新增：借阅图书数据
const borrowInfo = reactive({
  readerId: '',
  bookId: ''
});

// 新增：显示当前系统时间（用于借阅功能）
const currentTime = ref('');
const updateCurrentTime = () => {
  currentTime.value = new Date().toLocaleString();
};

// 初始化时更新时间并每秒刷新
onMounted(() => {
  updateCurrentTime();
  setInterval(updateCurrentTime, 1000);
});

// 响应结果
const responseData = ref(null);
const responseStatus = ref(null);
const formattedResponse = ref('');

// 通用请求函数（支持查询参数和请求体）
const apiRequest = async (method, url, data = null, params = null) => {
  try {
    const startTime = new Date();
    const fullUrl = `${baseUrl.value}/borrowing${url}`;
    
    // 配置参数（查询参数通过params传递）
    const config = params ? { params } : {};
    
    let response;
    switch(method) {
      case 'get':
        response = await axios.get(fullUrl, config);
        break;
      case 'post':
        response = await axios.post(fullUrl, data, config);
        break;
      case 'put':
        response = await axios.put(fullUrl, data, config);
        break;
      case 'delete':
        response = await axios.delete(fullUrl, config);
        break;
      default:
        throw new Error('不支持的请求方法');
    }
    
    const endTime = new Date();
    
    // 更新响应状态
    responseStatus.value = {
      code: response.status,
      text: response.statusText,
      time: `${endTime - startTime}ms`
    };
    
    // 更新响应数据
    responseData.value = response.data;
    formattedResponse.value = JSON.stringify(response.data, null, 2);
    
    return response.data;
  } catch (error) {
    const endTime = new Date();
    responseStatus.value = {
      code: error.response?.status || '未知',
      text: error.response?.statusText || error.message,
      time: 'N/A'
    };
    
    responseData.value = error.response?.data || { error: error.message };
    formattedResponse.value = JSON.stringify(responseData.value, null, 2);
    
    // 错误提示（根据后端返回的message）
    if (error.response?.data?.message) {
      alert(`操作失败：${error.response.data.message}`);
    } else {
      alert(`操作失败：${error.message}`);
    }
    throw error;
  }
};

// 获取所有借阅记录
const getAllBorrowRecords = async () => {
  await apiRequest('get', '',{withToken:true});
};

// 按ID查询借阅记录
const getBorrowRecordById = async () => {
  if (!queryById.value) {
    alert('请输入借阅记录ID');
    return;
  }
  await apiRequest('get', `/${queryById.value}`,{withToken:true});
};

// 按读者ID查询
const getBorrowRecordsByReader = async () => {
  if (!queryByReader.value) {
    alert('请输入读者ID');
    return;
  }
  await apiRequest('get', `/reader/${queryByReader.value}`,{withToken:true});
};

// 按图书ID查询
const getBorrowRecordsByBook = async () => {
  if (!queryByBook.value) {
    alert('请输入图书ID');
    return;
  }
  await apiRequest('get', `/book/${queryByBook.value}`,{withToken:true});
};

// 添加借阅记录
const addBorrowRecord = async () => {
  if (!newRecord.bookId || !newRecord.readerId || !newRecord.borrowTime) {
    alert('图书ID、读者ID和借阅时间为必填项');
    return;
  }
  
  await apiRequest('post', '', toRaw(newRecord),{withToken:true});
  alert('添加成功');
};

// 更新借阅记录
const updateBorrowRecord = async () => {
  if (!updateRecord.borrowRecordId || !updateRecord.bookId || !updateRecord.readerId) {
    alert('记录ID、图书ID和读者ID为必填项');
    return;
  }
  
  await apiRequest('put', '', toRaw(updateRecord),{withToken:true});
  alert('更新成功');
};

// 删除借阅记录
const deleteBorrowRecord = async () => {
  if (!deleteId.value) {
    alert('请输入要删除的借阅记录ID');
    return;
  }
  
  if (confirm(`确定要删除ID为${deleteId.value}的借阅记录吗？`)) {
    await apiRequest('delete', `/${deleteId.value}`,{withToken:true});
    alert('删除成功');
  }
};

// 归还图书方法
const returnBook = async () => {
  if (!returnInfo.readerId || !returnInfo.bookId) {
    alert('读者ID和图书ID为必填项');
    return;
  }
  
  await apiRequest(
    'post', 
    '/return', 
    null, 
    { readerId: returnInfo.readerId,bookId: returnInfo.bookId },
    {withToken:true}
);
alert (' 归还成功 ');
// 清空表单
returnInfo.readerId = '';
returnInfo.bookId = '';
};

// 新增：借阅图书方法
const borrowBook = async () => {
  // 参数校验
  if (!borrowInfo.readerId || !borrowInfo.bookId) {
    alert('读者 ID 和图书 ID 为必填项');
    return;
  }

  // 调用后端借阅接口，通过查询参数传递 readerId 和 bookId
  await apiRequest(
    'post',
    '/borrow', // 对应后端 BorrowBookAsync 接口的路由
    null, // 无请求体数据（后端自动处理借阅时间）
    {
      readerId: borrowInfo.readerId,
      bookId: borrowInfo.bookId
    },
    {withToken:true}
  );

  alert('借阅成功');
  // 清空表单
  borrowInfo.readerId = '';
  borrowInfo.bookId = '';
};

// 按读者ID查询当前读者的所有借阅记录
const getMyBorrowRecordsByReader = async () => {
  if (!queryByRecentReader.value) {
    alert('请输入读者ID');
    return;
  }
  await apiRequest('get', `/reader/MyBorrowRecords/${queryByRecentReader.value}`,{withToken:true});
};

// 日期格式化函数（用于表格中时间显示）
const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
};
</script>

<style scoped>
/* 原有样式保持不变 */
.container {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  font-family: Arial, sans-serif;
}

h1 {
  text-align: center;
  color: #333;
}

.form-group {
  margin-bottom: 15px;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  box-sizing: border-box;
}

.tabs {
  display: flex;
  flex-wrap: wrap;
  margin-bottom: 20px;
  border-bottom: 1px solid #ccc;
}

.tabs button {
  padding: 10px 15px;
  margin-right: 5px;
  background-color: #f1f1f1;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s;
}

.tabs button.active {
  background-color: #4CAF50;
  color: white;
}

.tab-content .tab-pane {
  padding: 15px;
  border: 1px solid #ccc;
  border-top: none;
  margin-bottom: 20px;
}

button {
  padding: 8px 15px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  color: white;
}

.btn-primary {
  background-color: #4CAF50;
}

.btn-danger {
  background-color: #f44336;
}

.result-container {
  margin-top: 20px;
  padding: 15px;
  background-color: #f9f9f9;
  border-radius: 4px;
}

.status-info {
  margin-bottom: 10px;
  padding: 10px;
  background-color: #e8f5e9;
  border-radius: 4px;
}

.status-code {
  font-weight: bold;
  color: #4CAF50;
}

.response-data pre {
  background-color: #fff;
  padding: 10px;
  border-radius: 4px;
  border: 1px solid #ddd;
  overflow-x: auto;
  white-space: pre-wrap;
  word-wrap: break-word;
}

/* 新增：系统时间显示样式 */
.system-time {
  margin: 5px 0;
  padding: 8px;
  background-color: #f0f8ff;
  border-radius: 4px;
  color: #333;
}

.result-table table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 15px;
}

.result-table th,
.result-table td {
  padding: 8px 10px;
  border: 1px solid #ccc;
  text-align: left;
}

.result-table th {
  background-color: #f5f7fa;
  font-weight: 600;
}

.result-table tr:nth-child(even) {
  background-color: #f9f9f9;
}
</style>