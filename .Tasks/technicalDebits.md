# Debitos tecnicos e plano de implementacao

Data da analise: 17/10/2026.

Escopo principal: TaskProjectApi e TaskProjectWeb. Application, Domain,
Infrastructure, consumidor RabbitMQ e testes foram consultados para entender
os fluxos completos. O diagnostico inicial e as propostas estao preservados;
entregas posteriores sao identificadas explicitamente abaixo.

## 1. Diagnostico inicial (antes da implementacao do TD-05)

- Os projetos analisados ja usam .NET 10; o EF Core esta na Infrastructure,
  na versao 10.0.11. Migrar de EF 5/8 nao e um debito atual.
- Application depende de Domain, nao de Infrastructure. As interfaces de
  persistencia estao em Application, com implementacoes scoped registradas em
  [DependencyInjection.cs](../TaskListProject.InfraStructure/DependencyInjection.cs).
  Preservar essa inversao de dependencia.
- Web consome a API por `ITaskReportApiClient`, sem referencia direta a EF.
  O envio do JWT ja esta isolado em um `DelegatingHandler`.
- CQRS existe para resumo de tarefas e movimentacoes, mas seus requests,
  handlers e validadores ficam na API e retornam modelos dessa camada.
  O CRUD de tarefas ainda passa por `TasksHandler` e `ITasksQueries`.
- Existem testes de fronteira, DI e handlers em
  [DataAccessBoundaryTests.cs](../TestProject/DataAccessBoundaryTests.cs).
  Ainda faltam testes HTTP e de integracao relacional confiaveis.
- A analise foi estatica. Nao foram executados build, testes, login,
  chamadas do React ou conexoes com SQL Server/RabbitMQ. Portanto, nao ha
  causa de falha de banco ou funcionamento do Swagger confirmados em runtime.

### Atualizacao de implementacao - TD-05

Implementado em 17/10/2026:
- Commands, queries, handlers, validadores existentes e resultados de tarefas,
  movimentacoes e relatorios estao em Application, registrados por `AddApplication`.
- Controllers desses recursos dependem apenas de `ISender`, adaptando modelos
  HTTP para casos de uso e resultados para modelos HTTP. Web nao referencia Domain.
- `ITasksQueries` e somente leitura; `ITasksCommands` possui criacao/edicao
  explicitas. Foram removidos `TasksHandler`, `TaskReportService`, `bool/ref` e
  a flag `Inclusion` do fluxo de persistencia.
- Edicao agora altera a entidade rastreada e persiste todos os campos;
  recurso ausente retorna 404. Criacao retorna o ID persistido no corpo e Location.
- Nomes/formatos JSON, DateTime, rotas e 204 de edicao foram preservados.
  Mapeamento de tabela fica na Infrastructure, nao nos modelos HTTP.
- Foi incluido tratamento central minimo de erros com ProblemDetails para
  viabilizar controllers finos, cobrindo parte do TD-04. Falhas de tarefas e
  resumo deixam de produzir sucesso vazio.
- Testes de fronteira/DI, persistencia SQLite isolada e HTTP com JWT/OpenAPI
  cobrem a migracao: 30 testes focados aprovados, build de API e Web sem erros.
  O modelo SQL Server continua igual ao snapshot de migrations. Comandos e
  limites da verificacao estao no README; avisos preexistentes de dependencias
  vulneraveis e nullable permanecem pendentes.

Nao conclui TD-01/04/06/07/08/10/11 integralmente. O pipeline FluentValidation
continua pendente; regras de sinais de debito, agenda, limites de periodo,
autorizacao de movimentacoes e geracao real de relatorios nao foram alteradas.
Nao ha alteracao de schema nem migration necessaria para esta entrega.

## 2. Pendencias originais preservadas

### ORIGINAL-01 - fazer o swagger funcionar com o login

Status: pendente de validacao ponta a ponta, nao de configuracao inicial.

Ja existem definicao Bearer em
[Program.cs](../TaskProjectApi/Program.cs), filtro de seguranca em
[SwaggerBearerSecurityRequirementsOperationFilter.cs](../TaskProjectApi/Configuration/SwaggerBearerSecurityRequirementsOperationFilter.cs)
e endpoint de login. Validar antes de adicionar outra solucao.

