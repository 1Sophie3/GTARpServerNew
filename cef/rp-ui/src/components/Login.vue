<template>
  <div class="limiter">
    <div class="container-login100">
      <div class="wrap-login100 p-l-50 p-r-50 p-t-77 p-b-30">
        
        <!-- LOGIN FORM -->
        <form v-if="!showRegister" class="login100-form validate-form" @submit.prevent="handleLogin">
          <div class="text-center w-full">
            <h3 class="login100-form-title">Bitte Logge dich ein!</h3>
            <br/>
          </div>
          
          <div class="alert" :class="alertClass" id="warning">
            {{ alertMessage }}
          </div>

          <div class="wrap-input100 validate-input m-b-16" data-validate="Bitte Benutzername eingeben!">
            <input 
              class="input100" 
              type="text" 
              v-model="loginData.username"
              placeholder="Benutzername"
              required
            />
            <span class="focus-input100"></span>
            <span class="symbol-input100">
              <span class="lnr lnr-user"></span>
            </span>
          </div>

          <div class="wrap-input100 validate-input m-b-16" data-validate="Bitte ein Passwort eingeben!">
            <input 
              class="input100" 
              type="password" 
              v-model="loginData.password"
              placeholder="Passwort"
              required
            />
            <span class="focus-input100"></span>
            <span class="symbol-input100">
              <span class="lnr lnr-lock"></span>
            </span>
          </div>

          <div class="container-login100-form-btn p-t-25">
            <button type="submit" class="login100-form-btn">
              Login
            </button>
          </div>

          <div class="text-center w-full p-t-15">
            <a class="txt1 bo1 hov1" @click="showRegister = true">
              Neuen Account erstellen!
            </a>
            <br/><hr style="margin: 15px 0; border: none; border-top: 1px solid #eee;"/>
            <a class="txt1 bo1 hov1" href="https://nemesus.de" target="_blank">
              Server Website
            </a>
          </div>
        </form>

        <!-- REGISTER FORM -->
        <form v-else class="login100-form validate-form" @submit.prevent="handleRegister">
          <div class="text-center w-full">
            <h3 class="login100-form-title">Neuen Account erstellen</h3>
            <br/>
          </div>
          
          <div class="alert" :class="alertClass" id="warning">
            {{ alertMessage }}
          </div>

          <div class="wrap-input100 validate-input m-b-16" data-validate="Bitte Benutzername eingeben!">
            <input 
              class="input100" 
              type="text" 
              v-model="registerData.username"
              placeholder="Benutzername"
              required
            />
            <span class="focus-input100"></span>
            <span class="symbol-input100">
              <span class="lnr lnr-user"></span>
            </span>
          </div>

          <!-- E-Mail entfernt: E-Mail wird nicht mehr bei Registrierung benötigt -->

          <div class="wrap-input100 validate-input m-b-16" data-validate="Bitte ein Passwort eingeben!">
            <input 
              class="input100" 
              type="password" 
              v-model="registerData.password"
              placeholder="Passwort"
              required
            />
            <span class="focus-input100"></span>
            <span class="symbol-input100">
              <span class="lnr lnr-lock"></span>
            </span>
          </div>

          <div class="wrap-input100 validate-input m-b-16" data-validate="Bitte Passwort wiederholen!">
            <input 
              class="input100" 
              type="password" 
              v-model="registerData.confirmPassword"
              placeholder="Passwort wiederholen"
              required
            />
            <span class="focus-input100"></span>
            <span class="symbol-input100">
              <span class="lnr lnr-lock"></span>
            </span>
          </div>

          <!-- Vor- & Nachname werden nicht mehr bei Registrierung benötigt -->

          <div class="container-login100-form-btn p-t-25">
            <button type="submit" class="login100-form-btn">
              Registrieren
            </button>
          </div>

          <div class="text-center w-full p-t-15">
            <a class="txt1 bo1 hov1" @click="showRegister = false">
              Zum Login
            </a>
            <br/><hr style="margin: 15px 0; border: none; border-top: 1px solid #eee;"/>
            <a class="txt1 bo1 hov1" href="https://nemesus.de" target="_blank">
              Server Website
            </a>
          </div>
        </form>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import '../assets/css/nemesus-login.css'

const showRegister = ref(false)
const alertType = ref('info') // 'info', 'danger', 'success'
const alertMessage = ref('Willkommen auf dem Server!')

const loginData = ref({
  username: '',
  password: ''
})

const registerData = ref({
  username: '',
  password: '',
  confirmPassword: ''
})

const alertClass = computed(() => {
  return {
    'alert-info': alertType.value === 'info',
    'alert-danger': alertType.value === 'danger',
    'alert-success': alertType.value === 'success'
  }
})

const setWarning = (text, type = 'danger') => {
  alertMessage.value = text
  alertType.value = type
}

const handleLogin = () => {
  if (!loginData.value.username || !loginData.value.password) {
    setWarning('Bitte alle Felder ausfüllen!')
    return
  }
  
  if (loginData.value.password.length < 5) {
    setWarning('Das Passwort muss mind. 5 Zeichen lang sein!')
    return
  }
  
  // An Client senden (wird dann an Server weitergeleitet)
  if (window.sendToClient) {
    window.sendToClient('cef:login', loginData.value.username, loginData.value.password)
  }
  
  // Für RageMP mp.trigger
  if (typeof mp !== 'undefined') {
    mp.trigger('Auth.Login', loginData.value.username, loginData.value.password)
  }

  setWarning('Anmeldung wird verarbeitet...', 'info')
}

const handleRegister = () => {
  // Validierung
  if (!registerData.value.username || 
      !registerData.value.password || !registerData.value.confirmPassword) {
    setWarning('Bitte alle Felder ausfüllen!')
    return
  }
  
  if (registerData.value.password.length < 5) {
    setWarning('Das Passwort muss mind. 5 Zeichen lang sein!')
    return
  }
  
  if (registerData.value.password !== registerData.value.confirmPassword) {
    setWarning('Die Passwörter stimmen nicht überein!')
    return
  }

  // Vor- und Nachname werden serverseitig gesetzt; kein Client-Check nötig

  // An Client senden
  if (window.sendToClient) {
    window.sendToClient('cef:register', 
      registerData.value.username, 
      registerData.value.password
    )
  }
  
  // Für RageMP mp.trigger
  if (typeof mp !== 'undefined') {
    mp.trigger('Auth.Register', 
      registerData.value.username,
      registerData.value.password
    )
  }

  setWarning('Registrierung wird verarbeitet...', 'info')
}

// Globale Funktionen für CEF Callbacks
window.SetWarning = (text) => {
  setWarning(text, 'danger')
}

window.SetSuccess = (text) => {
  setWarning(text, 'success')
}

window.SetInfo = (text) => {
  setWarning(text, 'info')
}
</script>

<style scoped>
/* Styles sind in nemesus-login.css */
</style>
