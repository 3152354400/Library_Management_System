<template>
  <div class="avatar-card">
    <h2>请选择头像</h2>

    <!-- 当前头像 -->
    <div class="section">
      <!-- 关闭按钮 -->
      <button class="close-button" @click="emit('close')">×</button>

      <p class="section-title">当前头像</p>
      <img :src="selectedAvatar" class="avatar-preview" />
    </div>

    <!-- 系统内置头像 -->
    <div class="section">
      <p class="section-title">系统内置头像</p>
      <div class="preset-container">
        <img
          v-for="(avatar, index) in presetAvatars"
          :key="index"
          :src="'/avatars/' + avatar"
          :class="['preset-avatar', { selected: avatar === selectedAvatarUrl }]"
          @click="selectPreset(avatar)"
        />
      </div>
    </div>

    <!-- 上传自定义头像 -->
    <div class="section">
      <p class="section-title">上传自定义头像</p>
      <input type="file" @change="handleUpload" accept="image/*" />
      <p v-if="uploadError" class="error">{{ uploadError }}</p>
    </div>

    <!-- 保存 -->
    <div class="section">
      <button @click="handleSave" :disabled="!selectedAvatar">确认修改</button>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import { updateAvatar, uploadAvatar } from '@/modules/reader/api.js'
import { useUserStore } from '@/stores/user.js'

const userStore = useUserStore()

//关闭键状态
const emit = defineEmits(['close'])

// 系统默认头像
const presetAvatars = [
  'system_0.png',
  'system_1.png',
  'system_2.png',
  'system_3.png',
  'system_4.png',
  'system_5.png',
  'system_6.png',
  'system_7.png',
  'system_8.png',
  'system_9.png',
  'system_10.png',
]

const selectedAvatarUrl = ref(userStore.avatar)

// 自动根据选中的头像类型返回完整路径
const selectedAvatar = computed(() => {
  if (presetAvatars.includes(selectedAvatarUrl.value)) {
    // 系统头像路径
    return '/avatars/' + selectedAvatarUrl.value
  } else {
    // 上传后的 base64 图片
    return selectedAvatarUrl.value
  }
})

const uploadError = ref('')

// 选择系统头像
const selectPreset = (url) => {
  selectedAvatarUrl.value = url
  uploadError.value = ''
}

// 处理上传图片
const handleUpload = (event) => {
  const file = event.target.files[0]
  if (!file) return

  if (!file.type.startsWith('image/')) {
    uploadError.value = '请上传图片格式文件'
    return
  }

  if (file.size > 1024 * 1024) {
    uploadError.value = '图片不能超过 1MB'
    return
  }

  const reader = new FileReader()
  reader.onload = (e) => {
    selectedAvatarUrl.value = e.target.result
    uploadError.value = ''
  }
  reader.readAsDataURL(file)
}

// 模拟保存
const handleSave = async () => {
  // 情况 1：系统内置头像，直接传 URL
  if (presetAvatars.includes(selectedAvatarUrl.value)) {
    await updateAvatar(selectedAvatarUrl.value)
  } else {
    // 情况 2：上传的 base64 图片，需要转成 Blob
    const blob = dataURLtoBlob(selectedAvatar.value)
    const file = new File([blob], 'avatar.png', { type: blob.type })
    // 上传图片
    selectedAvatarUrl.value = (await uploadAvatar(file)).data
  }

  userStore.setUser({avatar: selectedAvatarUrl.value })
  alert('头像已更新')
}

function dataURLtoBlob(dataurl) {
  const arr = dataurl.split(',')
  const mime = arr[0].match(/:(.*?);/)[1]
  const bstr = atob(arr[1])
  let n = bstr.length
  const u8arr = new Uint8Array(n)
  while (n--) {
    u8arr[n] = bstr.charCodeAt(n)
  }
  return new Blob([u8arr], { type: mime })
}
</script>

<style scoped>
.avatar-card {
  color: dimgray;
  max-width: 400px;
  margin: 20px auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-family: sans-serif;
}

.section {
  margin-top: 20px;
}

.section-title {
  font-weight: bold;
  margin-bottom: 8px;
}

.avatar-preview {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #555;
}

.preset-container {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.preset-avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  cursor: pointer;
  object-fit: cover;
  border: 2px solid transparent;
  transition: border 0.3s;
}

.preset-avatar.selected {
  border-color: #007bff;
}

button {
  padding: 8px 16px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

button:disabled {
  background-color: #aaa;
  cursor: not-allowed;
}

.error {
  color: red;
  font-size: 12px;
  margin-top: 5px;
}

.close-button {
  position: absolute;
  top: 10px;
  right: 10px;
  background: transparent;
  color: #999;
  border: none;
  font-size: 20px;
  cursor: pointer;
}

.close-button:hover {
  color: #000;
}
</style>
