/**
 * 通用工具函数 - 数据导出
 */

/**
 * 将二维数组数据导出为CSV
 */
export function exportToCSV(data, headers, filename = 'export') {
  if (!data || data.length === 0) {
    alert('暂无数据可导出')
    return
  }

  const headerRow = headers.map(h => `"${h.label}"`).join(',')
  const rows = data.map(row =>
    headers.map(h => {
      let value = row[h.key]
      if (value === null || value === undefined) value = ''
      if (typeof value === 'string') {
        value = value.replace(/"/g, '""')
      }
      return `"${value}"`
    }).join(',')
  )

  const csv = '\ufeff' + [headerRow, ...rows].join('\n')
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  downloadBlob(blob, `${filename}_${formatDateForFile(new Date())}.csv`)
}

/**
 * 导出为Excel（使用HTML表格方式，兼容Excel打开）
 */
export function exportToExcel(data, headers, filename = 'export', title = '') {
  if (!data || data.length === 0) {
    alert('暂无数据可导出')
    return
  }

  let html = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40">'
  html += '<head><meta charset="utf-8"><style>'
  html += 'table { border-collapse: collapse; width: 100%; }'
  html += 'th, td { border: 1px solid #999; padding: 8px 12px; text-align: center; }'
  html += 'th { background-color: #4da6ff; color: white; font-weight: bold; }'
  html += 'tr:nth-child(even) { background-color: #f9fbfd; }'
  html += '.title { font-size: 18px; font-weight: bold; text-align: center; padding: 16px; }'
  html += '</style></head><body>'

  if (title) {
    html += `<div class="title">${title}</div>`
  }

  html += '<table><thead><tr>'
  headers.forEach(h => {
    html += `<th>${h.label}</th>`
  })
  html += '</tr></thead><tbody>'

  data.forEach(row => {
    html += '<tr>'
    headers.forEach(h => {
      let value = row[h.key]
      if (value === null || value === undefined) value = ''
      html += `<td>${value}</td>`
    })
    html += '</tr>'
  })

  html += '</tbody></table></body></html>'

  const blob = new Blob([html], { type: 'application/vnd.ms-excel;charset=utf-8;' })
  downloadBlob(blob, `${filename}_${formatDateForFile(new Date())}.xls`)
}

/**
 * 下载Blob
 */
function downloadBlob(blob, filename) {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.style.display = 'none'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

/**
 * 格式化日期用于文件名
 */
function formatDateForFile(date) {
  const yyyy = date.getFullYear()
  const mm = String(date.getMonth() + 1).padStart(2, '0')
  const dd = String(date.getDate()).padStart(2, '0')
  const hh = String(date.getHours()).padStart(2, '0')
  const mi = String(date.getMinutes()).padStart(2, '0')
  return `${yyyy}${mm}${dd}${hh}${mi}`
}
