<template>
  <q-table
    title="Transações"
    :rows="payments"
    :columns="columns"
    row-key="id"
    flat
    bordered
  >
    <template #body-cell-status="props">
      <q-td :props="props">
        <StatusBadge :status="props.row.status" />
      </q-td>
    </template>

    <template #body-cell-createdAt="props">
      <q-td :props="props">
        {{ formatDate(props.row.createdAt) }}
      </q-td>
    </template>

    <template #body-cell-amount="props">
      <q-td :props="props">
        R$ {{ props.row.amount.toFixed(2) }}
      </q-td>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import type { Payment } from 'src/types/payment'
import StatusBadge from 'src/components/molecules/StatusBadge.vue'

defineProps<{
  payments: Payment[]
}>()

const columns = [
  { name: 'id', label: 'ID', field: 'id' },
  { name: 'amount', label: 'Valor', field: 'amount' },
  { name: 'status', label: 'Status', field: 'status' },
  { name: 'createdAt', label: 'Criado em', field: 'createdAt' }
]

function formatDate(date: string) {
  return new Date(date).toLocaleString()
}
</script>