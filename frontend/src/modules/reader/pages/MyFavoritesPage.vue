<template>
  <Layout>
    <h1 class="title">❤️ 我的收藏</h1>

    <div class="tabs">
      <button
        v-for="f in folders"
        :key="f.FolderName || 'all'"
        :class="['tab-btn', { active: activeFolder === (f.FolderName || 'all') }]"
        @click="changeFolder(f.FolderName)"
      >
        {{ f.FolderName || '全部' }}
        <span class="count">({{ f.Count }})</span>
      </button>
    </div>

    <div class="actions">
      <button class="btn-add-folder" @click="showAddFolderDialog = true">📁 新建收藏夹</button>
      <button class="btn-refresh" @click="loadData">🔄 刷新</button>
    </div>

    <div v-if="loading" class="loading">加载中...</div>
    <div v-else-if="favorites.length === 0" class="empty-state">
      <p>暂无收藏 ❤️</p>
      <p class="hint">在图书详情页点击"收藏"按钮即可添加</p>
    </div>
    <div v-else class="favorites-grid">
      <div v-for="f in favorites" :key="f.FavoriteID" class="favorite-card">
        <div class="card-cover">
          <img
            :src="`/covers/${f.ISBN}.jpg`"
            alt="封面"
            @error="e => (e.target.src = defaultCover)"
          />
        </div>
        <div class="card-info">
          <h3 class="book-title" @click="goToDetail(f.ISBN)">{{ f.Title }}</h3>
          <p class="book-author">作者：{{ f.Author || '-' }}</p>
          <p class="book-stock">
            <span :class="f.AvailableStock > 0 ? 'text-green' : 'text-red'">
              {{ f.AvailableStock > 0 ? `${f.AvailableStock} 本可借` : '当前无可借' }}
            </span>
            / 共 {{ f.TotalStock || 0 }} 本
          </p>
          <p class="favorite-time">收藏于 {{ formatDate(f.FavoriteTime) }}</p>
          <p class="folder-tag" v-if="f.FolderName">📁 {{ f.FolderName }}</p>
          <div class="card-actions">
            <button class="btn-edit" @click="openEditNotes(f)">✏️ 备注</button>
            <button class="btn-remove" @click="removeFavorite(f)">🗑️ 取消收藏</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 备注编辑弹窗 -->
    <div v-if="editDialog.show" class="modal-overlay" @click.self="editDialog.show = false">
      <div class="modal-content">
        <button @click="editDialog.show = false" class="close">&times;</button>
        <h3>✏️ 编辑收藏备注</h3>
        <p class="book-name">《{{ editDialog.favorite?.Title }}》</p>
        <div class="form-group">
          <label>收藏夹</label>
          <input v-model="editDialog.folderName" placeholder="收藏夹名称" class="form-input" />
        </div>
        <div class="form-group">
          <label>备注</label>
          <textarea v-model="editDialog.notes" rows="4" placeholder="添加备注..." maxlength="500" class="form-textarea"></textarea>
        </div>
        <div class="modal-actions">
          <button @click="editDialog.show = false" class="btn-cancel">取消</button>
          <button @click="submitEdit" :disabled="editDialog.submitting" class="btn-save">
            {{ editDialog.submitting ? '保存中...' : '保存' }}
          </button>
        </div>
      </div>
    </div>

    <!-- 新建收藏夹弹窗 -->
    <div v-if="showAddFolderDialog" class="modal-overlay" @click.self="showAddFolderDialog = false">
      <div class="modal-content">
        <button @click="showAddFolderDialog = false" class="close">&times;</button>
        <h3>📁 新建收藏夹</h3>
        <p class="hint-text">提示：新建收藏夹后，可在图书详情页收藏时选择该收藏夹</p>
        <div class="form-group">
          <label>收藏夹名称</label>
          <input v-model="newFolderName" placeholder="例如：技术书、文学..." class="form-input" />
        </div>
        <div class="modal-actions">
          <button @click="showAddFolderDialog = false" class="btn-cancel">取消</button>
          <button @click="addFolder" :disabled="!newFolderName.trim()" class="btn-save">创建</button>
        </div>
      </div>
    </div>
  </Layout>

  <teleport to="body">
    <div v-if="showToast" class="toast">{{ toastMsg }}</div>
  </teleport>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { getMyFavorites, removeFavorite as apiRemoveFavorite, removeFavoriteByISBN, updateFavoriteNotes, getFavoriteFolders } from '@/modules/reader/api.js'
