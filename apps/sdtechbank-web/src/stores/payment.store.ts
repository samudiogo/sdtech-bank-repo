import { defineStore } from 'pinia';
import { paymentService } from 'src/services/payment.service';
import type { Payment, PaymentSummary } from 'src/types/payment';

type PaymentState = {
  payments: Payment[]
  summary: PaymentSummary
  loading: boolean
}


export const usePaymentStore = defineStore('payment', {
  state: (): PaymentState => ({
    payments: [],
    summary: {
      total: 0,
      pending: 0,
      completed: 0,
      failed: 0
    },
    loading: false
  }),

  actions: {
    async fetchPayments() {
      this.loading = true

      try {
        this.payments = await paymentService.getAll()
        this.summary = await paymentService.getSummary()
      } finally {
        this.loading = false
      }
    }
  }
})