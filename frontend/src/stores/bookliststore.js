import { defineStore } from 'pinia'
import {
  createBooklist as apiCreateBooklist,
  deleteBooklist as apiDeleteBooklist,
  getBooklistDetails as apiGetBooklistDetails,
  recommendBooklists as apiRecommendBooklists,
  addBookToBooklist as apiAddBookToBooklist,
  removeBookFromBooklist as apiRemoveBookFromBooklist,
  collectBooklist as apiCollectBooklist,
  cancelCollectBooklist as apiCancelCollectBooklist,
  updateCollectNotes as apiUpdateCollectNotes,
  searchBooklistsByReader as apiSearchBooklistsByReader,
  updateBooklistName as apiUpdateBooklistName,
  updateBooklistIntro as apiUpdateBooklistIntro
} from '@/modules/book/api.js'

function pickError(err) {
  return err?.response?.data?.message || err?.message || '请求失败'
}

export const useBooklistStore = defineStore('booklist', {
  state: () => ({
    booklists: { Created: [], Collected: [] },
    currentBooklist: null,   // 对应 GetBooklistDetailsResponse
    recommended: [],         // RecommendBooklistsResponse.Items
    loading: false,
    error: null,
    lastReaderId: null       // 用于收藏/取消收藏后便捷刷新
  }),

  getters: {
    created: (s) => s.booklists.Created || [],
    collected: (s) => s.booklists.Collected || [],
    // 合并视图
    allForDisplay: (s) => {
      const created = (s.booklists.Created || []).map(x => ({ ...x, _source: 'Created' }))
      const collected = (s.booklists.Collected || []).map(x => ({ ...x, _source: 'Collected' }))
      return [...created, ...collected]
    }
  },

  actions: {
    clearError() { this.error = null },
    clearCurrent() { this.currentBooklist = null },

    async createBooklist(payload) {
      this.clearError()
      try {
        this.loading = true
        const { data } = await apiCreateBooklist(payload)
        if (this.lastReaderId != null) {
          try {
            await this.fetchBooklistsByReader(this.lastReaderId)
          } catch (refreshErr) {
            this.error = this.error || pickError(refreshErr)
          }
        }
        return data
      } catch (err) {
        this.error = pickError(err)
        throw err
      } finally {
        this.loading = false
      }
    },

    async deleteBooklist(BooklistId) {
      this.clearError()
      try {
        this.loading = true
        await apiDeleteBooklist(BooklistId)
        this.booklists = {
          Created: (this.booklists.Created || []).filter(b => b.BooklistId !== BooklistId),
          Collected: (this.booklists.Collected || []).filter(b => b.BooklistId !== BooklistId)
        }
        if (this.currentBooklist?.BooklistInfo?.BooklistId === BooklistId) {
          this.currentBooklist = null
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      } finally {
        this.loading = false
      }
    },

    async fetchBooklistDetails(BooklistId) {
      this.clearError()
      try {
        this.loading = true
        const { data } = await apiGetBooklistDetails(BooklistId)
        this.currentBooklist = data
        return data
      } catch (err) {
        this.error = pickError(err)
        throw err
      } finally {
        this.loading = false
      }
    },

    async fetchRecommended(BooklistId, limit = 10) {
      this.clearError()
      try {
        this.loading = true
        const { data } = await apiRecommendBooklists(BooklistId, limit)
        this.recommended = data.Items || []
        return this.recommended
      } catch (err) {
        console.error('获取推荐书单失败:', err)
        this.error = '获取推荐书单失败，请稍后再试'
        throw err
      } finally {
        this.loading = false
      }
    },

    async addBook(BooklistId, payload) {
      this.clearError()
      try {
        await apiAddBookToBooklist(BooklistId, payload)
        if (this.currentBooklist?.BooklistInfo?.BooklistId === BooklistId) {
          await this.fetchBooklistDetails(BooklistId)
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      }
    },

    async removeBook(BooklistId, ISBN) {
      this.clearError()
      try {
        await apiRemoveBookFromBooklist(BooklistId, ISBN)
        if (this.currentBooklist?.BooklistInfo?.BooklistId === BooklistId) {
          await this.fetchBooklistDetails(BooklistId)
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      }
    },

    async collect(BooklistId, payload) {
      this.clearError()
      try {
        await apiCollectBooklist(BooklistId, payload)
        if (this.lastReaderId != null) {
          await this.fetchBooklistsByReader(this.lastReaderId)
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      }
    },

    async cancelCollect(BooklistId) {
      this.clearError()
      try {
        await apiCancelCollectBooklist(BooklistId)
        if (this.lastReaderId != null) {
          await this.fetchBooklistsByReader(this.lastReaderId)
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      }
    },

    async updateCollectNotes(BooklistId, payload) {
      this.clearError()
      try {
        await apiUpdateCollectNotes(BooklistId, payload)
        if (this.currentBooklist?.BooklistInfo?.BooklistId === BooklistId) {
          await this.fetchBooklistDetails(BooklistId)
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      }
    },

    async fetchBooklistsByReader(ReaderId) {
      this.clearError()
      try {
        this.loading = true
        const { data } = await apiSearchBooklistsByReader(ReaderId)
        this.booklists = {
          Created: data.Created || [],
          Collected: data.Collected || []
        }
        this.lastReaderId = ReaderId
        return this.booklists
      } catch (err) {
        this.error = pickError(err)
        throw err
      } finally {
        this.loading = false
      }
    },

    async updateName(BooklistId, payload) {
      this.clearError()
      try {
        await apiUpdateBooklistName(BooklistId, payload)
        if (this.currentBooklist?.BooklistInfo?.BooklistId === BooklistId) {
          await this.fetchBooklistDetails(BooklistId)
        }
        this.booklists = {
          Created: (this.booklists.Created || []).map(b => b.BooklistId === BooklistId ? { ...b, BooklistName: payload.NewName } : b),
          Collected: (this.booklists.Collected || []).map(b => b.BooklistId === BooklistId ? { ...b, BooklistName: payload.NewName } : b)
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      }
    },

    async updateIntro(BooklistId, payload) {
      this.clearError()
      try {
        await apiUpdateBooklistIntro(BooklistId, payload)
        if (this.currentBooklist?.BooklistInfo?.BooklistId === BooklistId) {
          await this.fetchBooklistDetails(BooklistId)
        }
        this.booklists = {
          Created: (this.booklists.Created || []).map(b => b.BooklistId === BooklistId ? { ...b, BooklistIntroduction: payload.NewIntro ?? null } : b),
          Collected: (this.booklists.Collected || []).map(b => b.BooklistId === BooklistId ? { ...b, BooklistIntroduction: payload.NewIntro ?? null } : b)
        }
      } catch (err) {
        this.error = pickError(err)
        throw err
      }
    }
  }
})