Aceite:
- Login valido retorna JWT; credencial invalida retorna 401.
- Sem token, um endpoint protegido retorna 401.
- Com token valido, a chamada pelo Swagger envia `Authorization: Bearer`
  e e aceita; token expirado e rejeitado.
- O documento OpenAPI indica seguranca nas operacoes protegidas e nao exige
  token para login. Revalidar o filtro se for adotada uma fallback policy.

### ORIGINAL-02 - fazer o react funcionar, esta dando erro de banco de dados na api

Status: relato preservado; causa ainda nao diagnosticada.

Separar conectividade do banco, migrations, autenticacao, URL da API e CORS.
Em [Program.cs](../TaskProjectApi/Program.cs), as policies CORS sao configuradas
de forma aninhada, aplicadas em dois middlewares e incluem uma origem com barra
final. Simplificar para uma policy explicita por ambiente; isso nao prova nem
corrige, por si so, uma falha de SQL Server.

Aceite:
- Reproduzir a chamada diretamente na API e pelo React, registrando status,
  erro do servidor e identificador de correlacao, sem expor credenciais.
- Validar a connection string efetiva do ambiente, acesso ao SQL Server e
  migrations pendentes; identificar e corrigir a causa com teste de regressao.
- Preflight e chamada autenticada funcionam para a origem configurada do React.
- Falhas de persistencia nao retornam 200 com dados vazios.

## 3. Backlog priorizado

Prioridades: P0 = seguranca/integridade; P1 = confiabilidade e arquitetura;
P2 = operacao e manutencao. Esforco relativo: P = pequeno, M = medio,
G = grande; nao representa estimativa em dias.

| ID | Prioridade | Tema | Esforco | Dependencias |
| --- | --- | --- | --- | --- |
| TD-01 | P0 | Autenticacao e autorizacao por recurso | G | Nenhuma |
| TD-02 | P0 | Protecao de sessao e CSRF no Web | M | Nenhuma |
| TD-03 | P1 | Configuracao tipada e inicializacao previsivel | M | Nenhuma |
| TD-04 | P1 | Erros explicitos e contrato HTTP uniforme | M | Nenhuma |
| TD-05 | P1 | CQRS em Application e segregacao de interfaces | G | TD-04 |
| TD-06 | P1 | Pipeline de validacao e invariantes | M | TD-05 |
| TD-07 | P0 | Escritas atomicas e concorrencia | G | TD-05, TD-06 |
| TD-08 | P1 | Persistencia async e consultas limitadas | M | TD-05 |
| TD-09 | P1 | Cliente HTTP resiliente e UX de falhas | M | TD-02, TD-03, TD-04 |
| TD-10 | P1 | Relatorio semanal e mensageria confiaveis | G | TD-01, TD-03, TD-04, TD-05 |
| TD-11 | P1 | Testes reproduziveis e CI | G | Nenhuma para iniciar; acompanhar cada entrega |
| TD-12 | P2 | Observabilidade, ambiente local e entrega | M | TD-03, TD-04; TD-11 para gates de entrega |

### TD-01 - Separar autenticacao, emissao de token e autorizacao

Evidencias:
- [UserQueries.cs](../TaskListProject.InfraStructure/Data/UserQueries.cs)
  consulta usuario, compara senha em texto puro, le configuracao do disco e
  emite JWT: responsabilidades distintas no mesmo componente.
- [AccountMovementsController.cs](../TaskProjectApi/Controllers/AccountMovementsController.cs)
  tem `[Authorize]` comentado e recebe `UserId` do cliente.
- Tarefas exigem autenticacao, mas as consultas atuais nao filtram por usuario
  e [TaskDto.cs](../TaskListProject.Domain/Entities/TaskDto.cs) nao tem proprietario.

Plano:
- Usar um password hasher consolidado, com migracao controlada das credenciais
  existentes ou redefinicao de senha; nao apenas trocar a comparacao.
- Separar busca de usuario, verificacao de senha e emissao de JWT por interfaces
  pequenas de Application; implementacoes tecnicas ficam na Infrastructure.
- Proteger movimentacoes e validar ownership pelo usuario autenticado; permitir
  acesso a outro usuario apenas com permissao explicita.
