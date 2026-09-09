<template>
  <div class="rd-page">
    <PageHeader title="我的书单" subtitle="创建个性化书单,收藏喜欢的图书并分享给其他读者">
      <template #actions>
        <el-button type="primary" :icon="Plus" @click="openCreate">创建书单</el-button>
      </template>
    </PageHeader>

    <div class="card">
      <el-tabs v-model="tab">
        <el-tab-pane label="我创建的" name="created" />
        <el-tab-pane label="我收藏的" name="collected" />
        <el-tab-pane label="推荐书单" name="recommend" />
      </el-tabs>

      <div v-if="loading" class="loading-block"><el-skeleton :rows="3" animated /></div>
      <EmptyState v-else-if="!currentList.length" :title="emptyTitle" :description="emptyDesc">
        <el-button type="primary" @click="$router.push('/books')">去选书</el-button>
      </EmptyState>
      <div v-else class="bl-grid">
        <div v-for="bl in currentList" :key="bl.BooklistID || bl.BooklistId" class="bl-card" @click="openDetail(bl)">
          <div class="bl-cover">
            <el-icon :size="40"><Notebook /></el-icon>
          </div>
          <div class="bl-info">
            <div class="bl-name">{{ bl.BooklistName }}</div>
            <div class="bl-intro">{{ bl.MyNote || bl.BooklistIntroduction || '暂无简介' }}</div>
            <div class="bl-meta">
              <span><el-icon><Reading /></el-icon> {{ bl.BookCount || 0 }} 本</span>
              <span><el-icon><Star /></el-icon> {{ bl.CollectorCount || 0 }} 收藏</span>
            </div>
            <div class="bl-creator" v-if="bl.CreatorName || bl.CreatorNickname">by {{ bl.CreatorNickname || bl.CreatorName }}</div>
            <div class="bl-actions" v-if="tab === 'created'" @click.stop>
              <el-button size="small" text type="primary" :icon="Edit" @click="openEdit(bl)">编辑</el-button>
              <el-button size="small" text type="danger" :icon="Delete" @click="onDelete(bl)">删除</el-button>
            </div>
            <div class="bl-actions" v-else-if="tab === 'collected'" @click.stop>
              <el-button size="small" text type="primary" :icon="Edit" @click="openEditCollect(bl)">编辑收藏</el-button>
              <el-button size="small" text type="danger" :icon="Delete" @click="onCancelCollect(bl)">取消收藏</el-button>
            </div>
            <div class="bl-actions" v-else-if="tab === 'recommend'" @click.stop>
              <el-button size="small" text type="success" :icon="Star" :disabled="bl.IsCollected" @click="onCollectFromRecommend(bl)">{{ bl.IsCollected ? '已收藏' : '收藏' }}</el-button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 创建书单弹窗 -->
    <el-dialog v-model="createDialog" title="创建书单" width="480px">
      <el-form :model="newBl" label-width="80px">
        <el-form-item label="书单名"><el-input v-model="newBl.BooklistName" placeholder="如:前端必读Top10" /></el-form-item>
        <el-form-item label="简介"><el-input v-model="newBl.BooklistIntroduction" type="textarea" :rows="3" placeholder="为你的书单写一段介绍..." /></el-form-item>
        <el-form-item label="是否公开">
          <el-radio-group v-model="newBl.Status">
            <el-radio value="公开">公开（所有人可见、可收藏、可被推荐）</el-radio>
            <el-radio value="私有">私有（仅自己可见）</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="onCreate">创建</el-button>
      </template>
    </el-dialog>

    <!-- 编辑书单弹窗 -->
    <el-dialog v-model="editDialog" title="编辑书单" width="480px">
      <el-form :model="editForm" label-width="80px">
        <el-form-item label="书单名"><el-input v-model="editForm.BooklistName" placeholder="书单名称" /></el-form-item>
        <el-form-item label="简介"><el-input v-model="editForm.BooklistIntroduction" type="textarea" :rows="3" placeholder="书单简介" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="onEditSave">保存</el-button>
      </template>
    </el-dialog>

    <!-- 编辑收藏备注弹窗(仅收藏者本人可见) -->
    <el-dialog v-model="collectDialog" title="编辑收藏" width="480px">
      <el-alert type="info" :closable="false" title="备注仅你自己可见，不会修改创建人填写的信息" style="margin-bottom:12px" />
      <el-form :model="collectForm" label-width="80px">
        <el-form-item label="我的备注"><el-input v-model="collectForm.MyNote" type="textarea" :rows="3" placeholder="给这本书单写一句你自己的备注..." /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="collectDialog = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="onCollectSave">保存</el-button>
      </template>
    </el-dialog>

    <!-- 书单详情抽屉 -->
    <el-drawer v-model="detailDrawer" :title="detail?.BooklistName || '书单详情'" size="520px" v-if="detail">
      <div class="drawer-body">
        <p class="text-soft">{{ detail.MyNote || detail.BooklistIntroduction || '暂无简介' }}</p>
        <p class="text-soft" v-if="detail.MyNote && detail.BooklistIntroduction" style="font-size:12px;opacity:.7">创建人简介：{{ detail.BooklistIntroduction }}</p>
        <div class="d-actions" v-if="detail.creatorOwn">
          <el-button size="small" type="primary" plain :icon="Edit" @click="openEdit(detail)">编辑书单</el-button>
          <el-button size="small" type="danger" plain :icon="Delete" @click="onDelete(detail)">删除书单</el-button>
        </div>
        <div class="d-actions" v-else-if="detail.collected">
          <el-button size="small" type="primary" plain :icon="Edit" @click="openEditCollect(detail)">编辑收藏</el-button>
          <el-button size="small" type="danger" plain :icon="Delete" @click="onCancelCollect(detail)">取消收藏</el-button>
        </div>

        <h4>书单内图书（{{ detail.books?.length || 0 }}）</h4>
        <div v-if="!detail.books?.length" class="text-muted">暂无图书</div>
        <div v-else class="d-books">
          <div v-for="b in detail.books" :key="b.ISBN" class="d-book">
            <BookCover :title="b.Title" :author="b.Author" :isbn="b.ISBN" />
            <div class="d-bi">
              <div class="t">{{ b.Title }}</div>
              <div class="a">{{ b.Author }}</div>
            </div>
            <el-button v-if="detail.creatorOwn" size="small" text type="danger" @click="onRemoveBook(b)">移出</el-button>
          </div>
        </div>

        <!-- 添加图书 -->
        <template v-if="detail.creatorOwn">
          <div v-if="addBookVisible" class="d-add">
            <el-input v-model="addIsbn" placeholder="输入图书ISBN" size="small" clearable @keyup.enter="onAddBook" />
            <div class="d-add-btns">
              <el-button size="small" type="primary" :loading="saving" @click="onAddBook">确定</el-button>
              <el-button size="small" @click="closeAddBook">取消</el-button>
            </div>
          </div>
          <el-button v-else size="small" type="success" plain :icon="Plus" @click="addBookVisible = true">添加图书</el-button>
        </template>
      </div>
    </el-drawer>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'
