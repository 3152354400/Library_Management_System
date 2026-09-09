<template>
    <div class="header-section">
    <h1>图书管理系统</h1>
    <p>新增、查看并管理图书馆书籍所属书架信息</p>
  </div>
  <p class="search-tip">请输入要查询或加入书架的书籍</p>
  
  <details class="add-copies-panel">
    <summary class="panel-summary">+ 新书入库 (为已有图书添加副本)</summary>
    <div class="panel-content">
      <div class="form-grid">
        <div class="form-item">
          <label>ISBN (必填)</label>
          <input v-model="addCopyForm.ISBN" type="text" placeholder="输入要入库的图书ISBN">
        </div>
        <div class="form-item">
          <label>入库数量</label>
          <input v-model.number="addCopyForm.NumberOfCopies" type="number" min="1">
        </div>
        <div class="form-item">
          <label>所属楼宇</label>
          <select v-model="addCopyForm.buildingId">
            <option :value="null">请选择楼宇</option>
            <option :value="21">总图书馆</option>
            <option :value="22">德文图书馆</option>
          </select>
        </div>
        <div class="form-item">
          <label>楼层</label>
          <select v-model="addCopyForm.floor">
            <option :value="null">请选择楼层</option>
            <option v-for="floor in availableAddCopyFloors" :key="floor" :value="floor">{{ floor }}层</option>
          </select>
        </div>
        <div class="form-item">
          <label>区域</label>
          <select v-model="addCopyForm.zone">
            <option value="">请选择区域</option>
            <option v-for="zone in availableAddCopyZones" :key="zone" :value="zone">{{ zone }}</option>
          </select>
        </div>
        <div class="form-item">
          <label>书架编号</label>
          <select v-model="addCopyForm.shelfCode">
            <option :value="null">请选择书架</option>
            <option v-for="shelf in availableAddCopyShelves" :key="shelf" :value="shelf">{{ shelf }}号书架</option>
          </select>
        </div>
      </div>
      <div class="panel-footer">
          <button @click="handleAddCopies" class="confirm-btn">确认入库</button>
      </div>
    </div>
  </details>

  <div class="simple-search">
    
    <input
      v-model="searchText"
      type="text"
      placeholder="搜索图书..."
      @keyup.enter="handleSearch"  
    />
    <button @click="handleSearch">
      <i class="search-icon">搜索</i>
    </button>
  </div>
  
  <!-- 搜索结果展示区域 -->
      <!-- 简化后的搜索结果展示区域 -->
  <div v-if="searchPerformed" class="search-results">
    <div v-if="loading" class="loading">搜索中...</div>
    
    <div v-else-if="error" class="error">{{ error }}</div>
    
    <div v-else>
      <div v-if="foundBooks.length === 0" class="no-results">
        未找到"{{ searchText }}"的相关书籍
      </div>
      
      <div v-else class="results-list">
        <h3 class="results-title">搜索结果 (共{{ foundBooks.length }}条)</h3>
        <table class="books-table">
          <thead>
            <tr>
              <th>书名</th>
              <th>馆藏条码</th>
              <th>楼宇</th>
              <th>书架编码</th>
              <th>楼层</th>
              <th>区域</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
                    <tbody>
            <tr v-for="book in foundBooks" :key="book.TITLE">
            <!-- 书名 -->
                  <td>《{{ book.TITLE }}》</td>
                  
                  <!-- 馆藏条码 -->
                  <td>
                    <template v-if="book.BARCODE">
                      {{ book.BARCODE }}
                    </template>
                    <template v-else>
                      无信息
                    </template>
                  </td>
                  <!-- 楼宇ID -->
                  <td>
                    <template v-if="book.BUILDINGID">
                      {{ book.BUILDINGID == 21 ? '总图书馆' : '德文图书馆' }}
                    </template>
                    <template v-else>
                      无信息
                    </template>
                  </td>
                  
                  <!-- 书架编码 -->
                  <td>
                    <template v-if="book.SHELFCODE">
                      {{ book.SHELFCODE }}
                    </template>
                    <template v-else>
                      无信息
                    </template>
                  </td>
                  
                  <!-- 楼层 -->
                  <td>
                    <template v-if="book.FLOOR">
                      {{ book.FLOOR }}
                    </template>
                    <template v-else>
                      无信息
                    </template>
                  </td>
                  
                  <!-- 区域 -->
                  <td>
                    <template v-if="book.ZONE">
                      {{ book.ZONE }}
                    </template>
                    <template v-else>
                      无信息
                    </template>
                  </td>
                  
                                    <!-- 状态 -->
                  <td>
                    <template v-if="book.STATUS">
                      {{ book.STATUS }}
                    </template>
                    <template v-else>
                      下架
                    </template>
                  </td>


                  <!-- 操作 -->
                  <td>
                    
                    <template v-if="book.STATUS === '正常'">
                      <button class="borrow-btn" @click="handleDele(book)">下架</button>
                      <button class="edit-btn" @click="openEditDialog(book)">修改</button>
                    </template>
                    <template v-else-if="book.STATUS === '借出'">
                      <button class="return-btn" @click="handleReturn(book)">修改</button>
                     
                    </template>
                    <!-- 下架状态不显示任何按钮 -->


                  </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

 <!-- 编辑位置弹窗 -->
  <div v-if="showEditDialog" class="edit-dialog">
    <div class="dialog-content">
      <h3>修改书籍位置</h3>
      <div class="form-item">
        <label>所属楼宇：</label>
        <select v-model="editLocation.buildingId">
          <option :value="null">请选择楼宇</option>
          <option :value="21">总图书馆</option>
          <option :value="22">德文图书馆</option>
        </select>
      </div>
      
      <div class="form-item">
        <label>楼层：</label>
        <select v-model="editLocation.floor">
          <option :value=null>请选择楼层</option>
          <option v-for="floor in availableFloors" :key="floor" :value="floor">
            {{ floor }}层
          </option>
        </select>
      </div>
      
      <div class="form-item">
        <label>区域：</label>
        <select v-model="editLocation.zone">
          <option value="">请选择区域</option>
          <option v-for="zone in availableZones" :key="zone" :value="zone">
            {{ zone }}区
          </option>
        </select>
      </div>
      
      <div class="form-item">
        <label>书架编号：</label>
        <select v-model="editLocation.shelfCode">
          <option :value=null>请选择书架</option>
          <option v-for="shelf in availableShelves" :key="shelf" :value="shelf">
            {{ shelf }}号书架
          </option>
        </select>
      </div>
      
      <div class="dialog-buttons">
        <button class="cancel-btn" @click="closeEditDialog">取消</button>
        <button class="confirm-btn" @click="saveLocation">保存</button>
      </div>
    </div>
  </div>