- Definir se tarefas/relatorios sao compartilhados ou privados. Se privados,
  adicionar ownership, migracao e estrategia de backfill antes de filtrar.
- Limitar tentativas de login; revisar armazenamento e rotacao de segredos,
  sem copiar chaves para o documento ou codigo.

Aceite: senhas nao sao persistidas em texto puro; testes cobrem 401/403 e acesso
cruzado entre usuarios. A habilitacao da protecao preserva o acesso legitimo do
consumidor por identidade de servico, nao removendo autorizacao.

### TD-02 - Proteger mutacoes e alinhar cookie com JWT no Web

Evidencias: formularios ja geram tokens, mas a validacao antiforgery explicita
aparece apenas no envio semanal; logout e uma action acessada por link GET em
[HomeController.cs](../TaskProjectWeb/Controllers/HomeController.cs) e
[_Layout.cshtml](../TaskProjectWeb/Views/Shared/_Layout.cshtml).

Plano: aplicar validacao antiforgery global para metodos inseguros, adaptar
AJAX para enviar o token e transformar logout em POST protegido. Revisar
Secure, HttpOnly, SameSite, HTTPS/HSTS fora de desenvolvimento e persistencia
protegida das chaves de Data Protection em ambientes com replicas.
Alinhar expiracao do ticket com a do JWT e tratar 401 da API como sessao
expirada, sem loop de redirecionamento nem renovacao implicita de token vencido.

Aceite: POST sem token e rejeitado, GET nao encerra sessao, formularios e AJAX
validos continuam funcionando, expiracao exige nova autenticacao com mensagem clara.

### TD-03 - Centralizar configuracao na composicao do host

Evidencias iniciais: `TasksQueries` e `UserQueries` possuiam construtores que criavam
contextos e liam arquivos locais. O construtor alternativo de `TasksQueries`
foi removido no TD-05; `UserQueries` ainda precisa dessa correcao.
A emissao de JWT usa outra leitura de JSON,
ignorando a configuracao efetiva do host. A factory de design time tambem
depende do diretorio corrente. Web e Infrastructure mantem configuracoes de
banco/JWT que precisam ser revisadas quanto a necessidade.

Plano:
- Usar Options com `ValidateOnStart` para JWT, URL da API e RabbitMQ; exigir
  configuracoes obrigatorias em vez de fallback silencioso para localhost.
- Remover criacao manual de contexto apos migrar os chamadores/testes para DI;
  manter a factory EF de design time, com fontes e comandos documentados.
- Host e dono da configuracao; biblioteca Infrastructure recebe valores por DI.
  Usar User Secrets no desenvolvimento e provedor de segredos/variaveis na entrega.
- Configurar CORS por ambiente, com origens exatas e uma policy aplicada.
- Manter migrations na Infrastructure; executar deploy de schema de forma
  controlada. Revisar o uso de `EnsureCreated` em
  [DbInitializer.cs](../TaskListProject.InfraStructure/Data/DbInitializer.cs),
  sem misturar esse mecanismo com migrations.

Aceite: configuracao ausente impede startup com erro identificavel; overrides
de ambiente sao respeitados na validacao e emissao de token; migrations
funcionam em banco vazio e em atualizacao. Web nao precisa de acesso ao banco.

### TD-04 - Nao transformar indisponibilidade em sucesso

Evidencias iniciais: `TasksQueries` capturava erros e retornava `false`;
[GetSummarizedTasksQueryHandler.cs](../TaskListProject.Application/Tasks/Queries/GetSummarizedTasksQueryHandler.cs)
retornava modelo vazio ao falhar; controllers repetiam catches com strings.
Esses fluxos foram corrigidos como suporte ao TD-05; ainda falta padronizar
os demais erros, incluindo login, autorizacao e respostas de recurso ausente.
Movimentacoes ja propagam falhas em vez de retornar saldo/lista ficticios:
preservar esse comportamento.

Plano: centralizar tratamento com `IExceptionHandler` e `ProblemDetails`.
Usar resultados tipados para falhas esperadas e excecoes para falhas inesperadas.
Definir 400 para validacao, 401/403 para acesso, 404 para ausencia, 409 para
conflito e 500/503 conforme a falha operacional. Registrar excecao uma vez na
fronteira adequada, com trace ID e sem dados sensiveis.
Corrigir tambem a criacao de tarefa para retornar o ID realmente persistido.

