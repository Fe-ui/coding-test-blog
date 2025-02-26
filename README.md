# BlogDotkon

Este projeto implementa um sistema básico de blog onde os usuários podem visualizar, criar, editar e excluir postagens.

-Permite registro e login de usuarios;
-Realiza o gerenciamento das postagens, onde apenas usuarios autenticados podem realizar a criação, edição e exclusão de suas postagens;
-Qualquer visitante pode apenas visualizar postagens;
-Possui WebSockets configurado.

## 🚀 Instalação

Passo a passo para instalar e configurar o projeto localmente. Por exemplo:

1. Clone o repositório:
   git clone https://github.com/Fe-ui/coding-test-blog.git
   
2. Antes de rodar o projeto, abra o terminal e execute os seguintes comandos:

dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Swashbuckle.AspNetCore

3. Para criação do database com migrations, se faz necessário rodar os seguintes comandos no terminal:

dotnet ef migrations add CreateDatabase
dotnet ef database update

OBS: Ajustar no arquivo appsettings.json string de conexão de acordo as credenciais do executante do projeto.

