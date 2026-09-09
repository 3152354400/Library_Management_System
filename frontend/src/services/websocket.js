/**
 * WebSocket通知客户端
 * 提供实时通知推送功能，自动重连
 */

import { ref } from 'vue'

class WebSocketClient {
  constructor() {
    this.ws = null
    this.url = null
    this.reconnectAttempts = 0
    this.maxReconnectAttempts = 5
    this.reconnectDelay = 3000
    this.listeners = new Set()
    this.isConnected = ref(false)
    this.lastNotification = ref(null)
    this.heartbeatTimer = null
  }

  /**
   * 连接WebSocket
   */
  connect() {
    const token = localStorage.getItem('token')
    if (!token) {
      console.warn('WebSocket未连接：未登录')
      return
    }

    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:'
    const host = window.location.host
    // token 通过 query string 传递（WebSocket 无法携带自定义 Header）
    this.url = `${protocol}//${host}/api/ws/connect?token=${encodeURIComponent(token)}`

    try {
      this.ws = new WebSocket(this.url)

      this.ws.onopen = () => {
        console.log('✅ WebSocket连接成功')
        this.isConnected.value = true
        this.reconnectAttempts = 0
        this.startHeartbeat()
      }

      this.ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data)
          console.log('📩 收到WebSocket消息:', data)
          this.lastNotification.value = data
          this.listeners.forEach(callback => callback(data))
        } catch (e) {
          console.error('WebSocket消息解析失败:', e)
        }
      }

      this.ws.onerror = (error) => {
        console.error('❌ WebSocket错误:', error)
        this.isConnected.value = false
      }

      this.ws.onclose = () => {
        console.log('🔌 WebSocket连接断开')
        this.isConnected.value = false
        this.stopHeartbeat()
        this.attemptReconnect()
      }
    } catch (e) {
      console.error('WebSocket创建失败:', e)
      this.attemptReconnect()
    }
  }

  /**
   * 断开连接
   */
  disconnect() {
    if (this.ws) {
      this.stopHeartbeat()
      this.ws.close()
      this.ws = null
    }
  }

  /**
   * 尝试重连
   */
  attemptReconnect() {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.warn('WebSocket已达最大重连次数，停止重连')
      return
    }

    this.reconnectAttempts++
    console.log(`尝试第 ${this.reconnectAttempts} 次重连...`)
    setTimeout(() => this.connect(), this.reconnectDelay)
  }

  /**
   * 心跳检测
   */
  startHeartbeat() {
    this.heartbeatTimer = setInterval(() => {
      if (this.ws && this.ws.readyState === WebSocket.OPEN) {
        try {
          this.ws.send(JSON.stringify({ type: 'ping', timestamp: Date.now() }))
        } catch (e) {
          console.error('心跳发送失败:', e)
        }
      }
    }, 30000)
  }

  stopHeartbeat() {
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer)
      this.heartbeatTimer = null
    }
  }

  /**
   * 监听通知
   */
  onMessage(callback) {
    this.listeners.add(callback)
    return () => this.listeners.delete(callback)
  }
}

// 导出单例
const wsClient = new WebSocketClient()

export default wsClient
export { WebSocketClient }