Aceite: banco indisponivel nunca produz 200 vazio; erros possuem formato e
trace ID consistentes; tarefa inexistente nao vira erro generico de servidor;
`Location` e corpo do 201 identificam o mesmo recurso.

### TD-05 - Consolidar CQRS sem criar camadas desnecessarias

Status: implementado; ver atualizacao e verificacao acima.

Evidencias iniciais: [TasksController.cs](../TaskProjectApi/Controllers/TasksController.cs)
dependia simultaneamente de mediator, handler concreto e interface de dados.
[ITasksQueries.cs](../TaskListProject.Application/Data/ITasksQueries.cs) incluia
`AddTask` e parametros `ref`; `Inclusion = "new"/"edit"` escolhe a escrita.

Plano:
- Mover commands, queries, handlers, validadores e resultados de casos de uso
  para Application, organizados por funcionalidade; registrar seus assemblies.
- Criar casos explicitos para criar, editar, buscar, listar e resumir tarefas,
  alem de consultar/inserir relatorios; migrar rotas incrementalmente.
- Separar contratos de leitura e escrita por necessidade do consumidor (ISP).
  Substituir `bool/ref` por retornos tipados; controllers apenas adaptam HTTP.
- Remover gradualmente o handler intermediario que apenas delega e manter a
  InfraStructure como implementacao dos ports de Application (DIP).
- Separar entidade de dominio, resultado de caso de uso, contrato HTTP e
  ViewModel. Nao compartilhar entidade EF com a UI para eliminar duplicacao.
  Remover anotacoes de tabela dos modelos HTTP e mover mapeamento EF para Infra.
- Encapsular invariantes onde houver regra real (SRP/OCP); nao criar interfaces
  para toda classe nem um repository generico sem necessidade.

Aceite: Application nao referencia API/EF/Infrastructure; handlers nao retornam
modelos da API; CRUD nao usa flags textuais; rotas e payloads atuais continuam
compativeis, inclusive `POST api/tasks/include`, ate migracao explicita dos clientes.
Adicionar testes de fronteira para essas restricoes.

### TD-06 - Executar validacao e consolidar regras de negocio

Evidencias: existem validadores e `AddValidatorsFromAssembly`, mas nao foi
encontrado pipeline MediatR ou chamada explicita de validacao.
[CreateMovementCommandValidator.cs](../TaskListProject.Application/AccountMovements/Commands/CreateMovementCommandValidator.cs)
exige valor positivo, enquanto handler e testes aceitam debito negativo.

Plano: decidir primeiro a convencao de credito/debito e preservar/migrar o
contrato; depois registrar um `ValidationBehavior` com `ValidateAsync` e
CancellationToken. Validar intervalo de datas, horarios, prioridade, paginacao
e invariantes do relatorio. Unificar o limite de periodo: o controller de
relatorio compara 1200 dias, mas informa limite de um ano.
Separar validacao estrutural de regras dependentes de estado; validacao na UI
e complementar, nunca a unica protecao.

Aceite: request invalido nao chega ao handler/repositorio e retorna 400
estruturado; debitos validos nao sao bloqueados por ativar o pipeline; testes
cobrem limites, datas invertidas e metadados de paginacao invalidos.

### TD-07 - Garantir invariantes sob concorrencia

Evidencias: criar movimentacao consulta saldo e grava em operacoes separadas.
A validacao de sobreposicao e um endpoint separado e nao e executada pelo
fluxo atual de gravacao; sua consulta usa `Date >= data`, sem limitar o dia.

Plano: validar sobreposicao dentro do comando de escrita, definir intervalos
adjacentes e restringir a consulta ao dia correto. Proteger leitura-validacao-
escrita contra concorrencia com estrategia comprovada no SQL Server (isolamento,
bloqueio por conta/agenda ou escrita condicional); transacao padrao ou
`rowversion` isolado nao garantem essas invariantes entre multiplas linhas.
Para edicao, definir deteccao de conflito e retorno 409. Manter commit atomico
por caso de uso; abstrair unidade de trabalho apenas se coordenar repositorios.

