<template>
  <div class="ad-page">
    <PageHeader title="图书管理" subtitle="维护馆藏图书信息、新增/下架/副本管理">
      <template #actions>
          <el-input v-model="keyword" placeholder="搜索 ISBN / 书名 / 作者" style="width:280px" :prefix-icon="Search" clearable @keyup.enter="load" />
          <el-button :icon="Upload" @click="openImport">批量导入</el-button>
          <el-button type="primary" :icon="Plus" @click="onCreate">新增图书</el-button>
        </template>
    </PageHeader>

    <div class="card">
      <el-table :data="books" v-loading="loading" stripe>
        <el-table-column label="ISBN" prop="ISBN" width="180" />
        <el-table-column label="书名" min-width="220" prop="Title" />
        <el-table-column label="作者" width="160" prop="Author" />
        <el-table-column label="副本数" width="100">
          <template #default="{ row }">{{ row.TotalCopies }}</template>
        </el-table-column>
        <el-table-column label="可借" width="80">
          <template #default="{ row }">
            <span class="text-success">{{ row.AvailableCopies }}</span>
          </template>
        </el-table-column>
        <el-table-column label="在借" width="80">
          <template #default="{ row }">{{ row.BorrowedCopies }}</template>
        </el-table-column>
        <el-table-column label="下架" width="80">
          <template #default="{ row }">{{ row.TakedownCopies }}</template>
        </el-table-column>
        <el-table-column label="操作" width="260" fixed="right">
          <template #default="{ row }">
            <el-button text type="primary" @click="onAddCopies(row)"><el-icon><Plus /></el-icon> 加副本</el-button>
            <el-button text type="warning" @click="onEdit(row)"><el-icon><Edit /></el-icon> 编辑</el-button>
            <el-button text type="danger" @click="onTakedown(row)"><el-icon><Delete /></el-icon> 下架</el-button>
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !books.length" title="暂无图书" description="新增第一本馆藏图书" />
    </div>

    <el-dialog v-model="editDialog" :title="editing ? '编辑图书' : '新增图书'" width="520px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="ISBN"><el-input v-model="form.ISBN" :disabled="!!editing" /></el-form-item>
        <el-form-item label="书名"><el-input v-model="form.Title" /></el-form-item>
        <el-form-item label="作者"><el-input v-model="form.Author" /></el-form-item>
        <el-form-item label="入库数" v-if="!editing"><el-input-number v-model="form.NumberOfCopies" :min="1" :max="100" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmSave">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="copyDialog" title="追加副本" width="420px">
      <el-form :model="copyForm" label-width="120px">
        <el-form-item label="ISBN"><el-input v-model="copyForm.ISBN" disabled /></el-form-item>
        <el-form-item label="新增数量"><el-input-number v-model="copyForm.NumberOfCopies" :min="1" :max="50" /></el-form-item>
        <el-form-item label="入库书架"><el-input v-model="copyForm.ShelfID" placeholder="ShelfID" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="copyDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmAddCopies">确认入库</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="importDialog" title="批量导入图书" width="800px">
      <el-alert type="info" :closable="false" show-icon
        title="支持 .xlsx / .csv 文件，表头：ISBN、书名、作者、出版社、出版年份、价格、简介、副本数量、分类"
        description="ISBN / 书名 / 作者 为必填项，副本数量不填默认 1，分类不填则使用下方选择的默认分类。请先下载模板填写，再上传文件导入。" />
      <div style="margin:16px 0;display:flex;gap:12px;align-items:center;flex-wrap:wrap;">
        <el-button :icon="Download" @click="downloadTemplate">下载导入模板</el-button>
        <el-upload :auto-upload="false" accept=".xlsx,.xls,.csv" :limit="1"
          :file-list="fileList" :on-change="onFileChange" :on-remove="onFileRemove" :on-exceed="onFileExceed">
          <el-button :icon="Upload">选择文件</el-button>
        </el-upload>
        <el-select v-model="defaultCategory" filterable clearable placeholder="默认分类（行内未填分类时生效）" style="width:300px;">
          <el-option v-for="c in categoryOptions" :key="c.id" :label="c.label" :value="c.name" />
        </el-select>
      </div>

      <template v-if="parsed.length">
        <el-table :data="preview" size="small" max-height="240" border>
          <el-table-column prop="ISBN" label="ISBN" width="160" />
          <el-table-column prop="Title" label="书名" min-width="140" show-overflow-tooltip />
          <el-table-column prop="Author" label="作者" width="110" show-overflow-tooltip />
          <el-table-column prop="CategoryName" label="分类" width="110" show-overflow-tooltip />
          <el-table-column prop="NumberOfCopies" label="副本数" width="80" />
        </el-table>
        <div style="margin-top:8px;color:#909399;font-size:13px;">
          共解析 {{ parsed.length }} 条{{ parsed.length > preview.length ? '，仅预览前 10 条' : '' }}
        </div>
      </template>

      <template v-if="importResult">
        <el-divider />
        <div style="font-weight:600;margin-bottom:8px;">
          导入完成：成功 {{ importResult.successCount }} 条，失败 {{ importResult.failedCount }} 条
        </div>
        <el-table v-if="importResult.errors?.length" :data="importResult.errors" size="small" max-height="200" border>
          <el-table-column prop="row" label="行" width="70" />
          <el-table-column prop="isbn" label="ISBN" width="160" />
          <el-table-column prop="title" label="书名" min-width="120" show-overflow-tooltip />
          <el-table-column prop="error" label="失败原因" min-width="180" />
        </el-table>
      </template>

      <template #footer>
        <el-button @click="importDialog = false">关闭</el-button>
        <el-button type="primary" :loading="importing" :disabled="!parsed.length" @click="confirmImport">
          {{ importResult ? '再次导入' : '开始导入' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Plus, Edit, Delete, Upload, Download } from '@element-plus/icons-vue'
import * as XLSX from 'xlsx'
import { adminApi } from '@/api/admin'
import http from '@/services/http'
import PageHeader from '@/shared/components/PageHeader.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const books = ref([])
const loading = ref(false)
const keyword = ref('')
const editDialog = ref(false)
const copyDialog = ref(false)
const editing = ref(null)
const form = reactive({ ISBN: '', Title: '', Author: '', NumberOfCopies: 1 })
const copyForm = reactive({ ISBN: '', NumberOfCopies: 1, ShelfID: '' })

// 批量导入
const importDialog = ref(false)
const importing = ref(false)
const fileList = ref([])
const parsed = ref([])
const preview = ref([])
const importResult = ref(null)
const defaultCategory = ref('')
const categoryOptions = ref([])

const IMPORT_COLUMNS = ['ISBN', '书名', '作者', '出版社', '出版年份', '价格', '简介', '副本数量', '分类']

async function loadCategories() {
  try {
    const tree = await http.get('/Category/tree')
    const flat = []
    const walk = (nodes, depth) => {
      ;(nodes || []).forEach((n) => {
        flat.push({ id: n.CategoryID ?? n.categoryId, name: n.CategoryName ?? n.categoryName, label: `${'　'.repeat(depth)}${n.CategoryName ?? n.categoryName}` })
        walk(n.Children ?? n.children, depth + 1)
      })
    }
    walk(Array.isArray(tree) ? tree : (tree?.data ?? []), 0)
    categoryOptions.value = flat
  } catch (e) { console.warn(e) }
}

async function load() {
  loading.value = true
  try { books.value = await adminApi.books(keyword.value) }
  finally { loading.value = false }
}

function onCreate() {
  editing.value = null
  Object.assign(form, { ISBN: '', Title: '', Author: '', NumberOfCopies: 1 })
  editDialog.value = true
}

function onEdit(row) {
  editing.value = row
  Object.assign(form, { ISBN: row.ISBN, Title: row.Title, Author: row.Author })
  editDialog.value = true
}

async function confirmSave() {
  if (!form.ISBN || !form.Title || !form.Author) return ElMessage.warning('请填写完整')
  if (editing.value) {
    await adminApi.updateBook(form.ISBN, { Title: form.Title, Author: form.Author })
    ElMessage.success('已更新')
  } else {
    await adminApi.createBook({ ...form })
    ElMessage.success('已新增')
  }
  editDialog.value = false
  load()
}

function onAddCopies(row) {
  Object.assign(copyForm, { ISBN: row.ISBN, NumberOfCopies: 1, ShelfID: '' })
  copyDialog.value = true
}

async function confirmAddCopies() {
  await adminApi.addBookCopies({ ...copyForm })
  ElMessage.success('副本已入库')
  copyDialog.value = false
  load()
}

function onTakedown(row) {
  ElMessageBox.confirm(`确认下架《${row.Title}》的所有副本?`, '提示', { type: 'warning' })
    .then(async () => {
      await adminApi.takedownBook(row.ISBN)
      ElMessage.success('已下架')
      load()
    }).catch(() => {})
}

// ---- 批量导入 ----
function openImport() {
  importDialog.value = true
  importResult.value = null
}

function downloadTemplate() {
  const ws = XLSX.utils.aoa_to_sheet([IMPORT_COLUMNS])
  ws['!cols'] = [{ wch: 20 }, { wch: 30 }, { wch: 20 }, { wch: 20 }, { wch: 10 }, { wch: 10 }, { wch: 40 }, { wch: 10 }, { wch: 14 }]
  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, '图书导入')
  XLSX.writeFile(wb, '图书导入模板.xlsx')
}

