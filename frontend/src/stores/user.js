import { defineStore } from 'pinia'

export const useUserStore = defineStore('user', {
  state: () => ({
    token: localStorage.getItem('token') ,
    user: JSON.parse(localStorage.getItem('user') || 'null'),
  }),
  actions: {
    setToken(token) {
      this.token = token
      localStorage.setItem('token', token)
    },
    setUser(info) {
      this.user = { ...this.user, ...info }
      localStorage.setItem('user', JSON.stringify(this.user))
    },
    clearUser() {
      this.token = ''
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    }
  },
  getters: {
    isLoggedIn: (state) => !!state.token,
    userName: (state) => state.user?.userName || '用户',
    nickName: (state) => state.user?.nickName || '用户',
    fullName: (state) => state.user?.fullName || '',
    avatar: state => state.user?.avatar || 'system_0.png',
    creditScore: state => state.user?.creditScore || 100,
    accountStatus: state => state.user?.accountStatus || '正常',
    permission: state => state.user?.permission || '普通',

  }
})
