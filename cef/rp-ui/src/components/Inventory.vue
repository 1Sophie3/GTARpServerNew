<template>
  <div v-if="visible" class="inventory-overlay">
    <div class="inventory-wrapper">
      <div class="inv-header">
        <h2>Inventar</h2>
        <button class="close-btn" @click="closeInventory">✕</button>
      </div>

   


          <div class="inventory-layout">
            <aside class="left-inv">
              <div class="left-columns">
                <div class="clothing-column">
                  <div class="clothing-vertical">
                    <!-- Head (center) -->
                    <div class="clothing-row clothing-row--center">
                      <div class="clothing-slot big">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.head" :item="clothingSlots.head" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.head }}</div>
                        </div>
                      </div>
                    </div>

                    <!-- Glasses + Mask (paired) -->
                    <div class="clothing-row clothing-row--pair">
                      <div class="clothing-slot small">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.glasses" :item="clothingSlots.glasses" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.glasses }}</div>
                        </div>
                      </div>
                      <div class="clothing-slot small">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.mask" :item="clothingSlots.mask" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.mask }}</div>
                        </div>
                      </div>
                    </div>

                    <!-- Torso row: Bag - Torso - Vest (Torso centered) -->
                    <div class="clothing-row clothing-row--three-centered">
                      <div class="clothing-slot">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.bag" :item="clothingSlots.bag" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.bag }}</div>
                        </div>
                      </div>
                      <div class="clothing-slot">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.torso" :item="clothingSlots.torso" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.torso }}</div>
                        </div>
                      </div>
                      <div class="clothing-slot">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.vest" :item="clothingSlots.vest" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.vest }}</div>
                        </div>
                      </div>
                    </div>

                    <!-- Underwear row: three equal slots (watch - underwear - bracelet), underwear centered -->
                    <div class="clothing-row clothing-row--three-centered">
                      <div class="clothing-slot">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.watch" :item="clothingSlots.watch" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.watch }}</div>
                        </div>
                      </div>
                      <div class="clothing-slot">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.underwear" :item="clothingSlots.underwear" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.underwear }}</div>
                        </div>
                      </div>
                      <div class="clothing-slot">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.bracelet" :item="clothingSlots.bracelet" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.bracelet }}</div>
                        </div>
                      </div>
                    </div>

                    <!-- Legs -->
                    <div class="clothing-row clothing-row--center">
                      <div class="clothing-slot big">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.legs" :item="clothingSlots.legs" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.legs }}</div>
                        </div>
                      </div>
                    </div>

                    <!-- Shoes -->
                    <div class="clothing-row clothing-row--center">
                      <div class="clothing-slot big">
                        <div class="slot-inner inv-slot-inner inv-slot">
                          <InventoryItem v-if="clothingSlots.shoes" :item="clothingSlots.shoes" />
                          <div v-else class="empty"></div>
                          <div class="slot-label">{{ clothingLabels.shoes }}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="main-panel">
                  <div class="inv-panel-header">
                    <div>
                        <div class="inv-id">{{ categoryLabel }} · {{ visibleInventory.id || '—' }}</div>
                          <div class="weight-small">Belegt: {{ playerOccupied }} / {{ playerCapacity }} · Gewicht: {{ playerWeight }} / {{ playerCapacityWeight }} kg</div>
                    </div>
                  </div>
                  <div class="inventory-grid">
                    <div
                      v-for="(slot, idx) in visibleSlots"
                      :key="idx"
                      class="inv-slot"
                      @click="selectSlot(idx)"
                    >
                      <div class="slot-item" v-if="slot">
                        <InventoryItem :item="slot" />
                      </div>
                      <div class="empty" v-else></div>
                    </div>
                  </div>
                </div>
              </div>
            </aside>

            <aside class="right-inv">
              <div class="env-header">
                <div>
                  <div class="env-title">{{ targetLabel }}</div>
                  <div class="weight-small">Belegt: {{ targetOccupied }} / {{ targetCapacity }} · Gewicht: {{ targetWeight }} / {{ targetCapacityWeight }} kg</div>
                </div>
                <div class="env-id">{{ openTargetId || '—' }}</div>
              </div>
              <div class="env-grid">
                <div v-for="(slot, i) in targetSlots" :key="i" class="inv-slot env-slot">
                  <div v-if="slot"><InventoryItem :item="slot" /></div>
                  <div v-else class="empty"></div>
                </div>
              </div>
            </aside>
          </div>

      <div class="slot-actions" v-if="selectedSlot !== null">
        <div>Ausgewählt: Slot {{ selectedSlot + 1 }}</div>
        <div v-if="getSlotItem(selectedSlot)">
          <div>Gegenstand: {{ getSlotItem(selectedSlot).name }}</div>
          <input v-model.number="transferAmount" type="number" min="1" placeholder="Anzahl" />
          <input v-model="transferTargetId" placeholder="Ziel Inventar ID" />
          <select v-model="transferTargetType">
            <option value="player">Spieler</option>
            <option value="vehicle">Fahrzeug</option>
            <option value="wardrobe">Kleiderschrank</option>
            <option value="faction">Fraktion</option>
            <option value="house">Haus</option>
          </select>
          <button @click="transferSelected">Transfer</button>
        </div>
        <div v-else>Slot ist leer.</div>
        <button @click="clearSelection">Schließen</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import InventoryItem from './utils/InventoryItem.vue'

