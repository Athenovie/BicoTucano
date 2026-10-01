using Microsoft.AspNetCore.Mvc;

namespace BicoTucano.Libraries.Filtro
{
    using AppLogin.Libraries.Login;
    using AppLogin.Models.Constant;
    using BicoTucano.Libraries.Login;
    using BicoTucano.Models.Constants;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    namespace AppLogin.Libraries.Filtro
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
                    context.Result = new RedirectToActionResult("Login", "Home", null);
                }
                else
                {
                    if (funcionario.Tipo == FuncionarioTipoConstant.Comum && _tipoFuncionarioAutorizado == FuncionarioTipoConstant.Administrador)
                    {
                        context.Result = new ForbidResult();
                    }
                }
            }
        }
    }

}