Aceite: teste com dois debitos simultaneos nao permite saldo negativo; duas
inclusoes concorrentes nao violam a regra de agenda; edicao ausente retorna
404 e conflito de versao tem tratamento explicito. Nao validar apenas com mocks.

### TD-08 - Persistencia assincrona e leitura previsivel

Plano: substituir I/O sincrono por EF async com CancellationToken desde HTTP;
usar projecoes e `AsNoTracking` nas leituras. Paginar listagem de tarefas,
limitar pageSize e ordenar com desempate por ID. Agregar resumos/saldos no banco
quando apropriado, evitando carregar todos os movimentos/tarefas.
Corrigir semantica de agrupamento por dia, intervalo final exclusivo e divisao
por zero no resumo. Confirmar indices e precisao monetaria a partir dos planos
de consulta e requisitos, com migrations quando houver alteracao de schema.

Aceite: cancelamento chega ao EF; pagina tem limite e ordem deterministica;
resumo cobre virada de dia, duracao zero e tarefas concluidas; comparar valores
antes/depois e medir quantidade de linhas/tempo no mesmo conjunto representativo.

### TD-09 - Tornar a integracao Web/API confiavel

Evidencias: [TaskReportApiClient.cs](../TaskProjectWeb/Services/TaskReportApiClient.cs)
retorna listas/modelos vazios ou booleanos em falhas e perde detalhes de status.
Login monta HTTP/JSON diretamente no controller, fora do cliente tipado.

Plano: extrair cliente de autenticacao tipado; propagar cancelamento e
interpretar ProblemDetails/status sem transformar indisponibilidade em ausencia
de dados ou credencial invalida. Descartar respostas HTTP corretamente.
Configurar timeout, circuit breaker e retries limitados para leituras seguras;
nao repetir POST automaticamente sem chave de idempotencia e suporte no servidor.
Apresentar erros de validacao no formulario e diferenciar 401, 404 e 5xx.

Aceite: testes simulam timeout, 401, 404, 400 e 503; o usuario ve estado de
erro distinto de lista vazia e conserva dados do formulario. Falha de POST
nao duplica tarefa/relatorio.

### TD-10 - Relatorio semanal e RabbitMQ ponta a ponta

Evidencias:
- Web monta relatorio com contadores zerados em
  [TaskController.cs](../TaskProjectWeb/Controllers/TaskController.cs).
- O caso de uso de consulta de relatorios (antes `TaskReportService`) le snapshots
  persistidos, nao agrega tarefas.
- Publisher abre conexao/canal por envio; persistencia da mensagem e fila quorum
  ja existem, mas nao ha confirmacao explicita de publicacao.
- [Consumidor](../RabbitMqConsumerReceive/Program.cs) usa URL/fila fixas,
  desabilita validacao de certificado, nao envia autenticacao ao endpoint
  protegido e recoloca falhas na fila sem limite.

Plano: gerar resumo real em caso de uso da API/Application, definindo periodo,
fuso e semana ISO/ano; Web apenas solicita o envio. Manter consulta de snapshots
se essa for a regra e nomear claramente geracao versus leitura.
Hospedar consumidor como worker com shutdown/cancelamento, identidade de servico
e TLS valido; configurar endpoints. Usar conexao de longa duracao e canais com
ciclo de vida/concorrencia adequados, publisher confirms, retry limitado e DLQ.
Definir identificador/versao da mensagem e deduplicacao persistida no consumidor.
Adotar outbox somente se houver exigencia de atomicidade entre commit de banco
e publicacao; nao introduzir event sourcing por causa desse fluxo.

Aceite: relatorio reflete tarefas reais; redelivery nao duplica persistencia;
mensagem invalida ou falha permanente vai para DLQ; queda de broker/API nao
gera loop infinito nem notificacao falsa de sucesso. Consumidor autentica sem
relaxar TLS/autorizacao. Testar datas na virada de ano.

### TD-11 - Construir rede de seguranca antes e durante a migracao