const categories = ['player', 'vehicle', 'wardrobe', 'faction', 'house']
const labelMap = {
  player: 'Spieler',
  vehicle: 'Fahrzeug',
  wardrobe: 'Kleiderschrank',
  faction: 'Fraktion',
  house: 'Haus'
}

const category = ref('player')
const inventories = ref({}) // { category: { inventoryId: { slots: [...] } } }
const selectedSlot = ref(null)
const transferAmount = ref(1)
const transferTargetId = ref('')
const openTargetId = ref('')
const transferTargetType = ref('player')
const defaultSlots = {
  player: 30,
  vehicle: 20,
  wardrobe: 18,
  faction: 40,
  house: 30
}

const openTargetCategory = ref('vehicle')

const visible = ref(true)

// clothing state (vertical center column)
const clothingSlots = ref({
  head: null,
  glasses: null,
  mask: null,
  torso: null,
  bag: null,
  vest: null,
  underwear: null,
  watch: null,
  bracelet: null,
  legs: null,
  shoes: null
})

// order for vertical rendering (centered column)
const clothingOrderFlat = ['head','glasses','mask','torso','bag','vest','underwear','watch','bracelet','legs','shoes']
const clothingLabels = {
  head: 'Hut',
  glasses: 'Brille',
  mask: 'Maske',
  torso: 'Oberteil',
  bag: 'Tasche',
  vest: 'Weste',
  underwear: 'Unterteil',
  watch: 'Uhr',
  bracelet: 'Armband',
  legs: 'Hose',
  shoes: 'Schuhe'
}

function selectClothing(key) {
  selectedSlot.value = null
}

const visibleInventory = computed(() => {
  const cat = category.value
  const invs = inventories.value[cat] || {}
  const ids = Object.keys(invs)
  if (ids.length === 0) {
    return { id: 'local', slots: Array(defaultSlots[cat]).fill(null) }
  }
  const id = ids[0]
  return { id, slots: invs[id].slots }
})

const visibleSlots = computed(() => visibleInventory.value.slots)

const targetSlots = computed(() => {
  const cat = openTargetCategory.value || category.value
  const id = openTargetId.value || 'local'
  const invs = inventories.value[cat] || {}
  if (!invs[id]) return Array(defaultSlots[cat] || visibleSlots.value.length).fill(null)
  return invs[id].slots
})

// Occupied / Capacity and weight (show occupied and weight/max)
function parseMetaWeight(meta) {
  if (!meta) return 0
  try {
    const m = typeof meta === 'string' ? JSON.parse(meta) : meta
    return parseFloat(m.weight || m.weightKg || 0) || 0
  } catch (e) { return 0 }
}

function getItemWeight(item) {
  if (!item) return 0
  const amount = item.amount || 1
  const w = parseFloat(item.weight || 0) || parseMetaWeight(item.meta)
  return (w || 0) * amount
}

const playerOccupied = computed(() => visibleSlots.value.filter(s => !!s).length)
const playerCapacity = computed(() => visibleSlots.value.length)
const playerWeight = computed(() => Math.round(visibleSlots.value.reduce((acc, s) => acc + getItemWeight(s), 0) * 10) / 10)

