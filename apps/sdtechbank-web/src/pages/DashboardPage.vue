<template>
    <q-page class="q-pa-md">

        <!-- MÉTRICAS -->
        <div class="row q-col-gutter-md q-mb-lg">
            <div class="col-3">
                <MetricCard label="Pagamentos" :value="store.summary.total" />
            </div>
            <div class="col-3">
                <MetricCard label="Pendentes" :value="store.summary.pending" />
            </div>
            <div class="col-3">
                <MetricCard label="Efetivados" :value="store.summary.completed" />
            </div>
            <div class="col-3">
                <MetricCard label="Erro" :value="store.summary.failed" />
            </div>
        </div>
        <div class="row">
            <div class="col-2">
                <q-btn to="/create" color="primary" label="Nova Transferência"/>
            </div>
        </div>

        <!-- TABELA -->
        <TransactionsTable :payments="store.payments" />

    </q-page>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { usePaymentStore } from 'src/stores/payment.store'
import MetricCard from 'src/components/molecules/MetricCard.vue'
import TransactionsTable from 'src/components/organisms/TransactionsTable.vue'

const store = usePaymentStore()

onMounted(async () => {
    await store.fetchPayments()
})
</script>