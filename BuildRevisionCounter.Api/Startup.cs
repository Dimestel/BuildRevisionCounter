using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Filters;
using BuildRevisionCounter.Api;
using BuildRevisionCounter.Api.Security;
using BuildRevisionCounter.DAL.Repositories;
using BuildRevisionCounter.DAL.Repositories.Interfaces;
using Microsoft.Owin;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.FilterBindingSyntax;
using Ninject.Web.WebApi.OwinHost;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace BuildRevisionCounter.Api
{
    public class Startup
    {
        private static StandardKernel _kernel;
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();

            app.UseNinjectMiddleware(GetKernel);
            app.UseNinjectWebApi(config);

            Setup(GetKernel());
        }

        /// <summary>
        /// Создает ядро Ninject.
        /// </summary>
        /// <returns>Созданное ядро Ninject.</returns>
        private static IKernel GetKernel()
        {
            if (_kernel == null)
            {
                _kernel = new StandardKernel();
                try
                {
                    RegisterServices(_kernel);
                    return _kernel;
                }
                catch
                {
                    _kernel.Dispose();
                    throw;
                }
            }
            return _kernel;
        }

        /// <summary>
        /// Загрузка модулей Ninject и регистрация сервисов.
        /// </summary>
        /// <param name="kernel">Ядро Ninject.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IUserRepository>().To<UserRepository>();
            kernel.Bind<IRevisionRepository>().To<RevisionRepository>();
            kernel.BindHttpFilter<BasicAuthenticationFilter>(FilterScope.Controller).WhenControllerHas<BasicAuthenticationAttribute>();
        }

        private void Setup(IKernel kernel)
        {
            IUserRepository userRepository = kernel.Get<IUserRepository>();
            userRepository.EnsureUsersIndex();
            userRepository.EnsureAdminUser();
        }
    }
}
