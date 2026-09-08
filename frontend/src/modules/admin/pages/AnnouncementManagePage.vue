<template>
  <div class="grid grid-cols-1 lg:grid-cols-3 gap-8 h-full">
    <!-- 左侧/上半部分：编辑区 -->
    <div class="lg:col-span-1 bg-white/80 backdrop-blur-md p-6 rounded-lg shadow-md">
      <h2 class="text-xl font-bold mb-4">{{ editingId ? '修改公告' : '发布新公告' }}</h2>
      <form @submit.prevent="handleSubmit">
        <div class="space-y-4">
          <div>
            <label class="block font-medium">标题</label>
            <input type="text" v-model="form.Title" class="form-input" required>
          </div>
          <div>
            <label class="block font-medium">内容</label>
            <textarea rows="8" v-model="form.Content" class="form-input"></textarea>
          </div>
          <div>
            <label class="block font-medium">优先级</label>
            <div class="flex gap-4">
              <label><input type="radio" v-model="form.Priority" value="常规"> 常规</label>
              <label><input type="radio" v-model="form.Priority" value="紧急"> 紧急</label>
            </div>
          </div>
          <div class="flex gap-4">
            <button type="submit" class="btn-primary w-full">{{ editingId ? '更新' : '发布' }}</button>
            <button type="button" @click="resetForm" v-if="editingId" class="btn-secondary w-full">取消修改</button>
          </div>
        </div>
      </form>
    </div>

    <!-- 右侧/下半部分：历史公告 -->
    <div class="lg:col-span-2 bg-white/80 backdrop-blur-md p-6 rounded-lg shadow-md">
       <h2 class="text-xl font-bold mb-4">历史公告</h2>
       <div class="h-[60vh] overflow-y-auto">
        <table class="w-full text-sm text-left">
          <thead class="text-xs text-gray-700 uppercase bg-gray-50/80">
            <tr>
              <th scope="col" class="px-4 py-3">标题</th>
              <th scope="col" class="px-4 py-3">优先级</th>
              <th scope="col" class="px-4 py-3">状态</th>
              <th scope="col" class="px-4 py-3">发布时间</th>
              <th scope="col" class="px-4 py-3">操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="ann in announcements" :key="ann.AnnouncementID" class="border-b">
              <td class="px-4 py-3 font-medium">{{ ann.Title }}</td>
              <td class="px-4 py-3">
                <span :class="ann.Priority === '紧急' ? 'priority-urgent' : 'priority-regular'">{{ ann.Priority }}</span>
              </td>
              <td class="px-4 py-3">{{ ann.Status }}</td>
              <td class="px-4 py-3">{{ new Date(ann.CreateTime).toLocaleString() }}</td>
              <td class="px-4 py-3 flex gap-2">
                <button @click="editAnnouncement(ann)" :disabled="ann.Status === '已撤回'" class="btn-action-edit">修改</button>
                <button @click="takedown(ann.AnnouncementID)" :disabled="ann.Status === '已撤回'" class="btn-action-delete">下架</button>
              </td>
            </tr>
          </tbody>
        </table>
       </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { getAllAnnouncements, createAnnouncement, updateAnnouncement, takedownAnnouncement } from '../api.js';

const announcements = ref([]);
const editingId = ref(null);
const form = ref({
  Title: '',
  Content: '',
  Priority: '常规'
});

async function fetchData() {
  try {
    const res = await getAllAnnouncements();
    announcements.value = res.data;
  } catch (error) {
    console.error("Failed to fetch announcements:", error);
  }
}

function editAnnouncement(ann) {
  editingId.value = ann.AnnouncementID;
  form.value.Title = ann.Title;
  form.value.Content = ann.Content;
  form.value.Priority = ann.Priority;
}

function resetForm() {
  editingId.value = null;
  form.value = { Title: '', Content: '', Priority: '常规' };
}

async function handleSubmit() {
  const data = { ...form.value };
  try {
    if (editingId.value) {
      // Update
      await updateAnnouncement(editingId.value, data);
    } else {
      // Create
      await createAnnouncement(data);
    }
    resetForm();
    await fetchData(); // Refresh list
  } catch (error) {
    console.error("Failed to save announcement:", error);
    alert("操作失败！");
  }
}

async function takedown(id) {
  if (confirm("确定要下架此公告吗？")) {
    try {
      await takedownAnnouncement(id);
      await fetchData(); // Refresh list
    } catch (error) {
      console.error("Failed to takedown announcement:", error);
      alert("下架失败！");
    }
  }
}

onMounted(fetchData);
</script>

<style scoped>
.form-input { @apply w-full p-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 transition; }
.btn-primary { @apply bg-blue-600 text-white font-bold py-2 px-4 rounded-md hover:bg-blue-700 transition; }
.btn-secondary { @apply bg-gray-200 text-gray-700 font-bold py-2 px-4 rounded-md hover:bg-gray-300 transition; }
.btn-action-edit { @apply text-blue-600 hover:underline disabled:text-gray-400 disabled:no-underline; }
.btn-action-delete { @apply text-red-600 hover:underline disabled:text-gray-400 disabled:no-underline; }
.priority-urgent { @apply bg-red-100 text-red-800 text-xs font-medium px-2.5 py-0.5 rounded-full; }
.priority-regular { @apply bg-blue-100 text-blue-800 text-xs font-medium px-2.5 py-0.5 rounded-full; }
</style>