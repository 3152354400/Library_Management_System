<template>
  <div class="about-page">
    <PageHeader title="关于智书云" subtitle="现代化图书馆管理系统的课程设计项目" />

    <div class="grid">
      <div class="card">
        <h3>项目简介</h3>
        <p>本系统是数据库课程的实践项目,采用 Oracle 19c 作为底层数据库,后端基于 ASP.NET Core 8 构建 REST API,前端使用 Vue 3 + Element Plus 打造现代化管理界面。</p>
        <p>系统覆盖图书检索、借阅流通、读者服务、座位预约、采购分析等图书馆核心业务流程,可独立构建并在单机环境部署运行。</p>
      </div>

      <div class="card">
        <h3>技术栈</h3>
        <div class="tech-grid">
          <div class="tech" v-for="t in techs" :key="t.name">
            <div class="t-icon" :style="{ background: t.bg, color: t.fg }">{{ t.icon }}</div>
            <div class="t-name">{{ t.name }}</div>
            <div class="t-desc">{{ t.desc }}</div>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>团队分工</h3>
        <div class="members">
          <div class="m-card" v-for="m in members" :key="m.name">
            <div class="m-avatar">{{ m.name[0] }}</div>
            <div>
              <div class="m-name">{{ m.name }}</div>
              <div class="m-role">{{ m.role }}</div>
            </div>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>常见问题</h3>
        <el-collapse>
          <el-collapse-item title="忘记密码怎么办?" name="1">
            请联系系统管理员重置密码。读者也可在"账号安全"页面自助修改。
          </el-collapse-item>
          <el-collapse-item title="如何预约座位?" name="2">
            登录后进入"读者中心 → 座位预约",选择场馆和楼层后点击空闲座位进行预约。
          </el-collapse-item>
          <el-collapse-item title="图书逾期如何处理?" name="3">
            逾期会自动生成罚款,在"我的罚款"页面查看并缴纳。
          </el-collapse-item>
          <el-collapse-item title="如何提交图书荐购?" name="4">
            在"我的荐购"页面填写书名、作者、ISBN、推荐理由,管理员审核通过后会纳入采购计划。
          </el-collapse-item>
        </el-collapse>
      </div>
    </div>
  </div>
</template>

<script setup>
import PageHeader from '@/shared/components/PageHeader.vue'

const techs = [
  { name: 'Vue 3', desc: '渐进式前端框架', icon: 'V', bg: '#EBFBEE', fg: '#37B24D' },
  { name: 'Element Plus', desc: '桌面端 UI 组件', icon: 'E', bg: '#EEF1FE', fg: '#3B5BDB' },
  { name: 'Pinia', desc: 'Vue 状态管理', icon: 'P', bg: '#FFF4E6', fg: '#F59F00' },
  { name: 'ASP.NET Core 8', desc: '后端 Web 框架', icon: '.', bg: '#E3F8FA', fg: '#15AABF' },
  { name: 'Oracle 19c', desc: '关系型数据库', icon: 'O', bg: '#FFE3E3', fg: '#E03131' },
  { name: 'Tailwind', desc: '原子化 CSS', icon: 'T', bg: '#F3F0FF', fg: '#7950F2' }
]

const members = [
  { name: '张三', role: '前端开发' },
  { name: '李四', role: '后端开发' },
  { name: '王五', role: '数据库设计' },
  { name: '赵六', role: '测试与文档' }
]
</script>

<style scoped>
.about-page { padding: 32px 24px; max-width: 1100px; margin: 0 auto; }
.grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.card p { color: var(--color-text-soft); line-height: 1.8; margin: 8px 0; }
.tech-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-top: 12px; }
.tech { text-align: center; padding: 16px 8px; border: 1px solid var(--color-border-soft); border-radius: var(--radius-md); }
.t-icon { width: 40px; height: 40px; margin: 0 auto 8px; border-radius: var(--radius-md); display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: var(--fs-lg); }
.t-name { font-weight: 600; margin-bottom: 2px; }
.t-desc { font-size: var(--fs-xs); color: var(--color-text-muted); }
.members { display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; margin-top: 12px; }
.m-card { display: flex; gap: 12px; align-items: center; padding: 12px; border: 1px solid var(--color-border-soft); border-radius: var(--radius-md); }
.m-avatar { width: 44px; height: 44px; border-radius: 999px; background: linear-gradient(135deg, #3B5BDB, #748FFC); color: #fff; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: var(--fs-lg); }
.m-name { font-weight: 600; }
.m-role { font-size: var(--fs-xs); color: var(--color-text-muted); }
@media (max-width: 768px) {
  .grid { grid-template-columns: 1fr; }
  .tech-grid { grid-template-columns: repeat(2, 1fr); }
}
</style>