<!-- 归还书籍弹窗 -->
<div v-if="showReturnDialog" class="edit-dialog">
  <div class="dialog-content">
    <h3>修改书籍位置</h3>
    <div class="form-item">
      <label>所属楼宇：</label>
      <select v-model="returnLocation.buildingId">
        <option :value="null">请选择楼宇</option>
        <option :value="21">总图书馆</option>
        <option :value="22">德文图书馆</option>
      </select>
    </div>
    
    <div class="form-item">
      <label>楼层：</label>
      <select v-model="returnLocation.floor">
        <option :value=null>请选择楼层</option>
        <option v-for="floor in availableReturnFloors" :key="floor" :value="floor">
          {{ floor }}层
        </option>
      </select>
    </div>
    
    <div class="form-item">
      <label>区域：</label>
      <select v-model="returnLocation.zone">
        <option value="">请选择区域</option>
        <option v-for="zone in availableReturnZones" :key="zone" :value="zone">
          {{ zone }}区
        </option>
      </select>
    </div>
    
    <div class="form-item">
      <label>书架编号：</label>
      <select v-model="returnLocation.shelfCode">
        <option :value=null>请选择书架</option>
        <option v-for="shelf in availableReturnShelves" :key="shelf" :value="shelf">
          {{ shelf }}号书架
        </option>
      </select>
    </div>
    
    <div class="dialog-buttons">
      <button class="cancel-btn" @click="closeReturnDialog">取消</button>
      <button class="confirm-btn" @click="saveReturnLocation">保存</button>
    </div>
  </div>
</div>

</template>

