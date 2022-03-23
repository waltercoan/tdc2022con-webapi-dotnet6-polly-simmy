# .NET + Microsserviços: alternativas open source para descomplicar a sua vida
Autores: Walter Silvestre Coan e Renato Groffe

Apresentado na trilha de Arquitetura .NET no TDC 2022 Connections, no dia 23/03/2022 (https://thedevconf.com/tdc/2022/connections/trilha-arquitetura-dotnet).

Objetivo: demonstrar a implementação da técnica de Circuit Breaker em uma aplicação que recebe requisições REST e persiste os dados em uma fila no Azure Storage Account.

## Circuit Breaker

- appsettings.json: Arquivo de configuração da aplicação, que deve possuir a connection string para a Storage Account, o nome da fila que será criada, e as configurações para o ambiente de teste utilizando a técnica de Monkey Caos.

`"ConnectionStrings": {
    "StorageConnectionString": "UseDevelopmentStorage=true"
  },
  "QueueName": "queuecourseregistry",
  "MonkeyCaosEnable": false,
  "MonkeyCaosInjectionRate": 0.5
`

- Program: Classe main da aplicação no formato Top-level statements. Esse arquivo possui uma public partial class para permitir que a classe WebApplicationFactory consiga encontrar o Assembly ao simular a aplicação em memória para teste.
- CourseRegistration: Classe que representa o objeto model da aplicação.
- CourseRegistrationController : Classe que implementa o controller da REST API, recebe pelo construtor uma instância da class que implementa a interface ICircuitBreaker e outras dependências como ILogger. No método Post, executa a cadeia de políticas do Polly que protegem o código que realiza a conexão com a Queue Storage Account para publicar a mensagem.
- CircuitBreaker: Classe que cria a política de Circuit Breaker do Polly dentro de um PolicyWrap. Adiciona também a Bulkhead Isolation que limita o número de chamadas paralelas do código.

## Monkey Caos
- APIApplication: Classe que utiliza a WebApplicationFactory para criar uma instância da aplicação em memória para realizar os testes de integração. Substitui a implementação de ICircuitBreaker de CircuitBreaker por CircuitBreakerWithMonkeyCaos. Necessita que a classe Program possua uma classe parcial publica e que o projeto possua um arquivo .sln para definir corretamente o diretório base da aplicação.
- CircuitBreakerWithMonkeyCaos: Classe que implementa a interface ICircuitBreaker, e incluir na PolicyWrap a MonkeyPolicy.InjectExceptionAsync do projeto Simmy, para injetar falhas na aplicação considerando um percentual configurado no arquivo appsettings.json
- MonkeyCaosTest: Classe que implementa um teste de integração usando o xUnit. Utiliza a instância da aplicação através da classe APIApplication, e realiza request para o método da REST API validando o Status Code de retorno.
- Pacote XUnitLogger: implementação pública de um exemplo de instância do padrão ILogger permitindo que os logs sejam impressos no console da execução do xUnit.


## Instalação do Azurite
https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=npm?WT.mc_id=AZ-MVP-5003638

## Execução do Azurite
`  azurite --silent --location c:\azurite --debug c:\azurite\debug.log `

## Dependências do projeto
https://github.com/App-vNext/Polly
https://github.com/Polly-Contrib/Simmy

"Azure.Core" Version="1.22.0"
"Azure.Storage.Common" Version="12.10.0"
"Azure.Storage.Queues" Version="12.9.0"
"Microsoft.AspNetCore.Hosting" Version="2.2.7"
"Microsoft.AspNetCore.Mvc.Testing" Version="6.0.3
"Microsoft.AspNetCore.TestHost" Version="6.0.3"
"Microsoft.NET.Test.Sdk" Version="17.1.0"
"Polly" Version="7.2.3"
"Polly.Contrib.Simmy" Version="0.3.0" 
"Swashbuckle.AspNetCore" Version="6.3.0" 
"xunit" Version="2.4.1" 
"xunit.runner.visualstudio" Version="2.4.3"

## Arquivo appsettings.json
`{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "StorageConnectionString": "UseDevelopmentStorage=true"
  },
  "QueueName": "queuecourseregistry"
}
`
## Configuração da política de retry da Storage Account Client
https://docs.microsoft.com/pt-br/azure/architecture/best-practices/retry-service-specific?WT.mc_id=AZ-MVP-5003638

## Teste de integração com Minimal APIs
Fonte: https://www.hanselman.com/blog/minimal-apis-in-net-6-but-where-are-the-unit-tests

### Dicas importantes
- Criar o arquivo .sln para o projeto
