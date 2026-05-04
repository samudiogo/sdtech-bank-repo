import type { Payment, PaymentSummary, CreatePaymentRequest } from 'src/types/payment';
import { http } from './http';

export const paymentService = {
  async getAll(): Promise<Payment[]> {
    const { data } = await http.get('/api/payments');
    return data;
  },

  async getSummary(): Promise<PaymentSummary> {
    const payments = await this.getAll();

    return {
      total: payments.length,
      pending: payments.filter((p) => p.status === 'WAITING_FOR_DICT').length,
      completed: payments.filter((p) => p.status === 'COMPLETED').length,
      failed: payments.filter((p) => p.status === 'FAILED').length,
    };
  },

  async create(request: CreatePaymentRequest) {
    const { data } = await http.post('/api/payments', request);
    return data;
  },
};
