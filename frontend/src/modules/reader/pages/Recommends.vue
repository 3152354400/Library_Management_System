<template>
  <div class="rd-page">
    <PageHeader title="我的荐购" subtitle="把你希望图书馆采购的图书推荐给我们">
      <template #actions>
        <el-button type="primary" :icon="Plus" @click="dialog = true">推荐图书</el-button>
      </template>
    </PageHeader>

    <div class="card">
      <el-table :data="list" v-loading="loading" stripe>
        <el-table-column label="书名" prop="Title" min-width="200" />
        <el-table-column label="作者" prop="Author" width="140" />
        <el-table-column label="出版社" prop="Publisher" width="160" />
        <el-table-column label="年份" prop="PublishYear" width="80" />
        <el-table-column label="推荐理由" prop="Reason" min-width="240" show-overflow-tooltip />
        <el-table-column label="推荐时间" width="160">
          <template #default="{ row }">{{ formatDate(row.RecommendTime) }}</template>
        </el-table-column>
        <el-table-column label="状态" width="120">
          <template #default="{ row }">
            <StatusTag :status="row.Status" />
          </template>
        </el-table-column>
      </el-table>
      <EmptyState v-if="!loading && !list.length" title="还没有荐购记录" description="把你希望图书馆采购的图书告诉我们" />
    </div>

    <el-dialog v-model="dialog" title="推荐图书" width="520px">
      <el-form :model="form" label-width="80px">
        <el-form-item label="书名"><el-input v-model="form.Title" /></el-form-item>
        <el-form-item label="作者"><el-input v-model="form.Author" /></el-form-item>
        <el-form-item label="出版社"><el-input v-model="form.Publisher" /></el-form-item>
        <el-form-item label="出版年"><el-input-number v-model="form.PublishYear" :min="1900" :max="2100" /></el-form-item>
        <el-form-item label="ISBN"><el-input v-model="form.ISBN" /></el-form-item>
        <el-form-item label="推荐理由"><el-input v-model="form.Reason" type="textarea" :rows="4" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialog = false">取消</el-button>
        <el-button type="primary" @click="onSubmit">提交推荐</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { recommendApi } from '@/api/recommend'
import PageHeader from '@/shared/components/PageHeader.vue'
import StatusTag from '@/shared/components/StatusTag.vue'
import EmptyState from '@/shared/components/EmptyState.vue'

const list = ref([])
const loading = ref(false)
const dialog = ref(false)
const form = ref({ Title: '', Author: '', Publisher: '', PublishYear: new Date().getFullYear(), ISBN: '', Reason: '' })

async function load() {
  loading.value = true
  try {
    const data = await recommendApi.myList()
    list.value = Array.isArray(data) ? data : []
  } finally { loading.value = false }
}

async function onSubmit() {
  if (!form.value.Title) return ElMessage.warning('请输入书名')
  const r = await recommendApi.create(form.value)
  if (r?.Success) {
    ElMessage.success(r.Message || '已提交')
    dialog.value = false
    form.value = { Title: '', Author: '', Publisher: '', PublishYear: new Date().getFullYear(), ISBN: '', Reason: '' }
    load()
  } else {
    ElMessage.error(r?.Message || '提交失败')
  }
}

function formatDate(t) { return t ? new Date(t).toLocaleDateString('zh-CN') : '' }
onMounted(load)
</script>

<style scoped>
.rd-page { display: flex; flex-direction: column; gap: 16px; }
</style>