function onFileChange(file) {
  fileList.value = [file]
  const reader = new FileReader()
  reader.onload = (e) => {
    try {
      const wb = XLSX.read(e.target.result, { type: 'array' })
      const ws = wb.Sheets[wb.SheetNames[0]]
      const rows = XLSX.utils.sheet_to_json(ws, { defval: '' })
      parsed.value = rows.map((r) => ({
        ISBN: String(r['ISBN'] ?? '').trim(),
        Title: String(r['书名'] ?? '').trim(),
        Author: String(r['作者'] ?? '').trim(),
        Publisher: String(r['出版社'] ?? '').trim(),
        PublishYear: r['出版年份'] ? Number(r['出版年份']) : null,
        Price: r['价格'] ? Number(r['价格']) : null,
        Summary: String(r['简介'] ?? '').trim(),
        NumberOfCopies: r['副本数量'] ? Number(r['副本数量']) : 1,
        CategoryName: String(r['分类'] ?? '').trim() || defaultCategory.value
      }))
      preview.value = parsed.value.slice(0, 10)
      importResult.value = null
      if (!parsed.value.length) ElMessage.warning('文件中没有可解析的数据行')
    } catch (err) {
      parsed.value = []
      preview.value = []
      ElMessage.error('文件解析失败，请使用下载的模板文件填写')
    }
  }
  reader.readAsArrayBuffer(file.raw)
  return false
}

function onFileRemove() {
  fileList.value = []
  parsed.value = []
  preview.value = []
}

function onFileExceed() {
  ElMessage.warning('每次仅支持上传一个文件')
}

async function confirmImport() {
  if (!parsed.value.length) return ElMessage.warning('请先选择文件')
  importing.value = true
  try {
    const raw = await adminApi.importBooks(parsed.value)
    importResult.value = {
      successCount: raw.successCount ?? raw.SuccessCount ?? 0,
      failedCount: raw.failedCount ?? raw.FailedCount ?? 0,
      errors: (raw.errors ?? raw.Errors ?? []).map((e) => ({
        row: e.row ?? e.Row ?? '',
        isbn: e.isbn ?? e.ISBN ?? '',
        title: e.title ?? e.Title ?? '',
        error: e.error ?? e.Error ?? ''
      }))
    }
    if (importResult.value.successCount > 0) {
      ElMessage.success(`成功导入 ${importResult.value.successCount} 本`)
      load()
    }
    if (importResult.value.failedCount > 0) {
      ElMessage.warning(`${importResult.value.failedCount} 条导入失败，详见下方明细`)
    }
  } finally {
    importing.value = false
  }
}

onMounted(() => {
  load()
  loadCategories()
})
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
</style>