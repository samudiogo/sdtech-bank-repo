# Épico: Motor de cálculo e consolidação de fees sobre transações Pix utilizando Event Sourcing

## Objetivo

Implementar um módulo responsável pelo cálculo, consolidação e consulta de fees incidentes sobre transações Pix liquidadas.

Este módulo será desenvolvido utilizando os padrões de Event Sourcing, CQRS e Inbox/Outbox, permitindo auditoria completa, reprocessamento de eventos e reconstrução das visões de leitura a partir do histórico de eventos.

Nesta primeira fase, os fees serão apenas calculados e consolidados, sem execução do processo de cobrança ou liquidação financeira.

## Contexto

O sdtechbank já possui um fluxo transacional orientado a eventos com mensageria confiável baseada nos padrões Inbox/Outbox.

O módulo de fees será acionado a partir do evento de domínio `PixLiquidado`, utilizando-o como gatilho para cálculo e consolidação dos valores devidos.

Os eventos relacionados ao fee serão a fonte da verdade do domínio.

## Requisitos funcionais

### RF001 — Cálculo de fee

O sistema deve calcular um fee sempre que uma transação Pix for liquidada.

### RF002 — Configuração de free tier

O sistema deve permitir configurar uma quantidade de transações isentas por período.

Valor inicial padrão:

* 10 transações por período.

O valor deve ser parametrizável por cliente.

### RF003 — Configuração da taxa

O sistema deve permitir configurar a política de cobrança por cliente.

Valores iniciais padrão:

* percentual: 1,45%;
* valor mínimo: R$ 1,75;
* valor máximo: R$ 9,90.

Todos os valores devem ser parametrizáveis.

### RF004 — Consolidação por período

O cliente deve poder escolher o período de consolidação dos fees.

Períodos suportados:

* mensal;
* trimestral;
* semestral;
* anual.

### RF005 — Consulta consolidada

O sistema deve disponibilizar consultas que permitam visualizar:

* quantidade de transações liquidadas;
* quantidade de transações isentas;
* valor total de fees calculados;
* histórico de eventos relacionados ao cálculo;
* consolidação por período.

### RF006 — Administração de regras

O sistema deve disponibilizar uma interface administrativa para:

* cadastrar clientes elegíveis;
* configurar free tier;
* configurar percentual, valor mínimo e valor máximo;
* configurar período de consolidação;
* atualizar regras futuras.

## Requisitos não funcionais

### RNF001 — Event Sourcing

Os eventos relacionados ao domínio de fees devem ser a fonte da verdade do sistema.

### RNF002 — Reprocessamento

O sistema deve ser capaz de reconstruir todas as visões de leitura a partir da reprodução dos eventos armazenados.

### RNF003 — Idempotência

O processamento do evento `PixLiquidado` deve ser idempotente.

### RNF004 — Rastreabilidade

Cada fee calculado deve ser rastreável até a transação Pix que originou o cálculo.

### RNF005 — Consistência eventual

As visões de leitura poderão apresentar consistência eventual.

## Eventos iniciais do domínio

* FeeCalculado
* FeeIsentado
* FeeAcumulado
* PeriodoDeConsolidacaoEncerrado

## Fora do escopo desta fase

* estorno de Pix;
* estorno de fees;
* liquidação financeira de fees;
* emissão de boletos;
* cobrança automática via Pix;
* inadimplência;
* parcelamento;
* integração contábil;
* notificações ao cliente.

## Critério de sucesso

O módulo será considerado concluído quando for possível remover todas as visões de leitura e reconstruí-las integralmente a partir dos eventos persistidos.
