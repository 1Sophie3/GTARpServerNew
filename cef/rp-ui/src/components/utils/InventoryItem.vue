<template>
  <div class="inventoryitem">
    <div class="inventorytext">
      <span class="item-icon" v-if="item.descriptionitem">{{ iconPlaceholder }}</span>
      <span class="item-label">{{ item.descriptionitem || item.name || 'Gegenstand' }} <strong>[{{ item.amount }}]</strong></span>
      <div class="item-actions">
        <button @click="useitem">Benutzen</button>
        <button @click="destroyitem">Wegwerfen</button>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({ item: Object })
const iconPlaceholder = '📦'
function useitem() {
  // eslint-disable-next-line no-undef
  if (typeof mp !== 'undefined') mp.trigger('InventarAktion', props.item?.id, 'konsumieren')
}
function destroyitem() {
  if (typeof mp !== 'undefined') mp.trigger('InventarAktion', props.item?.id, 'wegwerfen')
}
</script>

<style scoped>
.inventoryitem { color: #fff; padding: 6px; background: #5c5b5b; border-radius: 6px; display:flex; align-items:center; justify-content:space-between }
.inventorytext { display:flex; align-items:center; gap:8px }
.item-label { font-size:14px }
.item-actions button { background:#333; color:#fff; border:none; padding:4px 8px; margin-left:6px; border-radius:4px }
</style>
