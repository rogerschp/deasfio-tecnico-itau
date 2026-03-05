# Sistema de Compra Programada de Ações

**Itaú Corretora** – Desafio Técnico | Arquitetura **Microsserviços**

---

## Visão Geral

Sistema de compra programada que permite aos clientes aderir a um plano de investimento recorrente na carteira **Top Five** (5 ações recomendadas). Desenvolvido em **.NET 9**, **MySQL**, **Apache Kafka**, com integração ao arquivo **COTAHIST da B3**.

---

## Arquitetura Microsserviços

```
                    ┌─────────────────────────────────────────────────────────┐
                    │                    API GATEWAY (YARP)                    │
                    │                     http://localhost:5000                │
                    └───────────────────────────┬─────────────────────────────┘
                                                │
        ┌───────────────────┬───────────────────┼───────────────────┬───────────────────┐
        │                   │                   │                   │                   │
        ▼                   ▼                   ▼                   ▼                   ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│   Clientes    │  │    Admin      │  │   Cotação     │  │ Motor Compra  │  │ Rebalanceamento│
│   Service     │  │   Service     │  │   Service     │  │   Service     │  │   Service      │
│   :5001       │  │   :5002       │  │   :5003       │  │   :5004       │  │   :5005        │
└───────┬───────┘  └───────┬───────┘  └───────┬───────┘  └───────┬───────┘  └───────┬───────┘
        │                  │                  │                  │                  │
        │                  │                  │                  │                  │
        ▼                  ▼                  ▼                  ▼                  ▼
   [MySQL]            [MySQL]            [MySQL]            [MySQL]            [MySQL]
   clientes_db        admin_db           CotacaoDb         motor_db           rebalanceamento_db
        │                  │                  │                  │                  │
        └──────────────────┴──────────────────┴──────────────────┴──────────────────┘
                                                │
                                                ▼
                                        ┌─────────────┐
                                        │    Kafka    │
                                        │  :9092      │
                                        │ ir-dedo-duro│
                                        └─────────────┘
```

### Responsabilidade dos Serviços

| Serviço | Responsabilidade | API Base |
|---------|------------------|----------|
| **ApiGateway** | Roteamento reverso (YARP), único ponto de entrada | `/` |
| **Clientes.Service** | Adesão, saída, alterar valor mensal, consultar carteira, rentabilidade | `/api/clientes` |
| **Admin.Service** | Cesta Top Five (CRUD), histórico, custódia master | `/api/admin` |
| **Cotacao.Service** | Leitura/parse COTAHIST B3, cache de cotações, fechamento por ticker | `/api/cotacoes` |
| **MotorCompra.Service** | Execução da compra programada (dias 5, 15, 25), distribuição, ordens | `/api/motor` |
| **MotorCompra.Worker** | Job em background que dispara a compra nas datas configuradas | — |
| **Rebalanceamento.Service** | Rebalanceamento por mudança de cesta ou desvio de proporção | `/api/rebalanceamento` |

### Comunicação

- **Síncrona:** Cliente → Gateway → Serviços via HTTP/REST.
- **Assíncrona:** MotorCompra e Rebalanceamento publicam eventos de **IR** no **Kafka** (tópico `ir-dedo-duro`).

### Banco de Dados (MySQL)

Cada microsserviço pode usar um **database próprio** na mesma instância MySQL (ou instâncias separadas):

- `clientes_db` – Clientes, ContasGraficas, Custodias (filhote)
- `admin_db` – CestasRecomendacao, ItensCesta
- `CotacaoDb` – Cache de cotações (COTAHIST)
- `motor_db` – OrdensCompra, Distribuicoes, ContaMaster, CustodiaMaster, ExecucaoCompras
- `rebalanceamento_db` – Rebalanceamentos, eventos de venda

---

## Estrutura do Projeto

```
/
├── src/
│   ├── ApiGateway/                 # YARP reverse proxy :5000
│   ├── Shared/
│   │   ├── Shared.Contracts/       # DTOs, eventos (Adesao, Cesta, EventoIR)
│   │   └── Shared.Kafka/           # IEventoIRPublisher, Kafka producer
│   └── Services/
│       ├── Clientes.Service/       # API clientes :5001
│       ├── Admin.Service/          # API admin :5002
│       ├── Cotacao.Service/        # API cotações + parser COTAHIST :5003
│       ├── MotorCompra.Service/    # API motor + orquestração :5004
│       ├── MotorCompra.Worker/     # Background job dias 5/15/25
│       └── Rebalanceamento.Service/# API rebalanceamento :5005
├── cotacoes/                       # Pasta padrão para COTAHIST (configurável no Cotacao.Service)
├── tests/                          # Testes unitários e integração
├── Docs/                           # Documentação do desafio
├── docker-compose.yml              # MySQL + Kafka (+ Zookeeper)
├── CompraProgramada.sln
└── README.md
```

---

## Pré-requisitos

- .NET 9 SDK
- Docker e Docker Compose (MySQL, Kafka)
- Arquivos COTAHIST na pasta `cotacoes/` (download no site B3)

