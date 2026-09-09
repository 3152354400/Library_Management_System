<template>
  <!-- 遮罩层 -->
  <div
    v-if="isVisible"
    class="modal-backdrop"
    @click="handleClose"
  >
    <!-- 弹窗主体 -->
    <div
      class="modal-container"
      @click.stop
      :class="{ 'modal-enter': isVisible, 'modal-leave': !isVisible }"
    >
      <!-- 头部 -->
      <div class="modal-header">
        <h3 class="modal-title">编辑个人资料</h3>
        <button
          class="close-btn"
          @click="handleClose"
          aria-label="关闭"
        >
          <span aria-hidden="true">×</span>
        </button>
      </div>

      <!-- 内容区 -->
      <div class="modal-body">
        <form @submit.prevent="handleSave">
          <!-- 用户名 -->
          <div class="form-group">
            <label for="username" class="form-label">用户名</label>
            <input
              type="text"
              id="username"
              v-model="formData.userName"
              class="form-control"
              required
              placeholder="请输入用户名"
            >
          </div>

          <!-- 姓名（必填） -->
          <div class="form-group">
            <label for="fullname" class="form-label">全名</label>
            <input
              type="text"
              id="fullname"
              v-model="formData.fullName"
              class="form-control"
              required
              placeholder="请输入您的姓名"
            >
          </div>

          <!-- 昵称 -->
          <div class="form-group">
            <label for="nickname" class="form-label">昵称</label>
            <input
              type="text"
              id="nickname"
              v-model="formData.nickName"
              class="form-control"
              placeholder="请输入您的昵称（可选）"
            >
          </div>

          <!-- 密码
          <div class="form-group">
            <label for="password" class="form-label">密码</label>
            <input
              type="password"
              id="password"
              v-model="formData.password"
              class="form-control"
              placeholder="密码暂时不允许修改，且用户名也不要随意修改，避免无法登录）"
            >
          </div> -->
        </form>
      </div>

      <!-- 底部按钮 -->
      <div class="modal-footer">
        <button
          type="button"
          class="btn btn-cancel"
          @click="handleClose"
        >
          取消
        </button>
        <button
          type="button"
          class="btn btn-save"
          @click="handleSave"
        >
          保存修改
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { watch, reactive } from 'vue';

const props = defineProps({
  isVisible: {
    type: Boolean,
    default: false
  },
  initialData: {
    type: Object,
    required: true,
    default: () => ({
      userName: '',
      fullName: '',
      nickName: '',
      //password: ''
    })
  }
});

const emit = defineEmits(['close', 'save']);

const formData = reactive({
  userName: '',
  fullName: '',
  nickName: '',
  //password: ''
});

// 初始化/监听更新
watch(
  () => props.initialData,
  (newVal) => {
    formData.userName = newVal.userName || '';
    formData.fullName = newVal.fullName || '';
    formData.nickName = newVal.nickName || '';
    //formData.password = '';
  },
  { immediate: true }
);

// 关闭弹窗
const handleClose = () => {
  emit('close');
};

// 保存修改
const handleSave = () => {
  if (!formData.userName.trim()) {
    alert('用户名不能为空');
    return;
  }

  if (!formData.fullName.trim()) {
    alert('全名不能为空');
    return;
  }

  const formDataCopy = { ...formData };
  emit('save', formDataCopy);
  handleClose();
};
</script>

<style scoped>
/* 遮罩层 */
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
  animation: fadeIn 0.3s ease;
}

.modal-container {
  width: 100%;
  max-width: 500px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
  overflow: hidden;
  transition: transform 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #eee;
}

.modal-title {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: #333;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #999;
  transition: color 0.2s;
  padding: 0 0.5rem;
}

.close-btn:hover {
  color: #333;
}

.modal-body {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #555;
}

.form-control {
  color: black;
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-control:focus {
  outline: none;
  border-color: #66afe9;
  box-shadow: 0 0 0 3px rgba(102, 175, 233, 0.1);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid #eee;
  background-color: #f9f9f9;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn-cancel {
  background-color: #f1f1f1;
  color: #555;
}

.btn-cancel:hover {
  background-color: #e5e5e5;
}

.btn-save {
  background-color: #007bff;
  color: white;
}

.btn-save:hover {
  background-color: #0056b3;
}
</style>
