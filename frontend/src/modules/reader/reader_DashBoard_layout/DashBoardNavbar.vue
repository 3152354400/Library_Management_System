<template>
  <div class="navbar">
    <div class="title">图书馆个人主页</div>
    <div class="user" @click="toggleDropdown">
      <img :src="avatar" class="avatar" />
      <span>您好，{{ nickname }}</span>
      <span class="arrow">▼</span>

      <div v-if="showDropdown" class="dropdown">
        <div class="dropdown-item" @click="toggleAvatarCard">👤 修改头像</div>
        <div class="dropdown-item" @click="toggleProfileCard">✏️ 完善资料</div>
        <div class="dropdown-item" @click="handleLogout">⏻ 退出登录</div>
      </div>
    </div>
  </div>

  <div v-if="avatarCard" class="modal-overlay" @click.self="toggleAvatarCard">
    <div class="modal-content">
      <ModifyAvatarCard @close="toggleAvatarCard" />
    </div>
  </div>

  <ProfileEditCard
    :is-visible="profileCard"
    :initial-data="initialProfileData"
    @close="toggleProfileCard"
    @save="handleSaveProfile"
  />
</template>

<script setup>
import { computed, ref } from 'vue'
import { logout, updateMyProfile } from '@/modules/reader/api.js'
import { useRouter } from 'vue-router'
import ModifyAvatarCard from '@/modules/reader/components/ModifyAvatarCard.vue'
import ProfileEditCard from '@/modules/reader/components/ProfileEditCard.vue'
import { useUserStore } from '@/stores/user.js'

const userStore = useUserStore()
const router = useRouter()

const baseAvatarUrl = import.meta.env.VITE_BASE_AVATAR_URL

const nickname = computed(() => userStore.nickName)
const avatar = computed(() => baseAvatarUrl + userStore.avatar)

const showDropdown = ref(false)
const avatarCard = ref(false)
const profileCard = ref(false)

const toggleDropdown = () => showDropdown.value = !showDropdown.value
const toggleAvatarCard = () => avatarCard.value = !avatarCard.value
const toggleProfileCard = () => profileCard.value = !profileCard.value

const initialProfileData = computed(() => ({
  userName: userStore.userName,
  fullName: userStore.fullName,
  nickName: userStore.nickName
}))

const handleSaveProfile = async (formData) => {
  const user = userStore.user
  if(formData.userName) user.userName = formData.userName
  if(formData.fullName) user.fullName = formData.fullName
  if(formData.nickName) user.nickName = formData.nickName

  const res = await updateMyProfile(user)
  userStore.setUser(user)
  alert(res.data)
  toggleProfileCard()
}

const handleLogout = async () => {
  if(!window.confirm("确定要退出登录吗？")) return
  try {
    await logout()
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    await router.push('/')
  } catch (err) {
    alert("退出失败，请稍后再试")
    console.error(err)
  }
}
</script>

<style scoped>
.navbar {
  background: white;
  padding: 20px 34px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #ddd;
  position: relative;

  width: calc(100% + 40px);
  margin-left: -40px;
}

.title {
  color: black;
  font-size: 32px;
  font-weight: bold;
  margin-left: 20px;
}

.user {
  display: flex;
  align-items: center;
  font-size: 16px;
  color: #666;
  cursor: pointer;
  position: relative;
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  margin-right: 10px;
  object-fit: cover;
}

.arrow {
  margin-left: 6px;
  font-size: 14px;
}

.dropdown {
  position: absolute;
  top: 52px;
  right: 0;
  width: 140px;
  background: white;
  border: 1px solid #ddd;
  border-radius: 4px;
  z-index: 10;
}

.dropdown-item {
  padding: 14px 20px;
  font-size: 16px;
  color: #333;
  cursor: pointer;
}

.dropdown-item:hover {
  background-color: #f5f5f5;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: -40px;
  width: calc(100vw + 40px);
  height: 30vh;
  background-color: rgba(0, 0, 0, 0.3);
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding-top: 20px;
  z-index: 9999;
}

.modal-content {
  background: white;
  border-radius: 12px;
  padding: 32px;
  max-width: 420px;
  width: 90%;
  position: relative;
  font-size: 16px;
}
</style>
