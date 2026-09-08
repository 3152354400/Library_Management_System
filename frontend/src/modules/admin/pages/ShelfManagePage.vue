<template>
  <div class="header-section">
    <h1>书架管理系统</h1>
    <p>管理图书馆书架信息与位置</p>
  </div>

  <div class="shelf-management">
    <div class="action-bar">
      <button class="add-btn" @click="handleAdd">+ 新增书架</button>
      <div class="filter-container">
        <span class="filter-label">筛选条件</span>
        <select v-model="filterBuildingId" class="filter-select" @change="handleFilter">
          <option value="">全部建筑</option>
          <option v-for="building in buildingOptions" :key="building" :value="building">
            {{ building === 21 ? '总图书馆' : '德文图书馆' }}
          </option>
        </select>
        <select v-model="filterFloor" class="filter-select" @change="handleFilter">
          <option value="">全部楼层</option>
          <option v-for="floor in floorOptions" :key="floor" :value="floor">
            {{ floor }}层
          </option>
        </select>
      </div>
    </div>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else class="shelf-table-container">
      <table class="shelf-table">
        <thead>
          <tr>
            <th>书架ID</th>
            <th>楼宇</th>
            <th>书架编码</th>
            <th>楼层</th>
            <th>区域</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="shelf in shelves" :key="shelf.SHELFID">
            <td>{{ shelf.SHELFID || '无信息' }}</td>
            <td>{{ formatBuilding(shelf.BUILDINGID) }}</td>
            <td>{{ shelf.SHELFCODE || '无信息' }}</td>
            <td>{{ shelf.FLOOR || '无信息' }}</td>
            <td>{{ shelf.ZONE || '无信息' }}</td>
            <td>
              <button class="view-btn" @click="handleView(shelf)">查看</button>
              <button class="delete-btn" @click="handleDelete(shelf.SHELFID)">删除</button>
            </td>
          </tr>
          <tr v-if="shelves.length === 0">
            <td colspan="6" class="no-data">暂无书架数据</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!-- 新增书架弹窗 -->
  <div v-if="showDialog" class="modal-mask">
    <div class="modal-container">
      <div class="modal-header">
        <h3>新增书架</h3>
        <button class="close-btn" @click="showDialog = false">&times;</button>
      </div>
      <div class="modal-body">
        <div class="form-group">
          <label>建筑:</label>
          <select v-model="newShelfForm.BUILDINGID" @change="updateOptions">
            <option value="">请选择建筑</option>
            <option value="21">总图书馆</option>
            <option value="22">德文图书馆</option>
          </select>
        </div>
        <div class="form-group">
          <label>楼层:</label>
          <select v-model="newShelfForm.FLOOR" @change="updateShelfCode">
            <option value="">请选择楼层</option>
            <option v-for="floor in availableFloors" :key="floor" :value="floor">{{ floor }}层</option>
          </select>
        </div>
        <div class="form-group">
          <label>区域:</label>
          <select v-model="newShelfForm.ZONE" @change="updateShelfCode">
            <option value="">请选择区域</option>
            <option v-for="zone in availableZones" :key="zone" :value="zone">{{ zone }}区</option>
          </select>
        </div>
        <div class="form-group">
          <label>序号:</label>
          <select v-model="newShelfForm.SEQUENCE" @change="updateShelfCode">
            <option value="">请选择序号</option>
            <option v-for="seq in availableSequences" :key="seq" :value="seq">{{ seq.toString().padStart(3, '0') }}号</option>
          </select>
        </div>
        <div class="form-group">
          <label>书架编码:</label>
          <input v-model="newShelfForm.SHELFCODE" type="text" placeholder="自动生成" readonly class="shelf-code-display">
        </div>
      </div>
      <div class="modal-footer">
        <button class="cancel-btn" @click="showDialog = false">取消</button>
        <button class="confirm-btn" @click="confirmAdd">确认</button>
      </div>
    </div>
  </div>

  <!-- 查看书架书籍弹窗 -->
  <div v-if="showViewDialog" class="modal-mask">
    <div class="modal-container view-modal">
      <div class="modal-header">
        <h3>书架书籍详情 - {{ currentShelf.SHELFCODE }}</h3>
        <button class="close-btn" @click="showViewDialog = false">&times;</button>
      </div>
      <div class="modal-body">
        <div class="shelf-info">
          <p><strong>书架编码：</strong>{{ currentShelf.SHELFCODE }}</p>
          <p><strong>位置：</strong>{{ formatBuilding(currentShelf.BUILDINGID) }} {{ currentShelf.FLOOR }}层 {{ currentShelf.ZONE }}</p>
        </div>
        <div class="books-list">
          <h4>书籍列表</h4>
          <div v-if="loadingBooks" class="loading-books">加载中...</div>
          <div v-else-if="shelfBooks.length === 0" class="no-books">该书架暂无书籍</div>
          <div v-else class="books-container">
            <div v-for="book in shelfBooks" :key="book.BOOKID" class="book-item">
              <p class="book-title">《{{ book.TITLE }}》</p>
              <p class="book-author">作者：{{ book.AUTHOR }}</p>
              <p class="book-status">状态：{{ book.STATUS || '正常' }}</p>
              <p class="book-barcode">馆藏条码：{{ book.BARCODE}}</p>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button class="confirm-btn" @click="showViewDialog = false">关闭</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { getShelf, addShelf, deleteShelf, checkShelfHasBooks, GetShelfBooks } from '@/modules/book/api.js';