const targetOccupied = computed(() => targetSlots.value.filter(s => !!s).length)
const targetCapacity = computed(() => targetSlots.value.length)
const targetWeight = computed(() => Math.round(targetSlots.value.reduce((acc, s) => acc + getItemWeight(s), 0) * 10) / 10)

const defaultWeightCapacity = { player: 120, vehicle: 800, wardrobe: 50, faction: 500, house: 300 }
const playerCapacityWeight = computed(() => defaultWeightCapacity[category.value] || 120)
const targetCapacityWeight = computed(() => defaultWeightCapacity[openTargetCategory.value] || 120)

const categoryLabel = computed(() => labelMap[category.value] || category.value)
const targetLabel = computed(() => labelMap[openTargetCategory.value] || openTargetCategory.value || 'Ziel')

function selectSlot(idx) {
  selectedSlot.value = idx
}

function clearSelection() {
  selectedSlot.value = null
  transferAmount.value = 1
  transferTargetId.value = ''
}

function getSlotItem(idx) {
  return visibleSlots.value[idx] || null
}

function openTarget() {
  const t = openTargetCategory.value || category.value
  const id = openTargetId.value || ''
  if (typeof mp !== 'undefined') {
    mp.trigger('server:inventoryOpen', t, id)
  } else {
    console.log('[DEV MODE] server:inventoryOpen', t, id)
    window.dispatchEvent(new CustomEvent('updateInventory', {
      detail: { category: t, id: id || 'local', slots: [{ name: 'Wasserflasche', amount: 2 }, null, { name: 'Pistole', amount: 1 }] }
    }))
  }
}

function transferSelected() {
  const fromInvId = visibleInventory.value.id
  const toType = transferTargetType.value
  const toId = transferTargetId.value
  const slotIdx = selectedSlot.value
  const amount = transferAmount.value
  if (typeof mp !== 'undefined') {
    mp.trigger('server:inventoryTransfer', category.value, fromInvId, toType, toId, slotIdx, amount)
  } else {
    console.log('[DEV MODE] Transfer', { from: { cat: category.value, inv: fromInvId, slot: slotIdx }, to: { type: toType, id: toId }, amount })
    window.dispatchEvent(new CustomEvent('inventoryTransferResult', { detail: { success: true, message: 'Transfer simuliert' } }))
  }
  clearSelection()
}

onMounted(() => {
  window.addEventListener('updateInventory', (e) => {
    const d = e.detail
    if (!inventories.value[d.category]) inventories.value[d.category] = {}
    inventories.value[d.category][d.id || 'local'] = { slots: d.slots }
    if (d.clothing) {
      for (const k of Object.keys(clothingSlots.value)) {
        clothingSlots.value[k] = d.clothing[k] || null
      }
    }
  })

  window.addEventListener('inventoryTransferResult', (e) => {
    const d = e.detail
    if (d.success) alert(d.message || 'Transfer erfolgreich')
    else alert(d.message || 'Transfer fehlgeschlagen')
  })
})

function closeInventory() {
  if (typeof mp !== 'undefined') {
    try { mp.trigger('cef:inventoryClose'); } catch (e) { console.log('mp.trigger error', e); }
  } else {
    visible.value = false
  }
}
</script>

<style scoped>
.inventory-wrapper {
  --slot-size: 72px; /* einheitliche Slot-Größe, anpassbar */
  max-width: 1100px;
  margin: 18px auto;
  color: #fff;
  font-family: 'Inter', system-ui, -apple-system, 'Segoe UI', Roboto, 'Helvetica Neue', Arial;
  background: linear-gradient(180deg, rgba(0,0,0,0.55), rgba(0,0,0,0.65));
  padding: 14px;
  border-radius: 10px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.6);
}
.inv-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.inv-controls select,
.inv-controls input {
  margin-left: 8px;
  padding: 6px 8px;
  border-radius: 6px;
  border: none;
}
.inventory-grid {
  display: grid;
  grid-template-columns: repeat(5, var(--slot-size)); /* 5 Spalten mit fester Slot-Größe */
  justify-content: center;
  gap: 10px;
  margin-top: 12px;
}

