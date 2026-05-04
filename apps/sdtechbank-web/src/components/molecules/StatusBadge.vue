<template>
    <q-badge :color="color" align="middle">
        {{ label }}
    </q-badge>
</template>
<script setup lang="ts">
import { computed } from 'vue'
const props = defineProps < {
    status: 'CREATED' | 'COMPLETED' | 'FAILED' | 'WAITING_FOR_DICT'
} > ()

const color = computed(() => {
    switch (props.status) {
        case 'COMPLETED': return 'positive'
        case 'FAILED': return 'negative'
        default: return 'warning'
    }
})

const label = computed(() => {
    switch (props.status) {
        case 'COMPLETED': return 'Efetivado'
        case 'FAILED': return 'Erro'
        default: return 'Pendente'
    }
})

</script>