const filterBuildingId = ref('');
const filterFloor = ref('');
const showViewDialog = ref(false);
const currentShelf = ref({});
const shelfBooks = ref([]);
const loadingBooks = ref(false);
const buildingOptions = [21, 22];
const floorOptions = Array.from({ length: 14 }, (_, i) => i + 1);

const handleFilter = () => fetchShelves();

const showDialog = ref(false);
const newShelfForm = reactive({
  BUILDINGID: '',
  SHELFCODE: '',
  FLOOR: '',
  ZONE: '',
  SEQUENCE: ''
});
const shelves = ref([]);
const loading = ref(false);

const fetchShelves = async () => {
  try {
    loading.value = true;
    const response = await getShelf('0'); // '0' might be a placeholder for all
    let filteredData = response.data || [];
    
    if (filterBuildingId.value) {
      filteredData = filteredData.filter(item => item.BUILDINGID == filterBuildingId.value);
    }
    if (filterFloor.value) {
      filteredData = filteredData.filter(item => item.FLOOR == filterFloor.value);
    }
    shelves.value = filteredData;
  } catch (error) {
    console.error('获取书架列表失败:', error);
    shelves.value = [];
  } finally {
    loading.value = false;
  }
};

const availableFloors = ref([]);
const availableZones = ref([]);
const availableSequences = ref([]);

const updateOptions = () => {
  if (newShelfForm.BUILDINGID === '21') {
    availableFloors.value = Array.from({ length: 14 }, (_, i) => i + 1);
    availableZones.value = ['A', 'B', 'C', 'D'];
    availableSequences.value = Array.from({ length: 10 }, (_, i) => i + 1);
  } else if (newShelfForm.BUILDINGID === '22') {
    availableFloors.value = Array.from({ length: 2 }, (_, i) => i + 1);
    availableZones.value = ['A', 'B'];
    availableSequences.value = Array.from({ length: 5 }, (_, i) => i + 1);
  } else {
    availableFloors.value = [];
    availableZones.value = [];
    availableSequences.value = [];
  }
  newShelfForm.FLOOR = '';
  newShelfForm.ZONE = '';
  newShelfForm.SEQUENCE = '';
  updateShelfCode();
};

const updateShelfCode = () => {
  if (newShelfForm.FLOOR && newShelfForm.ZONE && newShelfForm.SEQUENCE) {
    const floor = newShelfForm.FLOOR.toString().padStart(2, '0');
    
    const sequence = newShelfForm.SEQUENCE.toString().padStart(3, '0');
    newShelfForm.SHELFCODE = `${floor}${newShelfForm.ZONE}-${sequence}`;
  } else {
    newShelfForm.SHELFCODE = '';
  }
};

const handleAdd = () => {
  resetForm();
  showDialog.value = true;
};

const resetForm = () => {
  Object.assign(newShelfForm, { BUILDINGID: '', SHELFCODE: '', FLOOR: '', ZONE: '', SEQUENCE: '' });
  updateOptions();
};

const confirmAdd = async () => {
  if (!newShelfForm.BUILDINGID || !newShelfForm.SHELFCODE || !newShelfForm.FLOOR || !newShelfForm.ZONE) {
    alert('请填写完整信息');
    return;
  }
  try {
    await addShelf(Number(newShelfForm.BUILDINGID), newShelfForm.SHELFCODE, Number(newShelfForm.FLOOR), newShelfForm.ZONE+'区');
    alert('新增书架成功');
    showDialog.value = false;
    fetchShelves();
  } catch (error) {
    console.error('新增书架失败:', error);
    alert('新增书架失败: ' + (error.response?.data?.message || error.message));
  }
};