/* common slot appearance */
.inv-slot {
  background: linear-gradient(180deg, rgba(255,255,255,0.03), rgba(255,255,255,0.02));
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 8px;
  width: var(--slot-size);
  height: var(--slot-size);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  transition: transform 0.08s ease, box-shadow 0.08s ease;
  box-shadow: 0 2px 6px rgba(0,0,0,0.45) inset;
  position: relative;
}
.inv-slot:hover { transform: translateY(-3px); box-shadow: 0 6px 18px rgba(0,0,0,0.6); }
.slot .slot-index, .inv-slot .slot-index { position: absolute; top: 6px; left: 8px; font-size: 11px; color: rgba(255,255,255,0.6); }
.slot .amount { font-weight: 700; margin-top: 6px }
.empty { color: rgba(255,255,255,0.45) }

.slot:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 18px rgba(0,0,0,0.6);
}

.slot .slot-index {
  position: absolute;
  top: 6px;
  left: 8px;
  font-size: 11px;
  color: rgba(255,255,255,0.6);
}
.slot-actions { margin-top: 14px; background: rgba(0,0,0,0.5); padding: 12px; border-radius: 8px }

.inventory-overlay{
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(6,10,20,0.45);
  backdrop-filter: blur(6px);
  z-index: 9999;
}

.inventory-wrapper{
  width: 1100px;
  max-width: calc(100% - 40px);
}

.inventory-layout{ display:flex; gap:18px; align-items:flex-start }
.left-quick{ width:520px; }
.quick-title{ color:#ddd; margin-bottom:8px; font-weight:600 }
.quick-slots{ display:flex; flex-direction:column; gap:10px }
.quick-slot{ background: rgba(255,255,255,0.03); border-radius:8px; display:flex; align-items:center; justify-content:center; width: var(--slot-size); height: var(--slot-size) }

.main-area{ flex:1 }
.top-quick{ display:flex; gap:8px; margin-bottom:12px }
.top-slot{ background: rgba(255,255,255,0.03); border-radius:8px; display:flex; align-items:center; justify-content:center; width: var(--slot-size); height: var(--slot-size) }

.left-inv{ flex: 1 }
.right-inv{ width: calc(var(--slot-size) * 5 + 48px); }
.right-env{ width:260px }
.left-columns{ display:flex; gap:28px; align-items:flex-start }
.clothing-column{ display:flex; align-items:center; justify-content:center; width: calc(var(--slot-size) + 24px); }
.clothing-vertical{ display:flex; flex-direction:column; gap:12px; align-items:center }
.clothing-slot{ display:flex; flex-direction:column; align-items:center; gap:6px; width: var(--slot-size); height: var(--slot-size) }
.inv-slot-inner{ width: var(--slot-size); height: var(--slot-size); display:flex; align-items:center; justify-content:center; position:relative }
.slot-label{ position:absolute; bottom:6px; left:50%; transform:translateX(-50%); font-size:11px; color:#cfd8e3; pointer-events:none }
.main-panel{ flex: 1 }
.env-header{ display:flex; justify-content:space-between; align-items:center; margin-bottom:8px }
.env-title{ color:#ddd; font-weight:600 }
.env-id{ color:#9fb8d9; font-weight:700; font-size:13px }
.env-grid{ display:grid; grid-template-columns: repeat(5, var(--slot-size)); gap:8px }
.env-slot{ background: rgba(255,255,255,0.02); border-radius:6px; width: var(--slot-size); height: var(--slot-size) }

.weight-small { color:#ddd; font-size:12px; margin-top:6px }

.weight-row{ display:flex; align-items:center; gap:12px; margin-top:12px }
.weight-bar{ flex:1; height:8px; background: rgba(255,255,255,0.06); border-radius:6px; overflow:hidden }
.weight-fill{ height:100%; background: linear-gradient(90deg,#39a0ff,#6dd3ff) }
.weight-value{ color:#ddd; font-size:12px }

/* Clothing layout helpers */
.clothing-row{ display:flex; align-items:center; gap:10px }
.clothing-row--center{ justify-content:center }
.clothing-row--pair{ justify-content:center }
.clothing-row--side{ justify-content:space-between }
.clothing-slot.big{ width: var(--slot-size); height: var(--slot-size) }
.clothing-slot.small{ width: var(--slot-size); height: var(--slot-size) }
.clothing-side{ display:flex; flex-direction:column; gap:8px; justify-content:center }
.clothing-row--three-centered{ justify-content:center; gap:10px }
</style>
