# ADR-001: Organização do Domain por Feature (Feature Slicing)

## Status

Accepted

## Context

Inicialmente o projeto utilizava organização por tipo técnico (Entities, Enums, Contracts, ValueObjects).

Com o crescimento esperado do domínio (ex: PaymentOrders, Accounts, Customers), essa abordagem tende a gerar:

* Baixa coesão
* Dificuldade de navegação
* Alto acoplamento entre features

## Decision

Adotar organização por feature (feature slicing) no Domain.

Cada agregado/feature conterá seus próprios:

* Entities
* ValueObjects
* Enums
* Contracts (interfaces de repositório)

Itens realmente compartilhados serão movidos para `Shared`.

## Consequences

### Positivas

* Alta coesão por feature
* Melhor escalabilidade
* Código mais alinhado com DDD
* Facilita manutenção e onboarding

### Negativas

* Possível duplicação inicial de código
* Necessidade de disciplina para evitar abuso de `Shared`

## Guidelines

* Criar uma pasta por feature (ex: PaymentOrders, Accounts)
* Evitar criar novos diretórios técnicos na raiz do Domain
* Usar `Shared` apenas quando houver reutilização real entre features