const handleDelete = async (shelfId) => {
  if (!confirm('确定要删除这个书架吗？')) return;
  try {
    const numericShelfId = Number(shelfId);
    if (isNaN(numericShelfId)) throw new Error('书架ID格式不正确');
    const response = await checkShelfHasBooks(numericShelfId);
    if (response.data) {
      alert('书架上有图书，请先移除所有图书再删除');
      return;
    }
    await deleteShelf(numericShelfId);
    alert('删除成功');
    fetchShelves();
  } catch (error) {
    console.error('删除失败:', error);
    alert('删除失败: ' + (error.response?.data?.message || error.message));
  }
};

const handleView = async (shelf) => {
  currentShelf.value = shelf;
  showViewDialog.value = true;
  loadingBooks.value = true;
  try {
    const response = await GetShelfBooks(Number(shelf.SHELFID));
    shelfBooks.value = response.data || [];
  } catch (error) {
    console.error('获取书架书籍失败:', error);
    alert('获取书籍失败，请稍后重试');
    shelfBooks.value = [];
  } finally {
    loadingBooks.value = false;
  }
};

const formatBuilding = (buildingId) => {
  if (!buildingId) return '无信息';
  return buildingId == 21 ? '总图书馆' : '德文图书馆';
};

onMounted(fetchShelves);
</script>

<style scoped>
/* Scoped styles from BookshelfManage2.vue */
.shelf-management { padding: 20px; }
.header-section { background: linear-gradient(135deg, #2575fc 0%, #e7e9eeff 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px; }
.header-section h1 { margin: 0 0 8px 0; font-size: 28px; }
.header-section p { margin: 0; opacity: 0.9; }
.action-bar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 15px; }
.add-btn { padding: 8px 16px; background-color: #67c23a; color: white; border: none; border-radius: 4px; cursor: pointer; transition: all 0.3s; }
.filter-container { display: flex; gap: 15px; align-items: center; background: white; padding: 15px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.03); }
.filter-label { font-weight: 600; color: #475569; }
.filter-select { padding: 10px 15px; border: 1px solid #e2e8f0; border-radius: 6px; min-width: 140px; background-color: #f8fafc; }
.loading { padding: 20px; text-align: center; color: #666; }
.shelf-table-container { overflow-x: auto; background-color: #fff; padding: 1rem; border-radius: 8px; }
.shelf-table { width: 100%; border-collapse: collapse; }
.shelf-table th, .shelf-table td { padding: 12px 15px; border-bottom: 1px solid #ddd; text-align: left; }
.shelf-table th { background-color: #f5f5f5; font-weight: bold; }
.shelf-table tr:hover { background-color: #f9f9f9; }
.no-data { text-align: center; color: #999; padding: 20px; }
.view-btn { padding: 6px 12px; background-color: #1890ff; color: white; border: none; border-radius: 4px; cursor: pointer; margin-right: 8px; }
.delete-btn { padding: 6px 12px; background-color: #f56c6c; color: white; border: none; border-radius: 4px; cursor: pointer; }
.modal-mask { position: fixed; inset: 0; background-color: rgba(0, 0, 0, 0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.modal-container { background: white; border-radius: 8px; width: 400px; max-width: 90%; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.33); }
.modal-container.view-modal { width: 600px; }
.modal-header { padding: 15px 20px; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; }
.close-btn { background: none; border: none; font-size: 20px; cursor: pointer; }
.modal-body { padding: 20px; }
.form-group { margin-bottom: 15px; }
.form-group label { display: block; margin-bottom: 5px; font-weight: bold; }
.form-group input, .form-group select { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
.shelf-code-display { background-color: #f5f5f5; color: #666; font-weight: bold; }
.modal-footer { padding: 15px 20px; border-top: 1px solid #eee; text-align: right; }
.cancel-btn, .confirm-btn { padding: 8px 16px; border-radius: 4px; cursor: pointer; border: none; }
.cancel-btn { background-color: #f5f5f5; margin-right: 10px; }
.confirm-btn { background-color: #1890ff; color: white; }
.shelf-info { margin-bottom: 20px; padding: 15px; background-color: #f8f9fa; border-radius: 6px; }
.books-list h4 { margin-bottom: 10px; }
.no-books, .loading-books { text-align: center; color: #999; padding: 20px; }
.books-container { max-height: 300px; overflow-y: auto; }
.book-item { padding: 12px; border: 1px solid #eee; border-radius: 4px; margin-bottom: 10px; background-color: #fff; }
.book-title { font-weight: bold; }
.book-author, .book-status, .book-barcode { margin: 3px 0; color: #666; font-size: 14px; }
</style>