import Layout from '@/modules/reader/reader_DashBoard_layout/layout.vue'

const router = useRouter()
const favorites = ref([])
const folders = ref([{ FolderName: 'all', Count: 0 }])
const loading = ref(false)
const activeFolder = ref('all')
const showAddFolderDialog = ref(false)
const newFolderName = ref('')
const showToast = ref(false)
const toastMsg = ref('')

const defaultCover = new URL('@/assets/book_cover_default.jpg', import.meta.url).href

const editDialog = reactive({
  show: false,
  favorite: null,
  folderName: '',
  notes: '',
  submitting: false
})

async function loadData() {
  loading.value = true
  try {
    const folderParam = activeFolder.value === 'all' ? null : activeFolder.value
    const [favRes, folderRes] = await Promise.all([
      getMyFavorites(folderParam),
      getFavoriteFolders()
    ])
    favorites.value = favRes.data || []
    const folderList = folderRes.data || []
    const total = favorites.value.length
    folders.value = [
      { FolderName: 'all', Count: total },
      ...folderList
    ]
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

function changeFolder(folderName) {
  activeFolder.value = folderName || 'all'
  loadData()
}

function formatDate(d) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function goToDetail(isbn) {
  router.push({ path: '/book/detail', query: { isbn } })
}

async function removeFavorite(f) {
  if (!confirm(`确定取消收藏《${f.Title}》吗？`)) return
  try {
    await apiRemoveFavorite(f.FavoriteID)
    showToastMsg('已取消收藏')
    await loadData()
  } catch (e) {
    alert(e.response?.data?.message || '操作失败')
  }
}

function openEditNotes(f) {
  editDialog.favorite = f
  editDialog.folderName = f.FolderName || '默认收藏夹'
  editDialog.notes = f.Notes || ''
  editDialog.show = true
}

async function submitEdit() {
  try {
    editDialog.submitting = true
    await updateFavoriteNotes(editDialog.favorite.FavoriteID, {
      FolderName: editDialog.folderName,
      Notes: editDialog.notes
    })
    editDialog.show = false
    showToastMsg('更新成功')
    await loadData()
  } catch (e) {
    alert(e.response?.data?.message || '更新失败')
  } finally {
    editDialog.submitting = false
  }
}

function addFolder() {
  if (!newFolderName.value.trim()) return
  // 收藏夹通过收藏图书时创建，这里仅给出提示
  showAddFolderDialog.value = false
  showToastMsg('收藏夹将在下次收藏图书时自动创建')
  newFolderName.value = ''
}

function showToastMsg(msg) {
  toastMsg.value = msg
  showToast.value = true
  setTimeout(() => { showToast.value = false }, 1500)
}

onMounted(loadData)
</script>

<style scoped>
.title { text-align: center; margin-bottom: 24px; color: #2c3e50; font-size: 30px; font-weight: bold; }
.tabs { display: flex; gap: 10px; margin-bottom: 20px; flex-wrap: wrap; }
.tab-btn { padding: 8px 16px; border: 1px solid #d1d5db; background: white; border-radius: 20px; cursor: pointer; font-size: 14px; transition: all 0.2s; display: flex; align-items: center; gap: 6px; }
.tab-btn.active { background: #ec4899; color: white; border-color: #ec4899; }
.count { font-size: 12px; opacity: 0.8; }
.actions { display: flex; justify-content: flex-end; gap: 12px; margin-bottom: 16px; }
.btn-refresh { background: #3498db; color: white; padding: 8px 16px; border: none; border-radius: 6px; cursor: pointer; font-size: 14px; }
.btn-refresh:hover { background: #2980b9; }
.btn-add-folder { background: #ec4899; color: white; padding: 8px 16px; border: none; border-radius: 6px; cursor: pointer; font-size: 14px; }
.btn-add-folder:hover { background: #db2777; }
.loading { text-align: center; padding: 40px; color: #999; }
.empty-state { text-align: center; padding: 60px; background: white; border-radius: 12px; color: #999; }
.empty-state .hint { font-size: 14px; margin-top: 10px; color: #9ca3af; }
.favorites-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 20px; }
.favorite-card { background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08); transition: transform 0.3s, box-shadow 0.3s; }
.favorite-card:hover { transform: translateY(-3px); box-shadow: 0 8px 20px rgba(0,0,0,0.12); }
.card-cover { width: 100%; height: 220px; background: #f3f4f6; display: flex; align-items: center; justify-content: center; overflow: hidden; }
.card-cover img { width: 100%; height: 100%; object-fit: cover; }
.card-info { padding: 14px; }
.book-title { font-size: 16px; font-weight: 600; margin: 0 0 6px; color: #1f2937; cursor: pointer; }
.book-title:hover { color: #ec4899; }
.book-author { font-size: 13px; color: #6b7280; margin: 0 0 4px; }
.book-stock { font-size: 13px; margin: 4px 0; }
.text-green { color: #059669; }
.text-red { color: #dc2626; }
.favorite-time { font-size: 12px; color: #9ca3af; margin: 4px 0; }
.folder-tag { font-size: 12px; color: #ec4899; margin: 4px 0; }
.card-actions { display: flex; gap: 6px; margin-top: 10px; }
.btn-edit, .btn-remove { flex: 1; padding: 6px 8px; border: none; border-radius: 4px; cursor: pointer; font-size: 13px; }
.btn-edit { background: #f3f4f6; color: #374151; }
.btn-edit:hover { background: #e5e7eb; }
.btn-remove { background: #fee2e2; color: #dc2626; }
.btn-remove:hover { background: #fecaca; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 2000; }
.modal-content { background: white; padding: 24px; border-radius: 12px; width: 90%; max-width: 480px; position: relative; }
.close { position: absolute; top: 12px; right: 16px; font-size: 24px; background: none; border: none; cursor: pointer; color: #9ca3af; }
.modal-content h3 { margin: 0 0 12px; font-size: 18px; }
.book-name { color: #6b7280; margin-bottom: 12px; }
.hint-text { color: #9ca3af; font-size: 13px; margin-bottom: 12px; }
.form-group { margin-bottom: 12px; }
.form-group label { display: block; font-size: 14px; font-weight: 500; margin-bottom: 4px; }
.form-input, .form-textarea { width: 100%; padding: 8px 12px; border: 1px solid #d1d5db; border-radius: 6px; font-size: 14px; box-sizing: border-box; font-family: inherit; }
.form-textarea { resize: vertical; min-height: 80px; }
.modal-actions { display: flex; gap: 10px; margin-top: 16px; }
.btn-cancel, .btn-save { flex: 1; padding: 10px; border: none; border-radius: 6px; font-size: 15px; font-weight: 600; cursor: pointer; }
.btn-cancel { background: #6b7280; color: white; }
.btn-save { background: #ec4899; color: white; }
.btn-save:disabled { background: #9ca3af; cursor: not-allowed; }
.toast { position: fixed; top: 20px; left: 50%; transform: translateX(-50%); background: #2ecc71; color: #fff; padding: 10px 20px; border-radius: 6px; z-index: 9999; animation: fadeInOut 1.5s ease forwards; }
@keyframes fadeInOut {
  0% { opacity: 0; transform: translate(-50%, -20px); }
  20% { opacity: 1; transform: translate(-50%, 0); }
  80% { opacity: 1; }
  100% { opacity: 0; transform: translate(-50%, -20px); }
}
</style>
