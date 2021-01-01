![licence-MIT](https://img.shields.io/github/license/oliveira-michel/Jhulis?style=flat-square) 
![tag](https://img.shields.io/github/v/release/oliveira-michel/Jhulis?include_prereleases&style=flat-square)
![last-commit](https://img.shields.io/github/last-commit/oliveira-michel/jhulis?&style=flat-square)

<p align="center"><img src="docs/images/jhulis-logo-150.png"/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="docs/images/OpenAPI-Logo-50.png"/></p>

# Jhulis (beta)

Melhore a qualidade das suas REST APIs.

Jhulis é um validador de boas práticas para contratos de API em [OpenAPI Specification](https://swagger.io/specification) baseado no [Guia de Design de APIs](https://oliveira-michel.github.io/artigos/2019/07/11/guia-de-design-rest.htm). Não existe uma convergência absoluta sobre o que é boa prática ou não, no entanto, ter consistência entre os padrões adotados pelas diversas APIs da organização é desejável.

Contratos de APIs em OpenAPI Specification permitem definir APIs de várias formas. Eles não estimulam práticas, apenas são um meio para declará-las. Alguns iniciantes na construção de APIs têm dificuldades em adotar alguns padrões e os experientes podem deixar passar alguns detalhes.

O uso do Jhulis permite que o desenvolvedor faça a validação do contrato baseado em um conjunto de regras de qualidade de contrato de REST APIs.

Para quem escreve o contrato, ele guia através das práticas adotadas pelo administrador do sistema.

Para quem administra e faz a gestão da qualidade das APIs, ele permite que se foque na avaliação funcional da API deixando detalhes pequenos mas importantes ao cargo do Jhulis. O administrador pode utilizar as regras padrões, configurá-las, criar novas ou alterá-las de forma fácil, desde que conheça C# e .NET Core.

Principais recursos:
* acesso via biblioteca para uso em sistemas .NET Core
* acesso via REST APIs permite chamadas via cliente REST (ex: Postman, Curl etc.)
* acesso via REST APIs permite permite integração com esteiras de CI
* mais de 30 regras que ajudam o administrador de sistemas a automatizar a gestão da qualidade dos contratos de API
* customização e configuração das regras
* categorização das regras
  
> *"Não tropeçamos nas grandes montanhas mas nas pequenas pedras." Augusto Cury*

## Experimentar online

Acesse https://oliveira-michel.github.io/jhulis para ter uma visão de cliente sobre o funcionamento do Jhulis.

## Build & Run

Faça download do [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) e instale.

Clone o projeto na sua máquina.

Na pasta `src\Jhulis.Rest` execute no prompt de comando:

`dotnet run`

As APIs estarão prontas quando o console apresentar as seguintes mensagens:

```
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Também é possível usar a bliblioteca Jhulis.Core caso você queira utilizar em sua aplicação .NET Core.


## Como usar REST API

Exemplos das chamadas encontram-se em https://github.com/oliveira-michel/Jhulis/raw/master/docs/Exemplos.postman_collection.json

### POST em /validate

Essa API é a forma mais simples de o desenvolvedor validar o contrato. Faça uma chamada na API de validação passando um contrato de API escrito em OpenAPI Specification v1, v2 ou v3 em YAML ou JSON. O Jhulis retornará o resultado da validação e cada regra que não passou.

```
curl --location --request POST 'http://localhost:5000/jhulis/v0/validate' \
--header 'Accept: text/plain' \
--header 'Content-Type: text/plain' \
--header 'Content-Type: text/plain' \
--data-raw 'swagger: "2.0"
info:
  description: "This is a sample server Petstore server.  You can find out more about     Swagger at [http://swagger.io](http://swagger.io) or on [irc.freenode.net, #swagger](http://swagger.io/irc/).      For this sample, you can use the api key `special-key` to test the authorization     filters."
  version: "1.0.0"
  title: "Swagger Petstore"
[... Continuação do Swagger ...]'
```

![Exemplo de chamada na /validate](https://github.com/oliveira-michel/Jhulis/raw/master/docs/images/validate-postman-example.jpg)

A resposta da validação pode ser em JSON ou Text. Utilize os headers `Accept: application/json` e `Accept: text/plain`.

### POST em /full-validate

Esta API é útil para ser utilizada em esteiras de CI para fazer a validação do contrato. Faça uma chamada na API de validação passando o contrato de API escrito em OpenAPI Specification v1, v2 ou v3 em YAML ou JSON **escapado** no objeto `content` e opcionalmente uma lista de supressions no objeto `supressions` para ignorar a execução de alguma regra específica. O Jhulis retornará o resultado da validação e cada regra que não passou.

```
curl --location --request POST 'http://localhost:5000/jhulis/v0/full-validate' \
--header 'Accept: application/json' \
--header 'Content-Type: application/json' \
--header 'Content-Type: text/plain' \
--data-raw '{
	"content": "swagger: \"2.0\"\ninfo:\n  description: \"This is a sample server Petstore server.  You can find out more about     Swagger at [http://swagger.io](http://swagger.io) or on [irc.freenode.net, #swagger](http://swagger.io/irc/).      For this sample, you can use the api key `special-key` to test the authorization     filters.\"\n  version: \"1.0.0\"\n  title: \"Swagger Petstore\"\n  termsOfService: \"http://swagger.io/terms/\"\n
    [... Continuação do Swagger escapado...]'",
	"supressions":[
		{
			"ruleName": "ContentEnvelope",
			"target": "*",
			"justification": "only example"
		},
		{
			"ruleName": "PathCase",
			"target": "Path='/pet/findByTags",
			"justification": "only another example"
		}
	]
}'
```
Sua esteira de CI pode verificar o `result` para determinar se o contrato passou com sucesso ou não pela validação.

A resposta da validação pode ser em JSON ou Text. Utilize os headers `Accept: application/json` e `Accept: text/plain`.

### POST em /escape

Para passar o contrato **escapado** no objeto `content` em /full-validate, utilize esta API para transformar um contrato com quebrar de linhas em um contrato escapado (com \\n, tratamento de aspas etc.).

```
curl --location --request POST 'http://localhost:5000/jhulis/v0/escape' \
--header 'Accept: text/plain' \
--header 'Content-Type: text/plain' \
--header 'Content-Type: text/plain' \
--data-raw 'swagger: "2.0"
info:
  version: "1.0.0"
  title: "Swagger Petstore"
  termsOfService: "http://swagger.io/terms/"
  contact:
    email: "apiteam@swagger.io"
  license:
    name: "Apache 2.0"
    url: "http://www.apache.org/licenses/LICENSE-2.0.html"
host: "petstore.swagger.io"
basePath: "/v2"
[... Continuação do Swagger ...]'
```

## Supressions

Na API `/full-validation` é possível passar um array de supressions para que o Jhulis não valide algumas regras em alguns itens do contrato.

```
"supressions":[
    {
        "ruleName": "",
        "target": "",
        "justification": ""
    }
]
```

Onde:
* `ruleName` é o nome da regra;
* `target` é o item que a regra não deve validar. Podem ser utilizados: 
  *  `*` para aplicar em todos os itens do contrato
  * `Path` para aplicar em um caminho específico. Ex: Path='/collection/{collectionId}/sub-collection'
  * `Operation` para aplicar em uma operação específica. São válidos os verbos presentes no contrato e deve estar em letra minúscula. Ex: Operation='get'
  * `Parameter` para aplicar em um parâmetro específico (query string ou header). Ex: Parameter='city'
  * `ResponseCode` para aplicar em um código de resposta HTTP específico. Ex: ResponseCode='200'
  * `Content` para aplicar a um content-type de resposta específico. Ex: Content='application/json'
  * `PropertyFull` para aplicar a uma propriedade específica. PropertyFull='address.city'
* `justification` é o espaço onde quem está submetendo uma exceção de validação explique o motivo da exceção.

O `target` pode acumular vários parâmetros, desde que montem uma ordem hierárquica sobre como os elementos se organizam. Por exemplo:
* Path='/collection/{collectionId}/sub-collection',Operation='post'
* Path='/collection/{collectionId}/sub-collection',Operation='post',ResponseCode='200',Content='application/json',PropertyFull='address.city'
* Path='/collection',Operation='delete',ResponseCode='200'

> Uma implementação possível para uma esteira de CI é que o arquivo de supressions esteja presente no diretório da aplicação e sujeito a aprovações de merge request por pessoas que tenham perfil pra isso. Assim, ficam armazenadas as justificativas para suprimir a avaliação de alguma regra e ao mesmo tempo controladas conforme critérios do administrador do sistema ou gestor da qualidade das APIs.
> 
> A esteira de CI no momento da execução, busca o arquivo do contrato + o arquivo de supressions e monta a chamada à API `/full-validate` para fazer a validação. Em seguida, verifica o estado do `result` para verificar se foi `Passed`.

## Níveis de severidade

As regras são classificadas em níveis: 
* `Hint` são regras que apontam oportunidades de melhoria. Nem sempre é possível ou faz sentido implementá-las. Ex: paginação, tipos de dado em strings que podem ser number etc.
* `Information` são regras que apontam oportunidades de colocar mais informações no contrato. Na maioria das vezes é possível complementá-las. Ex: descrições, exemplos etc.
* `Warning` são regras que levam o contrato à aderir à algumas boas práticas. Na maioria das vezes é possível implementá-las, mas existirão casos em que a regra pode não fazer sentido para o contexto. Ex: aderência a case types pré-definidos, uso de plural, falta de headers padrões, resposta sem HTTP Status Code padrões etc.
* `Error` são regras para padrões que sempre podem ser implementados. Ex: formato padrão da versão, remoção de barras duplas, montar URLs válidas etc.

Se um desenvolvedor usa a API `/validate` e uma regra não faz sentido, basta ele ignorar.

Se uma esteira de CI usa a API `/full-validate` e usa o resultado para interromper o build, caso o contrato não passe pelo Jhulis, a regra que não faz sentido deve ser suprimida através de `supressions`.

## Result

Além dos detalhes de cada regra, a execução retona um dos seguintes resultados:

* `Passed` atendeu à todas as regras;
* `PassedWithInformations` as regras que ainda restaram são do tipo information;
* `PassedWithWarnings` as regras que ainda restaram são do tipo warning;
* `Error` as regras que ainda restaram são do tipo error;

Para o administrador do sistema, a recomendação é que uma esteira de CI interrompa o build em caso de um `result` do tipo `Error` ou `PassedWithWarnings` porque é onde se concentram a maioria das regras mais importantes e que impactam mais a modelagem das APIs.

O desenvolvedor pode suprimir alguma regra que não faça sentido para o contexto dele via `supressions` e com isso, tornar o resultado `Passed`.

## Falso Positivos

Algumas regras devem dar falso positivos e alguns deles já são esperados. Por exemplo:

A regra *PathPlural* verifica se o segmento da URL, que não é um path parameter, é um plural. (/segmento**s**/{idSegmento}). Ela parte do princípio de que a grande maioria dos recursos expostos via API são coleções de entidades manipuladas através dos verbos. Estas coleções vão no plural. Em alguns cenários com menos ocorrência, o segmento poderá estar representando uma ação (/sms/**enviar**). Neste caso, a validação gerará um falso positivo, pois **enviar** não deve estar no plural. Esta mesma regra também pode errar plural de palavras como *Campi* (plural de campus) ou *Quaisquer* (plural de qualquer) ou palavras compostas.

Entende-se como aceitável estes falso positivos nas regras, dado que a regra acelerará a validação dos contratos de API na maioria dos cenários, trazendo mais ganhos para o processo do que transtorno.

Regras que estejam apresentando muitos falso positivos podem ser refinadas, reparametrizadas ou removidas nas futuras versões da aplicação. Abra um Issue para compartilhar sua experiência e sugestões para as regras.

### Eliminando Falso Positivos

Na URL `/validate`, que serve para apoio ao desenvolvimento e permite rápida validação do contrato, ao receber um falso positivo o desenvolvedor pode apenas ignorá-lo.

Na URL `/full-validate` que pode fazer parte de uma execução de CI, o administrador da esteira de CI pode definir um padrão de arquivo para declarar os `supressions` no repositório da sua aplicação.

## Regras

As regras abaixo foram codificadas para validar falhas frequentes na definição dos contratos de APIs e que por terem características mais técnicas do que funcionais, muitas vezes passam batido tanto pelo desenvolvedor do sistema quanto pelo analista que faz o quality assurance do contrato.

Os valores dos parâmetros são definidos no appsettings.json do projeto Jhulis.Rest.

#### BaseUrl

Deve ser definida uma URL base válida.

<sup>*O conjunto de atributos 'host', 'basePath' e 'schemes' devem ser definidos de forma a juntos formarem uma URL válida. Ex: schemes = 'https', host = 'api.suaempresa.com.br' e basePath = '/clientes/cadastros' formando 'https://api.suaempresa.com.br/clientes/cadastros'.*</sup>

#### ContentEnvelope

As propriedades da resposta devem estar contidas em uma propriedade (envelope) '{0}'.

<sup>*Retornar as propriedades da resposta em um "envelope" permite separar suas propriedades de outras que não representam o objeto principal, como paginação, mensagens, etc. Sendo que estas outras propriedades com características de metadados também devem ter seu próprio "envelope".*</sup>

Parâmetros: 
* `EnvelopeName: data` define o nome do atributo/propriedade onde o conteúdo deve ser retornado. Substitui o {0} na descrição.

#### ContentIn204
Respostas HTTP 204 não devem retornar nenhum conteúdo.

<sup>*O Content-Lenght de uma requisição HTTP com retorno 204 deve ser 0.*</sup>

#### DateWithoutFormatRule
Atributos ou parâmetros que representem datas devem ter o campo format definidos como date ou date-time.

<sup>*Verifique se o atributo ou parâmetro é realmente uma data e se for, defina o format.*</sup>

#### Description
As diferentes partes do contrato devem ter suas descrições preenchidas de forma a explicar o negócio sendo exposto para o cliente.

<sup>*Quanto melhor os itens forem descritos, mais fácil será para o entendimento do consumidor da API. Evite descrições vazias ou muito curtas.*</sup>

Parâmetros: 
* `LargeDescriptionLength: 25` define a quantidade de caracteres que o Jhulis considera como mínima para validar o Info.Description do contrato.
* `MidDescriptionLength: 15` define a quantidade de caracteres que o Jhulis considera como mínima para validar o descrições de paths, parâmetros e operações do contrato.
* `MinDescriptionLength: 5` define a quantidade de caracteres que o Jhulis considera para validar as descrições das propriedades.
* `TestDescriptionInOperation: False` define se o Jhulis irá validar descrições em Operations (get, post etc.).
* `TestDescriptionInPaths: False` define se o Jhulis irá validar descrições em Paths (URLs).

#### DescriptionQuality
Descrição deve respeitar respeitar regras de pontuação, acentuação e uso de maiúsculas e minúsculas.

<sup>*Inicie as descrições dos campos 'description' ou 'summary' com letra maiúscula e finalize com um ponto final.*</sup>

#### DoubleSlashes
Cada segmento da URL deve ter apenas uma barra '/'.

<sup>*Não termine um path com '/', nem coloque duas barras seguidas '//'.*</sup>

#### Empty200
Respostas HTTP 200 ou 206 devem retornar alguma propriedade no body.

<sup>*Mesmo que o resultado da resposta seja consequência de filtros que não retornem resultado, retorne a propriedade de envelope com valor nulo ou array vazio.*</sup>
#### EmptyExamples
Adicione exemplo às suas respostas.

<sup>*Exemplos ajudam o usuário a entender como funciona a API.*</sup>

#### ErrorResponseFormat
Quando a resposta é de erro (4xx ou 5xx) as propriedades da resposta devem seguir o padrão.

<sup>*As propriedades da resposta devem devem estar dentre estas: {0}. *</sup>

Parâmetros: 
* `NonObligatoryErrorProperties: details,fields.name,fields.message,fields.value,fields.detail` define os campos permitidos em uma resposta de erro, mas não obrigatórios. Substitui o {0} na descrição.
* `ObligatoryErrorProperties: code,message` define os campos obrigatório em uma resposta de erro. Substitui o {0} na descrição.

#### Http200WithoutPagination
Respostas do tipo HTTP 200 que representem coleções podem ser paginadas.

<sup>*Oferecer paginação para o cliente, dá flexibilidade para ele trabalhar com um payload do tamanho que for melhor para ele.*</sup>

Parâmetros:
* `ContentEnvelopeName: data` define qual é o envelope em que o Jhulis irá verificar se é um array.
* `PaginationEnvelopeName: pagination` define o nome de paginação em que o Jhulis irá verificar a existência.

#### Http201WithoutLocationHeader
Resposta do tipo HTTP 201 deve ter um Header Location

<sup>*Este tipo de resposta representa criação de dados e precisa indicar a URL que representa o recurso recém criado.*</sup>

#### Http3xxWithoutLocationHeader
Respostas do tipo HTTP 300, 301, 302, 303 e 307 devem ter um Header Location.

<sup>*Estes tipos de respostas representam redirecionamentos e precisam indicar o local do redirecionamento no header Location.*</sup>

#### IdPropertyResponse
O recurso deve deve ter um identificador e ele deve se chamar apenas '{0}'.

<sup>*Normalmente, a API REST expões recursos e recursos têm um ID que o identifica e diferencia entre eles. O tipo do recurso é identificado pelo nome da sua URL e um id deve ser representado por '{0}'.*</sup>

Parâmetros:

* `IdPropertyName: id` define o nome da propriedade padrão que identifica o recurso.

#### InfoContact

Preencha as informações de contato.

#### MessagesEnvelopeFormat

As propriedades dentro do envelope {0} devem serguir o padrão.

<sup>*As propriedades dentro do envelope {0} devem estar dentre estas: {1}.*</sup>

Parâmetros:

* `MessagesEnvelopeProperties: code,message` define os parâmetros que devem estar contidos no envelope de mensagens. Substitui o {1} na descrição.
* `MessagesEnvelopePropertyName: messages` define o nome do envelope de mensagens. Substitui o {0} na descrição.

#### NestingDepth

Tipos de retorno com muitos objetos aninhados podem estar apontando um problema de referência cíclica.

<sup>*A validação das regras verificará objetos de no máximo {0} de profundidade de aninhamento. Evite objetos com muitos aninhamentos e verifique se não há referência cíclica nesta propriedade.*</sup>

Parâmetros:
* `Depth: 5` define o quanto o Jhulis irá validar em propriedades aninhadas.

#### OperationSuccessResponse

Uma operação deve ter pelo menos uma resposta de sucesso (2xx).

<sup>*Verifique se você não esqueceu de colocar uma resposta de sucesso.*</sup>

#### PaginationEnvelopeFormat

As propriedades dentro do envelope {0} devem serguir o padrão.

<sup>*As propriedades dentro do envelope {0} devem estar dentre estas: {1}.*</sup>

Parâmetros:
* `PaginationEnvelopePropertyName: pagination` define o nome do envelope que guarda as propriedades de paginação. Substitui o {0} na descrição.
* `PropertiesInPagination: first,last,previous,next,page,isFirst,isLast,totalElements` define as propriedades que devem estar no envelope de paginação; Substitui o {1} na descrição.

#### PathAndIDStructure
Nos Paths, utilizar estrutura '/colecao/{idColecao}/subcolecao/{idSubColecao}'.

<sup>*As melhores práticas orientam que organizemos os paths como coleções de entidades. Estas coleções ser relacionam entre si através da organização delas no path. Não devemos criar segmentos de path que não sejam entidades ou serviços da API, ou seja, segmentos apenas para agrupamento. A função de agrupamento se dá apenas no "basePath".*</sup>

#### PathCase

Nos Paths, utilizar notação {0}. Ex: {1}.

Parâmetros:
* `CaseType: KebabCase` define o tipo de case a ser utilizado nos Paths. Substitui o {0} na descrição.
* `CaseTypeTolerateNumber: True` define se serão aceitos números ao validar o tipo de case.
* `Example: distritos-federais` define um exemplo que entra na descrição. Substitui o {1} na descrição.

#### PathParameter
O Path Parameter deve ser identificado como {0} em {1}. Ex: {2}.

<sup>*Cada Path Parameter deve ter um id único na URL e seguir uma nomenclatura que o relacione com o nome do path que ele representa.*</sup>

Parâmetros:

* `CaseType: CamelCase` define o tipo de case a ser utilizado nos Path Parameters. Substitui o {1} na descrição.
* `Example: idCliente` define um exemplo que entra na descrição. Substitui o {2} na descrição.
* `HumanReadeableFormat: 'id' + 'NomeSingularDoPath'` define a explicação do formato que entra na descrição. Substitui o {0} na descrição.
* `MatchEntityNamePercentage: 0.6` define quantos porcento de coincidência deve existir entre o nome do Path Parameter e o nome do seu recurso no Path.
* `PrefixToRemove: id` define algum prefixo a ser removido antes de fazer o teste de coincidência entre o nome do Path Parameter e o nome do seu recurso no Path.
* `Regex: ^(id[a-zA-Z]+)$` define a expressão regular que testa o padrão esperado para os Path Parameters.
* `SufixToRemove: ` define algum sufixo a ser removido antes de fazer o teste de coincidência entre o nome do Path Parameter e o nome do seu recurso no Path.

#### PathPlural
O Path na maioria das vezes deve ser um substantivo no plural. Ex: clientes.

<sup>*Em alguns cenários menos frequentes, o nome do path pode não seguir a regra do substantivo e/ou representar um serviço (verbo), neste caso, utilize o arquivo de configuração para não processar esta regra.*</sup>
#### PathTrailingSlash
Não colocar '/' no final dos paths.

<sup>*Alguns sistemas ignoram-nas, no entanto, outros podem ter problemas no roteamento das chamadas com URLs terminadas em '/'.*</sup>

#### PathWithCrudNames

Paths não devem conter nomes representando ações de CRUD.

<sup>*As ações são representadas pelos verbos HTTP e os paths devem representar apenas os nomes das entidades e/ou serviços.*</sup>

Parâmetros:

* `WordsToAvoid: get,consultar,recuperar,listar,ler,obter,post,salvar,gravar,enviar,postar,path,atualizar,delete,apagar,deletar,remover,excluir` define palavras que são proibidas de usar nos Paths.

#### PropertyCase

Nas propriedades, utilizar notação {0}. Ex: {1}.

Parâmetros:

* `CaseType: CamelCase` define o tipo de case a ser utilizado nas propriedades. Substitui o {0} na descrição.
* `CaseTypeTolerateNumber: True` define se serão aceitos números ao validar o tipo de case.
* `Example: enderecoResidencial` define um exemplo que entra na descrição. Substitui o {1} na descrição.


#### PropertyNamingMatchingPath

As propriedades dentro de uma URL não precisam repetir o nome da URL.

<sup>*Por exemplo, em uma URL /clientes, as propriedades podem ser apenas nome, endereco, idade. Evite nomeá-las repetindo o nome do path (nomeCliente, enderecoCliente, idadeCliente, etc).*</sup>

#### PropertyStartingWithType

As propriedades não devem ter seus nomes iniciando com tipo delas.

<sup>*Não use notação húngara. Por exemplo, idade deve se chamar "idade", não intIdade. O tipo da propriedade pode ser melhor documentado na descrição e através dos exemplos.*</sup>

Parâmetros:

* `WordsToAvoid: bool,byte,char,dbl,decimal,double,flag,float,indicador,int,integer,long,nr,obj,sbyte,str,string,uint,ulong,short,ushort` define palavras que são proibidas de usar como prefixo nas propriedades.

#### ResponseWithout4xxAnd500

A API deve conter ao menos uma resposta de erro 4xx e uma resposta 500.

<sup>*Verifique se há pelo menos uma resposta de erro 4xx tratando o request e uma resposta 500 para problemas no servidor.*</sup>

#### StringCouldBeNumber

Atributos ou parâmetros que representem números ou valores financeiros devem ser tipados como number.

<sup>*Verifique se o atributo ou parâmetro pode ser representado como number.*</sup>

Parâmetros:

* `CurrencySymbols: $,.?,?,?.,?.?.,??,???,???.,¢,£,¥,€,AMD,Ar,AUD,B/.,BND,Br,Bs.,Bs.,Bs.S.,C$,CUC,D,Db,din.,Esc,ƒ,FOK,Fr,Fr.,Ft,G,GBP,GGP,INR,JOD,K,Kc,KM,kn,kr,Ks,Kz,L,Le,lei,m,MAD,MK,MRU,MT,Nfk,Nu.,NZD,P,PND,Q,R,R$,RM,Rp,Rs,RUB,S/.,SGD,Sh,Sl,so'm,T,T$,UM,USD,Vt,ZAR,ZK,zl` define uma lista de caracteres considerados como notação de moeda a ser utilizado para determinar se o valor do campo pode ser moeda.


#### ValidResponseCodes

A API deve utilizar códigos de resposta HTTP válidos.

<sup>*Utilize apenas um destes códigos HTTP: {0}.*</sup>

Parâmetros:

* `ValidHttpCodes: 200,201,202,204,206,301,303,304,307,400,401,403,404,405,410,414,422,428,429,500,501,503,504` define uma lista de HTTP Status Codes permitidos. Substitui o {0} na descrição.


#### VersionFormat

A versão da API deve respeitar o formato {0}. Ex: {1}.

<sup>*Regex do formato: {0}.*</sup>

Parâmetros: 

* `Example: v2` define um exemplo que entra na descrição. Substitui o {1} na descrição.
* `HumanReadeableFormat: 'v' + integer version number` define a explicação do formato que entra na descrição. Substitui o {0} na descrição.
* `RegexExpectedFormat: ^(v\d+)$` define a expressão regular que testa o padrão esperado para os Path Parameters.

# Contribua

Para feedbacks ou dúvidas, entre em contato via [Discussions](https://github.com/oliveira-michel/Jhulis/discussions) ou abra um [Issue](https://github.com/oliveira-michel/Jhulis/issues) em caso de bugs.

Para colaborar com o desenvolvimento, entre em contato via [Discussions](https://github.com/oliveira-michel/Jhulis/discussions). Sua colaboração é bem vinda.

Para Pull Requests, seguir o [Git Flow](https://github.com/App-vNext/Polly/wiki/Git-Workflow), criar seu próprio fork e solicitar o Pull Request.

Adote as boas práticas de código: revise os apontamentos do [Code Analysis](https://marketplace.visualstudio.com/items?itemName=VisualStudioPlatformTeam.MicrosoftCodeAnalysis2019) ou do [ReSharper](https://marketplace.visualstudio.com/items?itemName=JetBrains.ReSharper), adote as [Diretrizes de nomenclatura da Microsoft](https://docs.microsoft.com/pt-br/dotnet/standard/design-guidelines/naming-guidelines) e [Convenções de Nomes](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions).

## Versionamento

O Jhulis é versionado seguindo o https://semver.org. Portanto:
* **Path**: Documentação, correção de bugs e refatoração.
* **Minor**: Criação / remoção de regras, novas funcionalidades ou alteração do comportamento de alguma regra.
* **Major**: Alterações nas APIs ou no Core que quebrem os consumidores atuais.

## Detalhes técnicos

Se você pretende desenvolver ou alterar comportamentos do Jhulis que ainda não estão disponíveis através de parametrizações, verifique as informações à seguir.

### Funcionamento básico

O Jhulis, usando o [OpenAPI.Net](https://github.com/microsoft/OpenAPI.NET/), converte um documento OAS v1, v2 ou v3 em YAML ou JSON em um objeto comum do tipo `Microsoft.OpenApi.Models.OpenApiDocument` onde é possível acessar programaticamente todos os componentes do contrato: paths, verbos, exemplos, descrições, etc.

As controllers da `Jhulis.Rest.ContractController` chamam o `Jhulis.Core.Processor`, que executa cada umas das regras em sequência e acumula o resultado de cada uma delas em um objeto `Jhulis.Core.Result.ResultItens`.

Cada regra acessa os itens do `OpenApiDocument` pertinentes à ela e faz o teste. Se não passar, coloca o texto explicando a regra e onde infringiu no `Jhulis.Core.Result.ResultItens` que está presente em uma classe base `Jhulis.Core.Rules.RuleBase` da qual todas as regras herdam.

Como os itens dos contratos no `OpenApiDocument` são "aninhados", repetir vários loops o tempo todo nas regras não é produtivo. Para evitar isso, algumas extrações que são usadas com frequência, por exemplo, listar todas as propriedades de um body, estão presentes na `Jhulis.Core.Helpers.Extensions.OpenApiDocumentExtensions`. Como nestas extrações algumas estruturas do OpenApiDocument são simplificadas, para evitar reprocessamento o tempo todo, armazeno-as em cache após a primeira execução. O cache vem da Jhulis.Rest para que haja apenas um por contexto de execução da API.

### Como trocar o base path da aplicação

No `Jhulis.Rest\appsetting.json` definir o atributo `"BasePath": "/jhulis/v0"`.

### Como trocar o host e porta da aplicaçao

Editar o `Jhulis.Rest\Properties\launchSettings.json`

### Como criar uma nova regra

Crie uma classe nova em `Jhulis.Core.Rules`.com um nome representando o que ela valida + `Rule.cs`. Baseie sua implementação em uma já existente. Defina o ruleName = "NomeDaSuaRegra". Na herança à base, defina o nível de severidade `Severity.[Hint, Error etc.]`. Lembre-se de adicionar a verificação por supressions no código.

Em `Jhulis.Core.Resources.RuleSet.resx` crie uma nova entrada com o nome da sua regra e preencha a descrição e os detalhes. Baseie em uma regra já existente.

Adicione a chamada à nova regra no `Jhulis.Core.Processor`.

Em `Jhulis.Core.Test.Rules` baseie-se nos testes já existentes e crie um teste para a nova regra. Escreva com cenários um OAS que infrinjam a regra e que não infrinjam. Utilize o XUnit.

### Alterar uma regra

Baseie-se nas instruções acima e altere o código de uma regra já existente.

### Trocar a ordem da execução

Em `Jhulis.Core.Processor` troque a ordem.

### Remover regra

Em `Jhulis.Core.Processor` remova a regra.

### Alterar o texto da regra ou traduzir

Altere o `Jhulis.Core.Resources.RuleSet.resx`.

### Health Check

Responde em [host]/health. Está configurada na Startup.cs.

### CORS

Os hosts permitidos para fazer chamadas à partir de um website carregado no cliente via navegador ficam no appsettings.json na chave Cors. Use "\*" para liberar qualquer acesso.
```
 "Cors": {
    "AllowedClients": [ "https://oliveira-michel.github.io" ]
  }
```

### Throttling

Capacidades básicas de limitação na quantidade e tamanho das requisições estão configuradas no appsettings.json na chave "Kestrel"."Limits" conforme [documentação do Kestrel](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits?view=aspnetcore-5.0).

# Suporte

Para feedbacks ou dúvidas, entre em contato via [Discussions](https://github.com/oliveira-michel/Jhulis/discussions) ou abra um [Issue](https://github.com/oliveira-michel/Jhulis/issues) em caso de bugs.

# Publicações

https://oliveira-michel.github.io/artigos/2020/05/11/como-o-jhulis-te-ajuda-a-manter-a-qualidade-dos-seus-contratos-de-apis.htm
