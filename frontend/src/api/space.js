/**
 * api/space.js —— 空间服务(座位预约)
 */
import http from '@/services/http'

export const spaceApi = {
  getSeatLayout: (buildingId, floor) => http.get('/space/seats', { params: { buildingId, floor }, mock: 'space.seats' }),
  createSeatReservation: (data) => http.post('/space/reservations/seat', data, { mock: 'space.createReservation' }),
  myReservations: () => http.get('/space/my-reservations', { mock: 'space.myReservations' }),
  cancelReservation: (id) => http.put(`/space/reservations/${id}/cancel`, null, { mock: 'space.cancel' })
}