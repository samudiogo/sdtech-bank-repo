# ADR-002 - Resolver de Recebedor baseado em Specification Pattern

## Status
Accepted

## Contexto

O processo de resolução do recebedor (`ReceiverResolverChain`) utiliza múltiplos steps encadeados (`IReceiverResolutionStep`), onde cada implementação decide internamente se deve ou não atuar.

Exemplo atual:

- InternalAccountReceiverResolver
- ExternalAccountReceiverResolver

Cada resolver contém simultaneamente:

1. Regra para verificar se se aplica ao pagamento
2. Regra para consultar dados
3. Retorno null quando não aplicável

Esse modelo gera problemas:

- Forte dependência da ordem de registro no DI
- Logs pouco claros
- Dificuldade para testes unitários isolados
- Crescimento exponencial ao adicionar novos cenários:
  - Pix interno
  - Pix externo
  - QR Code
  - Conta manual
  - TED futura
  - Agendamento
- Violação do Single Responsibility Principle

## Decisão

Adotar Specification Pattern para separar:

- Elegibilidade do resolver (`CanResolve`)
- Execução da resolução (`ResolveAsync`)

Cada estratégia possuirá:

- Uma specification explícita
- Um resolver focado apenas em executar

Fluxo:

1. Pipeline percorre estratégias
2. Avalia `CanResolve(payment)`
3. Primeira elegível executa
4. Retorna resultado

## Estrutura proposta

```csharp
IReceiverSpecification
bool IsSatisfiedBy(PaymentOrder payment)

IReceiverStrategy
bool CanResolve(PaymentOrder payment)
Task<Guid?> ResolveAsync(...)