<template>
  <ul class="dropdown-menu" v-if="children && isVisible">
    <li v-for="(child, index) in children" :key="child.path" class="dropdown-item">
      <router-link
        :to="child.path"
        class="dropdown-link"
        :class="{ active: isActive(child.path) }"
      >
        {{ child.label }}
      </router-link>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { defineProps } from 'vue'
import { useRoute } from 'vue-router'

interface DropdownChild {
  path: string
  label: string
}

interface DropdownProps {
  children: DropdownChild[]
  isVisible: boolean
}

const props = defineProps<DropdownProps>()
const route = useRoute()

function isActive(path: string): boolean {
  return route.path.startsWith(path)
}
</script>

<style scoped>
.dropdown-menu {
  display: block;
  position: absolute;
  top: 100%; /* 位于主链接下方 */
  left: 0;
  background-color: #00559a; /* 背景颜色 */
  color: #fff;
  width: 81%;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  list-style: none;
  margin: 0;
  padding: 0;
  border-top-left-radius: 0; /* 上左方角为方角 */
  border-top-right-radius: 0; /* 上右方角为方角 */
  border-bottom-left-radius: 5px; /* 下左圆角 */
  border-bottom-right-radius: 5px; /* 下右圆角 */
}

.dropdown-item {
  display: block;
}

.dropdown-link {
  display: block;
  padding: 0.7rem 2.0rem;
  color: #ffffff;
  text-decoration: none;
  font-size: 1rem;
  line-height: 1;
  transition: background-color 0.2s ease, color 0.2s ease;
  text-align: center;
  border-radius: 3px;
}

.dropdown-link:hover {
  background-color: #a6f4feff;
  color: #00559a;
  border-radius: 3px;
}

.dropdown-link.active {
  background-color: #a6f4feff;
  color: #00559a;
  border-radius: 3px;
}
</style>
