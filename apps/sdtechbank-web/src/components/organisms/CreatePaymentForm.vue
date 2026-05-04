<template>
    <q-form class="q-gutter-md" @submit="onSubmit">
        <!-- Tipo -->
        <q-select v-model="type" :options="typeOptions" label="Tipo de pagamento" emit-value map-options />

        <!-- Valor -->
        <q-input v-model="amountMasked" label="Valor" filled mask="#,##" fill-mask="0" reverse-fill-mask prefix="R$ "
            inputmode="numeric" />

        <!-- Payer -->
        <q-input v-model="form.payerId" label="Payer ID" filled />

        <!-- PIX -->
        <q-input v-show="type === 'PIX'" v-model="form.receiver.pixKey" label="Chave Pix" filled />

        <!-- Conta bancária -->
        <div v-show="type === 'BANK_ACCOUNT'" class="q-gutter-sm">

            <q-input v-model="bank.fullName" label="Nome completo" filled />
            <q-input v-model="bank.bankCode" label="Banco" filled />
            <q-input v-model="bank.branch" label="Agência" filled />
            <q-input v-model="bank.account" label="Conta" filled />
            <q-input v-model="bank.cpf" label="CPF" filled />

        </div>

        <q-btn type="submit" label="Criar pagamento" color="primary" />

    </q-form>
</template>
<script setup lang="ts">
import { paymentService } from 'src/services/payment.service';
import type { CreatePaymentRequest, PaymentType } from 'src/types/payment';
import { ref } from 'vue';

const type = ref<PaymentType>('PIX');
const typeOptions = [
    { label: 'PIX', value: 'PIX' },
    { label: 'Conta Bancária', value: 'BANK_ACCOUNT' }
];
const amountMasked = ref('R$ 0,00');
const form = ref<CreatePaymentRequest>({
    idempotencyKey: crypto.randomUUID(),
    amount: 0,
    payerId: '',
    receiver: {
        pixKey: ''
    }
});
const bank = ref({
    fullName: '',
    bankCode: '',
    branch: '',
    account: '',
    cpf: ''
})

function parseCurrency(value: string): number {
    return Number(
        value
            .replaceAll('R$', '')
            .replaceAll('.', '')
            .replace(',', '.') // aqui pode manter replace (só existe uma vírgula)
            .trim()
    );
}

async function onSubmit() {
    const payload: CreatePaymentRequest = {
        ...form.value,
        idempotencyKey: crypto.randomUUID(),
        amount: parseCurrency(amountMasked.value),
        receiver: type.value === 'PIX' ? { pixKey: form.value.receiver.pixKey ?? null } : { pixKey: null, bankAccount: bank.value }
    }
    console.log(payload)
    await paymentService.create(payload)

    alert('Pagamento criado com sucesso!')
}
</script>