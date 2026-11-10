using BicoTucano.Libraries.Login;
using BicoTucano.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BicoTucano.Libraries.Filtro
{
    

   
        public class FuncionarioAutorizacaoAttribute : Attribute, IAuthorizationFilter
        {

            public string _tipoFuncionarioAutorizado;
            public FuncionarioAutorizacaoAttribute(string TipoFuncionarioAutorizado = FuncionarioTipoConstant.Comum)
            {
                _tipoFuncionarioAutorizado = TipoFuncionarioAutorizado;
            }

            LoginFuncionario _loginFuncionario;
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                _loginFuncionario = (LoginFuncionario)context.HttpContext.RequestServices.GetService(typeof(LoginFuncionario));
                Models.Funcionario funcionario = _loginFuncionario.GetFuncionario();
                if (funcionario == null)
                {
                    context.Result = new RedirectToActionResult("Login", "Home", new { area = "Funcionario" });
                }
                else
                {
                    if (funcionario.NivelAcesso == Models.NivelAcesso.Comum && _tipoFuncionarioAutorizado == FuncionarioTipoConstant.Gerente)
                    {
                        context.Result = new ContentResult() { Content = "Acesso Negado", StatusCode = 403 };
                    }
                }
            }
        }
    }