<script setup>
import { ref, reactive,computed,onMounted } from 'vue'
import axios from 'axios'
import { getBooks, getBooksBookShelf, checkShelfExists, returnBook, findShelfId, borrowBook, addBookCopies } from '@/modules/book/api.js'
const searchText = ref('')
const loading = ref(false)
const error = ref('')
const searchPerformed = ref(false) // 标记是否已执行过搜索
const foundBooks = ref([]) // 改为存储多个书籍的数组
const showEditDialog = ref(false)



// 编辑位置表单
const editLocation = reactive({
  buildingId: null,
  floor: null,
  zone: '',
  shelfwhich: null
})
// 根据选择的楼宇计算可用楼层
const availableFloors = computed(() => {
  if (editLocation.buildingId === 21) {
    return Array.from({length: 14}, (_, i) => i + 1)
  } else if (editLocation.buildingId === 22) {
    return Array.from({length: 2}, (_, i) => i + 1)
  }
  return []
})

// 根据选择的楼宇计算可用区域
const availableZones = computed(() => {
  if (editLocation.buildingId === 21) {
    return ['A', 'B', 'C', 'D']
  } else if (editLocation.buildingId === 22) {
    return ['A', 'B']
  }
  return []
})

// 根据选择的楼宇计算可用书架
const availableShelves = computed(() => {
  if (editLocation.buildingId === 21) {
    return Array.from({length: 10}, (_, i) => i + 1)
  } else if (editLocation.buildingId === 22) {
    return Array.from({length: 5}, (_, i) => i + 1)
  }
  return []
})


const handleSearch = async () => {

  
  loading.value = true
  error.value = ''
  searchPerformed.value = true
  foundBooks.value = []
  
  try {
    // 第一步：检查书籍是否存在
    
    const bookResponse = await getBooks(searchText.value.trim() ? searchText.value.toLowerCase() : '%')
    const books = bookResponse.data || []
    
    if (books.length === 0) {
      return // 没有找到书籍，直接返回
    }
    
    // 第二步：查询书架信息
    const shelfResponse = await getBooksBookShelf(searchText.value.trim() ? searchText.value.toLowerCase() : '%')
    
    // 合并结果
    books.forEach(book => {
      const shelfInfos = shelfResponse.data?.filter(item => item.TITLE === book.Title) || []
      
      if (shelfInfos.length === 0) {
        foundBooks.value.push({
          TITLE: book.Title,
          BARCODE:null,
          SHELFID: null,
          BUILDINGID: null,
          SHELFCODE: null,
          FLOOR: null,
          ZONE: null,
          STATUS: null,
          BOOKID: null
        })
      } else {
        shelfInfos.forEach(shelfInfo => {
          foundBooks.value.push({
            TITLE: book.Title,
            BARCODE: shelfInfo.BARCODE || null,
            SHELFID: shelfInfo.SHELFID || null,
            BUILDINGID: shelfInfo.BUILDINGID || null,
            SHELFCODE: shelfInfo.SHELFCODE || null,
            FLOOR: shelfInfo.FLOOR || null,
            ZONE: shelfInfo.ZONE || null,
            STATUS: shelfInfo.STATUS || null,
            BOOKID: shelfInfo.BOOKID || null
          })
        })
      }
    })
  } catch (err) {
    console.error('搜索失败:', err)
    error.value = '搜索失败，请稍后重试'
  } finally {
    loading.value = false
  }
}
// 添加显示所有书籍的函数
const showAllBooks = async () => {
  loading.value = true
  error.value = ''
  searchPerformed.value = true
  foundBooks.value = []
  
  try {
    const bookResponse = await getBooks('%')
    const books = bookResponse.data || []
    
    const shelfResponse = await getBooksBookShelf('%')
    
    books.forEach(book => {
      const shelfInfos = shelfResponse.data?.filter(item => item.TITLE === book.Title) || []
      
      if (shelfInfos.length === 0) {
        foundBooks.value.push({
          TITLE: book.Title,
          BARCODE:null,
          SHELFID: null,
          BUILDINGID: null,
          SHELFCODE: null,
          FLOOR: null,
          ZONE: null,
          STATUS: null,
          BOOKID: null
        })
      } else {
        shelfInfos.forEach(shelfInfo => {
          foundBooks.value.push({
            TITLE: book.Title,
            BARCODE: shelfInfo.BARCODE || null,
            SHELFID: shelfInfo.SHELFID || null,
            BUILDINGID: shelfInfo.BUILDINGID || null,
            SHELFCODE: shelfInfo.SHELFCODE || null,
            FLOOR: shelfInfo.FLOOR || null,
            ZONE: shelfInfo.ZONE || null,
            STATUS: shelfInfo.STATUS || null,
            BOOKID: shelfInfo.BOOKID || null
          })
        })
      }
    })
  } catch (err) {
    console.error('获取所有书籍失败:', err)
    error.value = '获取书籍失败，请稍后重试'
  } finally {
    loading.value = false
  }
}
onMounted(() => {
  showAllBooks()
})


