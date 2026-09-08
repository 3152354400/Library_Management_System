import axios from 'axios'

// 创建 axios 实例，统一使用环境变量中的 API 地址
const http = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,   // ✅ 来自 .env 文件
    timeout: 10000,
    headers: {
    'Content-Type': 'application/json'
    }
})

// 自动带上 JWT Token
http.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    // 不再需要 if(config.withToken) 判断，直接为所有请求添加 token
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
})

// 响应拦截器
http.interceptors.response.use(
  response => response,
  error => {

    console.log(error.response)
    const status = error.response?.status
    const message = error.response?.data

    if (status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      // alert('登录已过期，请重新登录。')

      // 可选跳转
      // router.push({ path: '/login' })
    } else if (status === 400) {
      // 可选：这里也可以不 alert，而是交给业务层抛出更明确的错误信息
      //alert(message || '请求有误')
    } else if (!error.response) {
      alert('无法连接服务器，请检查网络')
    }

    return Promise.reject(error)
  }
)




export default http

