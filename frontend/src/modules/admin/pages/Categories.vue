<template>
  <div class="ad-page">
    <PageHeader title="分类管理" subtitle="维护图书分类树" />

    <div class="layout">
      <div class="card tree-pane">
        <h4>分类树</h4>
        <div class="tree">
          <div v-for="root in tree" :key="root.CategoryID" class="tree-node">
            <div class="node-line">
              <el-icon><FolderOpened /></el-icon>
              <span class="name">{{ root.CategoryName }}</span>
              <span class="actions">
                <el-button text size="small" @click="onEdit(root)"><el-icon><Edit /></el-icon></el-button>
                <el-button text size="small" type="danger" @click="onDelete(root)"><el-icon><Delete /></el-icon></el-button>
              </span>
            </div>
            <div v-if="root.Children?.length" class="node-children">
              <div v-for="c in root.Children" :key="c.CategoryID" class="node-line small">
                <el-icon><Folder /></el-icon>
                <span class="name">{{ c.CategoryName }}</span>
                <span class="actions">
                  <el-button text size="small" @click="onEdit(c)"><el-icon><Edit /></el-icon></el-button>
                  <el-button text size="small" type="danger" @click="onDelete(c)"><el-icon><Delete /></el-icon></el-button>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card-head">
          <h3>新建分类</h3>
        </div>
        <el-form :model="form" label-width="100px">
          <el-form-item label="分类ID"><el-input v-model="form.CategoryID" placeholder="如:C11" /></el-form-item>
          <el-form-item label="分类名"><el-input v-model="form.CategoryName" /></el-form-item>
          <el-form-item label="父分类">
            <el-tree-select v-model="form.ParentCategoryID" :data="treeSelectData" check-strictly clearable style="width:100%" placeholder="顶级分类则不选" />
          </el-form-item>
          <el-button type="primary" @click="onSubmit">保存</el-button>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Edit, Delete } from '@element-plus/icons-vue'
import { bookApi, adminApi } from '@/api'
import PageHeader from '@/shared/components/PageHeader.vue'

const tree = ref([])
const form = reactive({ CategoryID: '', CategoryName: '', ParentCategoryID: '' })

const treeSelectData = computed(() => tree.value.map((r) => ({
  value: r.CategoryID,
  label: r.CategoryName,
  children: r.Children?.map((c) => ({ value: c.CategoryID, label: c.CategoryName }))
})))

async function load() {
  tree.value = await bookApi.getCategoryTree()
}

async function onSubmit() {
  if (!form.CategoryID || !form.CategoryName) return ElMessage.warning('请填写完整')
  const payload = { Category: { ...form } }
  await adminApi.addCategory(payload)
  ElMessage.success('已添加')
  Object.assign(form, { CategoryID: '', CategoryName: '', ParentCategoryID: '' })
  load()
}

function onEdit(node) {
  Object.assign(form, { CategoryID: node.CategoryID, CategoryName: node.CategoryName, ParentCategoryID: node.ParentCategoryID || '' })
}

function onDelete(node) {
  ElMessageBox.confirm(`确认删除分类 "${node.CategoryName}"?`, '提示', { type: 'warning' })
    .then(async () => {
      await adminApi.deleteCategory(node.CategoryID, 'LIB001')
      ElMessage.success('已删除')
      load()
    }).catch(() => {})
}

onMounted(load)
</script>

<style scoped>
.ad-page { display: flex; flex-direction: column; gap: 16px; }
.layout { display: grid; grid-template-columns: 380px 1fr; gap: 16px; align-items: flex-start; }
.tree-pane h4 { margin: 0 0 12px; }
.tree { max-height: 600px; overflow-y: auto; }
.tree-node { margin-bottom: 4px; }
.node-line {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: var(--radius-md);
  font-size: var(--fs-sm);
}
.node-line.small { padding-left: 32px; font-size: var(--fs-xs); color: var(--color-text-soft); }
.node-line:hover { background: var(--color-bg); }
.node-line .name { flex: 1; }
.node-line .actions { display: none; }
.node-line:hover .actions { display: flex; gap: 2px; }
.card-head { display: flex; justify-content: space-between; margin-bottom: 16px; }
.card-head h3 { margin: 0; }
@media (max-width: 900px) { .layout { grid-template-columns: 1fr; } }
</style>