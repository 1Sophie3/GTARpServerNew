<template>
  <div class="notifications">
   
  </div>
</template>

<script>
import Vue from "vue";
import Toast from "vue-toastification";
import "vue-toastification/dist/index.css";

Vue.use(Toast, {
  transition: "Vue-Toastification__bounce",
  maxToasts: 20,
  newestOnTop: true
});

export default {
  name: 'AppNotifications',
  methods: {
  showNotification: function(text, iconpic) {
    this.$toast.success(text, {
      position: "top-right",
      timeout: 3999,
      closeOnClick: true,
      pauseOnFocusLoss: true,
      pauseOnHover: true,
      draggable: true,
      draggablePercent: 0.6,
      showCloseButtonOnHover: false,
      hideProgressBar: true,
      closeButton: "button",
      icon: iconpic,
      rtl: false,
    });
  },
},
}

</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

</style>