import { bookApi } from '@/api/book'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import BookCover from '@/shared/components/BookCover.vue'

const tab = ref('created')
const loading = ref(false)
const saving = ref(false)
const created = ref([])
const collected = ref([])
const recommend = ref([])
const createDialog = ref(false)
const editDialog = ref(false)
const collectDialog = ref(false)
const detailDrawer = ref(false)
const detail = ref(null)
const newBl = ref({ BooklistName: '', BooklistIntroduction: '', Status: '公开' })
const editForm = ref({ BooklistID: null, BooklistName: '', BooklistIntroduction: '' })
const collectForm = ref({ BooklistID: null, MyNote: '' })
const addBookVisible = ref(false)
const addIsbn = ref('')

const currentList = computed(() => {
  if (tab.value === 'created') return created.value
  if (tab.value === 'collected') return collected.value
  return recommend.value
})

const emptyTitle = computed(() => ({
  created: '还没有创建书单',
  collected: '还没有收藏任何书单',
  recommend: '暂无推荐'
}[tab.value]))

const emptyDesc = computed(() => ({
  created: '创建你的第一个书单,把喜欢的书整理起来',
  collected: '从其他读者的书单中发现好书',
  recommend: '去借几本书后我们会为你推荐'
}[tab.value]))

async function load() {
  loading.value = true
  try {
    const resp1 = await bookApi.myBooklists()
    const list = resp1?.data ?? resp1
    const d = Array.isArray(list) ? { Created: list, Collected: [] } : (list || {})
    created.value = d.Created || []
    collected.value = d.Collected || []
    recommend.value = []
    if (tab.value === 'recommend') await loadRecommend()
  } finally { loading.value = false }
}

