<template>
  <div class="rd-page">
    <PageHeader title="我的收藏" subtitle="把感兴趣的书加入收藏夹,带备注更好归类">
      <template #actions>
        <el-button type="primary" :icon="Plus" @click="addDialog = true">手动收藏</el-button>
      </template>
    </PageHeader>

    <div class="layout">
      <aside class="folder-side card">
        <h4>收藏夹</h4>
        <div class="folder-list">
          <div :class="['folder-item', { active: activeFolder === null }]" @click="activeFolder = null">
            <el-icon><FolderOpened /></el-icon>
            <span>全部</span>
          </div>
          <div v-for="f in folders" :key="f.FolderName" :class="['folder-item', { active: activeFolder === f.FolderName }]" @click="activeFolder = f.FolderName">
            <el-icon><Folder /></el-icon>
            <span>{{ f.FolderName }}</span>
            <span class="count">{{ f.Count }}</span>
          </div>
        </div>
      </aside>

      <div class="fav-main">
        <div class="card">
          <div v-if="loading"><el-skeleton :rows="5" animated /></div>
          <EmptyState v-else-if="!filtered.length" title="收藏夹还是空的" description="去发现好书,加入收藏夹吧">
            <el-button type="primary" @click="$router.push('/books')">浏览图书</el-button>
          </EmptyState>
          <div v-else class="fav-grid">
            <div v-for="fav in filtered" :key="fav.FavoriteID" class="fav-card">
              <BookCover :title="fav.Title" :author="fav.Author" :isbn="fav.ISBN" class="cover" />
              <div class="fav-info">
                <div class="t">{{ fav.Title }}</div>
                <div class="a">{{ fav.Author }}</div>
                <div class="m">
                  <span>{{ fav.FolderName }}</span>
                  <span>{{ fav.AvailableStock }}/{{ fav.TotalStock }}</span>
                </div>
                <div class="note" v-if="fav.Notes">📝 {{ fav.Notes }}</div>
                <div class="actions">
                  <el-button size="small" @click="$router.push(`/book/${fav.ISBN}`)">查看</el-button>
                  <el-button size="small" type="danger" text @click="onRemove(fav)">移除</el-button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <el-dialog v-model="addDialog" title="手动添加收藏" width="420px">
      <el-form :model="addForm" label-width="80px">
        <el-form-item label="ISBN"><el-input v-model="addForm.ISBN" placeholder="图书 ISBN" /></el-form-item>
        <el-form-item label="收藏夹"><el-input v-model="addForm.FolderName" placeholder="默认收藏夹" /></el-form-item>
        <el-form-item label="备注"><el-input v-model="addForm.Notes" type="textarea" :rows="3" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="addDialog = false">取消</el-button>
        <el-button type="primary" @click="onAdd">添加</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { bookApi } from '@/api/book'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import BookCover from '@/shared/components/BookCover.vue'

const loading = ref(false)
const list = ref([])
const folders = ref([])
const activeFolder = ref(null)
const addDialog = ref(false)
const addForm = ref({ ISBN: '', FolderName: '默认收藏夹', Notes: '' })

const filtered = computed(() => {
  if (!activeFolder.value) return list.value
  return list.value.filter((f) => f.FolderName === activeFolder.value)
})

async function load() {
  loading.value = true
  try {
    list.value = await bookApi.favorites()
    folders.value = await bookApi.favoriteFolders()
  } finally { loading.value = false }
}

async function onRemove(fav) {
  await ElMessageBox.confirm(`确认移除《${fav.Title}》?`, '提示', { type: 'warning' }).catch(() => {})
  await bookApi.removeFavorite(fav.FavoriteID)
  ElMessage.success('已移除')
  load()
}

async function onAdd() {
  if (!addForm.value.ISBN) return ElMessage.warning('请输入 ISBN')
  await bookApi.addFavorite(addForm.value)
  ElMessage.success('收藏成功')
  addDialog.value = false
  addForm.value = { ISBN: '', FolderName: '默认收藏夹', Notes: '' }
  load()
}

onMounted(load)
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.layout { display: grid; grid-template-columns: 240px 1fr; gap: 16px; align-items: flex-start; }
.folder-side h4 { margin: 0 0 12px; }
.folder-list { display: flex; flex-direction: column; gap: 4px; }
.folder-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--fs-sm);
}
.folder-item:hover { background: var(--color-bg); }
.folder-item.active { background: var(--color-primary-50); color: var(--color-primary-600); font-weight: 600; }
.folder-item .count { margin-left: auto; font-size: var(--fs-xs); color: var(--color-text-muted); }
.fav-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px; }
.fav-card {
  display: grid;
  grid-template-columns: 80px 1fr;
  gap: 14px;
  padding: 14px;
  background: var(--color-bg);
  border-radius: var(--radius-md);
}
.cover { width: 80px; height: 120px; }
.fav-info .t { font-weight: 600; font-size: var(--fs-sm); display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.fav-info .a { font-size: var(--fs-xs); color: var(--color-text-soft); margin: 2px 0 6px; }
.fav-info .m { display: flex; justify-content: space-between; font-size: var(--fs-xs); color: var(--color-text-muted); margin-bottom: 6px; }
.fav-info .note { font-size: var(--fs-xs); color: var(--color-text-soft); background: #fff; padding: 6px 10px; border-radius: 4px; margin-bottom: 8px; }
.actions { display: flex; gap: 4px; }
@media (max-width: 768px) {
  .layout { grid-template-columns: 1fr; }
}
</style>