Evidencias iniciais: [DbTests.cs](../TestProject/DbTests.cs) dependia de banco/configuracao
local; foi substituido por testes SQLite isolados no TD-05.
[AccountMovementsTests.cs](../TestProject/AccountMovementsTests.cs) ainda cria
DbContext sem provider. Esses arquivos tambem usam asserts antigos, enquanto
o projeto referencia NUnit 4. O build foi validado na entrega TD-05; os testes
legados de movimentacoes sem provider nao fizeram parte da suite focada.

Plano:
- Preservar os testes de fronteira existentes e adaptar asserts ao NUnit atual.
- Separar testes unitarios de integracao; unitarios usam stubs dos ports,
  integracao usa SQL Server isolado/reproduzivel, com migrations reais.
- Adicionar testes HTTP com WebApplicationFactory para autenticacao, validacao,
  ProblemDetails e contratos, e testes do cliente Web com HttpMessageHandler.
- Incluir os cenarios concorrentes e de mensageria descritos acima.
- Criar CI para build de API/Web e testes, sem exigir banco pessoal; executar
  testes externos em job com dependencias provisionadas e segredos protegidos.

Aceite: checkout limpo executa suite documentada sem configuracao pessoal;
CI bloqueia regressao nas fronteiras e nos fluxos alterados. Cobertura e apoio,
nao substituto de testes das invariantes.

### TD-12 - Operacao e ambiente reproduziveis

Plano: expor liveness sem dependencia externa e readiness para SQL/broker
conforme os fluxos habilitados, sem publicar detalhes sensiveis. Padronizar
logging estruturado via ILogger e correlacao Web -> API -> mensagens; adicionar
traces/metricas de latencia, erros HTTP, falhas SQL, fila e DLQ.
Documentar/provisionar SQL Server e RabbitMQ locais, variaveis, TLS,
migrations e smoke tests. Fixar SDK compativel e revisar dependencias obsoletas;
habilitar Razor runtime compilation/BrowserLink apenas em desenvolvimento ou
remover se dispensaveis. Migrar Startup para Program e opcional, nao requisito
de SOLID nem prioridade maior que seguranca.
Corrigir o guia de execucao e o link do roadmap no
[README](../README.md) quando esse fluxo for consolidado.

Aceite: ambiente pode ser iniciado por outra pessoa seguindo o guia; readiness
reflete dependencias exigidas, liveness continua disponivel em falha externa;
uma falha pode ser rastreada entre Web/API/worker sem logar tokens ou senhas.

## 4. Sequencia sugerida de implementacao

1. **Baseline e contencao:** iniciar TD-11, reproduzir ORIGINAL-01/02,
   entregar TD-01/02 para os riscos de acesso e TD-03/04 para configuracao/erros.
   Nao esperar a migracao CQRS para fechar endpoints expostos.
2. **Uma fatia vertical de tarefas:** TD-05 para criar/editar com contratos
   preservados; TD-06 para validacao; TD-07 para atomicidade. Aplicar TD-08/09
   e testes a essa fatia antes de expandir aos demais casos.
3. **Demais funcionalidades:** migrar consultas, movimentacoes e relatorios;
   resolver a convencao de debitos antes de ativar validacao desses commands.
   Paralelizar TD-09 quando seus pre-requisitos estiverem entregues.
4. **Mensageria e operacao:** TD-10 apos disponibilizar identidade de servico
   e caso de uso da API; finalizar TD-12 e gates de CI/integracao do TD-11.

Cada entrega deve incluir testes do fluxo alterado, instrucoes de configuracao
e migrations quando necessarias, alem de estrategia de compatibilidade/rollback.
Nao remover rota ou alterar payload de Web, React, Angular ou consumidor sem
migrar seus chamadores ou manter adaptador temporario.

## 5. Direcao arquitetural e limites

```text
Web / React / Angular -> API -> Application -> Domain
                                ^
                                |
                         Infrastructure
```

API registra Infrastructure no composition root; implementacoes tecnicas
dependem dos ports definidos em Application. Web continua cliente HTTP, nao
orquestrador de regras de negocio nem cliente direto do banco.

Comecar com monolito modular, CQRS logico e o mesmo banco. Nao ha evidencia
nesta analise para justificar microservicos, bancos separados de leitura/escrita,
event sourcing, cache distribuido ou um framework generico de repositorios.
Introduzir essas estruturas apenas mediante necessidade e medicao.