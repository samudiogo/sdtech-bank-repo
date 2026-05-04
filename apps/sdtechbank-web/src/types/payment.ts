export interface Payment {
    id: string
    amount: number,
    status: 'CREATED' | 'COMPLETED' | 'FAILED' | 'WAITING_FOR_DICT'
    createdAt: string
}

export interface PaymentSummary {
  total: number
  pending: number
  completed: number
  failed: number
}

export type PaymentType = 'PIX' | 'BANK_ACCOUNT'

export interface CreatePaymentRequest {
  idempotencyKey: string
  amount: number
  payerId: string
  receiver: {
    pixKey?: string | null
    bankAccount?: {
      fullName: string
      bankCode: string
      branch: string
      account: string
      cpf: string
    }
  }
}