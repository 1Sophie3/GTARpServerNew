<template>
  <div class="login-container">
    <div class="login-box">
      <div class="logo">
        <h1>RAGE:MP Roleplay</h1>
        <p>Willkommen zurück</p>
      </div>

      <div v-if="!showRegister" class="form-container">
        <h2>Anmelden</h2>
        <form @submit.prevent="handleLogin">
          <div class="input-group">
            <input 
              v-model="loginData.username" 
              type="text" 
              placeholder="Benutzername" 
              required
            />
          </div>
          <div class="input-group">
            <input 
              v-model="loginData.password" 
              type="password" 
              placeholder="Passwort" 
              required
            />
          </div>
          <button type="submit" class="btn btn-primary">Einloggen</button>
        </form>
        <p class="switch-text">
          Noch kein Account? 
          <a @click="showRegister = true">Registrieren</a>
        </p>
      </div>

      <div v-else class="form-container">
        <h2>Registrieren</h2>
        <form @submit.prevent="handleRegister">
          <div class="input-group">
            <input 
              v-model="registerData.username" 
              type="text" 
              placeholder="Benutzername" 
              required
            />
          </div>
          <div class="input-group">
            <input 
              v-model="registerData.email" 
              type="email" 
              placeholder="E-Mail" 
              required
            />
          </div>
          <div class="input-group">
            <input 
              v-model="registerData.password" 
              type="password" 
              placeholder="Passwort" 
              required
            />
          </div>
          <div class="input-group">
            <input 
              v-model="registerData.confirmPassword" 
              type="password" 
              placeholder="Passwort bestätigen" 
              required
            />
          </div>
          <button type="submit" class="btn btn-primary">Registrieren</button>
        </form>
        <p class="switch-text">
          Bereits registriert? 
          <a @click="showRegister = false">Anmelden</a>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const showRegister = ref(false)
const loginData = ref({
  username: '',
  password: ''
})
const registerData = ref({
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const handleLogin = () => {
  if (!loginData.value.username || !loginData.value.password) {
    alert('Bitte alle Felder ausfüllen!')
    return
  }
  
  // An Client senden (wird dann an Server weitergeleitet)
  window.sendToClient('cef:login', loginData.value.username, loginData.value.password)
}

const handleRegister = () => {
  if (registerData.value.password !== registerData.value.confirmPassword) {
    alert('Passwörter stimmen nicht überein!')
    return
  }
  
  if (!registerData.value.username || !registerData.value.email || !registerData.value.password) {
    alert('Bitte alle Felder ausfüllen!')
    return
  }
  
  // An Client senden
  window.sendToClient('cef:register', 
    registerData.value.username, 
    registerData.value.email, 
    registerData.value.password
  )
}
</script>

<style scoped>
/* Styles werden in separate CSS Datei ausgelagert */
</style>