---

## Como Rodar

### 1. Subir infraestrutura

```bash
docker-compose up -d
```

Aguarde MySQL e Kafka estarem saudáveis.

### 2. Criar databases (se usar um único MySQL)

```bash
docker exec -it compraprogramada-mysql mysql -uroot -proot -e "
CREATE DATABASE IF NOT EXISTS clientes_db;
CREATE DATABASE IF NOT EXISTS admin_db;
CREATE DATABASE IF NOT EXISTS CotacaoDb;
CREATE DATABASE IF NOT EXISTS motor_db;
CREATE DATABASE IF NOT EXISTS rebalanceamento_db;
"
```

### 3. Restaurar dependências e rodar os serviços

Use a solution **CompraProgramada.sln**:

```bash
dotnet restore CompraProgramada.sln
dotnet build CompraProgramada.sln
```

Em terminais separados (ou via IDE):

```bash
# Terminal 1 – Gateway
dotnet run --project src/ApiGateway

# Terminal 2 – Clientes
dotnet run --project src/Services/Clientes.Service

# Terminal 3 – Admin
dotnet run --project src/Services/Admin.Service

# Terminal 4 – Cotação
dotnet run --project src/Services/Cotacao.Service

# Terminal 5 – Motor
dotnet run --project src/Services/MotorCompra.Service

# Terminal 6 – Rebalanceamento
dotnet run --project src/Services/Rebalanceamento.Service

# (Opcional) Worker do motor
dotnet run --project src/Services/MotorCompra.Worker
```

### 4. Acessar

- **Gateway (todas as APIs):** http://localhost:5000
- **Exemplos:**
  - `GET http://localhost:5000/api/clientes/1/carteira`
  - `GET http://localhost:5000/api/admin/cesta/atual`
  - `POST http://localhost:5000/api/motor/executar-compra`

Swagger/OpenAPI em cada serviço (quando habilitado) em `http://localhost:500X/swagger`.

---

## Decisões Técnicas

- **Gateway:** YARP para roteamento reverso leve e configurável.
- **Microsserviços:** um serviço por bounded context (clientes, cesta, cotação, motor, rebalanceamento).
- **Kafka:** um tópico para eventos de IR (dedo-duro e IR venda), consumível por sistemas fiscais.
- **B3:** integração via arquivo COTAHIST (layout posicional 245 caracteres); parser no **Cotacao.Service**.
- **Database:** MySQL por serviço (ou databases separados na mesma instância) para evolução independente.

---

## CI/CD

O projeto inclui pipelines **GitHub Actions** para integração e entrega contínuas.

### CI (`.github/workflows/ci.yml`)

- **Trigger:** push e pull requests em `main`, `master`, `develop`.
- **Jobs:**
  - **Build:** restore + build em Release.
  - **Test:** testes com xUnit, cobertura (Coverlet), relatório TRX e ReportGenerator; artefatos de cobertura e resultados de teste; report de testes em PRs.
  - **Format check:** `dotnet format --verify-no-changes`.
  - **Security:** listagem de pacotes vulneráveis (`dotnet list package --vulnerable`).
- **Cache:** NuGet em `~/.nuget/packages` por hash dos `.csproj`.
- **Cobertura:** mínimo de 70% (configurável por `MIN_COVERAGE`); relatório em HTML/Summary/Badges.

### CD (`.github/workflows/cd.yml`)

- **Trigger:** push em `main`/`master`, tags `v*`, e `workflow_dispatch`.
- **Jobs:** build e push de imagens Docker para **GitHub Container Registry** (`ghcr.io`):
  - `compraprogramada-api-gateway`
  - `compraprogramada-clientes`
  - `compraprogramada-admin`
  - `compraprogramada-cotacao`
  - `compraprogramada-motor`
  - `compraprogramada-motor-worker`
  - `compraprogramada-rebalanceamento`
- **Versão:** tags `v*` ou `sha-<commit>`.
- **Cache:** Docker layer cache (GitHub Actions cache).

### Security (`.github/workflows/security.yml`)

- **CodeQL:** análise estática em C# (push em `main`/`master`, PRs, agendamento semanal).
- **Dependency review:** revisão de dependências em PRs.

### Dependabot (`.github/dependabot.yml`)

- Atualizações semanais (segunda-feira) para NuGet e GitHub Actions.
- Agrupamento de pacotes Microsoft/xunit/coverlet; limite de 5 PRs abertos.

### Rodar localmente (equivalente ao CI)

```bash
dotnet restore CompraProgramada.sln
dotnet build CompraProgramada.sln -c Release
dotnet test CompraProgramada.sln -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

---

## Documentação do Desafio

Regras de negócio, layout COTAHIST, contratos de API e diagramas estão em **Docs/**:

- `desafio-tecnico-compra-programada.md`
- `regras-negocio-detalhadas.md`
- `layout-cotahist-b3.md`
- `exemplos-contratos-api.md`
- `glossario-compra-programada.md`
- Diagramas ER, sequência e negócios (drawio)

---

## Licença

Desafio técnico – Itaú Corretora.
