# Compra Programada – Itaú Corretora

Sistema de compra programada (carteira Top Five) em **.NET 9**, **MySQL**, **Kafka**, integração **COTAHIST B3**.

## Arquitetura

- **ApiGateway (YARP)** – `:5000` – roteamento reverso
- **Clientes.Service** – `:5001` – adesão, saída, valor mensal, carteira, rentabilidade
- **Admin.Service** – `:5002` – cesta Top Five, histórico, custódia master
- **Cotacao.Service** – `:5003` – COTAHIST, fechamento por ticker
- **MotorCompra.Service** – `:5004` – execução compra (dias 5, 15, 25), distribuição
- **Rebalanceamento.Service** – `:5005` – rebalanceamento por mudança de cesta ou desvio
- **MotorCompra.Worker** – job em background (datas de execução)
- **Kafka** – `:9092` – eventos IR (`ir-dedo-duro`, `ir-operacoes`)

Cada serviço pode usar DB próprio no MySQL: `clientes_db`, `admin_db`, `CotacaoDb`, `motor_db`, `rebalanceamento_db`.

## Estrutura

```
src/
├── ApiGateway/
├── Shared/ (Shared.Contracts, Shared.Kafka)
└── Services/ (Clientes, Admin, Cotacao, MotorCompra, MotorCompra.Worker, Rebalanceamento)
cotacoes/     → arquivos COTAHIST (configurável)
tests/        → CompraProgramada.Tests
Docs/         → regras, contratos API, layout COTAHIST
```

## Rodar

```bash
docker-compose up -d
# Criar DBs no MySQL (clientes_db, admin_db, CotacaoDb, motor_db, rebalanceamento_db)

dotnet restore CompraProgramada.sln
dotnet build CompraProgramada.sln
```

Subir cada serviço (ou via IDE):

```bash
dotnet run --project src/ApiGateway
dotnet run --project src/Services/Clientes.Service
dotnet run --project src/Services/Admin.Service
dotnet run --project src/Services/Cotacao.Service
dotnet run --project src/Services/MotorCompra.Service
dotnet run --project src/Services/Rebalanceamento.Service
# opcional: dotnet run --project src/Services/MotorCompra.Worker
```

APIs via Gateway: `http://localhost:5000` (ex.: `GET /api/clientes/1/carteira`, `POST /api/motor/executar-compra`).

## CI/CD

- **ci.yml** – build, testes (xUnit + Coverlet), format check, segurança; cobertura mínima 70% (coverlet.runsettings).
- **cd.yml** – build e push de imagens Docker para ghcr.io.
- **security.yml** – CodeQL, dependency review.
- **dependabot.yml** – atualizações NuGet e Actions.

## Docs

`Docs/` – desafio técnico, regras de negócio, layout COTAHIST, exemplos de contratos API.

---

Desafio técnico – Itaú Corretora.