// 打开编辑弹窗
const openEditDialog = (book) => {
  currentBook.value = book;
  showEditDialog.value = true
}

// 关闭编辑弹窗
const closeEditDialog = () => {
  showEditDialog.value = false
  currentBook.value = null;
  resetEditForm()
  handleSearch()
}
// 重置编辑表单
const resetEditForm = () => {
  editLocation.buildingId = ''
  editLocation.floor = ''
  editLocation.zone = ''
  editLocation.shelfCode = ''
}

// 格式化书架编码
const formatShelfCode = (shelfNumber) => {
  if (!shelfNumber) return null;
  
  const buildingId = editLocation.buildingId || returnLocation.buildingId;
  const floor = (editLocation.floor || returnLocation.floor).toString().padStart(2, '0');
  const zone = editLocation.zone || returnLocation.zone;
  const shelfNum = shelfNumber.toString().padStart(3, '0');
  
  return `${floor}${zone}-${shelfNum}`;
};

const saveLocation = async () => {
  if (!editLocation.buildingId || !editLocation.floor || 
      !editLocation.zone || !editLocation.shelfCode) {
    alert('请填写完整的位置信息');
    return;
  }
  const formattedShelfCode = formatShelfCode(editLocation.shelfCode);
  try {
    const zoneForBackend = editLocation.zone.includes('区') ? editLocation.zone : editLocation.zone + '区';
    const { data: shelfExists } = await checkShelfExists(
      editLocation.buildingId,
      formattedShelfCode,
      editLocation.floor,
      zoneForBackend
    );
    
    if (!shelfExists) {
      alert('指定位置的书架不存在');
      return;
    }

    const { data: shelfId } = await findShelfId(
      editLocation.buildingId,
      formattedShelfCode,
      editLocation.floor,
      zoneForBackend
    );
    
    await returnBook(currentBook.value.BOOKID, shelfId);
    alert('位置更新成功');
    closeEditDialog();
    
  } catch (error) {
    console.error('更新失败:', error);
    alert('更新失败: ' + (error.response?.data || error.message));
  }
}

const handleDele = async (book) => {
  try {
    if (!confirm(`确定要下架《${book.TITLE}》吗？`)) {
      return
    }
    
    await borrowBook(book.BOOKID)
    alert('下架成功')
    
    await handleSearch()
  } catch (error) {
    console.error('下架失败:', error)
    alert('下架失败: ' + (error.response?.data || error.message))
  }
}

const handleReturn = async (book) => {
  openReturnDialog(book);
};

const showReturnDialog = ref(false)
const currentBook = ref(null);
const returnLocation = reactive({
  buildingId: null,
  floor: null,
  zone: '',
  shelfCode: ''
})

const availableReturnFloors = computed(() => {
  if (returnLocation.buildingId === 21) {
    return Array.from({length: 14}, (_, i) => i + 1)
  } else if (returnLocation.buildingId === 22) {
    return Array.from({length: 2}, (_, i) => i + 1)
  }
  return []
})

const availableReturnZones = computed(() => {
  if (returnLocation.buildingId === 21) {
    return ['A', 'B', 'C', 'D']
  } else if (returnLocation.buildingId === 22) {
    return ['A', 'B']
  }
  return []
})

const availableReturnShelves = computed(() => {
  if (returnLocation.buildingId === 21) {
    return Array.from({length: 10}, (_, i) => i + 1)
  } else if (returnLocation.buildingId === 22) {
    return Array.from({length: 5}, (_, i) => i + 1)
  }
  return []
})

const openReturnDialog = (book) => {
   currentBook.value = book;
   console.log(currentBook.value);
  showReturnDialog.value = true

}

