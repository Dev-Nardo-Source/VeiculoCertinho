# VeiculoCertinho

## Requisitos para execução do Chrome Headless

Para que o serviço de consulta de veículos funcione corretamente, é necessário que o ambiente possua as dependências do Chrome instaladas. Em especial, no Windows, o Chrome headless pode exigir o **Microsoft Visual C++ Redistributable** correspondente à arquitetura do Chrome baixado.

- [Download Visual C++ Redistributable](https://learn.microsoft.com/en-US/cpp/windows/latest-supported-vc-redist?view=msvc-170)

Se ocorrer erro de "configuração lado a lado incorreta" ao iniciar o Chrome, instale o pacote acima e tente novamente.

## Diagnóstico

- Consulte o log de eventos do Windows para detalhes de falha lado a lado.
- Use a ferramenta de linha de comando `sxstrace.exe` para obter mais informações.

## Observação

O caminho do executável do Chrome e argumentos são registrados no log da aplicação para facilitar o diagnóstico de problemas de inicialização.
