<template>
  <div class="login-bg">
    <img
      v-if="!error"
      :src="`https://images.unsplash.com/photo-1507842217343-583bb7270b66?w=1920&q=80`"
      alt=""
      @error="error = true"
      class="bg-image"
    />
    <div v-else class="bg-fallback" />
    <div class="bg-overlay" />
  </div>
</template>

<script setup>
import { ref } from 'vue'
const error = ref(false)
</script>

<style scoped>
.login-bg { position: absolute; inset: 0; overflow: hidden; }
.bg-image { width: 100%; height: 100%; object-fit: cover; animation: slowZoom 20s ease-in-out infinite alternate; }
.bg-fallback {
  position: absolute; inset: 0;
  background: linear-gradient(135deg, #1E2A6E 0%, #3B5BDB 50%, #5C7CFA 100%);
}
.bg-overlay {
  position: absolute; inset: 0;
  background: radial-gradient(circle at 20% 30%, rgba(59,91,219,.6), transparent 60%),
              radial-gradient(circle at 80% 70%, rgba(116,143,252,.4), transparent 50%);
  mix-blend-mode: multiply;
}
@keyframes slowZoom {
  from { transform: scale(1); }
  to { transform: scale(1.05); }
}
</style>