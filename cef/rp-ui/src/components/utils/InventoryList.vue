<template>
  <div class="inventorylist">
    <div class="title">{{ title }}</div>
    <slot></slot>
    <button class="btn-sort" @click="sortitems">Items zusammenlegen</button>
  </div>
</template>

<script setup>
defineProps({ title: String })
function sortitems() {
  // TODO: implement grouping/stacking
}
</script>

<style scoped>
.inventorylist {
  padding: 10px 20px;
  margin: 0 10px;
  border: 1px solid #eee;
  font-family: sans-serif;
  width: 360px;
  background: #4C4C4C;
  border-radius: 6px;
}

.title {
  padding: 10px;
  font-size: 20px;
  font-weight: 600;
  margin-bottom: 10px;
  text-align: center;
  color: #fff;
  border-bottom: 1px solid #eee;
}

.btn-sort {
  margin-top: 8px;
  float: right;
  background: #333;
  color: #fff;
  border: none;
  padding: 6px 10px;
  border-radius: 4px;
}
</style>