const closeReturnDialog = () => {
  showReturnDialog.value = false
  currentBook.value = null;
  resetReturnForm()
  handleSearch()
}

const resetReturnForm = () => {
  returnLocation.buildingId = null
  returnLocation.floor = null
  returnLocation.zone = ''
  returnLocation.shelfCode = ''
}

const saveReturnLocation = async () => {
  if (!returnLocation.buildingId || !returnLocation.floor || 
      !returnLocation.zone || !returnLocation.shelfCode) {
    alert('请填写完整的位置信息');
    return;
  }
  
  try {
    const formattedShelfCode = formatShelfCode(returnLocation.shelfCode);
     const zoneForBackend = returnLocation.zone.includes('区') ? returnLocation.zone : returnLocation.zone + '区';
    const { data: shelfExists } = await checkShelfExists(
      returnLocation.buildingId,
      formattedShelfCode,
      returnLocation.floor,
      zoneForBackend
    );
    
    if (!shelfExists) {
      alert('指定位置的书架不存在');
      return;
    }

    const { data: shelfId } = await findShelfId(
      returnLocation.buildingId,
      formattedShelfCode,
      returnLocation.floor,
      zoneForBackend
    );
    
    await returnBook(currentBook.value.BOOKID, shelfId);
    alert('修改成功');
    closeReturnDialog();
    
  } catch (error) {
    console.error('修改失败:', error);
    alert('修改失败: ' + (error.response?.data || error.message));
  }
}

const addCopyForm = reactive({
  ISBN: '',
  NumberOfCopies: 1,
  buildingId: null,
  floor: null,
  zone: '',
  shelfCode: null
});

// 为入库表单创建独立的计算属性，以避免与“修改”和“归还”弹窗冲突
const availableAddCopyFloors = computed(() => {
  if (addCopyForm.buildingId === 21) return Array.from({ length: 14 }, (_, i) => i + 1);
  if (addCopyForm.buildingId === 22) return Array.from({ length: 2 }, (_, i) => i + 1);
  return [];
});
const availableAddCopyZones = computed(() => {
  if (addCopyForm.buildingId === 21) return ['A区', 'B区', 'C区', 'D区'];
  if (addCopyForm.buildingId === 22) return ['A区', 'B区'];
  return [];
});
const availableAddCopyShelves = computed(() => {
  if (addCopyForm.buildingId === 21) return Array.from({ length: 10 }, (_, i) => i + 1);
  if (addCopyForm.buildingId === 22) return Array.from({ length: 5 }, (_, i) => i + 1);
  return [];
});

async function handleAddCopies() {
  const form = addCopyForm;
  if (!form.ISBN || !form.NumberOfCopies || !form.buildingId || !form.floor || !form.zone || !form.shelfCode) {
    return alert('请填写入库表单的完整信息！');
  }

  const floorStr = String(form.floor).padStart(2, '0');
  const shelfNumStr = String(form.shelfCode).padStart(3, '0');

  const formattedShelfCode = `${floorStr}${form.zone.replace('区', '')}-${shelfNumStr}`;

  try {
    const { data: shelfExists } = await checkShelfExists(form.buildingId, formattedShelfCode, form.floor, form.zone);
    if (!shelfExists) return alert('指定的目标书架不存在，请在“书架管理”中创建。');

    const { data: shelfId } = await findShelfId(form.buildingId, formattedShelfCode, form.floor, form.zone);
    if (!shelfId) return alert('无法找到目标书架的ID。');

    await addBookCopies({
      ISBN: form.ISBN,
      NumberOfCopies: form.NumberOfCopies,
      ShelfID: shelfId
    });

    alert('新书入库成功！');
    resetAddCopyForm();
    // 刷新整个列表，或者只刷新当前搜索的列表
    if (searchText.value) {
        handleSearch();
    } else {
        showAllBooks();
    }
  } catch (error) {
    console.error('入库失败:', error);
    alert('入库失败: ' + (error.response?.data?.message || error.message));
  }
}

function resetAddCopyForm() {
    addCopyForm.ISBN = '';
    addCopyForm.NumberOfCopies = 1;
    addCopyForm.buildingId = null;
    addCopyForm.floor = null;
    addCopyForm.zone = '';
    addCopyForm.shelfCode = null;
}

</script>

