# VeiculoCertinho

## Requisitos para execu��o do Chrome Headless

Para que o servi�o de consulta de ve�culos funcione corretamente, � necess�rio que o ambiente possua as depend�ncias do Chrome instaladas. Em especial, no Windows, o Chrome headless pode exigir o **Microsoft Visual C++ Redistributable** correspondente � arquitetura do Chrome baixado.

- [Download Visual C++ Redistributable](https://learn.microsoft.com/en-US/cpp/windows/latest-supported-vc-redist?view=msvc-170)

Se ocorrer erro de "configura��o lado a lado incorreta" ao iniciar o Chrome, instale o pacote acima e tente novamente.

## Diagn�stico

- Consulte o log de eventos do Windows para detalhes de falha lado a lado.
- Use a ferramenta de linha de comando `sxstrace.exe` para obter mais informa��es.

## Observa��o

O caminho do execut�vel do Chrome e argumentos s�o registrados no log da aplica��o para facilitar o diagn�stico de problemas de inicializa��o.
