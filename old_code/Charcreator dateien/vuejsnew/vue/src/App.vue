

<template>
  <div id="app">
    <Notifications ref="notification" />
    <Charcreator ref="charcreator"/>
    <inventory ref="inventory" /> 
  </div>
</template>

<script>
import Notifications from './components/Notifications.vue'
import inventory from './components/inventorySystem.vue'
import Charcreator from './components/CharCreator.vue'
export default {
  name: 'App',
  components: {
    Notifications,
    inventory,
    Charcreator
  },
  mounted() {
    console.log("Setting global.gui.notify:", this.$refs.notification); // Debugging
    global.gui.notify = this.$refs.notification; // Zuweisung
    global.gui.inventory = this.$refs.inventory; // Zuweisung
    global.gui.charcreator = this.$refs.charcreator;
  }



}
</script>