// 推荐书单：每次进入推荐 tab 时重新拉取(后端随机)
async function loadRecommend() {
  try {
    const rec = await bookApi.recommendBooklists(0, 6)
    const rd = rec?.data ?? rec
    recommend.value = Array.isArray(rd) ? rd : (rd?.Items || [])
  } catch (e) { recommend.value = [] }
}

// 从推荐书单收藏
async function onCollectFromRecommend(bl) {
  const id = bl.BooklistID || bl.BooklistId
  try {
    const res = await bookApi.collectBooklist(id, { Notes: '' })
    const d = res?.data ?? res
    if (d?.Success === 0) return ElMessage.warning('你已收藏过该书单')
    ElMessage.success(`已收藏《${bl.BooklistName}》`)
    loadRecommend()  // 重新随机推送
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '收藏失败，请稍后再试')
  }
}

// 切换到推荐 tab 时刷新(随机)
watch(tab, (v) => { if (v === 'recommend') loadRecommend() })

function openCreate() {
  newBl.value = { BooklistName: '', BooklistIntroduction: '', Status: '公开' }
  createDialog.value = true
}

async function onCreate() {
  if (!newBl.value.BooklistName) return ElMessage.warning('请输入书单名')
  saving.value = true
  try {
    await bookApi.createBooklist(newBl.value)
    ElMessage.success('书单已创建')
    createDialog.value = false
    load()
  } finally { saving.value = false }
}

// 编辑书单
function openEdit(bl) {
  editForm.value = {
    BooklistID: bl.BooklistID || bl.BooklistId,
    BooklistName: bl.BooklistName || '',
    BooklistIntroduction: bl.BooklistIntroduction || ''
  }
  editDialog.value = true
}

async function onEditSave() {
  if (!editForm.value.BooklistName) return ElMessage.warning('请输入书单名')
  saving.value = true
  try {
    const id = editForm.value.BooklistID
    await bookApi.updateBooklistName(id, { NewName: editForm.value.BooklistName })
    await bookApi.updateBooklistIntro(id, { NewIntro: editForm.value.BooklistIntroduction })
    ElMessage.success('书单已更新')
    editDialog.value = false
    await load()
    if (detailDrawer.value && detail.value?.BooklistID === id) openDetail(detail.value)
  } finally { saving.value = false }
}

// 删除书单
async function onDelete(bl) {
  const id = bl.BooklistID || bl.BooklistId
  const name = bl.BooklistName || '该书单'
  try {
    await ElMessageBox.confirm(`确认删除书单《${name}》？删除后不可恢复。`, '删除书单', { type: 'warning' })
  } catch (e) { return }
  try {
    await bookApi.deleteBooklist(id)
    ElMessage.success('书单已删除')
    if (detailDrawer.value && detail.value?.BooklistID === id) detailDrawer.value = false
    load()
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '删除失败，请稍后再试')
  }
}

// 编辑收藏备注(仅收藏者本人可见)
function openEditCollect(bl) {
  collectForm.value = {
    BooklistID: bl.BooklistID || bl.BooklistId,
    MyNote: bl.MyNote || ''
  }
  collectDialog.value = true
}

async function onCollectSave() {
  saving.value = true
  try {
    const id = collectForm.value.BooklistID
    await bookApi.updateCollectNotes(id, { NewNotes: collectForm.value.MyNote })
    ElMessage.success('收藏备注已更新')
    collectDialog.value = false
    await load()
    if (detailDrawer.value && detail.value?.BooklistID === id) openDetail(detail.value)
  } finally { saving.value = false }
}

// 取消收藏(相当于删除收藏的书单)
async function onCancelCollect(bl) {
  const id = bl.BooklistID || bl.BooklistId
  const name = bl.BooklistName || '该书单'
  try {
    await ElMessageBox.confirm(`确认取消收藏书单《${name}》？`, '取消收藏', { type: 'warning' })
  } catch (e) { return }
  try {
    await bookApi.cancelCollect(id)
    ElMessage.success('已取消收藏')
    if (detailDrawer.value && detail.value?.BooklistID === id) detailDrawer.value = false
    load()
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '取消收藏失败，请稍后再试')
  }
}