<style scoped>
.search-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}
.search-tip { color: #666; font-size: 16px; margin-bottom: 8px; }
.simple-search { display: flex; width: 100%; max-width: 500px; border: 1px solid #ddd; border-radius: 4px; overflow: hidden; margin-bottom: 20px; }
.simple-search input { flex: 1; padding: 10px 15px; border: none; outline: none; font-size: 16px; }
.simple-search button { padding: 0 20px; border: none; background: #1890ff; color: white; cursor: pointer; transition: background 0.3s; }
.simple-search button:hover { background: #40a9ff; }
.search-icon { font-size: 16px; }
.search-results { margin: 30px 0; border: 1px solid #eee; border-radius: 8px; padding: 20px; background: white; }
.loading, .error, .no-results { text-align: center; padding: 20px; color: #666; }
.error { color: #f5222d; }
.results-title { margin-bottom: 15px; color: #333; font-size: 18px; }
.books-table { width: 100%; border-collapse: collapse; table-layout: fixed; }
.books-table th, .books-table td { padding: 12px 15px; border-bottom: 1px solid #eee; text-align: left; }
.books-table th:nth-child(1), .books-table td:nth-child(1) { width: 25%; }
.books-table th:nth-child(2), .books-table td:nth-child(2),
.books-table th:nth-child(3), .books-table td:nth-child(3),
.books-table th:nth-child(4), .books-table td:nth-child(4) { width: 10%; }
.books-table th:nth-child(5), .books-table td:nth-child(5),
.books-table th:nth-child(6), .books-table td:nth-child(6),
.books-table th:nth-child(7), .books-table td:nth-child(7) { width: 5%; }
.books-table th:nth-child(8), .books-table td:nth-child(8) { width: 10%; min-width: 120px; }
.books-table th { background-color: #f7f7f7; font-weight: 600; }
.books-table tr:hover { background-color: #f5f5f5; }
.edit-btn { padding: 6px 12px; background-color: #1890ff; color: white; border: none; border-radius: 4px; cursor: pointer; transition: background-color 0.3s; }
.edit-btn:hover { background-color: #40a9ff; }
.edit-dialog { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background-color: rgba(0, 0, 0, 0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; }
.dialog-content { background-color: white; padding: 20px; border-radius: 8px; width: 400px; max-width: 90%; }
.dialog-content h3 { margin-top: 0; margin-bottom: 20px; color: #333; }
.form-item { margin-bottom: 15px; }
.form-item label { display: block; margin-bottom: 5px; font-weight: 500; }
.form-item select { width: 100%; padding: 8px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; }
.dialog-buttons { display: flex; justify-content: flex-end; margin-top: 20px; }
.dialog-buttons button { padding: 8px 16px; margin-left: 10px; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; }
.cancel-btn { background-color: #f5f5f5; color: #666; }
.cancel-btn:hover { background-color: #e8e8e8; }
.confirm-btn { background-color: #1890ff; color: white; }
.confirm-btn:hover { background-color: #40a9ff; }
.borrow-btn { padding: 6px 12px; background-color: #67c23a; color: white; border: none; border-radius: 4px; cursor: pointer; margin-right: 8px; }
.borrow-btn:hover { background-color: #85ce61; }
.return-btn { padding: 6px 12px; background-color: #1890ff; color: white; border: none; border-radius: 4px; cursor: pointer; margin-right: 8px; }
.return-btn:hover { background-color: #40a9ff; }
.header-section { background: linear-gradient(135deg, #2575fc 0%, #e7e9eeff 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 25px; }
.header-section h1 { margin: 0 0 8px 0; font-size: 28px; }
.header-section p { margin: 0; opacity: 0.9; }
.shelf-management { padding: 25px; background-color: #f8fafc; min-height: 100vh; }
.add-copies-panel {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  margin-bottom: 1.5rem;
}
.panel-summary {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.25rem;
  font-weight: 600;
  cursor: pointer;
  color: #1890ff;
}
.panel-summary::-webkit-details-marker {
  display: none;
}
.panel-content {
  padding: 1.25rem;
  border-top: 1px solid #e5e7eb;
}
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 1rem;
}
.form-item label {
  font-size: 14px;
}
.panel-footer {
  margin-top: 1rem;
  text-align: right;
}
</style>