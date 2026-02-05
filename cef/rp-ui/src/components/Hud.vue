<template>
  <div class="hud-container">
    <!-- Top Left - Player Info -->
    <div class="player-info">
      <div class="info-item">
        <span class="label">💰</span>
        <span class="value">${{ formatNumber(playerData.money) }}</span>
      </div>
      <div class="info-item">
        <span class="label">🏦</span>
        <span class="value">${{ formatNumber(playerData.bankMoney) }}</span>
      </div>
      <div class="info-item">
        <span class="label">⭐</span>
        <span class="value">Level {{ playerData.level }}</span>
      </div>
    </div>

    <!-- Bottom Right - Health & Armor -->
    <div class="status-bars">
      <div class="bar-item">
        <div class="bar-label">❤️ Gesundheit</div>
        <div class="bar-container">
          <div class="bar-fill health" :style="{ width: health + '%' }"></div>
        </div>
        <div class="bar-value">{{ health }}%</div>
      </div>
      <div class="bar-item">
        <div class="bar-label">🛡️ Rüstung</div>
        <div class="bar-container">
          <div class="bar-fill armor" :style="{ width: armor + '%' }"></div>
        </div>
        <div class="bar-value">{{ armor }}%</div>
      </div>
    </div>

    <!-- Notifications -->
    <div class="notifications">
      <div 
        v-for="(notif, index) in notifications" 
        :key="index" 
        class="notification"
        :class="notif.type"
      >
        {{ notif.message }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const playerData = ref({
  username: 'Player',
  money: 5000,
  bankMoney: 0,
  level: 1,
  experience: 0,
  job: 'Arbeitslos'
})

const health = ref(100)
const armor = ref(0)
const notifications = ref([])

// Event Listener für Updates vom Client
onMounted(() => {
  window.addEventListener('updatePlayerData', (e) => {
    playerData.value = e.detail
  })
  
  window.addEventListener('updateHUD', (e) => {
    if (e.detail.health !== undefined) health.value = e.detail.health
    if (e.detail.armor !== undefined) armor.value = e.detail.armor
  })
})

// Hilfsfunktionen
const formatNumber = (num) => {
  return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

// Notification hinzufügen (wird vom Client aufgerufen)
const addNotification = (message, type = 'info') => {
  const id = Date.now()
  notifications.value.push({ id, message, type })
  
  setTimeout(() => {
    notifications.value = notifications.value.filter(n => n.id !== id)
  }, 5000)
}

// Für Client verfügbar machen
window.addNotification = addNotification
</script>

<style scoped>
.hud-container {
  position: fixed;
  width: 100vw;
  height: 100vh;
  pointer-events: none;
}

.player-info {
  position: absolute;
  top: 20px;
  left: 20px;
  background: rgba(0, 0, 0, 0.6);
  padding: 15px 20px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
}

.info-item {
  display: flex;
  align-items: center;
  margin-bottom: 8px;
  color: white;
  font-size: 16px;
}

.info-item:last-child {
  margin-bottom: 0;
}

.info-item .label {
  margin-right: 10px;
  font-size: 20px;
}

.info-item .value {
  font-weight: 600;
}

.status-bars {
  position: absolute;
  bottom: 30px;
  right: 30px;
}

.bar-item {
  margin-bottom: 15px;
}

.bar-label {
  color: white;
  font-size: 14px;
  margin-bottom: 5px;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
}

.bar-container {
  width: 200px;
  height: 25px;
  background: rgba(0, 0, 0, 0.6);
  border-radius: 12px;
  overflow: hidden;
  border: 2px solid rgba(255, 255, 255, 0.2);
}

.bar-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.bar-fill.health {
  background: linear-gradient(90deg, #e74c3c, #c0392b);
}

.bar-fill.armor {
  background: linear-gradient(90deg, #3498db, #2980b9);
}

.bar-value {
  color: white;
  font-size: 12px;
  margin-top: 3px;
  text-align: right;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
}

.notifications {
  position: absolute;
  top: 50%;
  right: 30px;
  transform: translateY(-50%);
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.notification {
  background: rgba(0, 0, 0, 0.8);
  color: white;
  padding: 12px 20px;
  border-radius: 8px;
  animation: slideIn 0.3s ease;
  border-left: 4px solid #3498db;
}

.notification.success {
  border-left-color: #2ecc71;
}

.notification.error {
  border-left-color: #e74c3c;
}

.notification.warning {
  border-left-color: #f39c12;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}
</style>
