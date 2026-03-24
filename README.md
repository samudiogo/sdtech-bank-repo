# 🏦 SDTECH Bank — Pix Payment System (Event-Driven)

Sistema distribuído para simulação de transferências Pix ponta a ponta, inspirado no fluxo oficial do Banco Central.

> Este projeto tem como objetivo demonstrar capacidade de **modelagem de domínio financeiro**, **arquitetura orientada a eventos** e **consistência em sistemas distribuídos**.

---

# 🎯 Objetivo

Construir um sistema capaz de:

- Processar transferências Pix end-to-end
- Garantir consistência financeira com **ledger (double-entry accounting)**
- Operar de forma **event-driven**
- Ser resiliente a duplicidade de mensagens (**idempotência**)
- Permitir rastreabilidade completa das transações

---

# 🧠 Contexto de Negócio

O fluxo Pix envolve:

1. Geração da ordem de pagamento
2. Validação dos dados
3. Confirmação do usuário
4. Liquidação da transação

Este projeto simula um **PSP (Payment Service Provider)** com integração ao DICT (via serviço mockado `BcDict`).

---

# 🧩 Domínio

## Entidades principais

### Transaction
Representa uma transferência Pix.

- `transactionId`
- `amount`
- `status`
- `payer`
- `receiver`
- `correlationId`

---

### Account
- `accountId`
- `status`

> ⚠️ Saldo não é fonte da verdade — é derivado do ledger

---

### PixKey
- `key`
- `accountId`

---

### LedgerEntry (core do sistema)

Modelo de **double-entry accounting**:

| Tipo   | Conta    | Valor |
|--------|---------|------|
| Débito | Pagador | -100 |
| Crédito| Recebedor | +100 |

Regras:
- Sempre balanceado
- Imutável
- Não pode ser atualizado

---

## Estados da transação

CREATED → RECEIVER_RESOLVED → VALIDATED → WAITING_CONFIRMATION → CONFIRMED → PROCESSING → COMPLETED → FAILED

---

## Invariantes

- Transação não pode ser processada mais de uma vez
- Ledger deve sempre estar balanceado
- Dados financeiros são imutáveis
- Consistência eventual controlada via eventos

---

# 🧱 Arquitetura

## Estilo

- Event-Driven Architecture
- Microsserviços desacoplados
- Comunicação assíncrona via RabbitMQ

---

## Componentes

### 🔹 Payment API
- Entrada do sistema
- Recebe requisição do cliente
- Publica evento `PaymentRequested`

---

### 🔹 Payment Worker
- Orquestra o fluxo
- Consome eventos
- Executa regras de negócio

---

### 🔹 BcDict Service
- Simula o DICT do Bacen
- Resolve chave Pix → dados bancários

---

### 🔹 Ledger Service
- Responsável pelo registro financeiro
- Garante consistência contábil

---

# 🔄 Fluxo de processamento

PaymentRequested → PixKeyResolved → PaymentValidated → PaymentConfirmed → PaymentProcessed → PaymentCompleted

---

# 📩 Eventos

## PaymentRequested

{
  "transactionId": "uuid",
  "amount": 100,
  "payerPixKey": "from@bank.com",
  "receiverPixKey": "to@bank.com",
  "correlationId": "abc-123"
}

---

## PixKeyResolved

{
  "transactionId": "uuid",
  "receiverAccount": {
    "bank": "236",
    "branch": "1218",
    "account": "123456-8"
  }
}

---

## PaymentCompleted

{
  "transactionId": "uuid",
  "status": "COMPLETED"
}

---

# 🔁 Idempotência

## Problema

Mensagens podem ser entregues mais de uma vez.

## Estratégia

- Uso de `eventId` e `transactionId`
- Controle de eventos já processados
- Operações idempotentes

## Garantias

- Nenhuma transação será executada duas vezes
- Ledger não será duplicado

---

# 📚 Ledger

## Estrutura

[
  {
    "transactionId": "uuid",
    "type": "DEBIT",
    "accountId": "payer",
    "amount": -100
  },
  {
    "transactionId": "uuid",
    "type": "CREDIT",
    "accountId": "receiver",
    "amount": 100
  }
]

---

## Regras

- Imutável
- Sempre balanceado
- Fonte única da verdade

---

# 📡 API

## Criar pagamento

POST /payments

{
  "amount": 100,
  "fromPixKey": "from@bank.com",
  "toPixKey": "to@bank.com",
  "correlationId": "abc-123"
}

---

# ⚠️ Tratamento de erros

- Chave Pix inválida
- Conta inexistente
- Saldo insuficiente
- Timeout no BcDict
- Falha no processamento
- Mensagem duplicada

---

# 🔍 Observabilidade

- correlationId em todos os fluxos
- logs estruturados
- rastreabilidade ponta a ponta

---

# 🔐 Segurança

- JWT ou API Key
- Validação de payload
- Proteção contra replay

---

# 🧪 Testes

Cobertura mínima: 80%

---

# 🛠️ Stack

- C#
- Python
- RabbitMQ
- MongoDB
- Docker

---

# 🚀 Como executar

docker-compose up --build

---

# 📄 Entregáveis

- Código no GitHub
- Diagramas
- Relatório técnico

---

# 💬 Considerações finais

Projeto focado em modelagem de domínio e sistemas distribuídos.