// 详情
async function openDetail(bl) {
  const id = bl.BooklistID || bl.BooklistId
  detail.value = {
    BooklistID: id,
    BooklistName: bl.BooklistName,
    BooklistIntroduction: bl.BooklistIntroduction,
    MyNote: bl.MyNote || '',
    books: [],
    creatorOwn: tab.value === 'created',
    collected: tab.value === 'collected'
  }
  detailDrawer.value = true
  try {
    const res = await bookApi.getBooklist(id)
    const d = res?.data ?? res
    if (d) {
      const info = d.BooklistInfo || {}
      detail.value = {
        BooklistID: id,
        BooklistName: info.BooklistName || bl.BooklistName,
        BooklistIntroduction: info.BooklistIntroduction || bl.BooklistIntroduction,
        MyNote: bl.MyNote || '',
        books: Array.isArray(d.Books) ? d.Books : [],
        creatorOwn: tab.value === 'created',
        collected: tab.value === 'collected'
      }
    }
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '加载书单详情失败')
  }
}

// 添加图书
function closeAddBook() {
  addBookVisible.value = false
  addIsbn.value = ''
}

async function onAddBook() {
  const isbn = addIsbn.value.trim()
  if (!isbn) return ElMessage.warning('请输入图书ISBN')
  saving.value = true
  try {
    await bookApi.addBookToBooklist(detail.value.BooklistID, { ISBN: isbn, Notes: '' })
    ElMessage.success('已加入书单')
    load()  // 刷新列表卡片计数
    closeAddBook()
    openDetail(detail.value)
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '添加失败，请确认ISBN是否正确')
  } finally { saving.value = false }
}

// 移出图书
async function onRemoveBook(b) {
  try {
    await ElMessageBox.confirm(`确认将《${b.Title}》移出书单？`, '移出图书', { type: 'warning' })
  } catch (e) { return }
  try {
    await bookApi.removeBookFromBooklist(detail.value.BooklistID, b.ISBN)
    ElMessage.success('已移出书单')
    load()  // 刷新列表卡片计数
    openDetail(detail.value)
  } catch (err) {
    ElMessage.error(err?.response?.data?.message || '移出失败，请稍后再试')
  }
}

onMounted(load)
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
.bl-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px; }
.bl-card {
  display: flex;
  gap: 14px;
  padding: 16px;
  background: var(--color-bg);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: transform .2s, box-shadow .2s;
}
.bl-card:hover { transform: translateY(-3px); box-shadow: var(--shadow-md); }
.bl-cover {
  width: 70px;
  height: 92px;
  background: linear-gradient(135deg, #3B5BDB, #748FFC);
  color: #fff;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.bl-info { flex: 1; min-width: 0; display: flex; flex-direction: column; }
.bl-name { font-size: var(--fs-md); font-weight: 600; }
.bl-intro { font-size: var(--fs-xs); color: var(--color-text-soft); margin: 4px 0 8px; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; flex: 1; }
.bl-meta { display: flex; gap: 12px; font-size: var(--fs-xs); color: var(--color-text-muted); }
.bl-meta span { display: flex; align-items: center; gap: 4px; }
.bl-creator { font-size: var(--fs-xs); color: var(--color-text-muted); margin-top: 4px; font-style: italic; }
.bl-actions { margin-top: 8px; display: flex; gap: 4px; }

.loading-block { padding: 20px 0; }
.drawer-body h4 { margin: 16px 0 12px; }
.d-actions { margin: 12px 0 8px; display: flex; gap: 8px; }
.d-books { display: flex; flex-direction: column; gap: 10px; }
.d-book { display: flex; gap: 12px; padding: 8px; background: var(--color-bg); border-radius: var(--radius-sm); align-items: center; }
.d-book :deep(svg) { width: 48px; height: 68px; flex-shrink: 0; }
.d-bi { flex: 1; min-width: 0; }
.d-bi .t { font-size: var(--fs-sm); font-weight: 600; }
.d-bi .a { font-size: var(--fs-xs); color: var(--color-text-soft); }
.d-add { display: flex; flex-direction: column; gap: 8px; margin-top: 12px; }
.d-add-btns { display: flex; gap: 8px; }
</style>
